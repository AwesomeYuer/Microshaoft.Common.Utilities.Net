//#if NET45
namespace Microshaoft
{
    using System;
    using System.Net;
    using System.Net.Sockets;
    using System.Threading;
    using System.Diagnostics;
    using System.Text;
    //using Microsoft.Boc.Communication.Configurations;
    public partial class SocketAsyncDataHandler<T> //ExtensionsMethodsManager
    {
        public void SendDataToSyncWithPerformaceCounter
                        (
                            //this SocketAsyncDataHandler<T> socketAsyncDataHandler
                            IPEndPoint remoteIPEndPoint
                            , byte[] buffer
                            , MultiPerformanceCountersTypeFlags enabledSendedMessagesPerformanceCounters
                            , bool needThrottleControl = true
                        )
        {
            if (needThrottleControl)
            {
                ConcurrentSendMessagesControlSleep();
            }
            
            EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                .CountPerformance
                    (
                        enabledSendedMessagesPerformanceCounters
                        , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .MessagesPerformanceCountersCategoryName
                        , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .MessagesPerformanceCountersCategoryInstanceServerSendedToClientMessagesCount
                        , null
                        , () =>
                        {
                            SendDataToSync
                                        (
                                            buffer
                                            , remoteIPEndPoint
                                        );
                            if 
                                (
                                    ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .EnableDebugLog
                                    //||
                                    //ConfigurationAppSettingsManager
                                    //    .RunTimeAppSettings
                                    //    .EnableDebugConsoleOutput
                                )
                            {
                                var s = Encoding.UTF8.GetString(buffer);
                                var log = string.Format
                                                    (
                                                        "ToRemoteIPEndPoint: {1}{2}{0}SocketID: {1}{3}{0}SendedData: {1}{4}"
                                                        , "\r\n"
                                                        , "\r\n\t"
                                                        , remoteIPEndPoint.ToString()
                                                        , SocketID
                                                        , s
                                                    );
                                //if
                                //    (
                                //        ConfigurationAppSettingsManager
                                //            .RunTimeAppSettings
                                //            .EnableDebugConsoleOutput
                                //    )
                                //{
                                //    Console.WriteLine(log);
                                //}

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
                        }
                        , null
                        , (xx) =>
                        {
                            //Exception
                            Console.WriteLine("Caught Exception: {0}", xx.ToString());
                            //to do: Exception Log
                            EventLogHelper
                                .WriteEventLogEntry
                                    (
                                        ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .EventLogSourceName
                                        , xx.ToString()
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
        public void SendDataToAsyncWithPerformaceCounter//<T>
                (
                    //this SocketAsyncDataHandler<T> socketAsyncDataHandler
                    //, IPEndPoint remoteIPEndPoint
                    //, byte[] buffer
                    SocketAsyncEventArgs sendSocketAsyncEventArgs
                    , MultiPerformanceCountersTypeFlags enabledSendedMessagesPerformanceCounters
                )
        {
            ConcurrentSendMessagesControlSleep();
            var buffer = sendSocketAsyncEventArgs.Buffer;
            EasyPerformanceCountersHelper<MessagesPerformanceCountersContainer>
                .CountPerformance
                    (
                        enabledSendedMessagesPerformanceCounters
                        , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .MessagesPerformanceCountersCategoryName
                        , ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .MessagesPerformanceCountersCategoryInstanceServerSendedToClientMessagesCount
                        , null
                        , () =>
                        {
                            //socketAsyncDataHandler
                                SendDataToAsync
                                        (
                                            sendSocketAsyncEventArgs
                                        );
                            if (ConfigurationAppSettingsManager.RunTimeAppSettings.EnableDebugLog)
                            {
                                var s = Encoding.UTF8.GetString(buffer);
                                var log = string.Format
                                                    (
                                                        "ToRemoteIPEndPoint: {1}{2}{0}SocketID: {1}{3}{0}SendedData: {1}{4}"
                                                        , "\r\n"
                                                        , "\r\n\t"
                                                        , sendSocketAsyncEventArgs.RemoteEndPoint.ToString()
                                                        , SocketID
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
                        }
                        , null
                        , (xx) =>
                        {
                            //Exception
                            Console.WriteLine("Caught Exception: {0}", xx.ToString());
                            //to do: Exception Log
                            EventLogHelper
                                .WriteEventLogEntry
                                    (
                                        ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .EventLogSourceName
                                        , xx.ToString()
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
        private static void ConcurrentSendMessagesControlSleep()
        {
            #region 流量性能计数器控制
            if
              (
                  ConfigurationAppSettingsManager
                      .RunTimeAppSettings
                      .ConcurrentSendMessagesPerSecondControlThrottle
                  > 0
                  &&
                  ConfigurationAppSettingsManager
                      .RunTimeAppSettings
                      .ConcurrentSendMessagesControlSleepInMilliseconds
                  > 0
                  &&
                  ConfigurationAppSettingsManager
                      .RunTimeAppSettings
                      .ConcurrentSendMessagesControlSleepsMaxTimes
                  > 0
              )
            {
                //MessagesPerformanceCountersContainer
                int i = 0;
                while
                    (
                        i
                        <
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .ConcurrentSendMessagesControlSleepsMaxTimes
                    )
                {
                    var v =
                        DataManager
                            .RecentLastValueOfServerSendedToClientMessagesPerformanceCountersRateOfCountsPerSecond;

                    if
                        (
                            v
                            >
                            ConfigurationAppSettingsManager
                                .RunTimeAppSettings
                                .ConcurrentSendMessagesPerSecondControlThrottle
                        )
                    {
                        if
                            (
                                ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .EnableDebugConsoleOutput
                            )
                        {
                            var log = string.Format
                                //Console.WriteLine
                                        (
                                            "当前服务器下行客户端消息并发[{0}(笔/秒)],超过限制[{1}(笔/秒)]! @ {2}"
                                            , v
                                            , ConfigurationAppSettingsManager
                                                .RunTimeAppSettings
                                                .ConcurrentSendMessagesPerSecondControlThrottle
                                            , DateTime.Now
                                        );
                            Console.WriteLine(log);
                            if (i > 1)
                            {
                                EventLogHelper.WriteEventLogEntry
                                    (
                                        ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .EventLogSourceName
                                        , log
                                        , EventLogEntryType.Warning
                                        , 10002
                                        , 0
                                    );
                            }
                        }
                        Thread.Sleep
                                (
                                    ConfigurationAppSettingsManager
                                        .RunTimeAppSettings
                                        .ConcurrentSendMessagesControlSleepInMilliseconds
                                );
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
            }
            #endregion

            #region 流量控制 时间对齐采样备份 2015-01-27
            //else if
            //  (
            //      ConfigurationAppSettingsManager
            //          .RunTimeAppSettings
            //          .ConcurrentSendMessagesPerPerodControlThrottle
            //      > 0
            //      &&
            //      ConfigurationAppSettingsManager
            //          .RunTimeAppSettings
            //          .ConcurrentSendMessagesPerPerodControlSamplingPerodInSeconds
            //      > 0
            //      &&
            //      ConfigurationAppSettingsManager
            //          .RunTimeAppSettings
            //          .ConcurrentSendMessagesPerPerodControlClearExpiredIntervalInSeconds
            //      > 0
            //      &&
            //      ConfigurationAppSettingsManager
            //          .RunTimeAppSettings
            //          .ConcurrentSendMessagesPerPerodControlClearExpiredInSeconds
            //      > 0
            //  )
            //{
            //    //MessagesPerformanceCountersContainer
            //    int i = 0;
            //    while
            //        (
            //            i
            //            <
            //            ConfigurationAppSettingsManager
            //                .RunTimeAppSettings
            //                .ConcurrentSendMessagesControlSleepsMaxTimes
            //        )
            //    {
            //        long sleep = -1;
            //        int current = -1;
            //        if
            //            (
            //                !DataManager
            //                    .ConcurrentSendMessagesPerPerodControlThrottler
            //                    .IncrementBy
            //                        (
            //                            1
            //                            , out current
            //                            , out sleep
            //                        )
            //            )
            //        {
            //            if (sleep > 10)
            //            {
            //                Thread.Sleep((int)sleep);
            //            }
            //            else
            //            {
            //                if (i > 1)
            //                {
            //                    Thread.Sleep(10);
            //                }
            //            }
            //            if
            //                (
            //                    ConfigurationAppSettingsManager
            //                        .RunTimeAppSettings
            //                        .EnableDebugConsoleOutput
            //                )
            //            {
            //                var log = string.Format
            //                    //Console.WriteLine
            //                            (
            //                                "当前服务器下行客户端消息并发[{0}(笔/秒)],超过限制[{1}(笔/秒)]! @ {2}"
            //                                , current
            //                                , ConfigurationAppSettingsManager
            //                                    .RunTimeAppSettings
            //                                    .ConcurrentSendMessagesPerSecondControlThrottle
            //                                , DateTime.Now
            //                            );
            //                Console.WriteLine(log);
            //                if (i > 1)
            //                {
            //                    EventLogHelper.WriteEventLogEntry
            //                        (
            //                            ConfigurationAppSettingsManager
            //                                .RunTimeAppSettings
            //                                .EventLogSourceName
            //                            , log
            //                            , EventLogEntryType.Warning
            //                            , 10002
            //                            , 0
            //                        );
            //                }
            //            }
            //            i++;
            //        }
            //        else
            //        {
            //            break;
            //        }
            //    }
            //}
            #endregion

            #region 流量控制 令牌桶
            else if
              (
                  ConfigurationAppSettingsManager
                      .RunTimeAppSettings
                      .ConcurrentSendMessagesPerPerodControlThrottle
                  > 0
                  &&
                  ConfigurationAppSettingsManager
                      .RunTimeAppSettings
                      .ConcurrentSendMessagesPerPerodControlSamplingPerodInSeconds
                  > 0
              )
            {
                //MessagesPerformanceCountersContainer
                int i = 0;
                var throttler = DataManager
                                    .ConcurrentSendMessagesPerPerodControlThrottler;
                while
                    (
                        //i
                        //<
                        //ConfigurationAppSettingsManager
                        //    .RunTimeAppSettings
                        //    .ConcurrentSendMessagesControlSleepsMaxTimes

                        1 == 1
                    )
                {
                    //long sleep = -1;
                    //int current = -1;
                    TimeSpan waitTimeSpan = TimeSpan.Zero;
                    if
                        (
                            throttler
                                .Strategy
                                .ShouldThrottle(1, out waitTimeSpan)
                        )
                    {
                        //if (sleep > 10)
                        //{
                        //    Thread.Sleep((int)sleep);
                        //}
                        //else
                        //{
                        //    if (i > 1)
                        //    {
                        //        Thread.Sleep(10);
                        //    }
                        //}
                        Thread.Sleep(waitTimeSpan);
                        if
                            (
                                ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .EnableDebugConsoleOutput
                            )
                        {
                            //不一定准
                            var log = string
                                        .Format
                                            (
                                                "当前服务器下行客户端消息并发[{0}(笔/秒)],超过限制[{1}(笔/秒)]! @ {2}"
                                                , throttler
                                                    .Strategy
                                                    .CurrentUsingTokensCount
                                                , ConfigurationAppSettingsManager
                                                    .RunTimeAppSettings
                                                    .ConcurrentSendMessagesPerSecondControlThrottle
                                                , DateTime.Now
                                            );
                            Console.WriteLine(log);
                            if (i > 1)
                            {
                                EventLogHelper.WriteEventLogEntry
                                    (
                                        ConfigurationAppSettingsManager
                                            .RunTimeAppSettings
                                            .EventLogSourceName
                                        , log
                                        , EventLogEntryType.Warning
                                        , 10002
                                        , 0
                                    );
                            }
                        }
                        i++;
                    }
                    else
                    {
                        break;
                    }
                }
                if
                    (
                        ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .EnableDebugConsoleOutput
                    )
                {
                    Console
                        .WriteLine
                            (
                                "Sending TokensBucket Current Using Tokens: [{0}] @ [{1}]"
                                , throttler
                                    .Strategy
                                    .CurrentUsingTokensCount
                                , DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fffff")
                            );
                }
            }
            #endregion
        }
    }
}
//#endif