using System.Globalization;
using System.Text;
using System.Text.Unicode;

namespace MCAsset2Font
{
    public partial class OTF
    {
        public class OS_2Table : Table
        {
            public override Tag Tag => "OS/2";
            public override int Index => 3;
            public readonly ushort version = 5;
            public short xAvgCharWidth; // See UpdateGlyphData()
            public UsWeightClass usWeightClass = UsWeightClass.NORMAL;
            public UsWidthClass usWidthClass = UsWidthClass.NORMAL;
            public ushort fsType = 0; // No restrictions
            public short ySubscriptXSize = 20; //
            public short ySubscriptYSize = 20; //
            public short ySubscriptXOffset = 0; //
            public short ySubscriptYOffset = -4; //
            public short ySuperscriptXSize = 20; //
            public short ySuperscriptYSize = 20; //
            public short ySuperscriptXOffset = 0; //
            public short ySuperscriptYOffset = 8; //
            public short yStrikeoutSize = 4; //
            public short yStrikeoutPosition = 14; // Hardcoded
            public short sFamilyClass = 0;
            public byte[] panose = new byte[10];
            public ulong ulUnicodeRangeUpper; //
            public ulong ulUnicodeRangeLower; // Too complicated
            public Tag achVendID = "    ";
            public FsSelection fsSelection = FsSelection.REGULAR | FsSelection.USE_TYPO_METRICS;
            public ushort usFirstCharIndex; //
            public ushort usLastCharIndex; //
            public short sTypoAscender; //
            public short sTypoDescender; //
            public short sTypoLineGap; //
            public ushort usWinAscent; //
            public ushort usWinDescent; // See UpdateGlyphData()
            public ulong ulCodePageRange; // Too complicated
            public short sxHeight = 20; //
            public short sCapHeight = 28; // Hardcoded
            public ushort usDefaultChar = 0; // Readonly?
            public ushort usBreakChar = 0x0020;
            public ushort usMaxContext = 0;
            public ushort usLowerOpticalPointSize = 0;
            public ushort usUpperOpticalPointSize = 0xFFFF;

            public OS_2Table(bool bold, bool italic)
            {
                this.usWeightClass = bold ? UsWeightClass.BOLD : UsWeightClass.NORMAL;
                this.fsSelection = (!bold && !italic ? FsSelection.REGULAR : 0) |
                    (bold ? FsSelection.BOLD : 0) |
                    (italic ? FsSelection.ITALIC : 0);
            }

            public override List<byte> Serialize()
            {
                List<byte> buffer = new List<byte>();
                buffer.AddRange(version.SerializeU16());
                buffer.AddRange(xAvgCharWidth.SerializeS16());
                buffer.AddRange(usWeightClass.SerializeU16());
                buffer.AddRange(usWidthClass.SerializeU16());
                buffer.AddRange(fsType.SerializeU16());
                buffer.AddRange(ySubscriptXSize.SerializeS16());
                buffer.AddRange(ySubscriptYSize.SerializeS16());
                buffer.AddRange(ySubscriptXOffset.SerializeS16());
                buffer.AddRange(ySubscriptYOffset.SerializeS16());
                buffer.AddRange(ySuperscriptXSize.SerializeS16());
                buffer.AddRange(ySuperscriptYSize.SerializeS16());
                buffer.AddRange(ySuperscriptXOffset.SerializeS16());
                buffer.AddRange(ySuperscriptYOffset.SerializeS16());
                buffer.AddRange(yStrikeoutSize.SerializeS16());
                buffer.AddRange(yStrikeoutPosition.SerializeS16());
                buffer.AddRange(sFamilyClass.SerializeS16());
                buffer.AddRange(panose);
                buffer.AddRange(ulUnicodeRangeUpper.SerializeU64());
                buffer.AddRange(ulUnicodeRangeLower.SerializeU64());
                buffer.AddRange(achVendID.SerializeT32());
                buffer.AddRange(fsSelection.SerializeU16());
                buffer.AddRange(usFirstCharIndex.SerializeU16());
                buffer.AddRange(usLastCharIndex.SerializeU16());
                buffer.AddRange(sTypoAscender.SerializeS16());
                buffer.AddRange(sTypoDescender.SerializeS16());
                buffer.AddRange(sTypoLineGap.SerializeS16());
                buffer.AddRange(usWinAscent.SerializeU16());
                buffer.AddRange(usWinDescent.SerializeU16());
                buffer.AddRange(ulCodePageRange.SerializeU64());
                buffer.AddRange(sxHeight.SerializeS16());
                buffer.AddRange(sCapHeight.SerializeS16());
                buffer.AddRange(usDefaultChar.SerializeU16());
                buffer.AddRange(usBreakChar.SerializeU16());
                buffer.AddRange(usMaxContext.SerializeU16());
                buffer.AddRange(usLowerOpticalPointSize.SerializeU16());
                buffer.AddRange(usUpperOpticalPointSize.SerializeU16());
                return buffer;
            }
            public enum UsWeightClass : ushort
            {
                THIN = 100,
                EXTRALIGHT = 200,
                LIGHT = 300,
                NORMAL = 400,
                MEDIUM = 500,
                SEMIBOLD = 600,
                BOLD = 700,
                EXTRABOLD = 800,
                BLACK = 900
            }
            public enum UsWidthClass : ushort
            {
                ULTRA_CONDENSED = 1,
                EXTRA_CONDENSED = 2,
                CONDENSED = 3,
                SEMI_CONDENSED = 4,
                NORMAL = 5,
                SEMI_EXPANDED = 6,
                EXPANDED = 7,
                EXTRA_EXPANDED = 8,
                ULTRA_EXPANDED = 9
            }
            public enum FsSelection : ushort
            {
                ITALIC = 0x0001,
                UNDERSCORE = 0x0002,
                NEGATIVE = 0x0004,
                OUTLINED = 0x0008,
                STRIKEOUT = 0x0010,
                BOLD = 0x0020,
                REGULAR = 0x0040,
                USE_TYPO_METRICS = 0x0080, // FontVal complains
                WWS = 0x0100,
                OBLIQUE = 0x0200
            }
        }
    }
}