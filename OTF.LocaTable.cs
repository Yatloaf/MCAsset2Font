namespace MCAsset2Font
{
    public partial class OTF
    {
        public class LocaTable : Table
        {
            public override Tag Tag => "loca";
            public override int Index => 12;
            public List<byte> cache = new List<byte>();
            public override List<byte> Serialize()
            {
                return this.cache; // See OTF.UpdateGlyphData()
            }
        }
    }
}