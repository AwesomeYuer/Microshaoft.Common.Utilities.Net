namespace Microshaoft
{
    using System;
    using System.IO;
    using System.IO.Compression;
    /// <summary>
    /// 数据压缩
    /// </summary>
    public static class CompressHelper
    {

        public static byte[] GZipCompress(byte[] DATA)
        {
            //Console.WriteLine("GZipCompress");
            MemoryStream ms = new MemoryStream();
            GZipStream stream = new GZipStream(ms, CompressionMode.Compress, true);
            stream.Write(DATA, 0, DATA.Length);

            stream.Close();

            stream.Dispose();
            stream = null;
            byte[] buffer = StreamDataHelper.ReadDataToBytes(ms);

            ms.Close();

            ms.Dispose();
            ms = null;
            return buffer;
        }
        public static byte[] GZipDecompress(byte[] data)
        {
            //Console.WriteLine("GZipDecompress");
            MemoryStream ms = new MemoryStream(data);
            GZipStream stream = new GZipStream(ms, CompressionMode.Decompress);
            byte[] buffer = StreamDataHelper.ReadDataToBytes(stream);

            ms.Close();

            ms.Dispose();
            ms = null;

            stream.Close();

            stream.Dispose();
            stream = null;
            return buffer;
        }
        public static Stream GZipCompress(Stream DATA)
        {
            Console.WriteLine("GZipCompress");
            byte[] buffer = StreamDataHelper.ReadDataToBytes(DATA);
            MemoryStream ms = new MemoryStream();
            GZipStream stream = new GZipStream(ms, CompressionMode.Compress, true);
            stream.Write(buffer, 0, buffer.Length);

            stream.Close();

            stream.Dispose();
            stream = null;
            if (ms.CanSeek)
            {
                ms.Position = 0;
            }
            return ms;
        }
        public static Stream GZipDecompress(Stream data)
        {

            byte[] buffer = StreamDataHelper.ReadDataToBytes(data);
            MemoryStream ms = new MemoryStream(buffer);
            GZipStream stream = new GZipStream(ms, CompressionMode.Decompress);
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            return stream;
        }
        public static byte[] DeflateCompress(byte[] DATA)
        {
            MemoryStream ms = new MemoryStream();
            DeflateStream stream = new DeflateStream(ms, CompressionMode.Compress, true);
            stream.Write(DATA, 0, DATA.Length);

            stream.Close();

            stream.Dispose();
            stream = null;
            byte[] buffer = StreamDataHelper.ReadDataToBytes(ms);

            ms.Close();
            ms.Dispose();
            ms = null;
            return buffer;
        }
        public static byte[] DeflateDecompress(byte[] data)
        {
            MemoryStream ms = new MemoryStream(data);
            DeflateStream stream = new DeflateStream(ms, CompressionMode.Decompress);
            byte[] buffer = StreamDataHelper.ReadDataToBytes(stream);

            ms.Close();
            ms.Dispose();
            ms = null;
            stream.Close();
            stream.Dispose();
            stream = null;
            return buffer;
        }
        public static Stream DeflateCompress(Stream DATA)
        {
            byte[] buffer = StreamDataHelper.ReadDataToBytes(DATA);
            MemoryStream ms = new MemoryStream();
            DeflateStream stream = new DeflateStream(ms, CompressionMode.Compress, true);
            stream.Write(buffer, 0, buffer.Length);

            stream.Close();
            stream.Dispose();
            stream = null;
            if (ms.CanSeek)
            {
                ms.Position = 0;
            }
            return ms;
        }
        public static Stream DeflateDecompress(Stream data)
        {
            byte[] buffer = StreamDataHelper.ReadDataToBytes(data);
            MemoryStream ms = new MemoryStream(buffer);
            DeflateStream stream = new DeflateStream(ms, CompressionMode.Decompress);
            if (stream.CanSeek)
            {
                stream.Position = 0;
            }
            return stream;
        }
    }
}
namespace TestConsoleApplication
{
    using System.Text;
    using System;
    using System.IO;
    using Microshaoft;
    /// <summary>
    /// Class1 的摘要说明。
    /// </summary>
    public class Class1
    {
        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        //[STAThread]
        static void Main111(string[] args)
        {
            //
            // TODO: 在此处添加代码以启动应用程序
            //
            Console.WriteLine("Hello World");
            //Console.WriteLine(Environment.Version.ToString());
            string s = "阿斯个贷哈根室电话个撒谎干大事个贷伽师将阿斯个贷哈根室电话个撒谎干大事个贷伽师将事个贷伽师将事个贷伽师将事个贷伽师将事个贷伽师将事个贷伽师将事个贷伽师将事个贷伽师将";
            byte[] buffer = Encoding.UTF8.GetBytes(s);
            byte[] bytes;
            bytes = CompressHelper.GZipCompress
                                    (
                                        buffer
                                    );
            Console.WriteLine
                        (
                            "{0},GZip: {1}; {2}"
                            , buffer.Length
                            , bytes.Length
                            , s.Length
                        );

            //bytes = CompressHelper.ReadStreamToBytes(ms);
            //string ss = Encoding.UTF8.GetString(bytes);
            Stream ms = new MemoryStream(bytes);
            //ms.Write(bytes, 0, bytes.Length);
            ms.Position = 0;
            ms = CompressHelper.GZipDecompress
                        (
                            ms
                        );
            bytes = StreamDataHelper.ReadDataToBytes(ms);
            string ss = Encoding.UTF8.GetString(bytes);
            Console.WriteLine(ss);
            bytes = CompressHelper.DeflateCompress
                                (
                                    buffer
                                );
            Console.WriteLine
                        (
                            "{0},Deflate: {1}; {2}"
                            , buffer.Length
                            , bytes.Length
                            , s.Length
                        );
            //Console.WriteLine("{0},Deflate: {1}", buffer.Length, bytes.Length);
            ss = Encoding.UTF8.GetString
                                (
                                    (
                                        CompressHelper.DeflateDecompress
                                            (
                                                bytes
                                            )
                                    )
                                );
            Console.WriteLine(ss);
        }
    }
}
