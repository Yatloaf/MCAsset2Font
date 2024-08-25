namespace MCAsset2Font
{
    public partial class OTF
    {
        public class HMtxTable : Table
        {
            public override Tag Tag => "hmtx";
            public override int Index => 4;
            public List<byte> cache = new List<byte>();

            public override List<byte> Serialize()
            {
                return cache; // See OTF.UpdateGlyphData()
            }
        }
    }
}