using IS = SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using System.Text.Json.Nodes;
using System.Text;
using System.Diagnostics;

namespace MCAsset2Font
{
    internal class Fontify
    {
        public string assetsPath;
        public bool verbose;
        private OTF? font;
        private bool bold;
        private bool italic;
        public Fontify(string assetsPath, bool verbose = false)
        {
            this.assetsPath = assetsPath;
            this.verbose = verbose;
        }
        public OTF MakeFont(string fontPath, bool bold = false, bool italic = false)
        {
            Stopwatch? timer = this.verbose ? Stopwatch.StartNew() : null;
            this.font = new OTF(bold, italic, this.verbose);
            this.bold = bold;
            this.italic = italic;
            JsonObject root = (JsonObject)(JsonNode.Parse(File.ReadAllText(fontPath)) ?? throw new Exception($"{fontPath} contains null!"));
            JsonArray providers = (JsonArray?)root["providers"] ?? throw new Exception($"{fontPath} contains no \"providers\"!");
            foreach (JsonObject? provider in providers)
            {
                this.ProcessReference(provider ?? throw new Exception($"{fontPath} how did we get here?!"), fontPath);
            }
            if (this.verbose)
            {
                timer!.Stop();
                Console.WriteLine($"[Fontify] Took {timer!.ElapsedMilliseconds}ms");
            }
            return this.font;
        }
        private void ProcessReference(in JsonObject reference, string referencePath)
        {
            string type = (string?)reference["type"] ?? throw new Exception($"{referencePath} contains a provider with no \"type\"!");
            switch (type)
            {
                case "reference":
                    string id = (string?)reference["id"] ?? throw new Exception($"{referencePath} contains a reference with no \"id\"!");
                    if (this.verbose) Console.WriteLine($"[ProcessReference] {id}");
                    string idFull = this.FontPath(id);
                    JsonObject root = (JsonObject)(JsonNode.Parse(File.ReadAllText(idFull)) ?? throw new Exception($"{idFull} contains null!"));
                    JsonArray providers = (JsonArray?)root["providers"] ?? throw new Exception($"{idFull} contains no \"providers\"!");
                    foreach (JsonObject? provider in providers)
                    {
                        if (provider is null) throw new Exception($"{idFull} has a provider that is null!");
                        this.ProcessReference(provider, idFull);
                    }
                    break;
                case "bitmap":
                    this.ProcessBitmap(reference, referencePath);
                    break;
                case "space":
                    this.ProcessSpace(reference, referencePath);
                    break;
                default: throw new NotImplementedException($"{referencePath} contains a provider with \"type\": \"{type}\"!");
            }
        }
        private void ProcessBitmap(in JsonObject provider, string bitmapPath)
        {
            string file = (string?)provider["file"] ?? throw new Exception($"{bitmapPath} contains a bitmap with no \"file\"!");
            if (this.verbose) Console.WriteLine($"[ProcessBitmap] {file}");
            int height = (int?)provider["height"] ?? 8;
            int ascent = (int?)provider["ascent"] ?? 7;
            JsonArray chars = (JsonArray?)provider["chars"] ?? throw new Exception($"{bitmapPath} contains bitmap {file} with no \"chars\"!");
            StringRuneEnumerator[] runesTable = new StringRuneEnumerator[chars.Count];
            for (int y = 0; y < chars.Count; y++)
            {
                runesTable[y] = ((string?)chars[y])?.EnumerateRunes() ?? throw new Exception($"{bitmapPath} contains bitmap {file} with no string at chars[{y}]!");
            }
            string fileFull = this.TexturePath(file);
            IS.Image<IS.PixelFormats.A8> pImage = IS.Image.Load<IS.PixelFormats.A8>(fileFull);
            int pYLength = chars.Count;
            int pXLength = runesTable[0].Count();
            int pWidth = pImage.Width / pXLength;
            Contourify regularContourify = new Contourify(4, 0, 4, 0, -4, (ascent - 0.5) * 4);
            Contourify contourify;
            if (italic)
            {
                contourify = new Contourify(4, -1, ascent + 0.5, 0, -4, (ascent - 0.5) * 4);
            }
            else
            {
                contourify = regularContourify;
            }
            for (int y = 0; y < runesTable.Length; y++)
            {
                StringRuneEnumerator runesRow = runesTable[y];
                int x = 0;
                foreach (Rune gChar in runesRow)
                {
                    if (gChar.Value != 0)
                    {
                        IS.Image<IS.PixelFormats.A8> gImage;
                        if (bold)
                        {
                            // Draw the glyph twice
                            IS.Image<IS.PixelFormats.A8> tImage = pImage.Clone(i => i.Crop(new IS.Rectangle(x * pWidth, y * height, pWidth, height)));
                            gImage = new IS.Image<IS.PixelFormats.A8>(tImage.Width + 1, tImage.Height, new IS.PixelFormats.A8(0));
                            gImage.Mutate(i => i.DrawImage(tImage, 1).DrawImage(tImage, new IS.Point(1, 0), 1));
                        }
                        else
                        {
                            gImage = pImage.Clone(i => i.Crop(new IS.Rectangle(x * pWidth, y * height, pWidth, height)));
                        }
                        List<OTF.Glyph.Contour> gContours = contourify.Bitmap2Contours(gImage, out short xMin, out short xMax);
                        if (gContours.Count > 0)
                        {
                            this.font!.AddGlyph(gChar, (ushort)((italic ? regularContourify.XMax(gImage) : xMax) + 2), xMin, gContours);
                        }
                        gImage.Dispose();
                    }
                    x++;
                }
            }
            pImage.Dispose();
        }
        private void ProcessSpace(in JsonObject provider, string spacePath)
        {
            if (this.verbose) Console.WriteLine("[ProcessSpace]");
            JsonObject advances = (JsonObject?)provider["advances"] ?? throw new Exception($"{spacePath} contains a space with no \"advances\"!");
            foreach (KeyValuePair<string, JsonNode?> kvp in advances)
            {
                Rune ch = (Rune)0;
                foreach (Rune r in kvp.Key.EnumerateRunes())
                {
                    ch = r;
                    break;
                };
                ushort advanceWidth = (ushort?)kvp.Value ?? throw new Exception($"{spacePath} contains advance \"{ch}\" with no number!");
                if (ch.Value != 0)
                {
                    this.font!.AddSpaceGlyph(ch, (ushort)(advanceWidth * 4));
                }
            }
        }
        private string TexturePath(string identifier)
        {
            string[] identifierParts = identifier.Split(':', 2);
            return (identifierParts.Length > 1 ?
                $"{this.assetsPath}/{identifierParts[0]}/textures/{identifierParts[1]}" :
                $"{this.assetsPath}/minecraft/textures/{identifierParts[0]}") +
                (identifierParts[^1].EndsWith(".png") ? "" : ".png");
        }
        private string FontPath(string identifier)
        {
            string[] identifierParts = identifier.Split(':', 2);
            return (identifierParts.Length > 1 ?
                $"{this.assetsPath}/{identifierParts[0]}/font/{identifierParts[1]}" :
                $"{this.assetsPath}/minecraft/font/{identifierParts[0]}") +
                (identifierParts[^1].EndsWith(".json") ? "" : ".json");
        }
    }
}
