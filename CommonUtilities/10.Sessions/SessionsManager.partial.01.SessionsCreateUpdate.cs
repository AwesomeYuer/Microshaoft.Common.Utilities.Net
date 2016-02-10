#if NET45
namespace Microsoft.Boc
{
    using Microsoft.Boc.Communication.Configurations;
    using Microsoft.Boc.Share;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Net;
    using System.Text;

    public class SessionContextInfo
    {
        public Party Owner { private set; get; }
        public IPEndPoint OwnerRemoteIPEndPoint { get; private set; }
        public Guid? OwnerClientGuid { get; private set; }
        public SessionContextInfo
                    (
                        SessionContextEntry sessionContextEntry
                    )
        {
            Owner = new Party()
            {
                AppID = sessionContextEntry.OwnerAppID
                 ,
                GroupID = sessionContextEntry.OwnerGroupID
                 ,
                UserID = sessionContextEntry.OwnerUserID
            };
            IPAddress ipa = new IPAddress
                                    (
                                        sessionContextEntry
                                            .OwnerRemoteIPEndPoint
                                            .Address
                                            .GetAddressBytes()
                                    );
            int port = sessionContextEntry
                            .OwnerRemoteIPEndPoint
                            .Port;
            OwnerRemoteIPEndPoint = new IPEndPoint
                                            (
                                                ipa
                                                , port
                                            );
            OwnerClientGuid = sessionContextEntry.OwnerClientGuid.Value;
            //Connection = connection;
        }
    }

