namespace Server
{
    using Microshaoft;
    using System;
    using System.Net;
    using System.Text;
#if NETCOREAPP2_X
    using System.Runtime.InteropServices;
#endif
    public class Program
    {
        public static void Main(string[] args)
        {
#if NETCOREAPP2_X
            Console.WriteLine(RuntimeInformation.OSArchitecture.ToString());
            Console.WriteLine(RuntimeInformation.OSDescription);
            Console.WriteLine(RuntimeInformation.FrameworkDescription);
#endif
            Console.Title = "Server";
           
            var receiveEncoding = Encoding.UTF8;
            var sendEncoding = Encoding.UTF8;
            var port = 18180;
            IPAddress ipa;
            IPAddress.TryParse("127.0.0.1", out ipa);
            ipa = IPAddress.Any;
            var es = new EchoServer<string>
                            (
                                new IPEndPoint(ipa, port)
                                , (x, y) =>
                                {
                                    var s = receiveEncoding.GetString(y);
                                    s = string
                                            .Format
                                                (
                                                    "Echo: {0}{1}{0}{2}{0}{3}{0}"
                                                    , "\r\n"
                                                    , s
                                                    , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff")
#if NETCOREAPP2_X
                                                    , RuntimeInformation.OSDescription
#else
                                                    , ""
#endif
                                                );
                                    Console.WriteLine(s);
                                    Console.WriteLine($"Server ReceivedAsyncCount: {x.ReceivedAsyncCount}");
                                    Console.WriteLine($"Server ReceivedSyncCount: {x.ReceivedSyncCount}");
                                    Console.WriteLine($"Server ReceivedCount: {x.ReceivedCount}");

                                    Console.WriteLine($"Server ReceivedHeadersCount: {x.ReceivedHeadersCount}");
                                    Console.WriteLine($"Server ReceivedBodysCount: {x.ReceivedBodysCount}");

                                    Console.WriteLine($"Server ReceivedTotalBytesCount: {x.ReceivedTotalBytesCount} bytes");

                                    var buffer = sendEncoding.GetBytes(s);
                                    byte[] intBytes = BytesHelper.GetLengthHeaderBytes(buffer);
                                    x.SendDataSync(intBytes);
                                    x.SendDataSync(buffer);
                                }
                            );
            Console.WriteLine("Hello World");
            Console.WriteLine(Environment.Version.ToString());
            Console.ReadLine();
        }
    }
}
