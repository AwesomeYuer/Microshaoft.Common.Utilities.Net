//Client.cs
namespace Client
{
    using Microshaoft;
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Text;
#if NETCOREAPP2_X
    using System.Runtime.InteropServices;
#endif
    class Class1
    {
        static void Main(string[] args)
        {
#if NETCOREAPP2_X
            Console.WriteLine(RuntimeInformation.OSArchitecture.ToString());
            Console.WriteLine(RuntimeInformation.OSDescription);
            Console.WriteLine(RuntimeInformation.FrameworkDescription);
#endif

            Console.Title = "Client";
            int port = 18180;
            Console.WriteLine("Please input port to connect ...");
            if
                (
                    int.TryParse
                        (
                            Console.ReadLine()
                            , out int p
                        )
                )
            {
                port = p;
            }
            Console.WriteLine($"Connect Port: {port}");
            IPAddress ipa;
            IPAddress.TryParse("127.0.0.1", out ipa);
            var socket = new Socket
                                (
                                    AddressFamily.InterNetwork
                                    , SocketType.Stream
                                    , ProtocolType.Tcp
                                );
            var ipep = new IPEndPoint(ipa, port);
            socket.Connect(ipep);

            //Console.ReadLine();
            var handler = new SocketAsyncDataHandler<string>
                                                        (
                                                            socket
                                                            , 1
                                                        );
            var sendEncoding = Encoding.UTF8;
            var receiveEncoding = Encoding.UTF8;

            handler
                .StartReceiveWholeDataPackets
                    (
                                    
                        4
                        , 0
                        , 4
                        , () =>
                        {
                            var saea = new SocketAsyncEventArgs();
                            saea.SetBuffer
                                    (
                                        new byte[64 * 1024]
                                        , 0
                                        , 64 * 1024
                                    );
                            return saea;
                        }
                        , (x, y, z, w) =>
                        {
                            var offset = 4;
                            var l = y.Length - z;
#if NETCOREAPP2_X
                            var buffer = new ReadOnlySpan<byte>(y, offset, l);
#else
                            var buffer = new byte[l];
                            Buffer
                                .BlockCopy
                                    (
                                        y
                                        , offset
                                        , buffer
                                        , 0
                                        , l
                                    );
#endif

                            var s = receiveEncoding.GetString(buffer);
                            //Console.WriteLine("SocketID: {1}{0}Length: {2}{0}Data: {2}", "\r\n", x.SocketID, y.Length ,s);
                            Console.Write(s);
                            return true;
                        }
                    );
            string input = string.Empty;
            while ((input = Console.ReadLine()) != "q")
            {
                try
                {
                    var buffer = sendEncoding.GetBytes(input);
                    var l = buffer.Length;
                    byte[] intBytes = BytesHelper.GetLengthHeaderBytes(buffer);
                    handler.SendDataSync(intBytes);
                    handler.SendDataSync(buffer);
                    Console.WriteLine("Sended");
                }
                catch (Exception e)
                {
                    Console.WriteLine(e.ToString());
                }
            }
        }
    }
}