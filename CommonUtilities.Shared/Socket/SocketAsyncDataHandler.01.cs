namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    public partial class SocketAsyncDataHandler<T>
    {
        private Socket _socket;
        public Socket WorkingSocket
        {
            get
            {
                return _socket;
            }
        }
        //public int ReceiveDataBufferLength
        //{
        //	get;
        //	private set;
        //}
        public T Context
        {
            get;
            set;
        }
        public IPAddress RemoteIPAddress
        {
            get
            {
                return ((IPEndPoint)_socket.RemoteEndPoint).Address;
            }
        }
        public IPAddress LocalIPAddress
        {
            get
            {
                return ((IPEndPoint)_socket.LocalEndPoint).Address;
            }
        }
        public int SocketID
        {
            get;
            private set;
        }
        public bool HasNoWorkingSocket
        {
            get
            { 
                return (_socket == null);
            }
        }
        //public SocketAsyncDataHandler()
        //{
            
        //}
        public SocketAsyncDataHandler
                            (
                                Socket socket
                                , int socketID
                            )
        {
            _socket = socket;
            _isUdp = (_socket.ProtocolType == ProtocolType.Udp);
            _sendSocketAsyncEventArgs = new SocketAsyncEventArgs();
            SocketID = socketID;
        }
        private SocketAsyncEventArgs _sendSocketAsyncEventArgs;
        public int HeaderBytesLength
        {
            get;
            private set;
        }
        public int HeaderBytesOffset
        {
            get;
            private set;
        }
        private long _receivedCount = 0;
        public long ReceivedCount
        {
            get
            {
                return _receivedCount;
            }
        }
        public int HeaderBytesCount
        {
            get;
            private set;
        }
        public SocketAsyncEventArgs ReceiveSocketAsyncEventArgs
        {
            get;
            private set;
        }
        private bool _isStartedReceiveData = false;
        private bool _isHeader = true;
        public bool StartReceiveWholeDataPackets
                            (
                                int headerBytesLength
                                , int headerBytesOffset
                                , int headerBytesCount
                                , Func<SocketAsyncEventArgs> getReceiveSocketAsyncEventArgsProcessFunc
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , byte[]
                                        , SocketAsyncEventArgs
                                        , bool
                                    > onOneWholeDataPacketReceivedProcessFunc
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , byte[]
                                        , SocketAsyncEventArgs
                                        , bool
                                    > onReceivedDataPacketErrorProcessFunc = null
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , SocketAsyncEventArgs
                                        , Exception
                                        , bool
                                    > onCaughtExceptionProcessFunc = null
                            )
        {
            return
                StartReceiveWholeDataPackets
                    (
                        headerBytesLength
                        , headerBytesOffset
                        , headerBytesCount
                        , getReceiveSocketAsyncEventArgsProcessFunc()
                        , onOneWholeDataPacketReceivedProcessFunc
                        , onReceivedDataPacketErrorProcessFunc
                        , onCaughtExceptionProcessFunc
                    );
        }

        private EventHandler<SocketAsyncEventArgs> _receiveSocketAsyncEventArgsCompletedEventHandlerProcessAction = null;
        public bool StartReceiveWholeDataPackets
                            (
                                int headerBytesLength
                                , int headerBytesOffset
                                , int headerBytesCount
                                , SocketAsyncEventArgs receiveSocketAsyncEventArgs
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , byte[]
                                        , SocketAsyncEventArgs
                                        , bool
                                    > onOneWholeDataPacketReceivedProcessFunc
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , byte[]
                                        , SocketAsyncEventArgs
                                        , bool
                                    > onReceivedDataPacketErrorProcessFunc = null
                                , Func
                                    <
                                        SocketAsyncDataHandler<T>
                                        , SocketAsyncEventArgs
                                        , Exception
                                        , bool
                                    > onCaughtExceptionProcessFunc = null
                            )
        {
            if (!_isStartedReceiveData)
            {
                HeaderBytesLength = headerBytesLength;
                HeaderBytesOffset = headerBytesOffset;
                HeaderBytesCount = headerBytesCount;
                int bodyLength = 0;
                ReceiveSocketAsyncEventArgs = receiveSocketAsyncEventArgs;

                ReceiveSocketAsyncEventArgs.Completed += new EventHandler<SocketAsyncEventArgs>
                                (
                                    (sender, e) =>
                                    {
                                        var socket = sender as Socket;
                                        if (e.BytesTransferred >= 0)
                                        {
                                            byte[] buffer = e.Buffer;
                                            int r = e.BytesTransferred;
                                            int p = e.Offset;
                                            int l = e.Count;
                                            if (r < l)
                                            {
                                                p += r;
                                                e.SetBuffer(p, l - r);
                                            }
                                            else if (r == l)
                                            {
                                                if (_isHeader)
                                                {
                                                    byte[] data = new byte[headerBytesCount];
                                                    Buffer.BlockCopy
                                                                (
                                                                    buffer
                                                                    , HeaderBytesOffset
                                                                    , data
                                                                    , 0
                                                                    , data.Length
                                                                );
                                                    byte[] intBytes = new byte[4];
                                                    l = (intBytes.Length < HeaderBytesCount ? intBytes.Length : HeaderBytesCount);
                                                    Buffer.BlockCopy
                                                                (
                                                                    data
                                                                    , 0
                                                                    , intBytes
                                                                    , 0
                                                                    , l
                                                                );
                                                    //Array.Reverse(intBytes);
                                                    bodyLength = BitConverter.ToInt32(intBytes, 0);
                                                    p += r;
                                                    e.SetBuffer(p, bodyLength);
                                                    Console.WriteLine(bodyLength);
                                                    _isHeader = false;
                                                }
                                                else
                                                {
                                                    byte[] data = new byte[bodyLength + HeaderBytesLength];
                                                    bodyLength = 0;
                                                    Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
                                                    _isHeader = true;
                                                    e.SetBuffer(0, HeaderBytesLength);
                                                    if (onOneWholeDataPacketReceivedProcessFunc != null)
                                                    {
                                                        onOneWholeDataPacketReceivedProcessFunc
                                                            (
                                                                this
                                                                , data
                                                                , e
                                                            );
                                                    }
                                                }
                                            }
                                            else
                                            {
                                                if (onReceivedDataPacketErrorProcessFunc != null)
                                                {
                                                    byte[] data = new byte[p + r + HeaderBytesLength];
                                                    Buffer.BlockCopy(buffer, 0, data, 0, data.Length);
                                                    bool b = onReceivedDataPacketErrorProcessFunc
                                                                (
                                                                    this
                                                                    , data
                                                                    , e
                                                                );
                                                    if (b)
                                                    {
                                                        bool i = DestoryWorkingSocket();
                                                    }
                                                    else
                                                    {
                                                        _isHeader = true;
                                                        e.SetBuffer(0, HeaderBytesLength);
                                                    }
                                                }
                                            }
                                        }
                                        if (!_isWorkingSocketDestoryed)
                                        {
                                            try
                                            {
                                                socket.ReceiveAsync(e);
                                            }
                                            catch (Exception exception)
                                            {
                                                var r = false;
                                                if (onCaughtExceptionProcessFunc != null)
                                                {
                                                    r = onCaughtExceptionProcessFunc
                                                                (
                                                                    this
                                                                    , e
                                                                    , exception
                                                                );
                                                }
                                                if (r)
                                                {
                                                    DestoryWorkingSocket();
                                                }
                                            }
                                        }
                                    }
                                );
                _socket.ReceiveAsync(ReceiveSocketAsyncEventArgs);
                _isStartedReceiveData = true;
            }
            return _isStartedReceiveData;
        }
        private bool _isUdp = false;
        public bool IsUdp
        {
            get
            {
                return _isUdp;
            }
        }
        private bool _isWorkingSocketDestoryed = false;
        public bool DestoryWorkingSocket()
        {
            //bool r = false;
            try
            {
                ReceiveSocketAsyncEventArgs.Completed -= _receiveSocketAsyncEventArgsCompletedEventHandlerProcessAction;

                if (_socket.Connected)
                {
                    _socket.Disconnect(false);
                }
                _socket.Shutdown(SocketShutdown.Both);
                _socket.Close();
                _socket.Dispose();
                _socket = null;
                _isWorkingSocketDestoryed = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                //r = false;
            }
            return _isWorkingSocketDestoryed;
        }
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
                ReceiveSocketAsyncEventArgs = receiveSocketAsyncEventArgs;
                ReceiveSocketAsyncEventArgs.RemoteEndPoint = remoteEndPoint;
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
            //if (!_isWorkingSocketDestoryed)
            {
                
            }
        
        
        }


        //private static object _sendStaticSyncLockObject = new object();
        private object _sendSyncLockObject = new object();
        public int SendDataSync(byte[] data)
        {
            var length = data.Length;
            var sentOffset = 0;
            //SocketError socketError; 
            //socketError = SocketError.Success;
            if (!_isUdp)
            {
                lock (_sendSyncLockObject)
                {
                    while (length > sentOffset)
                    {
                        try
                        {
                            sentOffset +=
                                            _socket
                                                .Send
                                                    (
                                                        data
                                                        , sentOffset
                                                        , length - sentOffset
                                                        , SocketFlags.None
                                                        //, out socketError
                                                    );
                        }
                        catch //(SocketException se)
                        {
                            //socketError = socketError + se.SocketErrorCode;
                            break;
                        }
                        //catch (Exception e)
                        //{
                        //    break;
                        //}
                    }
                }
            }
            return sentOffset;
        }
