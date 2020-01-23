namespace Microshaoft
{
    using System;
    public static class BytesHelper
    {
        public static string BytesArrayToHexString(byte[] data)
        {
            return BitConverter.ToString(data).Replace("-", "");
        }
        public static byte[] HexStringToBytesArray(string text)
        {
            text = text.Replace(" ", "");
            int l = text.Length;
            byte[] buffer = new byte[l / 2];
            for (int i = 0; i < l; i += 2)
            {
                buffer[i / 2] = Convert.ToByte(text.Substring(i, 2), 16);
            }
            return buffer;
        }
        public static byte[] GetLengthHeaderBytes(byte[] bodyBytes, int headerBytesLength = 4)
        {
            byte[] headerBytes = new byte[headerBytesLength];
            var l = bodyBytes.Length;
            var lengthBytes = BitConverter.GetBytes(l);
            Buffer.BlockCopy
                        (
                            lengthBytes
                            , 0
                            , headerBytes
                            , 0
                            , headerBytes.Length
                        );
            return headerBytes;
        }
    }
}
