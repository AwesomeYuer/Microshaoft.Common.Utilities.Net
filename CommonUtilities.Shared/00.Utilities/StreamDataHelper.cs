namespace Microshaoft
{
    using System.IO;
    using System;
    using System.Collections.Generic;
    public static class StreamDataHelper
    {

        public static IEnumerable<byte[]> ReadDataToBuffers(Stream stream, int bufferSize)
        {
            int l = bufferSize;
            int r = 1;
            while (r > 0)
            {
                var buffer = new byte[l];
                r = stream
                            .Read
                                (
                                    buffer
                                    , 0
                                    , buffer.Length
                                );
                if (r > 0)
                {
                    if (r != buffer.Length)
                    {
                        Array.Resize(ref buffer, r);
                    }
                    yield return buffer;
                }
            }
        }


        public static byte[] ReadDataToFixedLengthBytes
                            (
                                Stream stream,
                                int length
                            )
        {
            int p = 0;
            byte[] data = new byte[length];
            while (p < length)
            {
                int r = stream.Read
                                    (
                                        data
                                        , p
                                        , length - p
                                    );
                p += r;
            }
            return data;
        }
        public static byte[] ReadDataToBytes(Stream stream)
        {
            byte[] buffer = new byte[64 * 1024];
            MemoryStream ms = new MemoryStream();
            int r = 0;
            int l = 0;
            long position = -1;
            if (stream.CanSeek)
            {
                position = stream.Position;
                stream.Position = 0;
            }
            while (true)
            {
                r = stream.Read(buffer, 0, buffer.Length);
                if (r > 0)
                {
                    l += r;
                    ms.Write(buffer, 0, r);
                }
                else
                {
                    break;
                }
            }
            byte[] bytes = new byte[l];
            ms.Position = 0;
            ms.Read(bytes, 0, (int)l);

            ms.Close();
            ms.Dispose();
            ms = null;
            if (position >= 0)
            {
                stream.Position = position;
            }
            return bytes;
        }
    }
}
