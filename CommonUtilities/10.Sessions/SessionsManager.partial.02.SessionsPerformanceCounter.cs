#if NET45
namespace Microsoft.Boc
{
    using Microsoft.Boc.Communication.Configurations;
    public static partial class SessionsManager
    {
        public static void SessionsPerformanceCountIncrement
            (
                int sessions
                , string SessionsPerformanceCountersCategoryName
                , string SessionsPerformanceCountersCategoryInstanceName
            )
        
        {
            if 
                (
                    ConfigurationAppSettingsManager
                        .RunTimeAppSettings
                        .EnableCountPerformance
                    && sessions != 0
                )
            {
                var enabledCounters = ConfigurationAppSettingsManager.RunTimeAppSettings.EnabledSessionsPerformanceCounters;
                if (enabledCounters != MultiPerformanceCountersTypeFlags.None)
                {
                    if (sessions > 0)
                    {
                        for (int i = 0; i < sessions; i++)
                        {
                            EasyPerformanceCountersHelper<SessionsPerformanceCountersContainer>
                                .CountPerformanceBegin
                                    (
                                        enabledCounters
                                        , SessionsPerformanceCountersCategoryName
                                        , SessionsPerformanceCountersCategoryInstanceName
                                    );
                        }
                    }
                    else if (sessions < 0)
                    {
                        for (int i = sessions; i < 0; i++)
                        {
                            EasyPerformanceCountersHelper<SessionsPerformanceCountersContainer>
                                .CountPerformanceEnd
                                    (
                                        enabledCounters
                                        , SessionsPerformanceCountersCategoryName
                                        , SessionsPerformanceCountersCategoryInstanceName
                                        , null
                                    );
                        }
                    }
                }
            }
        }

        //private static void UsersConnectionsPerformanceCountIncrement(int users, int connections)
        //{
        //    if (ConfigurationAppSettingsManager.AppSettings.EnableCountPerformance)
        //    {
        //        var enabledCounters = ConfigurationAppSettingsManager.AppSettings.EnabledSessionsPerformanceCounters;
        //        if (enabledCounters != MultiPerformanceCountersTypeFlags.None)
        //        {
        //            if (users > 0)
        //            {
        //                for (int i = 0; i < users; i++)
        //                {
        //                    EasyPerformanceCountersHelper<SessionsPerformanceCountersContainer>
        //                        .CountPerformanceBegin
        //                            (
        //                                enabledCounters
        //                                , ConfigurationAppSettingsManager.AppSettings.SessionsPerformanceCountersCategoryName
        //                                , ConfigurationAppSettingsManager.AppSettings.SessionsPerformanceCountersCategoryInstance01Name
        //                            );
        //                }
        //            }
        //            else if (users < 0)
        //            {
        //                for (int i = users; i < 0; i++)
        //                {
        //                    EasyPerformanceCountersHelper<SessionsPerformanceCountersContainer>
        //                        .CountPerformanceEnd
        //                            (
        //                                enabledCounters
        //                                , ConfigurationAppSettingsManager.AppSettings.SessionsPerformanceCountersCategoryName
        //                                , ConfigurationAppSettingsManager.AppSettings.SessionsPerformanceCountersCategoryInstance01Name
        //                                , null
        //                            );
        //                }
        //            }
        //            if (connections > 0)
        //            {
        //                for (int i = 0; i < connections; i++)
        //                {
        //                    EasyPerformanceCountersHelper<SessionsPerformanceCountersContainer>
        //                        .CountPerformanceBegin
        //                            (
        //                                enabledCounters
        //                                , ConfigurationAppSettingsManager.AppSettings.SessionsPerformanceCountersCategoryName
        //                                , ConfigurationAppSettingsManager.AppSettings.SessionsPerformanceCountersCategoryInstance02Name
        //                            );
        //                }
        //            }
        //            else if (connections < 0)
        //            {
        //                for (int i = connections; i < 0; i++)
        //                {
        //                    EasyPerformanceCountersHelper<SessionsPerformanceCountersContainer>
        //                        .CountPerformanceEnd
        //                            (
        //                                enabledCounters
        //                                , ConfigurationAppSettingsManager.AppSettings.SessionsPerformanceCountersCategoryName
        //                                , ConfigurationAppSettingsManager.AppSettings.SessionsPerformanceCountersCategoryInstance02Name
        //                                , null
        //                            );
        //                }
        //            }
        //        }
        //    }
        //}
        //public static void RemoveUserConnectionByUserID
        //           (
        //               string userID
        //               , bool isOnly
        //               , out string removedUserID
        //               , out int removedSocketID
        //               , Action<string, SocketAsyncDataHandler<SessionContextEntry>> OnAfterRemovedByUserIDProcessAction = null
        //               , Action<int, SocketAsyncDataHandler<SessionContextEntry>> OnAfterRemovedBySocketIDProcessAction = null
        //           )
        //{
        //    removedUserID = string.Empty;
        //    removedSocketID = 0;
        //    int removedUsers = 0;
        //    int removedConnections = 0;
        //    var socketID = 0;
        //    try
        //    {
        //        SocketAsyncDataHandler<SessionContextEntry> handerByUserID = null;
        //        SocketAsyncDataHandler<SessionContextEntry> handlerBySocketID = null;
        //        //if
        //        //    (
        //        //        SessionsManager.UsersConnections.TryRemove
        //        //                    (
        //        //                        userID
        //        //                        , out handerByUserID
        //        //                     )
        //        //    )
        //        //{
        //        //    removedUserID = userID;
        //        //    removedUsers = 1;
        //        //    if (!isOnly)
        //        //    {
        //        //        socketID = handerByUserID.SocketID;
        //        //        if
        //        //            (
        //        //                SessionsManager.Connections.TryRemove
        //        //                    (
        //        //                        socketID
        //        //                        , out handlerBySocketID
        //        //                    )
        //        //            )
        //        //        {
        //        //            removedSocketID = socketID;
        //        //            removedConnections = 1;
        //        //            if (OnAfterRemovedByUserIDProcessAction != null)
        //        //            {
        //        //                OnAfterRemovedBySocketIDProcessAction(socketID, handlerBySocketID);
        //        //            }
        //        //        }
        //        //    }
        //        //    if (OnAfterRemovedByUserIDProcessAction != null)
        //        //    {
        //        //        OnAfterRemovedByUserIDProcessAction(userID, handlerBySocketID);
        //        //    }
        //        //}
        //    }
        //    catch (Exception e)
        //    {
        //        string m = string.Format
        //            (
        //                "Exception on {0}: {1}, {2}: {3} "
        //                , "RemoveUserConnectionByUserID"
        //                , userID
        //                , "SocketID"
        //                , socketID
        //            );
        //        Console.WriteLine(m);
        //        throw
        //            new Exception
        //                        (
        //                            m
        //                            , e
        //                        );
        //    }
        //    finally
        //    {
        //        #region 在线用户性能计数器 --
        //        //UsersConnectionsPerformanceCountIncrement(-1 * removedUsers, -1 * removedConnections);
        //        #endregion
        //    }
        //}
    }
}
#endif
