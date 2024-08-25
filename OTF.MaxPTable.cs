namespace MCAsset2Font
{
    public partial class OTF
    {
        public class MaxPTable : Table
        {
            public override Tag Tag => "maxp";
            public override int Index => 2;
            public readonly Fixed16_16 version = 1.0;
            public ushort numGlyphs; //
            public ushort maxPoints; //
            public ushort maxContours; // See OTF.UpdateGlyphData()
            public readonly ushort maxCompositePoints = 0;
            public readonly ushort maxCompositeContours = 0;
            public readonly ushort maxZones = 1;
            public readonly ushort maxTwilightPoints = 0;
            public readonly ushort maxStorage = 0;
            public readonly ushort maxFunctionDefs = 0;
            public readonly ushort maxInstructionDefs = 0;
            public readonly ushort maxStackElements = 0;
            public readonly ushort maxSizeOfInstructions = 0;
            public readonly ushort maxComponentElements = 0;
            public readonly ushort maxComponentDepth = 0;
            public override List<byte> Serialize()
            {
                List<byte> buffer = new List<byte>();
                buffer.AddRange(version.Serialize16F16());
                buffer.AddRange(numGlyphs.SerializeU16());
                buffer.AddRange(maxPoints.SerializeU16());
                buffer.AddRange(maxContours.SerializeU16());
                buffer.AddRange(maxCompositePoints.SerializeU16());
                buffer.AddRange(maxCompositeContours.SerializeU16());
                buffer.AddRange(maxZones.SerializeU16());
                buffer.AddRange(maxTwilightPoints.SerializeU16());
                buffer.AddRange(maxStorage.SerializeU16());
                buffer.AddRange(maxFunctionDefs.SerializeU16());
                buffer.AddRange(maxInstructionDefs.SerializeU16());
                buffer.AddRange(maxStackElements.SerializeU16());
                buffer.AddRange(maxSizeOfInstructions.SerializeU16());
                buffer.AddRange(maxComponentElements.SerializeU16());
                buffer.AddRange(maxComponentDepth.SerializeU16());
                return buffer;
            }
        }
    }
}