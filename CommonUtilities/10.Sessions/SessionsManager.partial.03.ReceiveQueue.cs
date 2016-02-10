#if NET45
namespace Microsoft.Boc
{
    using Microsoft.Boc.Communication.Configurations;
    using Microsoft.Boc.Share;
    using System;
    using System.Diagnostics;
    using System.Net;
    using System.Text;
    //using System.Data.ex
    /// <summary>
    /// 队列管理器（处理调用服务器消息）
    /// </summary>
    public static partial class SessionsManager
    {


        public static ConcurrentAsyncQueue
                                <
                                    Tuple
                                        <
                                            SocketAsyncDataHandler<SessionContextEntry>
                                            , EndPoint
                                            , byte[]
                                        >
                                > ReceivedQueue
                = new Func
                        <
                            ConcurrentAsyncQueue
                                <
                                    Tuple
                                        <
                                            SocketAsyncDataHandler<SessionContextEntry>
                                            , EndPoint
                                            , byte[]
                                        >
                                >
                        >
                            (
                                () =>
                                {
                                    ConcurrentAsyncQueue
                                        <
                                            Tuple
                                                <
                                                    SocketAsyncDataHandler<SessionContextEntry>
                                                    , EndPoint
                                                    , byte[]
                                                >
                                        >
                                            q = null;
                                    if 
                                        (
                                            ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EnableSocketReceivedAsyncQueueProcess
                                        )
                                    {
                                        q = new ConcurrentAsyncQueue<Tuple<SocketAsyncDataHandler<SessionContextEntry>, EndPoint, byte[]>>();
                                        if 
                                            (
                                                ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .EnableCountPerformance
                                            )
                                        {
                                            q.AttachPerformanceCounters
                                                        (
                                                            ConfigurationAppSettingsManager
                                                                .RunTimeAppSettings
                                                                .SocketReceivedQueuePerformanceCountersCategoryInstanceName
                                                            , ConfigurationAppSettingsManager
                                                                .RunTimeAppSettings
                                                                .QueuePerformanceCountersCategoryName
                                                            , new QueuePerformanceCountersContainer()
                                                        );
                                        }
                                        q.StartIncreaseDequeueProcessThreads
                                                (
                                                    ConfigurationAppSettingsManager
                                                        .RunTimeAppSettings
                                                        .SocketReceivedDequeueThreadsCount
                                                );
                                        q.OnDequeue +=
                                                    (
                                                        (x) =>
                                                        {
                                                            DataPackReceivedProcess(x);
                                                        }
                                                    );
                                    }
                                    return q;
                                }
                            )();

        public static void DataPackReceivedProcess
                        (
                            Tuple
                                <
                                    SocketAsyncDataHandler<SessionContextEntry>
                                    , EndPoint
                                    , byte[]
                                > data
                        )
        {
            //LTV ltv = new LTV(data);
            //var buffer = ltv.ValueBytes;
            //if (ltv.Tag == 1)
            //{
            //    buffer = CompressHelper.GZipDecompress(buffer);
            //}



            var socketAsyncDataHandler = data.Item1;
            var remoteIPEndPoint = data.Item2;
            var buffer = data.Item3;

            var json = Encoding.UTF8.GetString(buffer);
            //json = s.Trim();
            //var messageHeader = JsonHelper.DeserializeByJTokenPath<MessageHeader>(s, "H");
            var message = JsonsMessagesProcessorsCacheManager.GetJsonObjectByJson(json);
            var messageHeader = message.Header;

            if 
                (
                    ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .EnableDebugLog
                )
            {
                var log = string.Format
                                    (
                                        "FromRemoteIPEndPointIP: {1}{2}{0}SocketID: {1}{3}{0}ReceivedData: {1}{4}"
                                        , "\r\n"
                                        , "\r\n\t"
                                        , remoteIPEndPoint.ToString()
                                        , socketAsyncDataHandler.SocketID
                                        , json
                                    );
                FileLogHelper.LogToTimeAlignedFile
                            (
                                log
                                , "Received"
                                , ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .LogFileRootDirectoryPath
                                , ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .LogFileNameAlignSeconds
                             );
            }
            var tuple = Tuple
                            .Create
                                <
                                    SocketAsyncDataHandler<SessionContextEntry>
                                    , EndPoint
                                    , IMessage
                                    , string
                                >
                                    (socketAsyncDataHandler, remoteIPEndPoint, message, json);
            var key = messageHeader.Topic.ToLower().Trim();
            MultiPerformanceCountersTypeFlags enabledCounters = MultiPerformanceCountersTypeFlags.None;
            if 
                (
                    ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .EnableCountPerformance
                )
            {
                enabledCounters = ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .EnabledReceivedMessagesPerformanceCounters;
            }

            EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                .CountPerformance
                    (
                        enabledCounters
                        , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .MessagesPerformanceCountersCategoryName
                        , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .MessagesPerformanceCountersCategoryInstanceServerReceivedFromClientMessagesCount
                        , null
                        , () =>
                        {
                            MessagesReceiveProcessors.ProcessDataPack
                                            (
                                                key
                                                , tuple
                                            );
                        }
                        , null
                        , (x) =>        //caught exception
                        {
                            Console.WriteLine(x.ToString());
                            EventLogHelper
                                .WriteEventLogEntry
                                    (
                                        ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .EventLogSourceName
                                        , x.ToString()
                                        , EventLogEntryType.Error
                                        , ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .EventLogDefaultErrorExceptionCategory
                                        , ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EventLogDefaultErrorExceptionCategory
                                    );
                            return false;
                        }
                    );
        }
    }
}
#endif