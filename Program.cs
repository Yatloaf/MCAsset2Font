using NS = MCAsset2Font.OTF.NameTable.NameString;

namespace MCAsset2Font
{
    internal class Program
    {
        private static readonly Dictionary<string, NS> idToNameString = new Dictionary<string, NS>
        {
            ["copyright"] = NS.COPYRIGHT,
            ["family"] = NS.FAMILY,
            ["subfamily"] = NS.SUBFAMILY,
            ["identifier"] = NS.IDENTIFIER,
            ["full"] = NS.FULL,
            ["version"] = NS.VERSION,
            ["postscript"] = NS.POSTSCRIPT,
            ["trademark"] = NS.TRADEMARK,
            ["manufacturer"] = NS.MANUFACTURER,
            ["designer"] = NS.DESIGNER,
            ["description"] = NS.DESCRIPTION,
            ["vendor-url"] = NS.VENDOR_URL,
            ["designer-url"] = NS.DESIGNER_URL,
            ["license"] = NS.LICENSE,
            ["license-url"] = NS.LICENSE_URL,
            ["typographic-family"] = NS.TYPOGRAPHIC_FAMILY,
            ["typographic-subfamily"] = NS.TYPOGRAPHIC_SUBFAMILY,
            ["compatible-full"] = NS.COMPATIBLE_FULL,
            ["sample-text"] = NS.SAMPLE_TEXT,
            ["postscript-findfont"] = NS.POSTSCRIPT_FINDFONT,
            ["wws-family"] = NS.WWS_FAMILY,
            ["wws-subfamily"] = NS.WWS_SUBFAMILY,
            ["light-palette"] = NS.LIGHT_PALETTE,
            ["dark-palette"] = NS.DARK_PALETTE,
            ["postscript-prefix"] = NS.POSTSCRIPT_PREFIX
        };
        private static bool verbose = false;
        static void Main(string[] args)
        {
            /*
            string EXTRACTED_JAR_PATH = ...;
            string DESTINATION_PATH = ...;

            Fontify fontify = new Fontify($"{EXTRACTED_JAR_PATH}/assets");
            OTF font = fontify.MakeFont($"{EXTRACTED_JAR_PATH}/assets/minecraft/font/default.json");
            font.SetString(NS.FAMILY, "Foobar");
            font.SetString(NS.SUBFAMILY, "Regular");
            font.SetString(NS.IDENTIFIER, "FizzBuzz: Foobar Regular");
            font.SetString(NS.FULL, "Foobar Regular");
            font.SetString(NS.VERSION, "Version 0.001");
            font.SetString(NS.POSTSCRIPT, "Foobar-Regular");
            font.SetString(NS.DESCRIPTION, "MCAsset2Font Test");
            font.SetString(NS.SAMPLE_TEXT, "Minecrafty");
            Console.WriteLine("[Serialize]");
            File.WriteAllBytes($"{DESTINATION_PATH}", font.Serialize());
            */

            PromptLoop();
        }
        private static void PromptLoop()
        {
            string? assetsPath = null;
            string? namespacePath = null;
            string? destinationPath = null;
            Dictionary<NS, string> formattedNameStrings = new Dictionary<NS, string>();
            DateTime? created = null;
            DateTime? modified = null;
            bool loop = true;
            while (loop)
            {
                Console.Write("MCAsset2Font $ ");
                string cmdString = Console.ReadLine() ?? "run";
                string[] cmd = cmdString.Split(' ');
                switch (cmd[0])
                {
                    case "run":
                        loop = false;
                        break;
                    case "set":
                        switch (cmd[1])
                        {
                            case "verbose":
                                verbose = cmd.Length < 3 || !bool.TryParse(cmd[2], out bool v) || v;
                                break;
                            case "assets":
                                Console.Write("Set assets path: ");
                                assetsPath = ReadLineOrThrow();
                                break;
                            case "namespace":
                                Console.Write("Set namespace path: ");
                                namespacePath = ReadLineOrThrow();
                                break;
                            case "out":
                                Console.Write("Set font output: ");
                                destinationPath = ReadLineOrThrow();
                                break;
                            case "name":
                                Console.Write("Set global name string: ");
                                if (int.TryParse(cmd[2], out int n))
                                {
                                    formattedNameStrings[(NS)n] = ReadLineOrThrow();
                                }
                                else if (idToNameString.ContainsKey(cmd[2]))
                                {
                                    formattedNameStrings[idToNameString[cmd[2]]] = ReadLineOrThrow();
                                }
                                else
                                {
                                    Console.WriteLine($"{cmd[2]} is not a valid name string ID!");
                                }
                                break;
                            case "created":
                                Console.Write("Set created timestamp: ");
                                created = DateTime.Parse(ReadLineOrThrow(), null, System.Globalization.DateTimeStyles.RoundtripKind);
                                break;
                            case "modified":
                                Console.Write("Set modified timestamp: ");
                                modified = DateTime.Parse(ReadLineOrThrow(), null, System.Globalization.DateTimeStyles.RoundtripKind);
                                break;
                        }
                        break;
                    default:
                        Console.WriteLine("Not a recognized command!");
                        break;
                }
                if (!loop)
                {
                    loop = true;
                    if (assetsPath is null)
                    {
                        Console.WriteLine("Set an assets path before starting!");
                    }
                    else if (destinationPath is null)
                    {
                        Console.WriteLine("Set a destination path before starting!");
                    }
                    else
                    {
                        loop = false;
                    }
                }
            }
            Debug("[Main] Listing fonts");
            string[] fontPaths;
            if (namespacePath is null) fontPaths = Directory.GetFiles($"{assetsPath!}/minecraft/font");
            else if (Directory.Exists(namespacePath)) fontPaths = Directory.GetFiles($"{namespacePath}/font");
            else if (Directory.Exists($"{assetsPath!}/{namespacePath}")) fontPaths = Directory.GetFiles($"{assetsPath!}/{namespacePath}/font");
            else if (File.Exists($"{namespacePath}.json")) fontPaths = new string[] { $"{namespacePath}.json" };
            else if (File.Exists($"{assetsPath!}/{namespacePath}.json")) fontPaths = new string[] { $"{assetsPath!}/{namespacePath}.json" };
            else if (File.Exists($"{assetsPath!}/minecraft/font/{namespacePath}.json")) fontPaths = new string[] { $"{assetsPath!}/minecraft/font/{namespacePath}.json" };
            else throw new Exception($"namespace \"{namespacePath}\" is neither file nor folder!");
            Fontify fontify = new Fontify(assetsPath!, verbose);
            for (int i = 0; i < fontPaths.Length; i++)
            {
                string filename = fontPaths[i][(fontPaths[i].LastIndexOfAny(new char[] { '/', '\\' }) + 1)..];
                Debug($"[Main] Generate {filename}");
                OTF[] fonts =
                {
                    fontify.MakeFont(fontPaths[i], false, false), // Regular
                    fontify.MakeFont(fontPaths[i], true, false), // Bold
                    fontify.MakeFont(fontPaths[i], false, true), // Italic
                    fontify.MakeFont(fontPaths[i], true, true) // Bold Italic
                };
                Console.Write($"Set family name for {filename}: ");
                string family = Console.ReadLine() ?? filename[..filename.LastIndexOf('.')];
                for (int j = 0; j < fonts.Length; j++)
                {
                    bool bold = fonts[j].bold;
                    bool italic = fonts[j].italic;
                    string subfamily = bold ? (italic ? "Bold Italic" : "Bold") : (italic ? "Italic" : "Regular");
                    Debug($"[Main] Configure {filename}:{subfamily}");
                    foreach (KeyValuePair<NS, string> kvp in formattedNameStrings)
                    {
                        fonts[j].SetString(kvp.Key, kvp.Value.Replace("<family>", family).Replace("<subfamily>", subfamily));
                    }
                    fonts[j].TryAddString(NS.FAMILY, family);
                    fonts[j].TryAddString(NS.SUBFAMILY, subfamily);
                    fonts[j].TryAddString(NS.IDENTIFIER, $"{family} {subfamily} ({(modified ?? DateTime.UtcNow).Year})");
                    fonts[j].TryAddString(NS.FULL, $"{family} {subfamily}");
                    fonts[j].TryAddString(NS.VERSION, "Version 1.0");
                    fonts[j].TryAddString(NS.POSTSCRIPT, $"{family}-{subfamily}");
                    fonts[j].TryAddString(NS.SAMPLE_TEXT, "Minecrafty");
                    fonts[j].SetCreated(created ?? DateTime.UtcNow);
                    fonts[j].SetModified(modified ?? DateTime.UtcNow);
                    if (italic)
                    {
                        fonts[j].HHea.caretSlopeRise = 4;
                        fonts[j].HHea.caretSlopeRun = 1;
                        fonts[j].HHea.caretOffset = 0;
                        fonts[j].Post.italicAngle = -14.036243467926; // (deg) atan(-1/4)
                    }
                    Debug($"[Main] Serialize {filename}:{subfamily}");
                    File.WriteAllBytes($"{destinationPath}/{family}-{subfamily}.otf", fonts[j].Serialize().ToArray());
                }
            }
        }
        private static string ReadLineOrThrow()
        {
            return Console.ReadLine() ?? throw new Exception("Stream ended");
        }
        public static void Debug(string message)
        {
            if (verbose)
            {
                Console.WriteLine(message);
            }
        }
    }
}