    public static partial class SessionsManager
    {
        private static EasyTimer _heartBeatTimer =
                                    new EasyTimer
                                            (
                                                ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .HeartBeatTimerIntervalInSeconds
                                                , 10
                                                , (x) =>
                                                    {
                                                        //现将过期的用户与连接都删掉
                                                        int removedLocalSessions = 0;
                                                        int removedRemoteSessions = 0;
                                                        SessionsManager
                                                            .RemoveTimeOutLocalSessions
                                                                (
                                                                    out removedLocalSessions
                                                                    , out removedRemoteSessions
                                                                );

                                                        SessionsManager
                                                            .SessionsPerformanceCountIncrement
                                                                        (
                                                                            -1 * removedLocalSessions
                                                                            , ConfigurationAppSettingsManager
                                                                                    .RunTimeAppSettings
                                                                                        .SessionsPerformanceCountersCategoryName
                                                                            , ConfigurationAppSettingsManager
                                                                                    .RunTimeAppSettings
                                                                                        .SessionsPerformanceCountersCategoryInstanceLocalSessions
                                                                        );
                                                        SessionsManager
                                                            .SessionsPerformanceCountIncrement
                                                                        (
                                                                            -1 * removedRemoteSessions
                                                                            , ConfigurationAppSettingsManager
                                                                                .RunTimeAppSettings
                                                                                .SessionsPerformanceCountersCategoryName
                                                                            , ConfigurationAppSettingsManager
                                                                                .RunTimeAppSettings
                                                                                .SessionsPerformanceCountersCategoryInstanceRemoteSessions
                                                                        );
                                                        //再向所有用户下发心跳
                                                        ServerHeartBeatsToClients();
                                                    }
                                                , true
                                                , true
                                                , (x, y) =>
                                                    {
                                                        Console.WriteLine(y.ToString());
                                                        EventLogHelper
                                                            .WriteEventLogEntry
                                                                (
                                                                    ConfigurationAppSettingsManager
                                                                        .RunTimeAppSettings
                                                                        .EventLogSourceName
                                                                    , y.ToString()
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
        private static ConcurrentDictionary
                            <
                                string      //sessionID
                                , Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                            > 
                                _sessions =
                                        new ConcurrentDictionary
                                                <
                                                    string      //sessionID
                                                    , Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                                                >();
        public static ConcurrentDictionary
                            <
                                string      //sessionID
                                , Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                            >
                                Sessions
        {
            get
            {
                return _sessions;
            }
        }

        public static bool TryUpdateLocalHostSession
                                (
                                    Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> newValue
                                    , out Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> currentSession
                                )
        {
            var r = false;
            var newSessionContextEntry = newValue.Item1;
            var sessionID = newSessionContextEntry.SessionID;
            if
                (
                    _sessions
                        .TryGetValue
                            (
                                sessionID
                                , out currentSession
                            )
                )
            {
                var currentSessionContextEntry = currentSession.Item1;
                if (currentSessionContextEntry.IsLocalSession)
                {
                    if 
                        (
                            currentSessionContextEntry
                                .SessionEquals(newSessionContextEntry)
                        )
                    {
                        if 
                            (
                                newSessionContextEntry
                                    .LastClientHeartBeatTime != null
                                &&
                                newSessionContextEntry
                                    .LastClientHeartBeatTime
                                    .HasValue
                            )
                        {
                            currentSessionContextEntry
                                .LastClientHeartBeatTime = newSessionContextEntry
                                                                .LastClientHeartBeatTime;
                        }
                        if 
                            (
                                newSessionContextEntry
                                    .LastUpdateTime != null
                                &&
                                newSessionContextEntry
                                    .LastUpdateTime
                                    .HasValue
                            )
                        {
                            currentSessionContextEntry
                                .LastUpdateTime = newSessionContextEntry
                                                        .LastUpdateTime;
                        }
                        if (!string.IsNullOrEmpty(newSessionContextEntry.Presence))
                        {
                            currentSessionContextEntry.Presence = newSessionContextEntry.Presence;
                        }
                        r = true;
                    }
                }
            }
            return r;
        }

        public static void CreateOrUpdateRemoteHostSession
                                    (
                                        SessionContextEntry newSessionContextEntry
                                        , out Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> currentSession
                                        , out int addedRemoteSessions
                                        , out int removedLocalSessions
                                    )
        {
            int i = 0;
            int j = 0;
            i++;
            newSessionContextEntry.IsLocalSession = false;
            Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> session = null;
            var sessionID = newSessionContextEntry.SessionID;
            var newSession = Tuple.Create<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                                            (
                                                newSessionContextEntry
                                                , null
                                            );
            var handler = _sessions
                                .AddOrUpdate
                                    (
                                        sessionID
                                        , newSession
                                        , (x, y) => //updateValueFactory
                                        {
                                            var r = newSession;
                                            session = y;
                                            var sessionContextEntry = y.Item1;
                                            if (sessionContextEntry.IsLocalSession)
                                            {
                                                j++;
                                            }
                                            else
                                            {
                                                i--;
                                            }
                                            return r;
                                        }
                                    );
            currentSession = session;
            addedRemoteSessions = i;
            removedLocalSessions = j;
        }

        /// <summary>
        /// 创建用户Session
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="sessionID"></param>
        /// <param name="addedUsers"></param>
        /// <param name="addedConnections"></param>
        public static Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                            CreateOrUpdateLocalHostSession
                                    (
                                        Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> newSession
                                        , out SessionContextInfo replacedSessionContextInfo
                                        , out int addedLocalSessions
                                        , out int removedRemoteSessions
                                    )
        {
            int i = 0;
            int j = 0;
            i ++;
            replacedSessionContextInfo = null;
            SessionContextInfo sessionContextInfo = null;
            Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> r = null;
            var newSessionContextEntry = newSession.Item1;
            r = _sessions
                    .AddOrUpdate
                        (
                            newSessionContextEntry.SessionID
                            , newSession
                            , (x, y) => //updateValueFactory
                                {
                                    Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> rr = y;
                                    var currentSessionContextEntry = y.Item1;
                                    if (currentSessionContextEntry.IsLocalSession)
                                    {
                                        //本地会话
                                        i--;
                                        if (!currentSessionContextEntry.SessionEquals(newSessionContextEntry))
                                        {
                                            currentSessionContextEntry.LastUpdateTime = DateTime.Now;
                                            currentSessionContextEntry.LastClientHeartBeatTime = DateTime.Now;
                                            //复制原IPEP/PORT备用
                                            var ipEndPoint = new IPEndPoint
                                                                (
                                                                    currentSessionContextEntry
                                                                        .OwnerRemoteIPEndPoint
                                                                        .Address
                                                                    , currentSessionContextEntry
                                                                        .OwnerRemoteIPEndPoint
                                                                        .Port
                                                                );
                                            sessionContextInfo = new SessionContextInfo(currentSessionContextEntry);
                                            currentSessionContextEntry.OwnerClientGuid
                                                    = newSessionContextEntry.OwnerClientGuid.Value;
                                            currentSessionContextEntry.OwnerRemoteIPEndPoint
                                                    = newSessionContextEntry.OwnerRemoteIPEndPoint;
                                        }
                                    }
                                    else
                                    {
                                        //远程会话
                                        j++;
                                        rr = newSession;
                                    }
                                    return rr;
                                }
                        );
            addedLocalSessions = i;
            removedRemoteSessions = j;
            replacedSessionContextInfo = sessionContextInfo;
            return r;
        }

        //2014-10-08 重构备份
        public static Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                    CreateOrUpdateLocalHostSession_OLD
                            (
                                Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> newSession
                                , out Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> replacedCurrentSession
                                , out int addedLocalSessions
                                , out int removedRemoteSessions
                            )
        {
            int i = 0;
            int j = 0;
            i++;

            var newSessionContextEntry = newSession.Item1;
            replacedCurrentSession = null;
            Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> currentSession = null;
            newSession = _sessions
                            .AddOrUpdate
                                (
                                    newSessionContextEntry.SessionID
                                    , newSession
                                    , (x, y) => //updateValueFactory
                                    {
                                        Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> r = y;
                                        var currentSessionContextEntry = y.Item1;
                                        if (currentSessionContextEntry != newSessionContextEntry)
                                        {
                                            currentSession = y;
                                            r = newSession;
                                        }
                                        if (currentSessionContextEntry.IsLocalSession)
                                        {
                                            i--;
                                        }
                                        else
                                        {
                                            j++;
                                        }
                                        return r;
                                    }
                                );
            addedLocalSessions = i;
            removedRemoteSessions = j;
            replacedCurrentSession = currentSession;
            return newSession;
        }

        public static bool TryRemoveRemoteSessionBySessionID
                  (
                      string sessionID
                      , out Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>>
                                removedSession
                      , out int removedRemoteSessions
                  )
        {
            var r = false;
            removedSession = null;
            removedRemoteSessions = 0;
            Tuple<SessionContextEntry,SocketAsyncDataHandler<SessionContextEntry>> currentSession = null;
            if
                (
                    _sessions
                        .TryGetValue
                            (
                                sessionID
                                , out currentSession
                            )
                )
            {
                if (!currentSession.Item1.IsLocalSession)
                {
                    if
                        (
                            _sessions
                                .TryRemove
                                    (
                                        sessionID
                                        , out currentSession
                                    )
                        )
                    {
                        removedRemoteSessions++;
                        r = true;
                    }
                }
            }
            return r;
        }

        public static bool TryRemoveSessionBySessionID
                  (
                      string sessionID
                      , out Tuple
                                <
                                    SessionContextEntry
                                    , SocketAsyncDataHandler<SessionContextEntry>
                                > removedSession
                      , out int removedLocalSessions
                      , out int removedRemoteSessions
                  )
        {
            var r = false;
            removedSession = null;
            removedLocalSessions = 0;
            removedRemoteSessions = 0;
            r = _sessions
                    .TryRemove
                        (
                            sessionID
                            , out removedSession
                        );
            if (r)
            {
                if
                    (
                        removedSession
                            .Item1
                            .IsLocalSession
                    )
                {
                    removedLocalSessions = 1;
                }
                else
                {
                    removedRemoteSessions = 1;
                }
            }
            return r;
        }
        public static bool TryRemoveSession
                  (
                      Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> removeSession
                      , out Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> removedSession
                      , out int removedLocalSessions
                      , out int removedRemoteSessions
                  )
        {
            var r = false;
            int i = 0;
            int j = 0;
            removedSession = null;
            var removeSessionContextEntry = removeSession.Item1;
            var sessionID = removeSessionContextEntry.SessionID;
            Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> currentSession = null;
            if
                (
                    _sessions
                        .TryGetValue
                            (
                                sessionID
                                , out currentSession
                            )
                )
            {
                var currentSessionContextEntry = currentSession.Item1;
                if (currentSessionContextEntry.OwnershipEquals(removeSessionContextEntry))
                {
                    if
                        (
                            _sessions
                                .TryRemove
                                    (
                                        sessionID
                                        , out currentSession
                                    )
                        )
                    {
                        if (currentSessionContextEntry.IsLocalSession)
                        {
                            i++;
                        }
                        else
                        {
                            j++;
                        }
                        removedSession = currentSession;
                        r = true;
                    }
                }
            }
            removedLocalSessions = i;
            removedRemoteSessions = j;
            return r;
        }
        /// <summary>
        /// 移除超时的本地会话
        /// </summary>
        public static void RemoveTimeOutLocalSessions
                            (
                                out int removedLocalSessions
                                , out int removedRemoteSessions
                            )
        {
            removedLocalSessions = 0;
            removedRemoteSessions = 0;
            //2014-08-16 提高掉线标准判断的准确度
            DateTime nowBase = DateTime.Now;
            var predicate =
                    new Predicate<SessionContextEntry>
                            (
                                (x) =>
                                    {
                                        bool r = false;
                                        if (x.IsLocalSession)
                                        {
                                            var lastClientHeartBeatTime = x.LastClientHeartBeatTime;
                                            if (lastClientHeartBeatTime != null)
                                            {
                                                r =
                                                    (
                                                        (
                                                            DateTimeHelper
                                                                .SecondsDiff
                                                                        (
                                                                            lastClientHeartBeatTime.Value
                                                                            , nowBase
                                                                        )
                                                        )
                                                        >
                                                        ConfigurationAppSettingsManager
                                                            .RunTimeAppSettings
                                                            .SessionOfflineMinTimeOutInSeconds
                                                    );
                                            }
                                        }
                                        return r;
                                    }
                            );
            var removedSessions
                   = SessionsManager
                        .Sessions
                        .Where
                            (
                                (x) =>
                                {
                                    return
                                        predicate(x.Value.Item1);
                                }
                            )
                        .Select
                            (
                                (x) =>
                                {
                                    return x.Key;
                                }
                            )
                        .ToList();

            Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> removedSession = null;
            foreach (var x in removedSessions)
             {
                 if
                     (
                         SessionsManager
                            .Sessions
                            .TryRemove
                                (
                                    x
                                    , out removedSession
                                )
                     )
                 {
                     if (removedSession.Item1.IsLocalSession)
                     {
                         removedLocalSessions++;
                         //2014-09-17
                         //=========================================================
                         var removedSessionContextEntry = removedSession.Item1;
                         ForwardSession forwardSession = new ForwardSession()
                         {
                             SenderClientGuid = removedSessionContextEntry.OwnerClientGuid
                             , SyncAction = "Kickout"
                             , Session = removedSessionContextEntry
                         };
                         //2015-08-24 AsyncQueueProcessor Bug
                         AsyncQueuesProcessorsManager
                            .SessionsSyncToOtherServersAsyncQueueProcessor
                            .InternalQueue
                            .Enqueue
                               (
                                    forwardSession
                               );
                         //=========================================================
                     }
                     else
                     {
                         removedRemoteSessions++;
                     }
                 }
             }
        }
        /// <summary>
        /// 向Client发送心跳包
        /// </summary>
        /// 
        private static void ServerHeartBeatsToClients()
        {
            var enabledSendedMessagesPerformanceCounters = MultiPerformanceCountersTypeFlags.None;
            if 
                (
                    ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .EnableCountPerformance
                )
            {
                enabledSendedMessagesPerformanceCounters =
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .EnabledSendedMessagesPerformanceCounters;
            }

            SessionsManager
                .Sessions
                .Where
                    (
                        (x) =>
                        {
                            var sessionContextEntry = x.Value.Item1;
                            var r = false;
                            if (sessionContextEntry.IsLocalSession)
                            {
                                if
                                    (
                                        DateTimeHelper
                                            .SecondsDiffNow
                                                (
                                                    sessionContextEntry
                                                        .LastClientHeartBeatTime
                                                        .Value
                                                )
                                        >
                                        ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .HeartBeatMaxTimeOutInSeconds
                                    )
                                {
                                    r = true;
                                }
                            }
                            return r;
                        }
                    )
                    .AsParallel()
                    .WithDegreeOfParallelism
                        (
                            //Environment.ProcessorCount
                            ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .ServerToClientHeartBeatsSendingParallelismDegree
                        )
                    .ForAll
                        (
                            (x) =>
                            {
                                var sessionContextEntry = x.Value.Item1;
                                if (sessionContextEntry.IsLocalSession)
                                {
                                    var heartBeatRequest = new HeartBeatRequest();
                                    heartBeatRequest
                                        .Header
                                        .Receivers
                                            = new Party[]
                                                {
                                                    sessionContextEntry.Owner
                                                };
                                    heartBeatRequest
                                        .Header
                                        .SendTimeStamp = DateTime.Now;
                                    //S to C 心跳统一公共ID
                                    heartBeatRequest
                                        .Header
                                        .ID = -10000;

                                    var json = JsonHelper.Serialize(heartBeatRequest);
                                    var buffer = Encoding.UTF8.GetBytes(json);
                                    var socketAsyncDataHandler = x.Value.Item2;
                                    socketAsyncDataHandler
                                        .SendDataToSyncWithPerformaceCounter//<SessionContextEntry>
                                            (
                                                sessionContextEntry
                                                    .OwnerRemoteIPEndPoint
                                                , buffer
                                                , enabledSendedMessagesPerformanceCounters
                                                , ConfigurationAppSettingsManager
                                                        .RunTimeAppSettings
                                                        .NeedThrottleControlSendHeartBeatsRequestOrResponse
                                            );
                                }
                            }
                        );
        }

        public static void SendKickRequestAsync
                    (
                        string senderClientGuid
                        , SessionContextInfo targetSessionContextInfo
                        , SocketAsyncDataHandler<SessionContextEntry> targetConnection 
                        , bool forcedKick = false
                    )
        {
            //var currentSessionContextEntry = kickCurrentSession.Item1;
            //if (currentSessionContextEntry.IsLocalSession)
            {
                var socketAsyncDataHandler = targetConnection;
                var remoteIPEndPoint = targetSessionContextInfo
                                            .OwnerRemoteIPEndPoint;
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
                KickRequest kickRequest = new KickRequest()
                {
                    Body = new KickRequestBody()
                    {
                        Force = (forcedKick ? 1 : 0)
                        ,
                        Kicked = new Party[] 
                            {
                                targetSessionContextInfo.Owner
                            }
                        ,
                        ClientGuid = senderClientGuid
                    }
                };
                var json = JsonHelper.Serialize(kickRequest);
                var buffer = Encoding.UTF8.GetBytes(json);
                int times = ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .UdpSendKickRequestToClientTransmissionsTimes;

                //int sleep = ConfigurationAppSettingsManager
                //                .AppSettings
                //                .UdpSendSleepInMilliseconds;
                ////var x = WindowsServiceHost.SocketServer.SocketAsyncEventArgsPool;
                //var sendSocketAsyncEventArgs = new SocketAsyncEventArgs()
                //{
                //    RemoteEndPoint = remoteIPEndPoint
                //};
                //sendSocketAsyncEventArgs.SetBuffer(buffer, 0, buffer.Length);

                for (int i = 0; i < times; i++)
                {
                    //socketAsyncDataHandler
                    //    .SendDataToAsyncWithPerformaceCounter<SessionContextEntry>
                    //        (
                    //            //remoteIPEndPoint
                    //            //, buffer
                    //            sendSocketAsyncEventArgs
                    //            , MultiPerformanceCountersTypeFlags.None  //enabledSendedMessagesPerformanceCounters
                    //        );
                    socketAsyncDataHandler
                        .SendDataToSyncWithPerformaceCounter//<SessionContextEntry>
                            (
                                remoteIPEndPoint
                                , buffer
                                  //sendSocketAsyncEventArgs
                                , enabledSendedMessagesPerformanceCounters
                            );
                    // 2015-02-10 SendKick 要快注释掉 
                    //if (sleep > 0)
                    //{
                    //    Thread.Sleep(sleep);
                    //}
                }
            }
        }

        //2014-10-08 重构备份
        public static void SendKickRequest_OLD
            (
                string senderClientGuid
                , Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> targetSession
                , bool forcedKick = false
            )
        {
            var currentSessionContextEntry = targetSession.Item1;
            if (currentSessionContextEntry.IsLocalSession)
            {
                var socketAsyncDataHandler = targetSession.Item2;
                var remoteIPEndPoint = currentSessionContextEntry.OwnerRemoteIPEndPoint;
                var enabledSendedMessagesPerformanceCounters = MultiPerformanceCountersTypeFlags.None;
                if (ConfigurationAppSettingsManager.RunTimeAppSettings.EnableCountPerformance)
                {
                    enabledSendedMessagesPerformanceCounters
                            = ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .EnabledSendedMessagesPerformanceCounters;
                }
                KickRequest kickRequest = new KickRequest()
                {
                    Body = new KickRequestBody()
                    {
                        Force = (forcedKick ? 1 : 0)
                        ,
                        Kicked = new Party[] 
                            {
                                currentSessionContextEntry.Owner
                            }
                        ,
                        ClientGuid = senderClientGuid
                    }
                };
                var json = JsonHelper.Serialize(kickRequest);
                var buffer = Encoding.UTF8.GetBytes(json);
                for (int i = 0; i < 5; i++)
                {
                    socketAsyncDataHandler
                        .SendDataToSyncWithPerformaceCounter//<SessionContextEntry>
                            (
                                remoteIPEndPoint
                                , buffer
                                , enabledSendedMessagesPerformanceCounters
                            );
                    //Thread.Sleep(2 * 1000);
                }
            }
        }
        //public static void RemoveRemoteSessionsBySessionsIDs(IEnumerable<string> sessionsIDs)
        //{
        //    Array
        //        .ForEach
        //            (
        //                sessionsIDs.ToArray()
        //                , (x) =>
        //                {
        //                    //int removedLocalSessions = 0;
        //                    RemoveOneRemoteSessionBySessionID(x);
        //                }
        //            );
        //}

        public static void RemoveOneRemoteSessionBySessionID(string sessionID)
        {
            int removedRemoteSessions = 0;
            Tuple<SessionContextEntry, SocketAsyncDataHandler<SessionContextEntry>> removedSession = null;
            if
                (
                    SessionsManager
                        .TryRemoveRemoteSessionBySessionID
                            (
                                sessionID.ToLower().Trim()
                                , out removedSession
                                , out removedRemoteSessions
                            )
                )
            {
                //var sessionContextEntry = removedSession.Item1;
                //if (sessionContextEntry.IsLocalSession)
                {
                    //应该不会走到本分支
                    //var socketAsyncDataHandler = removedSession.Item2;
                    //var receiveSocketAsyncEventArgs = socketAsyncDataHandler.ReceiveSocketAsyncEventArgs;
                    ////SocketServer
                    ////SocketAsyncEventArgs 还回池子
                    //WindowsServiceHost
                    //    .WorkingSocketServer
                    //        .WorkingSocketAsyncEventArgsPool
                    //            .Push(receiveSocketAsyncEventArgs);
                    //socketAsyncDataHandler.DestoryWorkingSocket();
                }
                SessionsManager
                    .SessionsPerformanceCountIncrement
                        (
                            -1 * removedRemoteSessions
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .SessionsPerformanceCountersCategoryName
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .SessionsPerformanceCountersCategoryInstanceRemoteSessions
                        );
                //sessionContextEntry = null;
            }
        }
    }
}
#endif