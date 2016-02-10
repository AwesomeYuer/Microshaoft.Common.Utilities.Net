#if NET45
namespace Microsoft.Boc
{
    using Microsoft.Boc.Communication.Configurations;
    using Microsoft.Boc.Share;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Text;
    using System.Threading;
    //using System.Data.ex
    /// <summary>
    /// 队列管理器（处理调用服务器消息）
    /// </summary>
    public static partial class SessionsManager
    {

        public static void Run()
        {

        }

        //兄弟服务器转发消息吞入转发缓存队列
        public static ConcurrentAsyncQueue<ForwardMessage[]> ReceivedForwardMessagesSendQueue //user, bytes
            = new Func<ConcurrentAsyncQueue<ForwardMessage[]>>
            (
                () =>
                {
                    var q = new ConcurrentAsyncQueue<ForwardMessage[]>();
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
                                        .ReceivedForwardMessagesSendQueuePerformanceCountersCategoryInstanceName
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
                                    .ReceivedForwardMessagesSendDequeueThreadsCount
                            );
                    q.OnDequeue +=
                                (
                                    (x) =>
                                    {
                                        SessionsManager
                                            .MessagesForwardFromOtherServersProcess
                                                (
                                                    x
                                                );
                                    }
                                );
                    return q;
                }
            )();

        public static void MessagesForwardFromOtherServersProcess
            (
                ForwardMessage[] ForwardMessages
            )
        {
            foreach (ForwardMessage forwardMessage in ForwardMessages)
            {
                TryCatchFinallyProcessHelper
                    .TryProcessCatchFinally
                        (
                            ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .NeedTryProcess
                            , () =>
                            {
                                JObject messageJObject = forwardMessage.MessageJObject;
                                //var json = jObjectMessage.ToString();
                                //IMessage message = JsonsMessagesProcessorsCacheManager.GetJsonObjectByJson(json);
                                SessionsManager
                                    .ForwardMessageFromOtherServer
                                        (
                                            //message
                                            messageJObject
                                            //, json
                                            , forwardMessage.ForwardBy
                                            , forwardMessage.MessageID.Value
                                            , forwardMessage.ReceiverEntryID.Value
                                        );
                            }
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .NeedReThrowCaughtException
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
        }

        //原始消息吞入转发缓存队列
        public static ConcurrentAsyncQueue<MessageSendEntry> ReceivedOriginalMessagesSendQueue //user, bytes
                = new Func<ConcurrentAsyncQueue<MessageSendEntry>>
          (
              () =>
              {
                  var q = new ConcurrentAsyncQueue<MessageSendEntry>();
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
                                        .ReceivedOriginalMessagesSendQueuePerformanceCountersCategoryInstanceName
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
                                    .ReceivedOriginalMessagesSendDequeueThreadsCount
                          );
                  q.OnDequeue +=
                              (
                                  (x) =>
                                  {
                                      SessionsManager
                                          .ReceivedOriginalMessagesProcess
                                                (
                                                    x
                                                );
                                  }
                              );

                  return q;
              }
          )();

        public static ConcurrentDictionary
                                <
                                    string
                                    , MessageTransmissionTrackerEntry
                                > MessagesTransmissionsTrackers
                            = new ConcurrentDictionary<string, MessageTransmissionTrackerEntry>();

        public static ConcurrentAsyncQueue<MessageTransmissionTrackerEntry> SendQueue //user, bytes
               = new Func<ConcurrentAsyncQueue<MessageTransmissionTrackerEntry>>
                  (
                      () =>
                      {
                          var q = new ConcurrentAsyncQueue<MessageTransmissionTrackerEntry>();
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
                                                .SocketSendQueuePerformanceCountersCategoryInstanceName
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
                                            .SocketSendDequeueThreadsCount
                                  );
                          q.OnDequeue +=
                                      (
                                          (x) =>
                                          {
                                              SendMessageTransmissionOnceWithTrackingProcess(x);
                                          }
                                      );

                          return q;
                      }
                  )();

