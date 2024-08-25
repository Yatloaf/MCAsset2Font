namespace MCAsset2Font
{
    public partial class OTF
    {
        public class PostTable : Table
        {
            public override Tag Tag => "post";
            public override int Index => 16;
            public readonly Fixed16_16 version = 3.0; // Hardcoded
            public Fixed16_16 italicAngle = 0;
            public short underlinePosition = -4;
            public short underlineThickness = 4;
            public uint isFixedPitch = 0;
            public uint minMemType42 = 0;
            public uint maxMemType42 = 0;
            public uint minMemType1 = 0;
            public uint maxMemType1 = 0;
            public override List<byte> Serialize()
            {
                List<byte> buffer = new List<byte>();
                buffer.AddRange(version.Serialize16F16());
                buffer.AddRange(italicAngle.Serialize16F16());
                buffer.AddRange(underlinePosition.SerializeS16());
                buffer.AddRange(underlineThickness.SerializeS16());
                buffer.AddRange(isFixedPitch.SerializeU32());
                buffer.AddRange(minMemType42.SerializeU32());
                buffer.AddRange(maxMemType42.SerializeU32());
                buffer.AddRange(minMemType1.SerializeU32());
                buffer.AddRange(maxMemType1.SerializeU32());
                return buffer;
            }
        }
    }
}