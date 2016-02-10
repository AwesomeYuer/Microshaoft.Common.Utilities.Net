//===========================================================================================
//===========================================================================================
// Server.cs
// csc.exe Server.cs /r:Share.dll
namespace Microsoft.Boc
{
    using Microsoft.Boc.Communication.Configurations;
    using Microsoft.Boc.Share;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Text;
    public class SocketServer
    {
        BufferManager _bufferManager;
        SocketAsyncEventArgsPool _socketAsyncEventArgsPool;

        public SocketAsyncEventArgsPool WorkingSocketAsyncEventArgsPool
        {
            get { return _socketAsyncEventArgsPool; }
        }

        public SocketServer()
        {
            if 
                (
                    ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .UseReceiveSocketAsyncEventArgsBufferManager
                )
            {
                //int i = 65536 * 65536;
                _bufferManager = new BufferManager
                                            (
                                                ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .ReceiveSocketBufferManagerBuffersBlocksCount
                                                * ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .ReceiveSocketBufferManagerBufferBlockSizeInKB
                                                * (1024)
                                                ,
                                                ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .ReceiveSocketBufferManagerBufferBlockSizeInKB
                                                * (1024)
                                            );
                _bufferManager.InitBuffer();

            }
            _socketAsyncEventArgsPool = new SocketAsyncEventArgsPool
                                            (
                                                ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .ReceiveSocketBufferManagerBuffersBlocksCount
                                            );
        }

        public void StartUdpListening()
        {

            IPAddress ipAddress;
            IPAddress.TryParse
                            (
                                ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .UdpServerBindIP
                                , out ipAddress
                            );
            IPEndPoint localEndPoint = new IPEndPoint
                                                (
                                                    ipAddress
                                                    , ConfigurationAppSettingsManager
                                                        .RunTimeAppSettings
                                                        .UdpServerBindPort
                                                );
            Socket udpListener = new Socket
                                    (
                                        AddressFamily.InterNetwork
                                        , SocketType.Dgram
                                        , ProtocolType.Udp
                                    );
            udpListener.Bind(localEndPoint);
            var id = Interlocked.Increment(ref _i);
            var udpListenerHandler = new SocketAsyncDataHandler<SessionContextEntry>(udpListener, id);
            var remoteAnyIPEP = new IPEndPoint(IPAddress.Any, 0);
            var receiveSocketAsyncEventArgs = _socketAsyncEventArgsPool.Pop();

            if 
                (
                    ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .UseReceiveSocketAsyncEventArgsBufferManager
                )
            {
                _bufferManager.SetBuffer(receiveSocketAsyncEventArgs);
            }
            else
            { 
                var buffer = new byte
                                [
                                    (1024)
                                    *
                                    ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .ReceiveSocketBufferManagerBufferBlockSizeInKB
                                ];
                receiveSocketAsyncEventArgs
                    .SetBuffer
                        (
                            buffer
                            , 0
                            , buffer.Length
                        );

            }

            udpListenerHandler
                .StartReceiveDataFrom
                    (
                        remoteAnyIPEP
                        , receiveSocketAsyncEventArgs
                        , (x, y, z, w) =>
                        {
                            //if (ConfigurationAppSettingsManager.AppSettings.EnableDebugConsoleOutput)
                            //{
                            //    var s = Encoding.UTF8.GetString(z);
                            //    Console.WriteLine
                            //            (
                            //                "udp direct received from {0}, data {1}"
                            //                , y.ToString()
                            //                , s
                            //            );
                            //}
                            var tuple =
                                Tuple.Create
                                        <
                                            SocketAsyncDataHandler<SessionContextEntry>
                                            , EndPoint
                                            , byte[]
                                        >
                                        (
                                            x
                                            , w.RemoteEndPoint
                                            , z
                                        );
                            if 
                                (
                                    ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .EnableSocketReceivedAsyncQueueProcess
                                )
                            {
                                SessionsManager.ReceivedQueue.Enqueue(tuple);
                            }
                            else
                            {
                                SessionsManager.DataPackReceivedProcess(tuple);
                            }
                            return false;
                        }
                        , (x, y, z) =>
                            {
                                Console.WriteLine(z.ToString());
                                EventLogHelper
                                    .WriteEventLogEntry
                                        (
                                            ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EventLogSourceName
                                            , z.ToString()
                                            , EventLogEntryType.Error
                                            , ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EventLogDefaultErrorExceptionEventID
                                            , ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EventLogDefaultErrorExceptionCategory
                                        );
                                return false;
                            }
                    );
        }
        private void AcceptSocketAsyc(Socket listener)
        {
            var acceptSocketAsyncEventArgs = new SocketAsyncEventArgs();
            acceptSocketAsyncEventArgs.Completed += accept_OnCompleted;
            listener.AcceptAsync(acceptSocketAsyncEventArgs);
        }
        private int _i = 0;
        void accept_OnCompleted(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= accept_OnCompleted;
            var listener = sender as Socket;
            AcceptSocketAsyc(listener);
            Socket client = e.AcceptSocket;
            client.ReceiveTimeout = 1000 * 5;

            var id = Interlocked.Increment(ref _i);
            SocketAsyncEventArgs receiveSocketAsyncEventArgs = _socketAsyncEventArgsPool.Pop();
            _bufferManager.SetBuffer(receiveSocketAsyncEventArgs);
            receiveSocketAsyncEventArgs.SetBuffer
                                (
                                    //new byte[64 * 1024]
                                    0
                                    , 4
                                );
            //_bufferManager.SetBuffer(receiveSocketAsyncEventArgs);

            var handler = new SocketAsyncDataHandler<SessionContextEntry>(client, id);
            handler.StartReceiveWholeDataPackets
                            (
                                //64 * 1024
                                4
                                , 2
                                , 2
                                , receiveSocketAsyncEventArgs
                                , (x, y, z) =>
                                {

                                    var tuple =
                                                Tuple.Create
                                                        <
                                                            //int
                                                            SocketAsyncDataHandler<SessionContextEntry>
                                                            , EndPoint
                                                            , byte[]
                                                       >
                                                       (
                                                            x
                                                            , z.RemoteEndPoint
                                                            , y
                                                       );
                                    if 
                                        (
                                            ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EnableSocketReceivedAsyncQueueProcess
                                        )
                                    {
                                        SessionsManager
                                            .ReceivedQueue
                                            .Enqueue(tuple);
                                    }
                                    else
                                    {
                                        SessionsManager
                                            .DataPackReceivedProcess(tuple);
                                    }
                                    //x.ConnectionToken.LastUpdateTime = DateTime.Now;
                                    return true;
                                }
                            );
        }
    }
}