//        public bool SendDataToSyncWithPerformaceCounterAndWaitResponseWithRetry<TToken>
//              (
//                  byte[] data
//                  , IPEndPoint remoteIPEndPoint
//                  , bool blockForResponse
//                  , AutoResetEvent waiter
//                  , TToken token
//                  , Func
//                        <
//                            TToken
//                            , Tuple
//                                <
//                                    bool        //是否继续等待
//                                    , bool      //送达结果(是否收到应答)
//                                >
//                        > onSetAutoResetEventProcessFunc
//                  , MultiPerformanceCountersTypeFlags enabledSendedMessagesPerformanceCounters
//                  , out int sendTimes
//                  , out long elapsedMilliseconds
                  
//                  , Stopwatch stopWatch = null
//                  , Func<int, TToken, byte[]> onBeforeSendOnceProcessFunc = null
//                  , Action<int, byte[]> onAfterSendedOnceProcessAction = null
//                  , Func<SocketAsyncDataHandler<T>, Exception, bool> onCaughtExceptionProcessFunc = null
//                  , int sendMaxTimes = 100
//                  , int resendWaitOneIntervalInMillisecondsFactor = 1
//                  , int waitOneIntervalInMilliseconds = 1000
                  
//              )
//        {
            
//            var r = false;
//            elapsedMilliseconds = 0;
//            int tempSendTimes = 0;
//            long tempElapsedMilliseconds = 0;
//            if (stopWatch != null)
//            {
//                if (!stopWatch.IsRunning)
//                {
//                    stopWatch.Start();
//                }
//            }
//            AutoResetEvent autoResetEvent = waiter;
//            int i = 0;
//            var nextWaitOneIntervalInMilliseconds = waitOneIntervalInMilliseconds;
//            TryCatchFinallyProcessHelper
//                .TryProcessCatchFinally
//                    (
//                        true
//                        , () =>
//                            {
//                                var continueWaiting = true;
//                                while
//                                    (
//                                        i < sendMaxTimes
//                                        && continueWaiting
//                                        && !r
//                                    )
//                                {
//                                    #region loop body
//                                    if (onBeforeSendOnceProcessFunc != null)
//                                    {
//                                        data = onBeforeSendOnceProcessFunc(i, token);
//                                    }
//                                    if 
//                                        (
//                                            enabledSendedMessagesPerformanceCounters
//                                            ==
//                                            MultiPerformanceCountersTypeFlags.None
//                                        )
//                                    {
//                                        SendDataToSync
//                                            (
//                                                data
//                                                , remoteIPEndPoint
//                                            );
//                                    }
//                                    else
//                                    {
//                                        SendDataToSyncWithPerformaceCounter//<T>
//                                                (
//                                                    remoteIPEndPoint
//                                                    , data
//                                                    , enabledSendedMessagesPerformanceCounters
//                                                );
//                                    }
//                                    i++;
//                                    if (onAfterSendedOnceProcessAction != null)
//                                    {
//                                        onAfterSendedOnceProcessAction(i, data);
//                                    }
//                                    if (!blockForResponse)
//                                    {
//                                        r = true;
//                                        break;
//                                    }
//                                    Tuple<bool, bool> tuple = null;
//                                    if (autoResetEvent != null)
//                                    {
//                                        tuple = onSetAutoResetEventProcessFunc(token);
//                                        if (tuple.Item1)                 //是否继续等待
//                                        {
//                                            //继续等待
//                                            bool b = autoResetEvent
//                                                        .WaitOne
//                                                            (
//                                                                nextWaitOneIntervalInMilliseconds
//                                                                , true
//                                                            );
//                                            if (b)
//                                            {
//                                                // 有信号
//                                                r = true;
//                                                break;
//                                            }
//                                            nextWaitOneIntervalInMilliseconds
//                                                += 
//                                                    (
//                                                        waitOneIntervalInMilliseconds
//                                                        * resendWaitOneIntervalInMillisecondsFactor
//                                                    );
//                                        }
//                                        else
//                                        {
//                                            //不继续等待
//                                            continueWaiting = false;
//                                            if (!r)
//                                            {
//                                                r = tuple.Item2;       //可能的送达结果=false 或未知 因为不继续等待了
//                                            }
//                                            break;
//                                        }
//                                    }
//                                    #endregion
//                                }
//                            }
//                        , false
//                        , (x, y) =>        //catch
//                            {
//                                var reThrowException = false;
//                                if (onCaughtExceptionProcessFunc != null)
//                                {
//                                    reThrowException = onCaughtExceptionProcessFunc(this, x);
//                                }
//                                return reThrowException;
//                            }
//                        , (x, y) =>     //finally
//                            {
//                                if (blockForResponse)
//                                { 
//                                    if (stopWatch != null)
//                                    {
//                                        if (stopWatch.IsRunning)
//                                        {
//                                            stopWatch.Stop();
//                                        }
//                                        tempElapsedMilliseconds = stopWatch.ElapsedMilliseconds;
//                                    }
//                                    if (autoResetEvent != null)
//                                    {
//                                        if
//                                            (
//                                                !autoResetEvent.SafeWaitHandle.IsClosed
//                                                && !autoResetEvent.SafeWaitHandle.IsInvalid
//                                            )
//                                        { 
//                                            autoResetEvent.Close();
//#if NET45
//                                            autoResetEvent.Dispose();
//#endif
//                                            autoResetEvent = null;
//                                            //autoResetEventWaiter = null;
//                                        }
//                                    }
//                                }
//                                tempSendTimes = i;
//                            }
//                    );
//            sendTimes = tempSendTimes;
//            elapsedMilliseconds = tempElapsedMilliseconds;
//            return r;
//        }
        public int SendDataSyncWithRetry
                (
                    byte[] data
                    , int retry = 3
                    , int sleepInMilliseconds = 0
                )
        {
            //增加就地重试机制
            int r = -1;
            int i = 0;
            int l = data.Length;

            while (i < retry)
            {
                r = -1;
                //lock (_sendSyncLockObject)
                {
                    try
                    {
                        if (_socket != null)
                        {
                            r = SendDataSync(data);
                        }
                    }
                    catch (Exception e)
                    {
                        Console.WriteLine(e.ToString());
                    }
                }
                i++;
                if (r == l || i == retry)
                {
                    break;
                }
                else
                {
                    if (sleepInMilliseconds > 0)
                    {
                        Thread.Sleep(sleepInMilliseconds);
                    }
                }
            }
            return r;
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
