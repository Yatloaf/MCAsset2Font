namespace MCAsset2Font
{
    public partial class OTF
    {
        public class GlyfTable : Table
        {
            public override Tag Tag => "glyf";
            public override int Index => 13;
            public List<byte> cache = new List<byte>();
            public override List<byte> Serialize()
            {
                return this.cache; // See OTF.UpdateGlyphData()
            }
        }
    }
}