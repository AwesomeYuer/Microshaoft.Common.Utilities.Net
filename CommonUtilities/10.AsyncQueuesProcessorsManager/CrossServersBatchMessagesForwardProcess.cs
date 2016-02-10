#if NET45
namespace Microsoft.Boc
{
    using Microsoft.Boc.Communication.Configurations;
    using Microsoft.Boc.Share;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Linq;
    using System.Diagnostics;
    using System.Threading;
    using Newtonsoft.Json.Linq;
    public static partial class AsyncQueuesProcessorsManager
    {
       
        public static void RunAllAsyncQueuesProcesses()
        {
            //2014-08-25
            //转发消息
            RunMessagesForwardToOtherServersAsyncQueueProcess();
            
            //接收转发消息
            //2015-09-25 废弃改为使用 ConcurrentAsyncQueue
            //RunMessagesForwardFromOtherServersAsyncQueueProcess();
            
            //转发会话
            RunSessionsSyncToOtherServersAsyncQueueProcess();

            RunSessionsSyncFromOtherServersAsyncQueueProcess();

            //会话数据库同步线程        
            RunSessionsSyncToDataBaseAsyncQueueProcess();
                
            //消息数据库同步线程
            RunMessagesSyncToDataBaseAsyncQueueProcess();
        }

        //#region 接收处理队列
        //public static SingleThreadAsyncDequeueProcessor<IEnumerable<ForwardMessage>> MessagesForwardFromOtherServersAsyncQueueProcessor
        //    = new SingleThreadAsyncDequeueProcessor<IEnumerable<ForwardMessage>>();
        //public static void MessagesForwardFromOtherServersProcess
        //            (
        //                IEnumerable<ForwardMessage> ForwardMessages
        //            )
        //{
        //    foreach (ForwardMessage forwardMessage in ForwardMessages)
        //    {
        //        TryCatchFinallyProcessHelper
        //            .TryProcessCatchFinally
        //                (
        //                    true
        //                    , () =>
        //                    {
        //                        JObject jObjectMessage = forwardMessage.JObjectMessage;
        //                        var json = jObjectMessage.ToString();
        //                        IMessage message = JsonsMessagesProcessorsCacheManager.GetJsonObjectByJson(json);
        //                        SessionsManager
        //                            .ForwardMessageFromOtherServer
        //                                (
        //                                    message
        //                                    , jObjectMessage
        //                                    , json
        //                                    , forwardMessage.ForwardBy
        //                                    , forwardMessage.MessageID.Value
        //                                    , forwardMessage.ReceiverEntryID.Value
        //                                );
        //                    }
        //                    , false
        //                    , (x, y) =>
        //                    {
        //                        string s = string.Format
        //                                            (
        //                                                "{1}{0}{2}"
        //                                                , "\r\n"
        //                                                , y
        //                                                , x.ToString()
        //                                            );
        //                        Console.WriteLine(s);
        //                        EventLogHelper
        //                            .WriteEventLogEntry
        //                                (
        //                                    ConfigurationAppSettingsManager
        //                                        .AppSettings
        //                                            .EventLogSourceName
        //                                    , s
        //                                    , EventLogEntryType.Error
        //                                    , ConfigurationAppSettingsManager
        //                                        .AppSettings
        //                                            .EventLogDefaultErrorExceptionEventID
        //                                );
        //                        return false;
        //                    }
        //                );
        //    }
        //}
        //public static void RunMessagesForwardFromOtherServersAsyncQueueProcess()
        //{
        //    MessagesForwardFromOtherServersAsyncQueueProcessor
        //            .StartRunDequeuesThreadProcess
        //                (
        //                    (x, y) =>
        //                    {
        //                        MessagesForwardFromOtherServersProcess(y);
        //                    }
        //                    , ConfigurationAppSettingsManager
        //                        .AppSettings
        //                        .MessagesSyncAsyncFromOtherServersWaitSleepInMilliseconds
        //                    , null
        //                    , 1 * 1000
        //                    , 1000
        //                    , (x, y) =>
        //                        {
        //                            string s = string.Format
        //                                           (
        //                                               "{1}{0}{2}"
        //                                               , "\r\n"
        //                                               , y
        //                                               , x.ToString()
        //                                           );
        //                            Console.WriteLine(s);
        //                            EventLogHelper
        //                                .WriteEventLogEntry
        //                                    (
        //                                        ConfigurationAppSettingsManager
        //                                            .AppSettings
        //                                            .EventLogSourceName
        //                                        , s
        //                                        , EventLogEntryType.Error
        //                                        , ConfigurationAppSettingsManager
        //                                            .AppSettings
        //                                            .EventLogDefaultErrorExceptionEventID
        //                                    );
        //                            return false;
        //                        }
        //                );
        //} 
        //#endregion

        #region 发送通知队列

