#if NET45
namespace Microsoft.Boc
{
    using Communication.Configurations;
    using Share;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Data;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Net.Http;
    /// <summary>
    /// 会话管理器
    /// </summary>
    public static partial class SessionsManager
    {
        private static Dictionary<string, Tuple<string, string>>
                            _partnersServersSessionsServiceAddressesDictionary
                                = new Func<Dictionary<string, Tuple<string, string>>>
                                    (
                                        () =>
                                        {
                                            Dictionary<string, Tuple<string, string>> dictionary =
                                                            new Dictionary<string, Tuple<string, string>>();
                                            var config = ConfigurationManager
                                                            .GetSection
                                                                (
                                                                    "AdditionalConfigSectionGroup/PartnersServersSessionsServiceAddresses"
                                                                ) as IDictionary;
                                            foreach (DictionaryEntry de in config)
                                            {
                                                var key = de.Key.ToString().ToLower().Trim();
                                                if
                                                    (
                                                        string.Compare
                                                                (
                                                                    key
                                                                    , SessionsHostServerHelper
                                                                        .LocalSessionsHostServerMachineName
                                                                    , true
                                                                ) != 0
                                                    )
                                                {
                                                    var x = Tuple.Create<string, string>
                                                                (
                                                                    key
                                                                    , de.Value.ToString()
                                                                );
                                                    dictionary.Add(key, x);
                                                }
                                            }
                                            return dictionary;
                                        }
                                    )();
        //private static Dictionary<string, Tuple<string, string>>
        //            _partnersServersSessionServiceAddressesDictionary
        //                = new Func<Dictionary<string, Tuple<string, string>>>
        //                    (
        //                        () =>
        //                        {
        //                            Dictionary<string, Tuple<string, string>> dictionary =
        //                                            new Dictionary<string, Tuple<string, string>>();
        //                            var config = ConfigurationManager
        //                                            .GetSection
        //                                                (
        //                                                    "AdditionalConfigSectionGroup/PartnersServersSessionServiceAddresses"
        //                                                ) as IDictionary;
        //                            foreach (DictionaryEntry de in config)
        //                            {
        //                                var key = de.Key.ToString().ToLower().Trim();
        //                                if
        //                                    (
        //                                        string.Compare
        //                                                (
        //                                                    key
        //                                                    , SessionsHostServerHelper
        //                                                        .LocalSessionsHostServerMachineName
        //                                                    , true
        //                                                ) != 0
        //                                    )
        //                                {
        //                                    var x = Tuple.Create<string, string>
        //                                                (
        //                                                    key
        //                                                    , de.Value.ToString()
        //                                                );
        //                                    dictionary.Add(key, x);
        //                                }
        //                            }
        //                            return dictionary;
        //                        }
        //                    )();

        private static Dictionary<string, Tuple<string, string>>
                            _partnersServersMessagesServiceAddressesDictionary
                                    = new Func<Dictionary<string, Tuple<string, string>>>
                                      (
                                          () =>
                                          {
                                              Dictionary<string, Tuple<string, string>> dictionary
                                                    = new Dictionary<string, Tuple<string, string>>();
                                              var config = ConfigurationManager
                                                                .GetSection
                                                                    (
                                                                        "AdditionalConfigSectionGroup/PartnersServersMessagesServiceAddresses"
                                                                    ) as IDictionary;
                                              foreach (DictionaryEntry de in config)
                                              {
                                                  var key = de.Key.ToString().ToLower().Trim();
                                                  if
                                                      (
                                                        string.Compare
                                                                    (
                                                                        key
                                                                        , SessionsHostServerHelper
                                                                            .LocalSessionsHostServerMachineName
                                                                        , true
                                                                    ) != 0
                                                      )
                                                  {
                                                      var x = Tuple.Create<string, string>
                                                                (
                                                                    key
                                                                    , de.Value.ToString()
                                                                );
                                                      dictionary.Add(key, x);
                                                  }
                                              }
                                              return dictionary;
                                          }
                                      )();
        private static Tuple
                        <
                            string      //机器名称
                            , string     //url
                        >[] _forwardToAddresses = new Func<Tuple<string, string>[]>
                                   (
                                       () =>
                                       {
                                           Dictionary<string, Tuple<string, string>> dictionary
                                                = new Dictionary<string, Tuple<string, string>>();
                                           var config = ConfigurationManager
                                                            .GetSection
                                                                (
                                                                    "AdditionalConfigSectionGroup/PartnersServersMessagesServiceAddresses"
                                                                ) as IDictionary;
                                           foreach (DictionaryEntry de in config)
                                           {
                                               var key = de.Key.ToString().ToLower().Trim();
                                               if
                                                   (
                                                     string
                                                        .Compare
                                                            (
                                                                key
                                                                , SessionsHostServerHelper
                                                                    .LocalSessionsHostServerMachineName
                                                                , true
                                                            ) != 0
                                                   )
                                               {
                                                   var x = Tuple.Create<string, string>
                                                             (
                                                                 key
                                                                 , de.Value.ToString()
                                                             );
                                                   dictionary.Add(key, x);
                                               }
                                           }
                                           var r = dictionary
                                                     .Select
                                                         (
                                                             (kvp) =>
                                                             {
                                                                 return kvp.Value;
                                                             }
                                                         )
                                                     .ToArray();
                                           return r;
                                       }
                                   )();

