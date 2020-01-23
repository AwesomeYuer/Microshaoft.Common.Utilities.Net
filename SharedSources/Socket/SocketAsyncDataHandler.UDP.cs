namespace Microshaoft
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    public partial class SocketAsyncDataHandler<T>
    {
        public bool StartReceiveDataFrom
            (
                EndPoint remoteEndPoint
                , Func<SocketAsyncEventArgs> getReceiveSocketAsyncEventArgsProcessFunc
                , Func
                    <
                        SocketAsyncDataHandler<T>
                        , EndPoint
                        , byte[]
                        , SocketAsyncEventArgs
                        , bool
                    > onDataReceivedProcessFunc
                , Func
                    <
                        SocketAsyncDataHandler<T>
                        , SocketAsyncEventArgs
                        , Exception
                        , Exception
                        , string
                        , bool
                    > onCaughtExceptionProcessFunc = null
            )
        {
            return
                StartReceiveDataFrom
                    (
                        remoteEndPoint
                        , getReceiveSocketAsyncEventArgsProcessFunc()
                        , onDataReceivedProcessFunc
                        , onCaughtExceptionProcessFunc
                    );
        }
        public bool StartReceiveDataFrom
                    (
                        EndPoint remoteEndPoint
                        , SocketAsyncEventArgs receiveSocketAsyncEventArgs
                        , Func
                            <
                                SocketAsyncDataHandler<T>
                                , EndPoint
                                , byte[]
                                , SocketAsyncEventArgs
                                , bool                      //是否继续收
                            > onDataReceivedProcessFunc
                        , Func
                            <
                                SocketAsyncDataHandler<T>
                                , SocketAsyncEventArgs
                                , Exception
                                , Exception
                                , string
                                , bool                      //是否Rethrow
                            > onCaughtExceptionProcessFunc = null
                    )
        {
            if (!_isStartedReceiveData)
            {
                //ReceiveSocketAsyncEventArgs = receiveSocketAsyncEventArgs;
                //ReceiveSocketAsyncEventArgs.RemoteEndPoint = remoteEndPoint;
                _receiveSocketAsyncEventArgsCompletedEventHandlerProcessAction
                    = new EventHandler<SocketAsyncEventArgs>
                                (
                                    (sender, e) =>
                                    {
                                        var socket = sender as Socket;
                                        //2015-11-13 
                                        TryCatchFinallyProcessHelper
                                            .TryProcessCatchFinally
                                                (
                                                    true
                                                    , () =>
                                                    {
                                                        ProcessReceivedData
                                                            (
                                                                socket
                                                                , e
                                                                , onDataReceivedProcessFunc
                                                                , onCaughtExceptionProcessFunc
                                                            );
                                                        SocketReceiveFromAsyncAndSyncCompleteProcess
                                                            (
                                                                receiveSocketAsyncEventArgs
                                                                , onDataReceivedProcessFunc
                                                                , onCaughtExceptionProcessFunc
                                                            );
                                                    }
                                                    , false
                                                    , (x, y, w) =>
                                                    {
                                                        return onCaughtExceptionProcessFunc
                                                                    (
                                                                        this
                                                                        , receiveSocketAsyncEventArgs
                                                                        , x
                                                                        , y
                                                                        , w
                                                                    );
                                                    }
                                                );


                                    }
                                );
                ReceiveSocketAsyncEventArgs
                    .Completed += _receiveSocketAsyncEventArgsCompletedEventHandlerProcessAction;

                SocketReceiveFromAsyncAndSyncCompleteProcess
                    (
                        receiveSocketAsyncEventArgs
                        , onDataReceivedProcessFunc
                        , onCaughtExceptionProcessFunc
                    );
                _isStartedReceiveData = true;
            }
            return _isStartedReceiveData;
        }

        private void SocketReceiveFromAsyncAndSyncCompleteProcess
                        (
                            SocketAsyncEventArgs receiveSocketAsyncEventArgs
                            , Func
                                <
                                    SocketAsyncDataHandler<T>
                                    , EndPoint
                                    , byte[]
                                    , SocketAsyncEventArgs
                                    , bool                      //是否继续收
                                > onDataReceivedProcessFunc
                            , Func
                                <
                                    SocketAsyncDataHandler<T>
                                    , SocketAsyncEventArgs
                                    , Exception
                                    , Exception
                                    , string
                                    , bool                      //是否Rethrow
                                > onCaughtExceptionProcessFunc
                            , int sleepInMilliseconds = 10
                        )
        {
            TryCatchFinallyProcessHelper
                .TryProcessCatchFinally
                    (
                        true
                        , () =>
                        {
                            while (!_socket.ReceiveFromAsync(receiveSocketAsyncEventArgs))
                            {
                                if (receiveSocketAsyncEventArgs.BytesTransferred > 0)
                                {
                                    ProcessReceivedData
                                        (
                                            _socket
                                            , ReceiveSocketAsyncEventArgs
                                            , onDataReceivedProcessFunc
                                            , onCaughtExceptionProcessFunc
                                        );
                                }
                                else
                                {
                                    if (sleepInMilliseconds > 0)
                                    {
                                        Thread.Sleep(sleepInMilliseconds);
                                    }
                                }
                            }
                        }
                        , false
                        , (x, y, w) =>
                        {
                            return onCaughtExceptionProcessFunc
                                        (
                                            this
                                            , receiveSocketAsyncEventArgs
                                            , x
                                            , y
                                            , w
                                        );
                        }
                    );
        }

        private void ProcessReceivedData
                        (
                            Socket sender
                            , SocketAsyncEventArgs e
                            , Func
                                <
                                    SocketAsyncDataHandler<T>
                                    , EndPoint
                                    , byte[]
                                    , SocketAsyncEventArgs
                                    , bool                      //是否继续收
                                > onDataReceivedProcessFunc
                            , Func
                                <
                                    SocketAsyncDataHandler<T>
                                    , SocketAsyncEventArgs
                                    , Exception
                                    , Exception
                                    , string
                                    , bool                      //是否Rethrow
                                > onCaughtExceptionProcessFunc = null
                        )
        {
            Interlocked.Increment(ref _receivedCount);
            var socket = sender as Socket;
            int l = e.BytesTransferred;
            //Console.WriteLine(l);
            if (l > 0)
            {
                byte[] data = new byte[l];
                var buffer = e.Buffer;
                Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
                if (onDataReceivedProcessFunc != null)
                {
                    var fromRemoteIPEndPoint = e.RemoteEndPoint;
                    onDataReceivedProcessFunc
                            (
                                this
                                , fromRemoteIPEndPoint
                                , data
                                , e
                            );
                }
            }
        }
        public int SendDataToSync
                       (
                           byte[] data
                           , EndPoint remoteEndPoint
                           , int sleepMilliseconds = 0
                       )
        {
            var r = -1;
            if (_isUdp)
            {
                if (_socket != null && remoteEndPoint != null)
                {
                    r = _socket.SendTo(data, remoteEndPoint);
                    //Console.WriteLine("_socket.SendTo");
                }
                if (sleepMilliseconds > 0)
                {
                    Thread.Sleep(sleepMilliseconds);
                }
            }
            return r;
        }
        public bool SendDataToAsync
                 (
                    SocketAsyncEventArgs sendSocketAsyncEventArgs
                    , int sleepMilliseconds = 0
                 )
        {
            var r = false;
            if (_isUdp)
            {
                if
                    (
                        _socket != null
                        &&
                        sendSocketAsyncEventArgs.RemoteEndPoint != null
                    )
                {
                    //r = _socket
                    //        .SendToAsync(sendSocketAsyncEventArgs);
                }
                if (sleepMilliseconds > 0)
                {
                    Thread.Sleep(sleepMilliseconds);
                }
            }
            return r;
        }
    }
}

