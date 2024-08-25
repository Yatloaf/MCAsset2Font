using System.Text;

namespace MCAsset2Font
{
    public partial class OTF
    {
        public class NameTable : Table
        {
            public override Tag Tag => "name";
            public override int Index => 15;
            public readonly ushort version = 0x0000; // This could be different, but we don’t need anything else
            public ushort count => (ushort)(nameStrings.Count * 3);
            public ushort storageOffset => (ushort)(6 + 12 * count);
            public SortedDictionary<NameString, string> nameStrings = new SortedDictionary<NameString, string>();

            public override List<byte> Serialize()
            {
                List<byte> nameRecordBufferUni = new List<byte>();
                List<byte> nameRecordBufferMac = new List<byte>();
                List<byte> nameRecordBufferWin = new List<byte>();
                List<byte> nameStringBufferUni = new List<byte>();
                List<byte> nameStringBufferMac = new List<byte>();
                foreach (NameString s in nameStrings.Keys)
                {
                    byte[] uniBytes = Encoding.BigEndianUnicode.GetBytes(nameStrings[s]);

                    nameRecordBufferUni.AddRange(new byte[] { 0x00, 0x00, 0x00, 0x03, 0x00, 0x00 }); // Unicode, BMP, Unspecified
                    nameRecordBufferUni.AddRange(s.SerializeU16());
                    nameRecordBufferUni.AddRange(uniBytes.Length.SerializeU16());
                    nameRecordBufferUni.AddRange(nameStringBufferUni.Count.SerializeU16());

                    nameRecordBufferWin.AddRange(new byte[] { 0x00, 0x03, 0x00, 0x01, 0x04, 0x09 }); // Windows, Unicode BMP, English US
                    nameRecordBufferWin.AddRange(s.SerializeU16());
                    nameRecordBufferWin.AddRange(uniBytes.Length.SerializeU16());
                    nameRecordBufferWin.AddRange(nameStringBufferUni.Count.SerializeU16());

                    nameStringBufferUni.AddRange(uniBytes);
                }
                foreach (NameString s in nameStrings.Keys)
                {
                    byte[] macBytes = Encoding.ASCII.GetBytes(nameStrings[s]);
                    nameRecordBufferMac.AddRange(new byte[] { 0x00, 0x01, 0x00, 0x00, 0x00, 0x00 }); // Mac, Roman, English
                    nameRecordBufferMac.AddRange(s.SerializeU16());
                    nameRecordBufferMac.AddRange(macBytes.Length.SerializeU16());
                    nameRecordBufferMac.AddRange((nameStringBufferMac.Count() + nameStringBufferUni.Count()).SerializeU16());
                    nameStringBufferMac.AddRange(macBytes);
                }
                List<byte> buffer = new List<byte>();
                buffer.AddRange(version.SerializeU16());
                buffer.AddRange(this.count.SerializeU16());
                buffer.AddRange(this.storageOffset.SerializeU16());
                buffer.AddRange(nameRecordBufferUni);
                buffer.AddRange(nameRecordBufferMac);
                buffer.AddRange(nameRecordBufferWin);
                buffer.AddRange(nameStringBufferUni);
                buffer.AddRange(nameStringBufferMac);
                return buffer;
            }
            public enum NameString : ushort
            {
                COPYRIGHT = 0,
                FAMILY = 1,
                SUBFAMILY = 2,
                IDENTIFIER = 3,
                FULL = 4,
                VERSION = 5,
                POSTSCRIPT = 6,
                TRADEMARK = 7,
                MANUFACTURER = 8,
                DESIGNER = 9,
                DESCRIPTION = 10,
                VENDOR_URL = 11,
                DESIGNER_URL = 12,
                LICENSE = 13,
                LICENSE_URL = 14,
                TYPOGRAPHIC_FAMILY = 16,
                TYPOGRAPHIC_SUBFAMILY = 17,
                COMPATIBLE_FULL = 18,
                SAMPLE_TEXT = 19,
                POSTSCRIPT_FINDFONT = 20,
                WWS_FAMILY = 21,
                WWS_SUBFAMILY = 22,
                LIGHT_PALETTE = 23,
                DARK_PALETTE = 24,
                POSTSCRIPT_PREFIX = 25
            }
        }
    }
}