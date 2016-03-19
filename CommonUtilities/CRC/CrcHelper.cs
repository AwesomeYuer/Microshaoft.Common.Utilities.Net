namespace Microshaoft
{
    public static class CrcHelper
    {
        private static Crc16 _crc16 = new Crc16();
        private static Crc32 _crc32 = new Crc32();
        public static byte[] ComputeCrc16ChecksumBytes(byte[] bytes)
        {
            return
                _crc16.ComputeChecksumBytes(bytes);
        }
        public static byte[] ComputeCrc32ChecksumBytes(byte[] bytes)
        {
            return
                _crc32.ComputeChecksumBytes(bytes);
        }
        public static ushort ComputeCrc16Checksum(byte[] bytes)
        {
            return
                _crc16.ComputeChecksum(bytes);
        }
        public static uint ComputeCrc32Checksum(byte[] bytes)
        {
            return
                _crc32.ComputeChecksum(bytes);
        }
    }
}
