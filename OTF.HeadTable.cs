namespace MCAsset2Font
{
    public partial class OTF
    {
        public class HeadTable : Table
        {
            public override Tag Tag => "head";
            public override int Index => 0;
            public readonly ushort majorVersion = 0x0001;
            public readonly ushort minorVersion = 0x0000;
            public Fixed16_16 fontRevision = 1.0;
            public readonly uint checksumAdjustment = 0; // This is set at the very end
            public readonly uint magicNumber = 0x5F0F3CF5;
            public Flags flags = Flags.Baseline0 | Flags.Sidebearing0 | Flags.IntegerMath;
            public ushort unitsPerEm = 32; // Hardcoded
            public DateTime created = DateTime.UtcNow;
            public DateTime modified = DateTime.UtcNow;
            public short xMin; //
            public short yMin; //
            public short xMax; //
            public short yMax; // See UpdateGlyphData()
            public MacStyle macStyle = 0;
            public ushort lowestRecPPEM = 8; // Hardcoded
            public readonly short fontDirectionHint = 2; // Deprecated
            public readonly short indexToLocFormat = 1; // 0 not supported
            public readonly short glyphDataFormat = 0;

            public HeadTable(bool bold, bool italic)
            {
                this.macStyle = (bold ? MacStyle.Bold : 0) | (italic ? MacStyle.Italic : 0);
            }

            public override List<byte> Serialize()
            {
                List<byte> buffer = new List<byte>();
                buffer.AddRange(majorVersion.SerializeU16());
                buffer.AddRange(minorVersion.SerializeU16());
                buffer.AddRange(fontRevision.Serialize16F16());
                buffer.AddRange(checksumAdjustment.SerializeU32());
                buffer.AddRange(magicNumber.SerializeU32());
                buffer.AddRange(flags.SerializeU16());
                buffer.AddRange(unitsPerEm.SerializeU16());
                buffer.AddRange(created.SerializeU64());
                buffer.AddRange(modified.SerializeU64());
                buffer.AddRange(xMin.SerializeS16());
                buffer.AddRange(yMin.SerializeS16());
                buffer.AddRange(xMax.SerializeS16());
                buffer.AddRange(yMax.SerializeS16());
                buffer.AddRange(macStyle.SerializeU16());
                buffer.AddRange(lowestRecPPEM.SerializeU16());
                buffer.AddRange(fontDirectionHint.SerializeS16());
                buffer.AddRange(indexToLocFormat.SerializeS16());
                buffer.AddRange(glyphDataFormat.SerializeS16());
                return buffer;
            }
            public enum Flags : ushort
            {
                Baseline0 = 0x0001,
                Sidebearing0 = 0x0002,
                InstDependPointSize = 0x0004,
                IntegerMath = 0x0008,
                InstAlterAdvanceWidth = 0x0010,
                Lossless = 0x0800,
                Converted = 0x1000,
                ClearTypeOptimized = 0x2000,
                LastResort = 0x4000
            }
            public enum MacStyle : ushort
            {
                Bold = 0x0001,
                Italic = 0x0002,
                Underline = 0x0004,
                Outline = 0x0008,
                Shadow = 0x0010,
                Condensed = 0x0020,
                Extended = 0x0040
            }
        }
    }
}