        //2014-08-14 批量同步会话
        public static void BroadcastParallelBatchCreateOrUpdateSessions
                                (
                                    List<ForwardSession> forwardSessions
                                )
        {
            var fowardSessionsParallelismDegree = ConfigurationAppSettingsManager
                                                     .RunTimeAppSettings
                                                     .FowardSessionsParallelismDegree;
            if (fowardSessionsParallelismDegree <= 1)
            {
                foreach (var x in _partnersServersSessionsServiceAddressesDictionary)
                {
                    var url = x.Value.Item2;
                    url = string.Format("{1}{0}{2}", "/", url, "");
                    PostForwardSessions(forwardSessions, url);
                }
            }
            else
            {
                try
                {
                    _partnersServersSessionsServiceAddressesDictionary
                                .AsParallel()
                                .WithDegreeOfParallelism
                                    (
                                        fowardSessionsParallelismDegree
                                    )
                                .ForAll
                                    (
                                        (x) =>
                                        {
                                            var url = x.Value.Item2;
                                            url = string.Format("{1}{0}{2}", "/", url, "");
                                            PostForwardSessions(forwardSessions, url);
                                        }
                                    );
                }
                catch (AggregateException ae)
                {
                    var innerExceptions = ae.Flatten().InnerExceptions;
                    foreach (var e in innerExceptions)
                    {
                        string s = string
                                        .Format
                                            (
                                                "{1}{0}{2}"
                                                , "\r\n"
                                                , "caught AggregateException InnerExceptions BroadcastParallelBatchCreateOrUpdateSessions"
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
                    string s = string
                                    .Format
                                        (
                                            "{1}{0}{2}"
                                            , "\r\n"
                                            , "Caught Exception BroadcastParallelBatchCreateOrUpdateSessions"
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

        private static void PostForwardSessions
                                (
                                    List<ForwardSession> forwardSessions
                                    , string url
                                )
        {
            using
                (
                    HttpClient httpClient = new HttpClient()
                    {
                        Timeout = TimeSpan.FromSeconds
                                                (
                                                    ConfigurationAppSettingsManager
                                                        .RunTimeAppSettings
                                                        .HttpClientTimeOutInSeconds
                                                )
                    }
                )
            {
                TryCatchFinallyProcessHelper
                    .TryProcessCatchFinally
                        (
                            ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .NeedTryProcess
                            , () =>
                            {
                                SessionsManager
                                    .DebugLog
                                        (
                                            "CrossServerPostForwardSessions"
                                            , () =>
                                            {
                                                var log = string
                                                            .Format
                                                                (
                                                                    "begin send PostForwardSessions [{0}] Notify to [{1}] @ [{2}]"
                                                                    , ""
                                                                    , url
                                                                    , DateTime.Now
                                                                );
                                                return log;
                                            }
                                        );
                                var response =
                                        httpClient
                                            .PostAsJsonAsync<List<ForwardSession>>
                                                (
                                                    url
                                                    , forwardSessions
                                                ).Result;
                                SessionsManager
                                    .DebugLog
                                        (
                                            "CrossServerPostForwardSessions"
                                            , () =>
                                            {
                                                var log = string
                                                            .Format
                                                                (
                                                                    "end sended PostForwardSessions [{0}] Notify to [{1}] @ [{2}]"
                                                                    , ""
                                                                    , url
                                                                    , DateTime.Now
                                                                );
                                                return log;
                                            }
                                        );
                            }
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .NeedReThrowCaughtException
                            , (xx, yy) =>
                            {
                                string s = string.Format
                                    (
                                        "{1}{0}{2}{0}{3}"
                                        , "\r\n"
                                        , "PostForwardSessions To Url: [" + url + "]"
                                        , yy
                                        , xx.ToString()
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
        public static void ForwardMessageFromOtherServer
                                (
                                    JObject jObject
                                    , string forwardBy = null
                                    , long messageID = -1
                                    , int receiverEntryID = -1
                                )
        {
            JToken jTokenMessageHeader = jObject["H"];
            //2015-01-22 Topic 入库
            var topic = string.Empty;
            JTokenHelper
                .TryGetNonNullValue
                    (
                        jTokenMessageHeader.SelectToken("T")
                        , ref topic
                    );
            var receiversEntriesJArray = jTokenMessageHeader["R"];
            var receiversEntries =
                    receiversEntriesJArray
                        .Where
                            (
                                (x) =>
                                    {
                                        var r = false;
                                        if 
                                            (
                                                x != null
                                                &&
                                                x is JObject    //解决注释问题
                                            )
                                        {
                                            //不同时为空 null
                                            r =
                                                !(
                                                    x.SelectToken("A") == null
                                                    &&
                                                    x.SelectToken("G") == null
                                                    &&
                                                    x.SelectToken("U") == null
                                                );
                                        }
                                        return r;    
                                    }
                            )
                        .Select
                            (
                                (x) =>
                                {
                                    var appID   = "*";
                                    var groupID = "*";
                                    var userID  = "*";
                                    var s = string.Empty;
                                    JToken jToken = null; 
                                    jToken = x.SelectToken("A");
                                    if (JTokenHelper.TryGetNonNullValue(jToken, ref s))
                                    {
                                        appID = s;
                                    }
                                    jToken = x.SelectToken("G");
                                    if (JTokenHelper.TryGetNonNullValue(jToken, ref s))
                                    {
                                        groupID = s;
                                    }
                                    jToken = x.SelectToken("U");
                                    if (JTokenHelper.TryGetNonNullValue(jToken, ref s))
                                    {
                                        userID = s;
                                    }
                                    var r =
                                            new Party()
                                            {
                                                AppID = appID
                                                ,
                                                GroupID = groupID.TrimStart('0', ' ')
                                                ,
                                                UserID = userID.TrimStart('0', ' ')
                                            };
                                    return r;
                                }
                            )
                        .ToArray();
            var needSaveToDB = true;
            //2015-01-27 TraceInToDataBase
            bool messageNeedTraceInToDataBase = true;
            int iMessageNeedTraceInToDataBase = 1;
            if
                (
                    JTokenHelper.TryGetNullableValue<int>
                                    (
                                        jTokenMessageHeader["TIDB"]
                                        , ref iMessageNeedTraceInToDataBase
                                    )
                )
            {
                messageNeedTraceInToDataBase = (iMessageNeedTraceInToDataBase == 1);
                //if (!messageNeedTraceInToDataBase)
                //{
                //    jTokenMessageHeader["TIDB"] = 0;
                //}
            }
            //=========================================================
            if (receiverEntryID >= 0)
            {
                receiversEntries = new Party[]
                    {
                         receiversEntries[receiverEntryID]
                    };
                needSaveToDB = false;
            }
            receiverEntryID = 0;
            var sender = new Party()
                    {
                        AppID = jTokenMessageHeader["S"]["A"].Value<string>()
                        , GroupID = jTokenMessageHeader["S"]["G"].Value<string>()
                        , UserID = jTokenMessageHeader["S"]["U"].Value<string>()
                    };
            bool canForward = string.IsNullOrEmpty(forwardBy);
            //bool isWildSend = false;
            JToken expireTimeJToken = jTokenMessageHeader["ET"];
            DateTime? expireTime = null;
            if (expireTimeJToken != null)
            {
                expireTime = expireTimeJToken.Value<DateTime>();
            }
            var json = JsonHelper.Serialize(jObject);
            Array.ForEach
                (
                    receiversEntries
                    , (x) =>
                    {
                        if (needSaveToDB)
                        {
                            messageID =
                                    DataAccess
                                        .InsertOneMessageForOneReceiverEntry
                                            (
                                                x.AppID
                                                , x.GroupID
                                                , x.UserID
                                                , sender.AppID
                                                , sender.GroupID
                                                , sender.UserID
                                                , messageNeedTraceInToDataBase
                                                , topic
                                                , json
                                                , 0
                                                , expireTime
                                                , messageID
                                            );
                            if (messageID == -10000)
                            {
                                //过期消息
                                return;
                            }
                        }
                        //2015-08-28 needSaveDB if 判断 Bug
                        JObject jObjectClone = jObject.DeepClone() as JObject;
                        var messageSendEntry = new MessageSendEntry()
                        {
                                CanForward = canForward
                                ,
                                MessageJObject = jObjectClone
                                ,
                                MessageID = messageID
                                ,
                                ReceiverEntry = x
                                ,
                                ReceiverEntryID = receiverEntryID
                                ,
                                Sender = sender
                        };
                        if
                            (
                                ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .ReceivedOriginalMessagesSendDequeueThreadsCount
                                > 0
                            )
                        {
                            //使用异步队列处理
                            SessionsManager
                                .ReceivedOriginalMessagesSendQueue
                                .Enqueue
                                    (
                                        messageSendEntry
                                    );
                        }
                        else
                        {
                            //直接同步调用
                            SessionsManager
                                .ReceivedOriginalMessagesProcess
                                    (
                                        messageSendEntry
                                    );
                        }
                        receiverEntryID++;
                    }
                );
        }

        public static void ReceivedOriginalMessagesProcess
                        (
                            MessageSendEntry messageSendEntry
                        )
        {
            var canForward = messageSendEntry.CanForward;
            var isWildSend = false;
            var sender = messageSendEntry.Sender;
            var messageJObject = messageSendEntry.MessageJObject;
            var messageID = messageSendEntry.MessageID;
            var receiverEntry = messageSendEntry.ReceiverEntry;
            var receiverEntryID = messageSendEntry.ReceiverEntryID;
            isWildSend =
                        (
                            receiverEntry.UserID == "*"
                            || receiverEntry.AppID == "*"
                            || receiverEntry.GroupID == "*"
                        );

            if 
                (
                    isWildSend
                    &&
                    canForward
                )
            {
                if 
                    (
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .NeedBroadcastToPartnersServers
                    )
                {
                     SessionsManager
                        .BroadcastForwardMessages
                            (
                                messageJObject
                                , _forwardToAddresses
                                , messageID.Value
                                , receiverEntryID
                            );
                }
            }

            var needSessionsForeachLoop = false;
            IEnumerable
                <
                    KeyValuePair
                        <
                            string
                            , Tuple
                                <
                                    SessionContextEntry
                                    , SocketAsyncDataHandler<SessionContextEntry>
                                >
                        >
                   > sessions = null;

            if (isWildSend)
            {
                //"BancsLink-BJ-*"
                if
                    (
                        receiverEntry.AppID != "*"
                        &&
                        receiverEntry.GroupID != "*"
                        &&
                        receiverEntry.UserID == "*"
                    )
                {
                    string key1 = string
                                        .Format
                                            (
                                                "{1}{0}{2}"
                                                , "-"
                                                , receiverEntry.AppID.Trim().ToLower()
                                                , receiverEntry.GroupID.TrimStart('0', ' ')
                                            ).Trim().ToLower();
                    sessions =
                            SessionsIndex
                                .GetSessionsByKey1
                                    (
                                        key1
                                    );
                    needSessionsForeachLoop = true;
                }
                else if
                    //向所有本机在线用户发送
                    (
                        receiverEntry.AppID == "*"
                        &&
                        receiverEntry.GroupID == "*"
                        &&
                        receiverEntry.UserID == "*"
                    )
                {
                    sessions = SessionsManager
                                    .Sessions
                                    .Where
                                        (
                                            (x) =>
                                            {
                                                return
                                                    x.Value.Item1.IsLocalSession;
                                            }
                                        );
                    needSessionsForeachLoop = true;
                }
            }
            else
            {
                Tuple
                    <
                        SessionContextEntry
                        , SocketAsyncDataHandler<SessionContextEntry>
                    > oneSession = null;
                var oneSessionID = receiverEntry.ID;
                if
                    (
                        SessionsManager
                            .Sessions
                            .TryGetValue
                                (
                                    oneSessionID
                                    , out oneSession
                                )
                    )
                {
                    KeyValuePair
                            <
                                string
                                , Tuple
                                    <
                                        SessionContextEntry
                                        , SocketAsyncDataHandler<SessionContextEntry>
                                    >
                            > oneKvp =
                                new KeyValuePair
                                        <
                                            string
                                            , Tuple
                                                <
                                                    SessionContextEntry
                                                    , SocketAsyncDataHandler<SessionContextEntry>
                                                >
                                        >
                                            (
                                                oneSessionID
                                                , oneSession
                                            );
                    var list =
                            new List
                                <
                                    KeyValuePair
                                        <
                                            string
                                            , Tuple
                                                <
                                                    SessionContextEntry
                                                    , SocketAsyncDataHandler<SessionContextEntry>
                                                >
                                        >
                                >();
                    list.Add(oneKvp);
                    sessions = list.AsEnumerable();
                    needSessionsForeachLoop = true;
                }
            }
            if (needSessionsForeachLoop)
            {
                foreach (var kvp in sessions)
                {
#region Send To one Receiver not one receiver entry (分解到具体的接收人会话)
                    var sessionContextEntry = kvp.Value.Item1;
                    if (sessionContextEntry.IsLocalSession)
                    {
                        //发送消息
#region 本地会话
                        var socketAsyncDataHandler = kvp.Value.Item2;
                        var remoteIPEndPoint = kvp.Value.Item1.OwnerRemoteIPEndPoint;
                        JObject messageJObjectClone = messageJObject.DeepClone() as JObject;
                        var messageTransmissionTrackerEntry = new MessageTransmissionTrackerEntry()
                        {
                            MessageID = messageID
                                ,
                            MessageJObject = messageJObjectClone
                                ,
                            ReceiverSessionContextEntry = sessionContextEntry
                                ,
                            ReceiverSocketAsyncDataHandler = socketAsyncDataHandler
                                ,
                            Sender = sender
                                ,
                            RequestTime = DateTime.Now
                                ,
                            ResponseWaiterPairID = string.Format
                                                  (
                                                      "{1}{0}{2}{0}{3}{0}{4}"   //{0}{5}"
                                                      , "-"
                                                      , sessionContextEntry.OwnerAppID.Trim().ToLower()
                                                      , sessionContextEntry.OwnerGroupID.Trim().ToLower()
                                                      , sessionContextEntry.OwnerUserID.Trim().ToLower()
                                                      , messageID
                                                      //, newSequence
                                                  )
                                ,
                            ReceiverEntryID = receiverEntryID
                        };
                        if
                            (
                                SessionsManager
                                    .MessagesTransmissionsTrackers
                                    .TryAdd
                                        (
                                            messageTransmissionTrackerEntry.ResponseWaiterPairID
                                            , messageTransmissionTrackerEntry
                                        )
                              )
                        {
                            if
                                (
                                    ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .EnableFirstSocketSendTransmissionUseAsyncQueueProcess
                                )
                            {
                                SessionsManager
                                    .SendQueue
                                    .Enqueue
                                        (messageTransmissionTrackerEntry);
                            }
                            else
                            {
                                //2014-10-04 第一次不走队列
                                SessionsManager
                                    .SendMessageTransmissionOnceWithTrackingProcess
                                        (messageTransmissionTrackerEntry);
                            }
                        }
#endregion
                    }
                    else
                    {
                        //在线但不是本地会话
                        if (!isWildSend && canForward)
                        {
#region 是单发没有**
                            //是单发没有**
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
                                                    sessionContextEntry
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
                                        MessageJObject = messageJObject
                                        ,
                                        ForwardBy = SessionsHostServerHelper
                                                      .LocalSessionsHostServerMachineName
                                        ,
                                        ForwardToUrl = url
                                        ,
                                        MessageID = messageID
                                        ,
                                        ReceiverEntryID = receiverEntryID
                                        ,
                                        ForwardToServerName = tuple.Item1
                                    };

                                    AsyncQueuesProcessorsManager
                                        .MessagesForwardToOtherServersAsyncQueueProcessor
                                        .InternalQueue
                                        .Enqueue(forwardMessage);
                                    // ======================================================================
                                }
                            }
#endregion
                        }
                    }
#endregion
                }
            }
        }
        //2014-09-25 即将废弃
        //=============================================================================================
        //public static void ForwardMessageFromOtherServerOld
        //                        (
        //                            IMessage message
        //                            , JObject jObjectMessage
        //                            , string data
        //                            , string forwardBy = null
        //                            , long messageID = -1
        //                            , int receiverEntryID = -1
        //                        )
        //{
        //    var json = data;
        //    var messageHeader = message.Header;
        //    var receiversEntries = messageHeader
        //                                .Receivers
        //                                .Where
        //                                    (
        //                                        (x) =>
        //                                        {
        //                                            return (x != null);
        //                                        }
        //                                    )
        //                                .Select
        //                                    (
        //                                        (x) =>
        //                                        {
        //                                            x.GroupID = x.GroupID.TrimStart('0', ' ');
        //                                            x.UserID = x.UserID.TrimStart('0', ' ');
        //                                            return x;
        //                                        }
        //                                    )
        //                                .ToArray();

        //    var needSaveToDB = true;
        //    if (receiverEntryID >= 0)
        //    {
        //        needSaveToDB = false;
        //        receiversEntries = new Party[]
        //                            {
        //                                receiversEntries[receiverEntryID]
        //                            };
        //    }
        //    receiverEntryID = 0;
        //    var sender = messageHeader.Sender;
        //    bool canForward = string.IsNullOrEmpty(forwardBy);
        //    bool isWildSend = false;
        //    DateTime? expireTime = messageHeader.ExpireTime; //DateTime.Now; //messageHeader.ex


        //    //2014-08-28 11:25
        //    //int ii = -1;
        //    //==================================


        //    Array.ForEach
        //        (
        //            receiversEntries
        //            , (x) =>
        //            {
                        
        //                isWildSend =
        //                            (
        //                                x.UserID == "*"
        //                                || x.AppID == "*"
        //                                || x.GroupID == "*"
        //                            );

        //                #region 如果是从别的合作服务器转过来的,不必再次转发导致死循环,是第一次首发
        //                if (canForward)
        //                {
        //                    //是首发
        //                    if (needSaveToDB)
        //                    {
        //                        messageID =
        //                                    DataAccess
        //                                        .InsertOneMessageForOneReceiverEntry
        //                                                (
        //                                                    x.AppID
        //                                                    , x.GroupID
        //                                                    , x.UserID
        //                                                    , sender.AppID
        //                                                    , sender.GroupID
        //                                                    , sender.UserID
        //                                                    , data
        //                                                    , 0
        //                                                    , expireTime
        //                                                    , messageID
        //                                                );
        //                        if (messageID == -10000)
        //                        {
        //                            //过期消息
        //                            return;
        //                        }
        //                    }

        //                    // 2014-08-28 09:30 移动到最前面先转发到其他机器 =================================
        //                    if (isWildSend && canForward)
        //                    {
        //                        if (ConfigurationAppSettingsManager.AppSettings.NeedBroadcastToPartnersServers)
        //                        {
        //                            //SessionsManager
        //                            //    .MessagesForwardToOtherServersAsyncQueueProcessor
        //                            //    .InternalQueue
        //                            //    .Enqueue(forwardMessage);
        //                            SessionsManager
        //                                .BroadcastForwardMessages
        //                                    (
        //                                        jObjectMessage
        //                                        , _forwardToAddresses
        //                                        , messageID
        //                                        , receiverEntryID
        //                                    );
        //                        }
        //                    }
        //                    //====================================================================
        //                }
        //                else
        //                {
        //                    ////是转发来的
        //                    //if (messageID == 0)
        //                    //{
        //                    //    //***群发
        //                    //    //messageIDForOneReceiverEntry = messageID;
        //                    //}
        //                    //else
        //                    //{ 
        //                    //    //单发
        //                    //    //messageIDForOneReceiverEntry = messageID;
        //                    //}

        //                }
        //                #endregion

        //                //2014-08-18 21 =====================================
        //                var needFor = false;
        //                IEnumerable
        //                    <
        //                        KeyValuePair
        //                            <
        //                                string
        //                                , Tuple
        //                                    <
        //                                        SessionContextEntry
        //                                        , SocketAsyncDataHandler<SessionContextEntry>
        //                                    >
        //                            >
        //                       > sessions = null;



        //                #region 所有服务器上的会话
        //                if (isWildSend)
        //                {
        //                    isWildSend = true;
                            
        //                    /*
        //                    sessions
        //                        = SessionsManager
        //                            .GetSessions
        //                                    (
        //                                        x                             //单接收方Entry
        //                                        , "*"                         //所有服务器上的会话
        //                                        , 0
        //                                        , int.MaxValue
        //                                    );
        //                    */


        //                    //.AsParallel()
        //                    //.WithDegreeOfParallelism
        //                    //(
        //                    //    Environment.ProcessorCount
        //                    //);
                            
        //                    //2014-08-29 11:13 改为使用而级索引遍历


        //                   // SessionsIndex
        //                     //   .
        //                    string key1 = string.Format
        //                                    (
        //                                        "{1}{0}{2}"
        //                                        ,"-"
        //                                        , x.AppID.Trim().ToLower()
        //                                        , x.GroupID.TrimStart('0', ' ')
        //                                    ).Trim().ToLower();
        //                    sessions =
        //                            SessionsIndex.GetSessionsByKey1
        //                                    (
        //                                        key1
        //                                    );
        //                    if (sessions != null)
        //                    {
        //                        if (sessions.Count() > 0)
        //                        {
        //                            needFor = true;    
        //                        }
        //                    }
                            
        //                }
        //                else
        //                {
        //                    Tuple
        //                        <
        //                            SessionContextEntry
        //                            , SocketAsyncDataHandler<SessionContextEntry>
        //                        > oneSession = null;
        //                    var oneSessionID = string.Format
        //                                            (
        //                                                "{0}-{1}-{2}"
        //                                                , x.AppID
        //                                                , x.GroupID.TrimStart('0', ' ')
        //                                                , x.UserID.TrimStart('0', ' ')
        //                                            ).ToLower();
        //                    if
        //                        (
        //                            SessionsManager
        //                                .Sessions
        //                                .TryGetValue
        //                                    (
        //                                        oneSessionID
        //                                        , out oneSession
        //                                    )
        //                        )
        //                    {
        //                        KeyValuePair
        //                               <
        //                                   string
        //                                   , Tuple
        //                                       <
        //                                           SessionContextEntry
        //                                           , SocketAsyncDataHandler<SessionContextEntry>
        //                                       >
        //                               > oneKvp =
        //                                    new KeyValuePair
        //                                            <
        //                                                string
        //                                                , Tuple
        //                                                    <
        //                                                        SessionContextEntry
        //                                                        , SocketAsyncDataHandler<SessionContextEntry>
        //                                                    >
        //                                            >
        //                                            (
        //                                                oneSessionID
        //                                                , oneSession
        //                                            );
        //                        var list =
        //                                new List
        //                                    <
        //                                        KeyValuePair
        //                                            <
        //                                                string
        //                                                , Tuple
        //                                                    <
        //                                                        SessionContextEntry
        //                                                        , SocketAsyncDataHandler<SessionContextEntry>
        //                                                    >
        //                                            >
        //                                    >();
        //                        list.Add(oneKvp);
        //                        sessions = list.AsEnumerable();
        //                        needFor = true;
        //                    }
        //                }
        //                //2014-08-18 21 =====================================
        //                //2014-08-18 21 新增 needFor
        //                if (needFor)
        //                {
        //                    foreach (var kvp in sessions)
        //                    {
        //                        #region Send To one Receiver not one receiver entry (分解到具体的接收人会话)
        //                        var sessionContextEntry = kvp.Value.Item1;
        //                        if (sessionContextEntry.IsLocalSession)
        //                        {
        //                            //发送消息
        //                            #region 本地会话
        //                            var socketAsyncDataHandler = kvp.Value.Item2;
        //                            var remoteIPEndPoint = kvp.Value.Item1.OwnerRemoteIPEndPoint;
        //                            message = JsonsMessagesProcessorsCacheManager.GetJsonObjectByJson(json);
        //                            var newSequence = SequenceSeedHelper.NewID();
        //                            var messageTransmissionTrackerEntry = new MessageTransmissionTrackerEntry()
        //                            { 
        //                                MessageID = messageID
        //                                //    ,
        //                                //Sequence = newSequence
        //                                //    ,
        //                                //Message = message
        //                                //    ,
        //                                //MessageDatagram = json
        //                                    ,
        //                                ReceiverSessionContextEntry = sessionContextEntry
        //                                    ,
        //                                ReceiverSocketAsyncDataHandler = socketAsyncDataHandler
        //                                    ,
        //                                Sender = sender
        //                                    ,
        //                                RequestTime = DateTime.Now
        //                                    ,
        //                                ResponseWaiterPairID = string
        //                                                            .Format
        //                                                                (
        //                                                                    "{1}{0}{2}{0}{3}{0}{4}"   //{0}{5}"
        //                                                                    , "-"
        //                                                                    , sessionContextEntry
        //                                                                            .OwnerAppID
        //                                                                            .Trim()
        //                                                                            .ToLower()
        //                                                                    , sessionContextEntry
        //                                                                            .OwnerGroupID
        //                                                                            .Trim()
        //                                                                            .ToLower()
        //                                                                    , sessionContextEntry
        //                                                                            .OwnerUserID
        //                                                                            .Trim()
        //                                                                            .ToLower()
        //                                                                    , messageID
        //                                                                    //, newSequence
        //                                                                )
        //                            };
        //                            if
        //                                (
        //                                    SessionsManager
        //                                        .MessagesTransmissionsTrackers
        //                                        .TryAdd
        //                                            (
        //                                                messageTransmissionTrackerEntry.ResponseWaiterPairID
        //                                                , messageTransmissionTrackerEntry
        //                                            )
        //                                  )
        //                            {

        //                                if
        //                                    (
        //                                        ConfigurationAppSettingsManager
        //                                            .AppSettings
        //                                            .IsSocketSendAsyncQueueProcess
        //                                    )
        //                                {
        //                                    SessionsManager
        //                                        .SendQueue
        //                                        .Enqueue
        //                                            (messageTransmissionTrackerEntry);
        //                                }
        //                                else
        //                                {
        //                                    SessionsManager
        //                                        .SendOneMessageWithWaitResponseProcess
        //                                            (messageTransmissionTrackerEntry);
        //                                }
        //                            }
        //                            #endregion
        //                        }
        //                        else
        //                        {
        //                            //在线但不是本地会话
        //                            if (!isWildSend && canForward)
        //                            {
        //                                #region 是单发没有**
        //                                //是单发没有**
        //                                if
        //                                    (
        //                                        ConfigurationAppSettingsManager
        //                                            .AppSettings
        //                                            .NeedBroadcastToPartnersServers
        //                                    )
        //                                {
        //                                    Tuple<string, string> tuple = null;
        //                                    if
        //                                        (
        //                                            _partnersServersMessagesServiceAddressesDictionary
        //                                                .TryGetValue
        //                                                    (
        //                                                        sessionContextEntry
        //                                                            .SessionHostServer
        //                                                            .ToLower()
        //                                                            .Trim()
        //                                                        , out tuple
        //                                                    )
        //                                        )
        //                                    {
        //                                        //2014-08-25 于溪玥
        //                                        //========================================
        //                                        var url = tuple.Item2;          //url
                                                
        //                                        //url = string.Format
        //                                        //            (
        //                                        //                "{1}{0}{2}{0}{3}{0}{4}"
        //                                        //                , "/"
        //                                        //                , url
        //                                        //                , SessionsHostServerHelper
        //                                        //                    .LocalSessionsHostServerMachineName       //forwardBy
        //                                        //                , messageID                     //messageID
        //                                        //                , receiverEntryID
        //                                        //            );

        //                                        ForwardMessage forwardMessage = new ForwardMessage()
        //                                        {
        //                                            JObjectMessage = jObjectMessage
        //                                            , ForwardBy = SessionsHostServerHelper
        //                                                            .LocalSessionsHostServerMachineName
        //                                            , ForwardToUrl = url
        //                                            , MessageID  = messageID
        //                                            , ReceiverEntryID = receiverEntryID
        //                                            , ForwardToServerName = tuple.Item1
        //                                        };

        //                                        AsyncQueuesProcessorsManager
        //                                            .MessagesForwardToOtherServersAsyncQueueProcessor
        //                                            .InternalQueue
        //                                            .Enqueue(forwardMessage);

        //                                        #region 如下功能移动到出列处理
        //                                        //====================================================================
        //                                        //    
        //                                        //    TryCatchFinallyProcessHelper
        //                                        //        .TryProcessCatchFinally
        //                                        //            (
        //                                        //                true
        //                                        //                , () =>
        //                                        //                {
        //                                        //                    //2014-08-18 22
        //                                        //                    PostJObjects
        //                                        //                       (
        //                                        //                           json
        //                                        //                          , url
        //                                        //                       );
        //                                        //                    //SessionsManager
        //                                        //                    //    .MessagesForwardToOtherServersAsyncQueue
        //                                        //                    //        .Enqueue
        //                                        //                    //            (
        //                                        //                    //                Tuple.Create
        //                                        //                    //                    <
        //                                        //                    //                        string      //json
        //                                        //                    //                       , string    //url
        //                                        //                    //                       , string    //baseUrl
        //                                        //                    //                    >
        //                                        //                    //                (
        //                                        //                    //                    json
        //                                        //                    //                    , url
        //                                        //                    //                    , baseUrl
        //                                        //                    //                )
        //                                        //                    //            );
        //                                        //                }
        //                                        //                , false
        //                                        //                , (xx, yy) =>
        //                                        //                {
        //                                        //                    string s = string.Format
        //                                        //                                        (
        //                                        //                                            "{1}{0}{2}"
        //                                        //                                            , "\r\n"
        //                                        //                                            , yy
        //                                        //                                            , xx.ToString()
        //                                        //                                        );
        //                                        //                    Console.WriteLine(s);
        //                                        //                    EventLogHelper
        //                                        //                        .WriteEventLogEntry
        //                                        //                            (
        //                                        //                                ConfigurationAppSettingsManager
        //                                        //                                    .AppSettings
        //                                        //                                        .EventLogSourceName
        //                                        //                                , s
        //                                        //                                , EventLogEntryType.Error
        //                                        //                                , ConfigurationAppSettingsManager
        //                                        //                                    .AppSettings
        //                                        //                                        .EventLogDefaultErrorExceptionEventID
        //                                        //                            );
        //                                        //                    return false;
        //                                        //                }
        //                                        //            );
        //                                        //======================================================================================= 
        //                                        #endregion
        //                                        // ======================================================================
        //                                    }
        //                                }
        //                                #endregion
        //                            }
        //                        }
        //                        #endregion
        //                    }
        //                    //);
        //                }
        //                // 2014-08-28 09:30 移动到最前面先转发到其他机器
        //                //if (isWildSend && canForward)
        //                //{
        //                //    if (ConfigurationAppSettingsManager.AppSettings.NeedBroadcastToPartnersServers)
        //                //    {
        //                //        //SessionsManager
        //                //        //    .MessagesForwardToOtherServersAsyncQueueProcessor
        //                //        //    .InternalQueue
        //                //        //    .Enqueue(forwardMessage);
        //                //        SessionsManager
        //                //            .BroadcastForwardMessages
        //                //                (
        //                //                    jObjectMessage
        //                //                    , _forwardToAddresses
        //                //                    , messageID
        //                //                    , receiverEntryID
        //                //                );
        //                //    }
        //                //}
        //                #endregion


        //                receiverEntryID++;
        //            }
        //        );
        //}
        //===============================================================================================
        public static void BroadcastForwardMessages
                            (
                                JObject messageJObject
                                , Tuple
                                    <
                                        string              //机器名
                                        , string            //URL地址
                                    >[] addresses
                                , long messageID
                                , int receiverEntryID
                            )
        {
            Array
                .ForEach
                    (
                        addresses
                        , (x) =>
                        {
                            TryCatchFinallyProcessHelper
                                .TryProcessCatchFinally
                                    (
                                        ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .NeedTryProcess
                                        , () =>
                                        {
                                            //PostJson(message, url);

                                            ForwardMessage forwardMessage = new ForwardMessage()
                                            {
                                                ForwardBy = SessionsHostServerHelper
                                                                .LocalSessionsHostServerMachineName  ////转发自发起方服务器名称
                                                ,
                                                MessageJObject = messageJObject
                                                ,
                                                ForwardToUrl = x.Item2
                                                ,
                                                MessageID = messageID
                                                ,
                                                ReceiverEntryID = receiverEntryID
                                                ,
                                                ForwardToServerName = x.Item1           //转发到目标服务器名称
                                            };
                                            AsyncQueuesProcessorsManager
                                                .MessagesForwardToOtherServersAsyncQueueProcessor
                                                .InternalQueue
                                                .Enqueue
                                                    (
                                                        forwardMessage
                                                    );
                                        }
                                        , ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .NeedReThrowCaughtException
                                        , (xx, yy) =>
                                        {
                                            string s = string
                                                            .Format
                                                                (
                                                                    "{1}{0}{2}"
                                                                    , "\r\n"
                                                                    , yy
                                                                    , xx.ToString()
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
                    );
        }
        public static void PostObjectsAsJson<T>(T jsonObject, string url)
        {
            using
                    (
                        HttpClient httpClient =
                            new HttpClient()
                                {
                                    Timeout = TimeSpan
                                                .FromSeconds
                                                    (
                                                        ConfigurationAppSettingsManager
                                                            .RunTimeAppSettings
                                                            .HttpClientTimeOutInSeconds
                                                    )
                                }
                    )
            {
                var response = httpClient
                                .PostAsJsonAsync<T>
                                        (
                                            url
                                            , jsonObject
                                        ).Result;
                //坑爹
                //.Start();
            }
        }

        public static void PostJson(string json, string url)
        {
            using
                (
                    HttpClient httpClient =
                        new HttpClient()
                            {
                                Timeout = TimeSpan
                                            .FromSeconds
                                                (
                                                    ConfigurationAppSettingsManager
                                                        .RunTimeAppSettings
                                                        .HttpClientTimeOutInSeconds
                                                )
                            }
                )
            {
                var messages = new JObject[]
                                {
                                    JObject.Parse(json)
                                };
                var response = httpClient
                                .PostAsJsonAsync<IEnumerable<JObject>>
                                        (
                                            url
                                            , messages
                                        ).Result;
                    //坑爹
                    //.Start();
            }
        }
        public static void LoadInitialRemoteSessions()
        {
            TryCatchFinallyProcessHelper
                .TryProcessCatchFinally
                    (
                        ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .NeedTryProcess
                        , () =>
                        {
                            DataAccess
                                .GetExclusiveServersSessionsPages
                                     (
                                         new string[]
                                                {
                                                    SessionsHostServerHelper
                                                        .LocalSessionsHostServerMachineName
                                                }
                                         , ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .InitializationLoadSessionsPageSizeInRows
                                         , (x, y) =>
                                         {
                                             var sessions =
                                                     from
                                                         T in y.AsEnumerable()
                                                     select
                                                         new Func<SessionContextEntry>
                                                         (
                                                             () =>
                                                             {
                                                                 var sessionContextEntry = new SessionContextEntry()
                                                                 {
                                                                     OwnerAppID = T.Field<string>("OwnerAppID")
                                                                     ,
                                                                     OwnerGroupID = T.Field<string>("OwnerGroupID")
                                                                     ,
                                                                     OwnerUserID = T.Field<string>("OwnerUserID")
                                                                     ,
                                                                     LastClientHeartBeatTime = DateTime.Now //T.Field<DateTime?>("LastClientHeartBeatTime")
                                                                     ,
                                                                     LastUpdateTime = DateTime.Now //T.Field<DateTime?>("LastUpdateTime")
                                                                     ,
                                                                     SessionHostServer = T.Field<string>("SessionHostServer")
                                                                     ,
                                                                     IsLocalSession = false

                                                                 };
                                                                 var s = T.Field<string>("OwnerRemoteIPEndPoint");
                                                                 var a = s.Split(':');
                                                                 IPAddress ipa = null;
                                                                 if (IPAddress.TryParse(a[0], out ipa))
                                                                 {
                                                                     sessionContextEntry.OwnerRemoteIP = a[0];
                                                                 }
                                                                 int port = 0;
                                                                 if (int.TryParse(a[1], out port))
                                                                 {
                                                                     sessionContextEntry.OwnerRemotePort = port;
                                                                 }
                                                                 return sessionContextEntry;
                                                             }
                                                         )();
                                             foreach (var entry in sessions)
                                             {
                                                 int addedRemoteSessions = 0;
                                                 int removedLocalSessions = 0;
                                                 Tuple
                                                     <
                                                         SessionContextEntry
                                                         , SocketAsyncDataHandler<SessionContextEntry>
                                                     > currentSession = null;
                                                 SessionsManager.CreateOrUpdateRemoteHostSession
                                                                 (
                                                                     entry
                                                                     , out currentSession
                                                                     , out addedRemoteSessions
                                                                     , out removedLocalSessions
                                                                 );
                                                 SessionsManager.SessionsPerformanceCountIncrement
                                                     (
                                                         addedRemoteSessions
                                                         , ConfigurationAppSettingsManager
                                                             .RunTimeAppSettings
                                                             .SessionsPerformanceCountersCategoryName
                                                         , ConfigurationAppSettingsManager
                                                             .RunTimeAppSettings
                                                             .SessionsPerformanceCountersCategoryInstanceRemoteSessions
                                                     );
                                                 SessionsManager.SessionsPerformanceCountIncrement
                                                     (
                                                         -1 * removedLocalSessions
                                                         , ConfigurationAppSettingsManager
                                                             .RunTimeAppSettings
                                                             .SessionsPerformanceCountersCategoryName
                                                         , ConfigurationAppSettingsManager
                                                             .RunTimeAppSettings
                                                             .SessionsPerformanceCountersCategoryInstanceLocalSessions
                                                     );
                                             }
                                             return false;
                                         }
                                     );
                        }
                        , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .NeedReThrowCaughtException
                        , (xx, yy) =>
                        {
                            string s = string.Format
                                            (
                                                "{1}{0}{2}"
                                                , "\r\n"
                                                , yy
                                                , xx.ToString()
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
        public static void DebugLog
                    (
                        string logType
                        , Func<string> onGetLogProcessFunc
                    )
        {
            //var sessionsIDsArray = sessionsIDs.ToArray();
            string log = string.Empty;
            if
                (
                    ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .EnableDebugLog
                    ||
                    ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .EnableDebugConsoleOutput
                )
            {
                log = onGetLogProcessFunc();
                if 
                    (
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .EnableDebugLog
                    )
                {
                    FileLogHelper
                        .LogToTimeAlignedFile
                                (
                                    log
                                    , logType //"CrossServerReceive"
                                    , ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .LogFileRootDirectoryPath
                                    , ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .LogFileNameAlignSeconds
                                 );
                }
                if
                    (
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .EnableDebugConsoleOutput
                    )
                {
                    Console.WriteLine(log);
                }
            }
        }

        //public static void BroadcastDeleteSessionsTaskAsyncRun(string[] sessionsIDs)
        //{
        //    Task
        //        .Factory
        //        .StartNew
        //        //Task.Run
        //            (
        //                () =>
        //                {
        //                    BroadcastDeleteSessions(sessionsIDs);
        //                }
        //                , CancellationToken.None
        //                , TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach
        //                , TaskScheduler.Default
        //            );
        //}
        //public static void BroadcastDeleteSessions(string[] sessionsIDs)
        //{
        //    using
        //        (
        //            HttpClient httpClient = new HttpClient()
        //            {
        //                Timeout = TimeSpan.FromSeconds
        //                                        (
        //                                            ConfigurationAppSettingsManager
        //                                                .AppSettings
        //                                                    .HttpClientTimeOutInSeconds
        //                                        )
        //            }
        //        )
        //    {
        //        foreach (var x in _partnersServersSessionsServiceAddressesDictionary)
        //        {
        //            var url = x.Value.Item2;
        //            //url = string.Format("{1}{0}{2}", "", url, "delete");

        //            TryCatchFinallyProcessHelper
        //                .TryProcessCatchFinally
        //                    (
        //                        true
        //                        , () =>
        //                        {
        //                            SessionsManager
        //                                .DebugLog
        //                                    (
        //                                        "CrossServerSend"
        //                                        , () =>
        //                                        {
        //                                            var ss = string.Join(",", sessionsIDs);
        //                                            var log = string.Format
        //                                                    (
        //                                                        "begin BroadcastDeleteMultipleSessions [{0}] Notify to [{1}]"
        //                                                        , ss
        //                                                        , url
        //                                                    );
        //                                            return log;
        //                                        }
        //                                    );
        //                            var response = httpClient.PostAsJsonAsync<IEnumerable<string>>
        //                                                (
        //                                                    url
        //                                                    ,
        //                                                    sessionsIDs
        //                                                ).Result;

        //                            SessionsManager
        //                                .DebugLog
        //                                    (
        //                                        "CrossServerSend"
        //                                        , () =>
        //                                        {
        //                                            var ss = string.Join(",", sessionsIDs);
        //                                            var log = string.Format
        //                                                    (
        //                                                        "end BroadcastDeleteMultipleSessions [{0}] Notify to [{1}]"
        //                                                        , ss
        //                                                        , url
        //                                                    );
        //                                            return log;
        //                                        }
        //                                    );
        //                        }
        //                        , false
        //                        , (xx, yy) =>
        //                        {
        //                            string s = string.Format
        //                                                (
        //                                                    "{1}{0}{2}"
        //                                                    , "\r\n"
        //                                                    , yy
        //                                                    , xx.ToString()
        //                                                );
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
        //                    );
        //        }
        //    }
        //}

        //已经废弃
        //public static void BroadcastDeleteOneSessionTaskAsyncRun(string sendClientGuid, string sessionID)
        //{
        //    Task
        //        .Factory
        //            .StartNew
        //        //Task.Run
        //                (
        //                    () =>
        //                    {
        //                        BroadcastDeleteOneSession(sendClientGuid, sessionID);
        //                    }
        //                     , CancellationToken.None
        //                     , TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach
        //                     , TaskScheduler.Default
        //                );
        //}
        //public static void BroadcastDeleteOneSession(string senderClientGuid, string sessionID)
        //{
        //    using
        //         (
        //             HttpClient httpClient = new HttpClient()
        //             {
        //                 Timeout = TimeSpan.FromSeconds
        //                                         (
        //                                             ConfigurationAppSettingsManager
        //                                                 .AppSettings
        //                                                 .HttpClientTimeOutInSeconds
        //                                         )
        //             }
        //         )
        //    {
        //        {
        //            foreach (var x in _partnersServersSessionServiceAddressesDictionary)
        //            {
        //                var url = x.Value.Item2;
        //                url = string.Format
        //                            (
        //                                "{1}{0}{2}{0}{3}"
        //                                , "/"
        //                                , url
        //                                , senderClientGuid
        //                                , sessionID
        //                            );


        //                TryCatchFinallyProcessHelper
        //                    .TryProcessCatchFinally
        //                        (
        //                            true
        //                            , () =>
        //                            {
        //                                SessionsManager
        //                                    .DebugLog
        //                                        (
        //                                            "CrossServerSend"
        //                                            , () =>
        //                                            {
        //                                                var log = string.Format
        //                                                            (
        //                                                                "begin BroadcastDeleteOneSession [{0}] Logout Notify to [{1}]"
        //                                                                , sessionID
        //                                                                , url
        //                                                            );
        //                                                return log;
        //                                            }
        //                                        );
        //                                var response = httpClient.DeleteAsync
        //                                                    (
        //                                                        url
        //                                                    ).Result;
        //                                SessionsManager
        //                                    .DebugLog
        //                                        (
        //                                            "CrossServerSend"
        //                                            , () =>
        //                                            {
        //                                                var log = string.Format
        //                                                            (
        //                                                                "end BroadcastDeleteOneSession [{0}] Logout Notify to [{1}]"
        //                                                                , sessionID
        //                                                                , url
        //                                                            );
        //                                                return log;
        //                                            }
        //                                        );
        //                            }
        //                            , false
        //                            , (xx, yy) =>
        //                            {
        //                                string s = string.Format
        //                                                    (
        //                                                        "{1}{0}{2}"
        //                                                        , "\r\n"
        //                                                        , yy
        //                                                        , xx.ToString()
        //                                                    );
        //                                Console.WriteLine(s);
        //                                EventLogHelper
        //                                    .WriteEventLogEntry
        //                                        (
        //                                            ConfigurationAppSettingsManager
        //                                                .AppSettings
        //                                                .EventLogSourceName
        //                                            , s
        //                                            , EventLogEntryType.Error
        //                                            , ConfigurationAppSettingsManager
        //                                                .AppSettings
        //                                                .EventLogDefaultErrorExceptionEventID
        //                                        );
        //                                return false;
        //                            }
        //                        );
        //            }
        //        }
        //    }
        //}



        
        ////login 发起
        //public static void BroadcastCreateOrUpdateSessionTaskAsyncRun
        //                            (
        //                                string senderClientGuid
        //                                , SessionContextEntry sessionContextEntry
        //                            )
        //{
        //    Task.Factory.StartNew
        //        //Task.Run
        //                    (
        //                        () =>
        //                        {
        //                            BroadcastCreateOrUpdateSession(senderClientGuid, sessionContextEntry);
        //                        }
        //                        , CancellationToken.None
        //                        , TaskCreationOptions.LongRunning | TaskCreationOptions.DenyChildAttach
        //                        , TaskScheduler.Default
        //                    );
        //}
        //public static void BroadcastCreateOrUpdateSession(string senderClientGuid, SessionContextEntry sessionContextEntry)
        //{
        //    using
        //        (
        //            HttpClient httpClient = new HttpClient()
        //            {
        //                Timeout = TimeSpan.FromSeconds
        //                                        (
        //                                            ConfigurationAppSettingsManager
        //                                                .AppSettings
        //                                                    .HttpClientTimeOutInSeconds
        //                                        )
        //            }
        //        )
        //    {
        //        foreach (var x in _partnersServersSessionServiceAddressesDictionary)
        //        {
        //            var url = x.Value.Item2;
        //            url = string.Format("{1}{0}{2}", "/", url, senderClientGuid);
        //            TryCatchFinallyProcessHelper
        //                .TryProcessCatchFinally
        //                    (
        //                        true
        //                        , () =>
        //                        {
        //                            SessionsManager
        //                                .DebugLog
        //                                    (
        //                                        "CrossServerSend"
        //                                        , () =>
        //                                        {
        //                                            var log = string.Format
        //                                                                (
        //                                                                    "begin BroadcastCreateOrUpdateSession [{0}] Login Notify to [{1}]"
        //                                                                    , sessionContextEntry.SessionID
        //                                                                    , url
        //                                                                );
        //                                            return log;
        //                                        }
        //                                    );
        //                            var response =
        //                                    httpClient
        //                                        .PostAsJsonAsync<SessionContextEntry>
        //                                            (
        //                                                url
        //                                                , sessionContextEntry
        //                                            ).Result;
        //                            SessionsManager
        //                                .DebugLog
        //                                    (
        //                                        "CrossServerSend"
        //                                        , () =>
        //                                        {
        //                                            var log = string.Format
        //                                                            (
        //                                                                "end BroadcastCreateOrUpdateSession [{0}] Login Notify to [{1}]"
        //                                                                , sessionContextEntry.SessionID
        //                                                                , url
        //                                                            );
        //                                            return log;
        //                                        }
        //                                    );
        //                        }
        //                        , false
        //                        , (xx, yy) =>
        //                        {
        //                            string s = string.Format
        //                                (
        //                                    "{1}{0}{2}"
        //                                    , "\r\n"
        //                                    , yy
        //                                    , xx.ToString()
        //                                );
        //                            Console.WriteLine(s);
        //                            EventLogHelper
        //                                .WriteEventLogEntry
        //                                    (
        //                                        ConfigurationAppSettingsManager
        //                                            .AppSettings
        //                                                .EventLogSourceName
        //                                        , s
        //                                        , EventLogEntryType.Error
        //                                        , ConfigurationAppSettingsManager
        //                                            .AppSettings
        //                                                .EventLogDefaultErrorExceptionEventID
        //                                    );
        //                            return false;
        //                        }
        //                    );
        //        }
        //        //);
        //    }
        //}
    }
}

#endif