namespace MCAsset2Font
{
    public static class Serialization
    {
        public static uint Checksum(this IList<byte> instance)
        {
            if (instance.Count % 4 != 0) throw new ArgumentException($"Cannot calculate checksum of IList<byte> with length {instance.Count}");
            uint sum = 0;
            for (int i = 0; i < instance.Count; i += 4)
            {
                sum += (uint)instance[i] << 24 | (uint)instance[i + 1] << 16 | (uint)instance[i + 2] << 8 | (uint)instance[i + 3];
            }
            return sum;
        }
        public static void Pad(this IList<byte> instance)
        {
            while (instance.Count % 4 != 0)
            {
                instance.Add(0);
            }
        }
        public static byte[] SerializeU8(this byte instance)
        {
            return new byte[1] { instance };
        }
        public static byte[] SerializeS8(this sbyte instance)
        {
            return new byte[1] { (byte)instance };
        }
        public static byte[] SerializeU16(this ushort instance)
        {
            return new byte[2] { (byte)(instance >> 8 & 0xFF), (byte)(instance & 0xFF) };
        }
        public static byte[] SerializeU16(this int instance)
        {
            return ((ushort)instance).SerializeU16();
        }
        public static byte[] SerializeU16A(this ushort[] instance)
        {
            byte[] buffer = new byte[instance.Length * 2];
            for (int i = 0; i < instance.Length; ++i)
            {
                buffer[i * 2] = (byte)(instance[i] >> 8 & 0xFF);
                buffer[i * 2 + 1] = (byte)(instance[i] & 0xFF);
            }
            return buffer;
        }
        public static byte[] SerializeS16(this short instance)
        {
            return new byte[2] { (byte)(instance >> 8 & 0xFF), (byte)(instance & 0xFF) };
        }
        public static byte[] SerializeS16(this int instance)
        {
            return ((short)instance).SerializeS16();
        }
        public static byte[] SerializeU32(this uint instance)
        {
            return new byte[4] { (byte)(instance >> 24 & 0xFF), (byte)(instance >> 16 & 0xFF), (byte)(instance >> 8 & 0xFF), (byte)(instance & 0xFF) };
        }
        public static byte[] SerializeU32(this int instance)
        {
            return ((uint)instance).SerializeU32();
        }
        public static byte[] SerializeS32(this int instance)
        {
            return new byte[4] { (byte)(instance >> 24 & 0xFF), (byte)(instance >> 16 & 0xFF), (byte)(instance >> 8 & 0xFF), (byte)(instance & 0xFF) };
        }
        public static byte[] SerializeU64(this ulong instance)
        {
            return new byte[8] { (byte)(instance >> 56 & 0xFF), (byte)(instance >> 48 & 0xFF), (byte)(instance >> 40 & 0xFF), (byte)(instance >> 32 & 0xFF),
                (byte)(instance >> 24 & 0xFF), (byte)(instance >> 16 & 0xFF), (byte)(instance >> 8 & 0xFF), (byte)(instance & 0xFF) };
        }
        public static byte[] SerializeU64(this DateTime instance)
        {
            return ((ulong)instance.Subtract(new DateTime(1904, 1, 1, 0, 0, 0)).TotalSeconds).SerializeU64();
        }
        public static byte[] SerializeU16(this OTF.HeadTable.Flags instance)
        {
            return ((ushort)instance).SerializeU16();
        }
        public static byte[] SerializeU16(this OTF.HeadTable.MacStyle instance)
        {
            return ((ushort)instance).SerializeU16();
        }
        public static byte[] SerializeU16(this OTF.OS_2Table.UsWeightClass instance)
        {
            return ((ushort)instance).SerializeU16();
        }
        public static byte[] SerializeU16(this OTF.OS_2Table.UsWidthClass instance)
        {
            return ((ushort)instance).SerializeU16();
        }
        public static byte[] SerializeU16(this OTF.OS_2Table.FsSelection instance)
        {
            return ((ushort)instance).SerializeU16();
        }
        public static byte[] SerializeU16(this OTF.NameTable.NameString instance)
        {
            return ((ushort)instance).SerializeU16();
        }
        
        public static void InsertU16(this IList<byte> instance, int index, ushort data)
        {
            byte[] serialized = data.SerializeU16();
            instance[index] = serialized[0];
            instance[index + 1] = serialized[1];
        }
        public static void InsertS16(this IList<byte> instance, int index, short data)
        {
            byte[] serialized = data.SerializeS16();
            instance[index] = serialized[0];
            instance[index + 1] = serialized[1];
        }
        public static void InsertU32(this IList<byte> instance, int index, uint data)
        {
            byte[] serialized = data.SerializeU32();
            instance[index] = serialized[0];
            instance[index + 1] = serialized[1];
            instance[index + 2] = serialized[2];
            instance[index + 3] = serialized[3];
        }
        public static void InsertS32(this IList<byte> instance, int index, int data)
        {
            byte[] serialized = data.SerializeS32();
            instance[index] = serialized[0];
            instance[index + 1] = serialized[1];
            instance[index + 2] = serialized[2];
            instance[index + 3] = serialized[3];
        }
    }
}