        public static SingleThreadAsyncDequeueProcessor<ForwardMessage> MessagesForwardToOtherServersAsyncQueueProcessor
            = new SingleThreadAsyncDequeueProcessor<ForwardMessage>();
        public static void RunMessagesForwardToOtherServersAsyncQueueProcess()
        {
            MessagesForwardToOtherServersAsyncQueueProcessor
                    .StartRunDequeuesThreadProcess
                        (
                            null
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .MessagesSyncAsyncToOtherServersWaitSleepInMilliseconds
                            , (x, y) =>
                                {
                                    var list = y.Select
                                                    (
                                                        (xx) =>
                                                        {
                                                            return xx.Item2;
                                                        }
                                                    );
                                    ParallelForwardMessages(x, list);
                                }
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .MessagesSyncAsyncToOtherServersWaitBatchTimeoutInSeconds * 1000
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .MessagesSyncAsyncToOtherServersWaitBatchMaxSizeInEntriesCount
                            , (x, y) =>
                                {
                                    string s = string.Format
                                                    (
                                                        "{1}{0}{2}"
                                                        , "\r\n"
                                                        , y
                                                        , x.ToString()
                                                    );
                                    Console.WriteLine(s);
                                    EventLogHelper
                                        .WriteEventLogEntry
                                            (
                                                ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .EventLogSourceName
                                                , s
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

        private static void ParallelForwardMessages(long x, IEnumerable<ForwardMessage> y)
        {
            #region MyRegion
            if (x > 0)
            {
                var groups = y.ToLookup
                                    <
                                        ForwardMessage
                                        , string                //url
                                    >
                (
                    (xx) =>
                    {
                        return xx.ForwardToUrl;                 //baseUrl 分组
                    }
                );
                var fowardMessagesParallelismDegree =
                                ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .FowardMessagesParallelismDegree;
                if
                    (
                        fowardMessagesParallelismDegree <= 1
                    )
                {
                    foreach (var xx in groups)
                    {
                        PostJObjectWithTryCatch(xx);
                    }
                }
                else
                {
                    try
                    {
                        groups
                            .AsParallel()
                            .WithDegreeOfParallelism
                                (
                                    fowardMessagesParallelismDegree
                                )
                            .ForAll
                                (
                                    (xx) =>
                                    {
                                        PostJObjectWithTryCatch(xx);
                                    }
                                );
                    }
                    catch (AggregateException ae)
                    {
                        var innerExceptions = ae.Flatten().InnerExceptions;
                        foreach (var e in innerExceptions)
                        {
                            string s = string.Format
                                                    (
                                                        "{1}{0}{2}"
                                                        , "\r\n"
                                                        , "caught AggregateException InnerExceptions BroadcastFowardMessagesAsParallel ForwardPostJObjectWithTryCatch"
                                                        , e.ToString()
                                                    );
                            Console.WriteLine(s);
                            EventLogHelper
                                .WriteEventLogEntry
                                    (
                                        ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                                .EventLogSourceName
                                        , s
                                        , EventLogEntryType.Error
                                        , ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                                .EventLogDefaultErrorExceptionEventID
                                        , ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EventLogDefaultErrorExceptionCategory
                                    );
                        }
                    }
                    catch (Exception e)
                    {
                        string s = string.Format
                                            (
                                                "{1}{0}{2}"
                                                , "\r\n"
                                                , "Caught Exception BroadcastFowardMessagesAsParallel ForwardPostJObjectWithTryCatch"
                                                , e.ToString()
                                            );
                        Console.WriteLine(s);
                        EventLogHelper
                            .WriteEventLogEntry
                                (
                                    ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                            .EventLogSourceName
                                    , s
                                    , EventLogEntryType.Error
                                    , ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                            .EventLogDefaultErrorExceptionEventID
                                    , ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .EventLogDefaultErrorExceptionCategory
                                );
                    }
                }
            }
            #endregion
        }

        private static void PostJObjectWithTryCatch
                            (
                                IGrouping<string, ForwardMessage> grouping
                            )
        {
            try
            {
                SessionsManager
                    .PostObjectsAsJson
                        <IEnumerable<ForwardMessage>>
                            (
                                grouping               //data
                                , grouping.Key       //url
                            );
            }
            catch (Exception e)
            {
                string s = string.Format
                                    (
                                        "{1}{0}{2}"
                                        , "\r\n"
                                        , "Forward Messages PostJObjectWithTryCatch to Url: " + grouping.Key
                                        , e.ToString()
                                    );
                Console.WriteLine(s);
                EventLogHelper
                    .WriteEventLogEntry
                        (
                            ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .EventLogSourceName
                            , s
                            , EventLogEntryType.Error
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .EventLogDefaultErrorExceptionEventID
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .EventLogDefaultErrorExceptionCategory
                        );
            }
        } 
        #endregion
    }
}
#endif