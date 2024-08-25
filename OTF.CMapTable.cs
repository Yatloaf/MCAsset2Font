namespace MCAsset2Font
{
    public partial class OTF
    {
        public class CMapTable : Table
        {
            public override Tag Tag => "cmap";
            public override int Index => 8;
            public readonly ushort version = 0;
            public ushort numTables = 1;
            public ushort platformID = 0; // Unicode
            public ushort encodingID = 4; // Full repertoire
            public uint subtableOffset = 12;
            public ushort format = 12; // Segmented coverage
            // 2 bytes reserved
            public uint length => (ushort)(numGroups * 12 + 16);
            public uint language = 0;
            public uint numGroups => (uint)groups.Count;
            public List<SequentialMapGroup> groups = new List<SequentialMapGroup>();

            public void UpdateSegments(List<Glyph> glyphs) // sorted!
            {
                this.groups = new List<SequentialMapGroup>();
                if (glyphs.Count < 3)
                {
                    this.groups.Add(new SequentialMapGroup
                    {
                        startCharCode = 0xFFFD,
                        endCharCode = 0xFFFD,
                        startGlyphID = 0
                    });
                    return;
                }
                int i = 1; // [0] is missing glyph
                SequentialMapGroup currentGroup = new SequentialMapGroup
                {
                    startCharCode = (uint)glyphs[i].ch.Value,
                    startGlyphID = (uint)i
                };
                for (i = 2; i < glyphs.Count; i++)
                {

                    if (glyphs[i].ch.Value != glyphs[i - 1].ch.Value + 1)
                    {
                        currentGroup.endCharCode = (uint)glyphs[i - 1].ch.Value;
                        if (currentGroup.startCharCode == 0)
                        {
                            throw new Exception($"startCharCode is 0; i = {i}, glyphs[i] = {glyphs[i]}");
                        }
                        this.groups.Add(currentGroup);
                        if (glyphs[i].ch.Value > 0xFFFD && glyphs[i - 1].ch.Value < 0xFFFD)
                        {
                            this.groups.Add(new SequentialMapGroup
                            {
                                startCharCode = 0xFFFD,
                                endCharCode = 0xFFFD,
                                startGlyphID = 0
                            });
                        }
                        currentGroup = new SequentialMapGroup
                        {
                            startCharCode = (uint)glyphs[i].ch.Value,
                            startGlyphID = (uint)i
                        };
                    }
                }
                currentGroup.endCharCode = (uint)glyphs[i - 1].ch.Value;
                this.groups.Add(currentGroup);
            }
            public override List<byte> Serialize()
            {
                byte[] groupBuffer = new byte[numGroups * 12];
                for (int i = 0; i < numGroups; i++)
                {
                    groupBuffer.InsertU32(i * 12, groups[i].startCharCode);
                    groupBuffer.InsertU32(i * 12 + 4, groups[i].endCharCode);
                    groupBuffer.InsertU32(i * 12 + 8, groups[i].startGlyphID);
                }
                List<byte> buffer = new List<byte>();
                buffer.AddRange(version.SerializeU16());
                buffer.AddRange(numTables.SerializeU16());
                buffer.AddRange(platformID.SerializeU16());
                buffer.AddRange(encodingID.SerializeU16());
                buffer.AddRange(subtableOffset.SerializeU32());
                buffer.AddRange(format.SerializeU16());
                buffer.AddRange(new byte[2]); // Reserved
                buffer.AddRange(length.SerializeU32());
                buffer.AddRange(language.SerializeU32());
                buffer.AddRange(numGroups.SerializeU32());
                buffer.AddRange(groupBuffer);
                return buffer;
            }
            public struct SequentialMapGroup
            {
                public uint endCharCode;
                public uint startCharCode;
                public uint startGlyphID;
                // public ushort idRangeOffsets;
            }
        }
    }
}