        public static void SendMessageTransmissionOnceWithTrackingProcess
                                (
                                    MessageTransmissionTrackerEntry messageTransmissionTrackerEntry
                                )
        {
            if (messageTransmissionTrackerEntry.IsResponsed)
            {
                ReleaseMessageTransmissionTracker(messageTransmissionTrackerEntry);
                return;
            }

            var toSessionContextEntry = messageTransmissionTrackerEntry.ReceiverSessionContextEntry;
            var toAppID = toSessionContextEntry.OwnerAppID;
            var toGroupID = toSessionContextEntry.OwnerGroupID;
            var toUserID = toSessionContextEntry.OwnerUserID;

            var toSessionID = toSessionContextEntry.SessionID;
            //2014-09-23 使用 JObject =================================
            var jObject = messageTransmissionTrackerEntry
                                .MessageJObject;

            //2014-10-08 天然就是RenewSessionRemoteIP的

            //2014-10-04 RenewSession 优化增加转发的机会 ==============================================================
#region RenewSession 2014-10-04
            var udpRenewSessionSendOneMessageTransmissionsMaxTimes
                    = ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .UdpRenewSessionSendOneMessageTransmissionsMaxTimes;
            if (udpRenewSessionSendOneMessageTransmissionsMaxTimes > 0)
            {
                int m =
                        (
                            messageTransmissionTrackerEntry.TransmissionTimes
                            %
                            udpRenewSessionSendOneMessageTransmissionsMaxTimes
                        );
                if
                    (
                        messageTransmissionTrackerEntry.TransmissionTimes > 0
                        &&
                        m == 0
                    )
                {
                    Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> toNewSession = null;
                    if
                        (
                            SessionsManager
                                .Sessions
                                .TryGetValue
                                    (
                                        toSessionID
                                        , out  toNewSession
                                    )
                        )
                    {
#region 找到会话且不是本地会话
                        //找到会话且不是本地会话
                        var toNewSessionContextEntry = toNewSession.Item1;
                        if
                            (
                                !toNewSessionContextEntry
                                    .IsLocalSession
                            )
                        {
                            if
                                (
                                    ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .NeedBroadcastToPartnersServers
                                )
                            {
                                Tuple<string, string> tuple = null;
                                if
                                    (
                                        _partnersServersMessagesServiceAddressesDictionary
                                            .TryGetValue
                                                (
                                                    toNewSessionContextEntry
                                                        .SessionHostServer
                                                        .ToLower()
                                                        .Trim()
                                                    , out tuple
                                                )
                                    )
                                {
                                    //2014-08-25 于溪玥
                                    //========================================
                                    var url = tuple.Item2;          //url
                                    ForwardMessage forwardMessage = new ForwardMessage()
                                    {
                                        MessageJObject = jObject
                                        ,
                                        ForwardBy = SessionsHostServerHelper
                                                      .LocalSessionsHostServerMachineName
                                        ,
                                        ForwardToUrl = url
                                        ,
                                        MessageID = messageTransmissionTrackerEntry.MessageID
                                        ,
                                        ReceiverEntryID = messageTransmissionTrackerEntry.ReceiverEntryID
                                        ,
                                        ForwardToServerName = tuple.Item1
                                    };
                                    AsyncQueuesProcessorsManager
                                        .MessagesForwardToOtherServersAsyncQueueProcessor
                                        .InternalQueue
                                        .Enqueue(forwardMessage);
                                    RemoveAndReleaseMessageTransmissionTracker
                                        (
                                            messageTransmissionTrackerEntry
                                        );
                                    return;
                                }
                            }
                        } 
#endregion
                    }
                    else
                    {
                        //没找到回话 Session
                        RemoveAndReleaseMessageTransmissionTracker
                            (
                                messageTransmissionTrackerEntry
                            );
                        return;
                    }
                }
            } 
#endregion
            //===========================================================================================


#region 更新时间戳、发送次数\必须重新付一个新值避免重复 ====================
            //更新时间戳、发送次数
            var messageResponseWaiterPairID = messageTransmissionTrackerEntry.ResponseWaiterPairID;

            JToken jTokenMessageHeader = jObject["H"];
            jTokenMessageHeader["ST"] = DateTime.Now;
            jTokenMessageHeader["SC"] = messageTransmissionTrackerEntry.TransmissionTimes + 1;
            jTokenMessageHeader["I"] = messageTransmissionTrackerEntry.MessageID;

            int requireResponse = 0;
            int i = -1;
            JToken jToken = jTokenMessageHeader["RR"];
            if (JTokenHelper.TryGetNullableValue(jToken, ref i))
            {
                requireResponse = i;
            }
            var json = JsonHelper.Serialize(jObject);
            // ===========================================================
#endregion ====================

            //var topic = messageHeader.Topic.ToLower().Trim();
            var socketAsyncDataHandler = messageTransmissionTrackerEntry
                                                .ReceiverSocketAsyncDataHandler;
            var remoteIPEndPoint = messageTransmissionTrackerEntry
                                                .ReceiverSessionContextEntry
                                                .OwnerRemoteIPEndPoint; //Resceiver
            //必须重新序列化 因为调整了字段值
            var buffer = Encoding.UTF8.GetBytes(json);
            var enabledSendedMessagesPerformanceCounters = MultiPerformanceCountersTypeFlags.None;
            if
                (
                    ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .EnableCountPerformance
                )
            {
                enabledSendedMessagesPerformanceCounters
                                = ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .EnabledSendedMessagesPerformanceCounters;
            }
            //messageTransmissionEntry.FirstTransmissionTime = DateTime.Now;
            if (requireResponse == 0)
            {
#region 不必重试
                var now = DateTime.Now;
                socketAsyncDataHandler
                    .SendDataToSyncWithPerformaceCounter//<SessionContextEntry>
                        (
                            remoteIPEndPoint
                            , buffer
                            , enabledSendedMessagesPerformanceCounters
                        );
                //messageTransmissionTrackerEntry.FirstTransmissionTime = now;
                //messageTransmissionTrackerEntry.LastTransmissionTime = now;
                //messageTransmissionTrackerEntry.TransmissionTimes = 1;
                RemoveAndReleaseMessageTransmissionTracker(messageTransmissionTrackerEntry);
#endregion
            }
            else
            {
#region 需要重试机制
                AutoResetEvent autoResetEvent = null;
                var enabledClientResponsedMessagesPerformanceCounters = MultiPerformanceCountersTypeFlags.None;
                if
                    (
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .EnableCountPerformance
                    )
                {
                    enabledClientResponsedMessagesPerformanceCounters
                            = ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .EnabledClientResponsedMessagesPerformanceCounters;
                }

                Stopwatch stopwatch = null;
                if
                    (
                        messageTransmissionTrackerEntry.TransmissionTimes < 1
                    )
                {
                    stopwatch = EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                                      .CountPerformanceBegin
                                          (
                                              enabledClientResponsedMessagesPerformanceCounters
                                              , ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .MessagesPerformanceCountersCategoryName
                                              , ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .MessagesPerformanceCountersCategoryInstanceClientResponsedMessagesCount
                                          );
                    messageTransmissionTrackerEntry.Stopwatcher = stopwatch;
                }
                else
                {
                    stopwatch = messageTransmissionTrackerEntry.Stopwatcher;
                }

                var sendTimes = 0;
                var elapsedMilliseconds = 0L;
                //var isResponsed = false;
                if 
                    (
                        messageTransmissionTrackerEntry.TransmissionTimes >= 1
                        &&
                        !messageTransmissionTrackerEntry.IsResponsed
                    )
                {
     
                    int iShouldWaitInMilliseconds = 0;
                    long shouldWaitInMilliseconds
                                = ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .UdpResendWaitOneIntervalInMilliseconds
                                    +
                                    (
                                        (messageTransmissionTrackerEntry.TransmissionTimes - 1)
                                        *
                                        ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .UdpResendWaitOneIntervalInMillisecondsFactor
                                        *
                                        ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .UdpResendWaitOneIntervalInMilliseconds
                                    );
                    elapsedMilliseconds = messageTransmissionTrackerEntry
                                                .Stopwatcher
                                                .ElapsedMilliseconds;
                    if
                        (
                            elapsedMilliseconds
                            <
                            shouldWaitInMilliseconds
                        )
                    {
                        iShouldWaitInMilliseconds = (int)(shouldWaitInMilliseconds - elapsedMilliseconds);
                    }
                    else
                    {
                        iShouldWaitInMilliseconds = ConfigurationAppSettingsManager
                                                        .RunTimeAppSettings
                                                        .UdpSendSleepInMilliseconds;
                    }
                    if (iShouldWaitInMilliseconds > 0)
                    {

                        Thread.Sleep(iShouldWaitInMilliseconds);
                        //Console.WriteLine("{0}, sleep: {1}ms, elapsed {2}ms", messageTransmissionTrackerEntry.TransmissionTimes, shouldWaitInMilliseconds, elapsedMilliseconds);
                    }
                }

                if (!messageTransmissionTrackerEntry.IsResponsed)
                {
#region 尚未送达
                    //2015-07-18
                    //Stopwatch stopwatchForOnce = null;
                    var r = false;

                    r = socketAsyncDataHandler
                                .SendDataToSyncWithPerformaceCounterAndWaitResponseWithRetry<MessageTransmissionTrackerEntry>
#region SendDataToSyncWaitResponseWithRetry 参数
                                        (
                                              buffer
                                              , remoteIPEndPoint
                                              , false
                                              , autoResetEvent
                                              , messageTransmissionTrackerEntry
                                              , messageTransmissionTrackerEntry.OnSetAutoResetEventProcessFunc
                                              , enabledSendedMessagesPerformanceCounters
                                              , out sendTimes
                                              , out elapsedMilliseconds
                                              , stopwatch
                                              , (xx, yy) =>     //before once
                                              {
                                                  if (xx > 0)
                                                  {
                                                      //2014-09-25 几乎走不到该分支除非是阻塞模式队列
                                                      //messageHeader.SendTimeStamp = DateTime.Now;
                                                      //messageHeader.SendCount += 1;
                                                      jTokenMessageHeader["ST"] = DateTime.Now;
                                                      jToken = jTokenMessageHeader["SC"];
                                                      if (jToken != null)
                                                      {
                                                          jTokenMessageHeader["SC"] = jToken.Value<int>() + 1;
                                                      }
                                                      json = JsonHelper.Serialize(jObject);
                                                      buffer = Encoding.UTF8.GetBytes(json);
                                                  }
                                                  //2015-07-18
                                                  //stopwatchForOnce =
                                                  //    EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                                                  //          .CountPerformanceBegin
                                                  //                (
                                                  //                    enabledSendedMessagesPerformanceCounters
                                                  //                    , ConfigurationAppSettingsManager
                                                  //                          .RunTimeAppSettings
                                                  //                          .MessagesPerformanceCountersCategoryName
                                                  //                    , ConfigurationAppSettingsManager
                                                  //                          .RunTimeAppSettings
                                                  //                          .MessagesPerformanceCountersCategoryInstanceServerSendedToClientMessagesCount
                                                  //                );
                                                  return buffer;
                                              }
                                              , (xx, yy) =>     //after once
                                              {
                                                  var now = DateTime.Now;
                                                  if (messageTransmissionTrackerEntry.TransmissionTimes <= 1)
                                                  {
                                                      messageTransmissionTrackerEntry.FirstTransmissionTime = now;
                                                  }
                                                  messageTransmissionTrackerEntry.LastTransmissionTime = now;
                                                  messageTransmissionTrackerEntry.TransmissionTimes += 1;

                                                  if (ConfigurationAppSettingsManager.RunTimeAppSettings.EnableDebugLog)
                                                  {
                                                      var s = Encoding.UTF8.GetString(yy);
                                                      var log = string
                                                                    .Format
                                                                        (
                                                                            "ToRemoteIPEndPoint: {1}{2}{0}SocketID: {1}{3}{0}SendedData: {1}{4}"
                                                                            , "\r\n"
                                                                            , "\r\n\t"
                                                                            , remoteIPEndPoint.ToString()
                                                                            , socketAsyncDataHandler.SocketID
                                                                            , s
                                                                        );
                                                      FileLogHelper.LogToTimeAlignedFile
                                                                  (
                                                                      log
                                                                      , "Sended"
                                                                      , ConfigurationAppSettingsManager
                                                                            .RunTimeAppSettings
                                                                            .LogFileRootDirectoryPath
                                                                      , ConfigurationAppSettingsManager
                                                                            .RunTimeAppSettings
                                                                            .LogFileNameAlignSeconds
                                                                   );
                                                  }
                                                  //2015-07-18
                                                  //EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                                                  //        .CountPerformanceEnd
                                                  //            (
                                                  //                enabledSendedMessagesPerformanceCounters
                                                  //                , ConfigurationAppSettingsManager
                                                  //                      .RunTimeAppSettings
                                                  //                      .MessagesPerformanceCountersCategoryName
                                                  //                , ConfigurationAppSettingsManager
                                                  //                      .RunTimeAppSettings
                                                  //                      .MessagesPerformanceCountersCategoryInstanceServerSendedToClientMessagesCount
                                                  //                , stopwatchForOnce
                                                  //            );
                                              }
                                             , (xx, yy) =>
                                             {
                                                 Console.WriteLine(yy.ToString());
                                                 EventLogHelper
                                                     .WriteEventLogEntry
                                                         (
                                                             ConfigurationAppSettingsManager
                                                                .RunTimeAppSettings
                                                                .EventLogSourceName
                                                             , yy.ToString()
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
                                             , ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .UdpSendOneMessageTransmissionsMaxTimes
                                             , ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .UdpResendWaitOneIntervalInMillisecondsFactor
                                             , ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .UdpResendWaitOneIntervalInMilliseconds
                                        );
#endregion
                    if
                        (
                            !messageTransmissionTrackerEntry.IsResponsed
                        )
                    {
#region 继续送除非超过最大次数
                        sendTimes = messageTransmissionTrackerEntry.TransmissionTimes;
                        if
                            (
                               sendTimes
                               <=
                               ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .UdpSendOneMessageTransmissionsMaxTimes
                            )
                        {
                            SessionsManager
                                .SendQueue
                                .Enqueue
                                    (messageTransmissionTrackerEntry);
                        }
                        else
                        {
                            //最后一次
                            //未送达 不必更新数据库 丢掉
                            Thread
                                .Sleep
                                    (
                                        ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .UdpSendSleepInMilliseconds
                                    );
                            RemoveAndReleaseMessageTransmissionTracker(messageTransmissionTrackerEntry);
                        }
#endregion
                    }
                    else
                    {
                        //已送达
                        ReleaseMessageTransmissionTracker(messageTransmissionTrackerEntry);
                    }
#endregion
                }
                else
                {
                    //已送达
                    ReleaseMessageTransmissionTracker(messageTransmissionTrackerEntry);
                }
#endregion
            }
        }

        private static void RemoveAndReleaseMessageTransmissionTracker
                                (
                                    MessageTransmissionTrackerEntry messageTransmissionTrackerEntry
                                )
        {
            var responseWaiterPairID = messageTransmissionTrackerEntry.ResponseWaiterPairID;
            var sendTimes = messageTransmissionTrackerEntry.TransmissionTimes;
            MessageTransmissionTrackerEntry removedMessageTransmissionTrackerEntry = null;
            if
                (
                    SessionsManager
                        .MessagesTransmissionsTrackers
                        .TryRemove
                            (
                                responseWaiterPairID
                                , out removedMessageTransmissionTrackerEntry
                            )
                )
            {
                messageTransmissionTrackerEntry
                                        .Stopwatcher
                                        .Stop();
                var elapsedMilliseconds = messageTransmissionTrackerEntry
                                            .Stopwatcher
                                            .ElapsedMilliseconds;
                if
                    (
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .EnableDebugConsoleOutput
                    )
                {
                    Console.WriteLine
                            (
                                "非阻塞出列处理模式[{0}]: [{1}],[{2}]次,[{3}]送达否,[{4}]秒"
                                , DateTime.Now
                                , responseWaiterPairID
                                , sendTimes
                                , "Unknown"
                                , elapsedMilliseconds / 1000.0
                            );
                }
                //放弃 拉倒了 不管了
                ReleaseMessageTransmissionTracker(messageTransmissionTrackerEntry);
                removedMessageTransmissionTrackerEntry = null;
            }
        }

        private static void ReleaseMessageTransmissionTracker
                                    (
                                        MessageTransmissionTrackerEntry messageTransmissionTrackerEntry
                                    )
        {
            messageTransmissionTrackerEntry.Stopwatcher = null;
            //2015-01-12 first chance 
            var responseWaiter = messageTransmissionTrackerEntry.ResponseWaiter;
            if (responseWaiter != null)
            {
                responseWaiter.Dispose();
                responseWaiter = null;
            }
            messageTransmissionTrackerEntry = null;
            //return messageTransmissionTrackerEntry;
        }

#region 2014-09-28 支持阻塞模式的备份不许删 保留
        //=====================================================================================================================
        //2014-09-28 支持阻塞模式的备份不许删 保留
        /*
        public static void SendOneMessageWithWaitResponseProcessOLD(MessageTransmissionTrackerEntry messageTransmissionTrackerEntry)
        {
            //var json = messageTransmissionTrackerEntry.MessageDatagram;

            //必须反序列化clone一次
            //var message = JsonsMessagesProcessorsCacheManager.GetJsonObjectByJson(json);
            //var messageHeader = message.Header;
            var toSessionContextEntry = messageTransmissionTrackerEntry.ReceiverSessionContextEntry;
            var toAppID = toSessionContextEntry.OwnerAppID;
            var toGroupID = toSessionContextEntry.OwnerGroupID;
            var toUserID = toSessionContextEntry.OwnerUserID;

#region 更新时间戳、发送次数\必须重新付一个新值避免重复 ====================
            //更新时间戳、发送次数

            //messageHeader.SendTimeStamp = DateTime.Now;
            ////必须重新付一个新值避免重复
            //messageHeader.ID = messageTransmissionTrackerEntry.MessageID; //MessageIDForOneReceiverEntry; //总的MessageID
            //messageHeader.Sequence = messageTransmissionTrackerEntry.Sequence;

            var messageResponseWaiterPairID = messageTransmissionTrackerEntry.ResponseWaiterPairID;
            //json = JsonHelper.Serialize(message);

            //2014-09-23 使用 JObject =================================
            var jObject = messageTransmissionTrackerEntry.JObjectMessage;
            JToken jTokenMessageHeader = jObject["H"];
            jTokenMessageHeader["ST"] = DateTime.Now;
            jTokenMessageHeader["SC"] = messageTransmissionTrackerEntry.TransmissionTimes + 1;
            jTokenMessageHeader["I"] = messageTransmissionTrackerEntry.MessageID;
            //jTokenMessageHeader["SEQ"] = messageTransmissionTrackerEntry.Sequence;

            int requireResponse = 0;
            JToken jToken = jTokenMessageHeader["RR"];
            if (jToken != null)
            {
                requireResponse = jTokenMessageHeader["RR"].Value<int>();
            }

            var json = JsonHelper.Serialize(jObject);
            // ===========================================================
#endregion ====================

            //var topic = messageHeader.Topic.ToLower().Trim();
            var socketAsyncDataHandler = messageTransmissionTrackerEntry.ReceiverSocketAsyncDataHandler;
            var remoteIPEndPoint = messageTransmissionTrackerEntry
                                        .ReceiverSessionContextEntry
                                        .OwnerRemoteIPEndPoint; //Resceiver
            //必须重新序列化 因为调整了字段值

            var buffer = Encoding.UTF8.GetBytes(json);
            var enabledSendedMessagesPerformanceCounters = MultiPerformanceCountersTypeFlags.None;
            if
                (
                    ConfigurationAppSettingsManager
                        .AppSettings
                        .EnableCountPerformance
                )
            {
                enabledSendedMessagesPerformanceCounters
                        = ConfigurationAppSettingsManager
                                .AppSettings
                                .EnabledSendedMessagesPerformanceCounters;
            }
            //messageTransmissionEntry.FirstTransmissionTime = DateTime.Now;
            if (requireResponse == 0)
            {
#region 不必重试
                var now = DateTime.Now;
                socketAsyncDataHandler
                    .SendDataToSyncWithPerformaceCounter<SessionContextEntry>
                        (
                            remoteIPEndPoint
                            , buffer
                            , enabledSendedMessagesPerformanceCounters
                        );
                messageTransmissionTrackerEntry
                                .FirstTransmissionTime = now;
                messageTransmissionTrackerEntry
                                .LastTransmissionTime = now;
                messageTransmissionTrackerEntry
                                .TransmissionTimes = 1;
#endregion
            }
            else
            {
#region 需要重试机制
                AutoResetEvent autoResetEvent = null;
                if
                    (
                        ConfigurationAppSettingsManager
                            .AppSettings
                            .IsSendOneMessageWithBlockWaitResponse
                    )
                {
                    //阻塞模式
                    autoResetEvent = new AutoResetEvent(false);
                    messageTransmissionTrackerEntry.ResponseWaiter = autoResetEvent;
                    messageTransmissionTrackerEntry
                            .OnSetAutoResetEventProcessFunc
                                = new Func<MessageTransmissionTrackerEntry, Tuple<bool, bool>>
                                            (
                                                (x) =>
                                                {
                                                    var rr = Tuple
                                                                .Create<bool, bool>
                                                                    (
                                                                        x.ContinueWaiting
                                                                        , (x.Status == 10)
                                                                    );
                                                    return rr;
                                                }
                                            );
                }

                var enabledClientResponsedMessagesPerformanceCounters = MultiPerformanceCountersTypeFlags.None;
                if
                    (
                        ConfigurationAppSettingsManager
                            .AppSettings
                            .EnableCountPerformance
                    )
                {
                    enabledClientResponsedMessagesPerformanceCounters
                            = ConfigurationAppSettingsManager
                                    .AppSettings
                                    .EnabledClientResponsedMessagesPerformanceCounters;
                }

                Stopwatch stopwatch = null;
                if
                    (
                        messageTransmissionTrackerEntry.TransmissionTimes < 1
                    //阻塞队列模式
                        ||
                        ConfigurationAppSettingsManager
                            .AppSettings
                            .IsSendOneMessageWithBlockWaitResponse
                    )
                {
                    stopwatch = EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                                      .CountPerformanceBegin
                                          (
                                              enabledClientResponsedMessagesPerformanceCounters
                                              , ConfigurationAppSettingsManager
                                                    .AppSettings
                                                    .MessagesPerformanceCountersCategoryName
                                              , ConfigurationAppSettingsManager
                                                    .AppSettings
                                                    .MessagesPerformanceCountersCategoryInstanceClientResponsedMessagesCount
                                          );
                    messageTransmissionTrackerEntry.Stopwatcher = stopwatch;
                }
                else
                {
                    stopwatch = messageTransmissionTrackerEntry.Stopwatcher;
                }

                var sendTimes = 0;
                var elapsedMilliseconds = 0L;
                var isResponsed = false;
                if
                    (
                        messageTransmissionTrackerEntry.TransmissionTimes >= 1
                        &&
                    // 非阻塞队列模式
                        !ConfigurationAppSettingsManager
                            .AppSettings
                            .IsSendOneMessageWithBlockWaitResponse
                    )
                {
                    elapsedMilliseconds = messageTransmissionTrackerEntry
                                                .Stopwatcher
                                                .ElapsedMilliseconds;
                    long shouldWaitInMilliseconds =
                                                    ConfigurationAppSettingsManager
                                                        .AppSettings
                                                        .UdpResendWaitOneIntervalInMilliseconds
                                                    +
                                                    (messageTransmissionTrackerEntry.TransmissionTimes - 1)

                                                    * ConfigurationAppSettingsManager
                                                            .AppSettings
                                                            .UdpResendWaitOneIntervalInMillisecondsFactor
                                                    * ConfigurationAppSettingsManager
                                                            .AppSettings
                                                            .UdpResendWaitOneIntervalInMilliseconds;
                    if
                        (
                            elapsedMilliseconds
                            <=
                            shouldWaitInMilliseconds
                        )
                    {
                        Thread.Sleep
                            (
                                (int)(shouldWaitInMilliseconds - elapsedMilliseconds)
                            );
                    }
                    else
                    {
                        Thread.Sleep
                            (
                                ConfigurationAppSettingsManager
                                    .AppSettings
                                    .UdpSendSleepInMilliseconds
                            );
                    }

                    if
                        (
                            !SessionsManager.MessagesTransmissionsTrackers.ContainsKey
                                (
                                    messageTransmissionTrackerEntry.ResponseWaiterPairID
                                )
                        )
                    {
                        //已送达 有应答
                        //EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                        //                          .CountPerformanceEnd
                        //                              (
                        //                                  enabledClientResponsedMessagesPerformanceCounters
                        //                                  , ConfigurationAppSettingsManager.AppSettings.MessagesPerformanceCountersCategoryName
                        //                                  , ConfigurationAppSettingsManager.AppSettings.MessagesPerformanceCountersCategoryInstanceClientResponsedMessagesCount
                        //                                  , stopwatch
                        //                              );
                        isResponsed = true;
                    }
                    else
                    {
                        //未收到应答

                    }
                }
                Stopwatch stopwatchForOnce = null;
                var r = false;
                if (!isResponsed)
                {
                    r = socketAsyncDataHandler
                                .SendDataToSyncWaitResponseWithRetry
                                              <MessageTransmissionTrackerEntry>
                                  (
                                      buffer
                                      , remoteIPEndPoint
                                      , ConfigurationAppSettingsManager
                                            .AppSettings
                                                .IsSendOneMessageWithBlockWaitResponse
                                      , autoResetEvent
                                      , messageTransmissionTrackerEntry
                                      , messageTransmissionTrackerEntry.OnSetAutoResetEventProcessFunc
                                        //, messageResponseWaitID
                                      , out sendTimes
                                      , out elapsedMilliseconds
                                      , stopwatch
                                      , (xx, yy) =>     //before once
                                      {
                                          if (xx > 0)
                                          {
                                              //2014-09-25 几乎走不到该分支除非是阻塞模式队列
                                              //messageHeader.SendTimeStamp = DateTime.Now;
                                              //messageHeader.SendCount += 1;
                                              jTokenMessageHeader["ST"] = DateTime.Now;
                                              jToken = jTokenMessageHeader["SC"];
                                              if (jToken != null)
                                              {
                                                  jTokenMessageHeader["SC"] = jToken.Value<int>() + 1;
                                              }
                                              json = JsonHelper.Serialize(jObject);
                                              buffer = Encoding.UTF8.GetBytes(json);
                                          }
                                          stopwatchForOnce =
                                              EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                                                    .CountPerformanceBegin
                                                          (
                                                              enabledSendedMessagesPerformanceCounters
                                                              , ConfigurationAppSettingsManager
                                                                    .AppSettings
                                                                        .MessagesPerformanceCountersCategoryName
                                                              , ConfigurationAppSettingsManager
                                                                    .AppSettings
                                                                        .MessagesPerformanceCountersCategoryInstanceServerSendedToClientMessagesCount
                                                          );
                                          return buffer;
                                      }
                                      , (xx, yy) =>     //after once
                                      {
                                          var now = DateTime.Now;
                                          if (messageTransmissionTrackerEntry.TransmissionTimes <= 1)
                                          {
                                              messageTransmissionTrackerEntry.FirstTransmissionTime = now;
                                          }
                                          messageTransmissionTrackerEntry.LastTransmissionTime = now;
                                          messageTransmissionTrackerEntry.TransmissionTimes += 1;

                                          if (ConfigurationAppSettingsManager.AppSettings.EnableDebugLog)
                                          {
                                              var s = Encoding.UTF8.GetString(yy);
                                              var log = string.Format
                                                                  (
                                                                      "ToRemoteIPEndPoint: {1}{2}{0}SocketID: {1}{3}{0}SendedData: {1}{4}"
                                                                      , "\r\n"
                                                                      , "\r\n\t"
                                                                      , remoteIPEndPoint.ToString()
                                                                      , socketAsyncDataHandler.SocketID
                                                                      , s
                                                                  );
                                              FileLogHelper.LogToTimeAlignedFile
                                                          (
                                                              log
                                                              , "Sended"
                                                              , ConfigurationAppSettingsManager.AppSettings.LogFileRootDirectoryPath
                                                              , ConfigurationAppSettingsManager.AppSettings.LogFileNameAlignSeconds
                                                           );
                                          }
                                          EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                                                  .CountPerformanceEnd
                                                      (
                                                          enabledSendedMessagesPerformanceCounters
                                                          , ConfigurationAppSettingsManager
                                                                .AppSettings
                                                                    .MessagesPerformanceCountersCategoryName
                                                          , ConfigurationAppSettingsManager
                                                                .AppSettings
                                                                    .MessagesPerformanceCountersCategoryInstanceServerSendedToClientMessagesCount
                                                          , stopwatchForOnce
                                                      );
                                      }
                                     , (xx, yy) =>
                                     {
                                         Console.WriteLine(yy.ToString());
                                         EventLogHelper
                                             .WriteEventLogEntry
                                                 (
                                                     ConfigurationAppSettingsManager.AppSettings.EventLogSourceName
                                                     , yy.ToString()
                                                     , EventLogEntryType.Error
                                                     , ConfigurationAppSettingsManager.AppSettings.EventLogDefaultErrorExceptionEventID
                                                 );
                                         return false;
                                     }
                                     , ConfigurationAppSettingsManager
                                            .AppSettings
                                                .UdpSendOneOriginalMessageMaxTimes
                                     , ConfigurationAppSettingsManager
                                            .AppSettings
                                                .UdpResendWaitOneIntervalInMillisecondsFactor
                                     , ConfigurationAppSettingsManager
                                            .AppSettings
                                                .UdpResendWaitOneIntervalInMilliseconds
                                  );
                    if
                        (
                            ConfigurationAppSettingsManager
                                .AppSettings
                                .IsSendOneMessageWithBlockWaitResponse
                        )
                    {
                        messageTransmissionTrackerEntry.OnSetAutoResetEventProcessFunc = null;
                        //阻塞模式
                        if (r)
                        {
                            isResponsed = true;
                            //if
                            //    (
                            //        !autoResetEvent.SafeWaitHandle.IsClosed
                            //        && !autoResetEvent.SafeWaitHandle.IsInvalid
                            //    )
                            //{
                            //    autoResetEvent.Close();
                            //    autoResetEvent.Dispose();
                            //    autoResetEvent = null;
                            //}
                        }

                        if (ConfigurationAppSettingsManager.AppSettings.EnableDebugConsoleOutput)
                        {
                            Console.WriteLine
                                    (
                                        "阻塞出列处理队列模式[{0}]: [{1}],[{2}]次,[{3}]送达否,[{4}]秒"
                                        , DateTime.Now
                                        , messageResponseWaiterPairID
                                        , sendTimes
                                        , r
                                        , elapsedMilliseconds / 1000.0
                                    );
                        }
                    }
                }
                if (r && isResponsed)
                {
                    //已送达 有应答
                    if (ConfigurationAppSettingsManager.AppSettings.IsSendOneMessageWithBlockWaitResponse)
                    {
                        EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                            .CountPerformanceEnd
                                (
                                    enabledClientResponsedMessagesPerformanceCounters
                                    , ConfigurationAppSettingsManager
                                        .AppSettings
                                            .MessagesPerformanceCountersCategoryName
                                    , ConfigurationAppSettingsManager
                                        .AppSettings
                                            .MessagesPerformanceCountersCategoryInstanceClientResponsedMessagesCount
                                    , stopwatch
                                );
                    }
                }
                else
                {
                    //非阻塞队列
                    if (!ConfigurationAppSettingsManager.AppSettings.IsSendOneMessageWithBlockWaitResponse)
                    {
                        if
                            (
                                SessionsManager.MessagesTransmissionsTrackers.ContainsKey
                                    (
                                        messageTransmissionTrackerEntry.ResponseWaiterPairID
                                    )
                            )
                        {
                            sendTimes = messageTransmissionTrackerEntry.TransmissionTimes;
                            if
                                (
                                   sendTimes
                                        <=
                                            ConfigurationAppSettingsManager
                                                .AppSettings
                                                    .UdpSendOneOriginalMessageMaxTimes
                                )
                            {
                                //messageTransmissionEntry.TransmissionTimes += 1;
                                //messageTransmissionEntry.LastTransmissionTime = DateTime.Now;
                                //多送一次
                                SessionsManager.SendQueue.Enqueue(messageTransmissionTrackerEntry);
                            }
                            else
                            {
                                //未送达 不必更新数据库 丢掉
                                Thread.Sleep(2 * ConfigurationAppSettingsManager.AppSettings.UdpSendSleepInMilliseconds);
                                MessageTransmissionTrackerEntry removedMessageTransmissionTrackerEntry = null;
                                if
                                    (
                                        SessionsManager
                                            .MessagesTransmissionsTrackers
                                            .TryRemove
                                                (
                                                    messageTransmissionTrackerEntry.ResponseWaiterPairID
                                                    , out removedMessageTransmissionTrackerEntry
                                                )
                                    )
                                {
                                    messageTransmissionTrackerEntry.Stopwatcher.Stop();
                                    elapsedMilliseconds = messageTransmissionTrackerEntry.Stopwatcher.ElapsedMilliseconds;
                                    if (ConfigurationAppSettingsManager.AppSettings.EnableDebugConsoleOutput)
                                    {
                                        Console.WriteLine
                                                (
                                                    "非阻塞出列处理队列模式[{0}]: [{1}],[{2}]次,[{3}]送达否,[{4}]秒"
                                                    , DateTime.Now
                                                    , messageResponseWaiterPairID
                                                    , sendTimes
                                                    , "Unknown last time"
                                                    , elapsedMilliseconds / 1000.0
                                                );
                                    }
                                    messageTransmissionTrackerEntry.Stopwatcher = null;
                                    messageTransmissionTrackerEntry.ResponseWaiter.Dispose();
                                    messageTransmissionTrackerEntry.ResponseWaiter = null;
                                    removedMessageTransmissionTrackerEntry = null;

                                }



                            }
                        }
                    }
                }
                if
                    (
                        ConfigurationAppSettingsManager
                            .AppSettings
                                .IsSendOneMessageWithBlockWaitResponse
                    )
                {
                    //阻塞队列处理模式
                    if
                        (
                            MessagesTransmissionsTrackers.TryRemove
                            (
                                    messageResponseWaiterPairID
                                    , out messageTransmissionTrackerEntry
                            )
                        )
                    {
                        //autoResetEvent.Dispose();
                        //autoResetEvent = null;
                        //PoolManager.AutoResetEventsWaitersPool.Put(autoResetEvent);
                        messageTransmissionTrackerEntry = null;
                    }
                }
#endregion
            }
        }
        */
#endregion
    }
}
#endif