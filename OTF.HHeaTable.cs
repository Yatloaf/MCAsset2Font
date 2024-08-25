namespace MCAsset2Font
{
    public partial class OTF
    {
        public class HHeaTable : Table
        {
            public override Tag Tag => "hhea";
            public override int Index => 1;
            public readonly ushort majorVersion = 1;
            public readonly ushort minorVersion = 0;
            public short ascender = 28; //
            public short descender = -4; //
            public short lineGap = 4; // Hardcoded
            public ushort advanceWidthMax; //
            public short minLeftSideBearing; //
            public short minRightSideBearing; //
            public short xMaxExtent; // See OTF.UpdateGlyphData()
            public short caretSlopeRise = 1;
            public short caretSlopeRun = 0;
            public short caretOffset = 0;
            // 8 bytes reserved
            public readonly short metricDataFormat = 0;
            public ushort numberOfHMetrics; // See OTF.UpdateGlyphData()

            public override List<byte> Serialize()
            {
                List<byte> buffer = new List<byte>();
                buffer.AddRange(majorVersion.SerializeU16());
                buffer.AddRange(minorVersion.SerializeU16());
                buffer.AddRange(ascender.SerializeS16());
                buffer.AddRange(descender.SerializeS16());
                buffer.AddRange(lineGap.SerializeS16());
                buffer.AddRange(advanceWidthMax.SerializeU16());
                buffer.AddRange(minLeftSideBearing.SerializeS16());
                buffer.AddRange(minRightSideBearing.SerializeS16());
                buffer.AddRange(xMaxExtent.SerializeS16());
                buffer.AddRange(caretSlopeRise.SerializeS16());
                buffer.AddRange(caretSlopeRun.SerializeS16());
                buffer.AddRange(caretOffset.SerializeS16());
                buffer.AddRange(new byte[8]);
                buffer.AddRange(metricDataFormat.SerializeS16());
                buffer.AddRange(numberOfHMetrics.SerializeU16());
                return buffer;
            }
        }
    }
}