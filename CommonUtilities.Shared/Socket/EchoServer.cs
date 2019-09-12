namespace Microshaoft
{
    using Microshaoft;
    using System;
    using System.Net;
    using System.Net.Sockets;
    public class EchoServer<T>
    {
        //Socket _socketListener;
        private Action<SocketAsyncDataHandler<T>, byte[], int> _onReceivedDataProcessAction;
        public EchoServer
                    (
                        IPEndPoint localPoint
                        , Action
                            <
                                SocketAsyncDataHandler<T>
                                , byte[]
                                , int
                            >
                            onReceivedDataProcessAction
                    )
        {
            _onReceivedDataProcessAction = onReceivedDataProcessAction;
            var listener = new Socket
                            (
                                localPoint.AddressFamily
                                , SocketType.Stream
                                , ProtocolType.Tcp
                            );
            listener.Bind(localPoint);
            listener.Listen(5);
            AcceptSocketAsync(listener);
        }
        private void AcceptSocketAsync(Socket listener)
        {
            var acceptSocketAsyncEventArgs = new SocketAsyncEventArgs();
            acceptSocketAsyncEventArgs.Completed += acceptSocketAsyncEventArgs_AcceptOnceCompleted;
            var r = listener.AcceptAsync(acceptSocketAsyncEventArgs);
            if (!r)
            {
                if (acceptSocketAsyncEventArgs.BytesTransferred > 0)
                {
                    acceptSocketAsyncEventArgs_AcceptOnceCompleted
                        (
                            acceptSocketAsyncEventArgs
                            , acceptSocketAsyncEventArgs
                        );
                }
            }

        }
        private int _socketID = 0;
        void acceptSocketAsyncEventArgs_AcceptOnceCompleted(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= acceptSocketAsyncEventArgs_AcceptOnceCompleted;
            var client = e.AcceptSocket;
            var listener = sender as Socket;
            AcceptSocketAsync(listener);
            
            var handler = new SocketAsyncDataHandler<T>
                                    (
                                        client
                                        , _socketID++
                                    );

            Console.WriteLine("Accepted Socket:[{0}] @ [{1}]", _socketID, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            handler
                .StartReceiveWholeDataPackets
                    (
                        4           //header bytes length
                        , 0         //header bytes offset
                        , 4         //header bytes count
                        ,() =>
                        {
                            var saea = new SocketAsyncEventArgs();
                            saea
                                .SetBuffer
                                    (
                                        new byte[64*1024]
                                        , 0
                                        , 64 * 1024
                                    );
                            return saea;
                        }
                        , (x, y, z, w) =>
                        {
                            _onReceivedDataProcessAction?.Invoke(x, y, z);
                            return true;
                        }
                    );
        }
    }
}