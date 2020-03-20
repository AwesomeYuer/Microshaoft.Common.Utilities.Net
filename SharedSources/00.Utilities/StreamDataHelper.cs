namespace Microshaoft
{
    using System.IO;
    using System;
    using System.Collections.Generic;
    public static class StreamDataHelper
    {

        public static IEnumerable<byte[]> ReadDataToBuffers(this Stream @this, int bufferSize)
        {
            int l = bufferSize;
            int r = 1;
            while (r > 0)
            {
                var buffer = new byte[l];
                r = @this
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
                                this Stream @this,
                                int length
                            )
        {
            int p = 0;
            byte[] data = new byte[length];
            while (p < length)
            {
                int r = @this.Read
                                    (
                                        data
                                        , p
                                        , length - p
                                    );
                p += r;
            }
            return data;
        }
        public static byte[] ReadDataToBytes(this Stream @this)
        {
            byte[] buffer = new byte[64 * 1024];
            using MemoryStream ms = new MemoryStream();
            int l = 0;
            long position = -1;
            if (@this.CanSeek)
            {
                position = @this.Position;
                @this.Position = 0;
            }
            while (true)
            {
                int r = @this.Read(buffer, 0, buffer.Length);
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
            //ms.Dispose();
            //ms = null;
            if (position >= 0)
            {
                @this.Position = position;
            }
            return bytes;
        }
    }
}
