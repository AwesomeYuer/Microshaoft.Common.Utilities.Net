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
    public static partial class AsyncQueuesProcessorsManager
    {
        #region 发送通知队列
        public static SingleThreadAsyncDequeueProcessor<ForwardSession> SessionsSyncToOtherServersAsyncQueueProcessor
            = new SingleThreadAsyncDequeueProcessor<ForwardSession>();

        //2015-08-24 AsyncQueueProcessor Bug
        //public static ConcurrentQueue<ForwardSession> SessionsSyncToOtherServersAsyncQueue //user, bytes
        //    = new ConcurrentQueue<ForwardSession>();

        public static void RunSessionsSyncToOtherServersAsyncQueueProcess()
        {
            SessionsSyncToOtherServersAsyncQueueProcessor
                    .StartRunDequeuesThreadProcess
                        (
                            null
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .SessionsSyncAsyncToOtherServersWaitSleepInMilliseconds
                            , (x, y) =>
                                {
                                    var list = y.Select
                                                    (
                                                        (xx) =>
                                                        {
                                                            return xx.Item2;
                                                        }
                                                    ).ToList();

                                    SessionsManager
                                        .BroadcastParallelBatchCreateOrUpdateSessions
                                            (
                                                list
                                            );
                                }
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .SessionsSyncAsyncToOtherServersWaitBatchTimeoutInSeconds * 1000
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .SessionsSyncAsyncToOtherServersWaitBatchMaxSizeInEntriesCount
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

        public static SingleThreadAsyncDequeueProcessor<ForwardSession[]> SessionsSyncFromOtherServersAsyncQueueProcessor
            = new SingleThreadAsyncDequeueProcessor<ForwardSession[]>();

        #endregion

        #region 接收处理队列
        //2015-08-24 AsyncQueueProcessor Bug
        //public static ConcurrentQueue<ForwardSession[]> SessionsSyncFromOtherServersAsyncQueue //user, bytes
        //    = new ConcurrentQueue<ForwardSession[]>();

        public static void RunSessionsSyncFromOtherServersAsyncQueueProcess()
        {
            SessionsSyncFromOtherServersAsyncQueueProcessor
                    .StartRunDequeuesThreadProcess
                        (
                            (x, y) =>
                            {
                                SessionsBatchUpdateProcess(y);
                            }
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .SessionsSyncAsyncFromOtherServersWaitSleepInMilliseconds
                            , null
                            , 1 * 1000  //因为 OnOnceProcess, 其实没用
                                //, ConfigurationAppSettingsManager
                                //    .RunTimeAppSettings
                                //    .SessionsSyncAsyncFromOtherServersWaitBatchTimeoutInSeconds
                                //        * 1000
                            , 10  //因为 OnOnceProcess, 其实没用
                                //, ConfigurationAppSettingsManager
                                //    .RunTimeAppSettings
                                //    .SessionsSyncAsyncFromOtherServersWaitBatchMaxSizeInEntriesCount
                            , (x, y) =>
                            {
                                string s = string
                                                .Format
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

        private static void SessionsBatchUpdateProcess
                (
                    ForwardSession[] forwardSessions
                )
        {
            foreach (ForwardSession session in forwardSessions)
            {
                SessionUpdateProcess(session);
            }
        }

        private static void SessionUpdateProcess
                                    (
                                        ForwardSession forwardSession
                                    )
        {
            //2014-08-16
            var senderClientGuid = forwardSession.SenderClientGuid.Value.ToString("N");
            var action = forwardSession.SyncAction.Trim().ToLower();
            var sessionContextEntry = forwardSession.Session;
            SessionsManager
                .DebugLog
                    (
                        "CrossServerReceive"
                        , () =>
                        {
                            var log = string
                                        .Format
                                            (
                                                "CrossServer received SessionController Post (new) by [{0}] for [{1}] @ [{2}]"
                                                , senderClientGuid
                                                , sessionContextEntry.SessionID
                                                , DateTime.Now
                                            );
                            return log;
                        }
                    );
            int addedRemoteSessions = 0;
            int removedLocalSessions = 0;

            if (action == "login")
            {
                sessionContextEntry.IsLocalSession = false;
                Tuple
                    <
                        SessionContextEntry
                        , SocketAsyncDataHandler<SessionContextEntry>
                    > currentSession = null;
                SessionsManager
                    .CreateOrUpdateRemoteHostSession
                        (
                            sessionContextEntry
                            , out currentSession
                            , out addedRemoteSessions
                            , out removedLocalSessions
                        );
                SessionsManager
                    .SessionsPerformanceCountIncrement
                        (
                            addedRemoteSessions
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .SessionsPerformanceCountersCategoryName
                            , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .SessionsPerformanceCountersCategoryInstanceRemoteSessions
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
                if (currentSession != null)
                {
                    var currentSessionContextEntry = currentSession.Item1;
                    if (currentSession.Item1.IsLocalSession)
                    {
                        //踢掉老的用户会话先
                        var targetSessionContextInfo = new SessionContextInfo(currentSessionContextEntry);
                        var targetSocketDataHandler = currentSession.Item2;
                        SessionsManager
                            .SendKickRequestAsync
                                (
                                    senderClientGuid
                                    , targetSessionContextInfo
                                    , targetSocketDataHandler
                                    , true
                                );
                    }
                }
            }
            else if 
                (
                    action == "logout"
                    ||
                    action == "kickout"     //2014-09-17
                )
            {
                SessionDeleteProcess
                    (
                         senderClientGuid
                         , sessionContextEntry.SessionID
                    );
            }
        }

        private static void SessionDeleteProcess
                        (
                            string senderClientGuid
                            , string sessionID
                        )
        {
            SessionsManager
                .DebugLog
                    (
                        "CrossServerReceive"
                        , () =>
                        {
                            var log = string
                                        .Format
                                            (
                                                "CrossServer received SessionController Delete by [{0}] for session: [{1}] @ [{2}]"
                                                , senderClientGuid
                                                , sessionID
                                                , DateTime.Now
                                            );
                            return log;
                        }
                    );
            //由其他机器 Logout 发起通知到本服务器的仅删除远程会话
            SessionsManager
                .RemoveOneRemoteSessionBySessionID
                    (
                        sessionID
                    );
        } 
        #endregion
    }
}
#endif