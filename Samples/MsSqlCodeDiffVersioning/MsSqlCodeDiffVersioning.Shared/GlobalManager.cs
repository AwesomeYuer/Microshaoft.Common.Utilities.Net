namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microshaoft.Extensions.Logging;
    using Microsoft.Extensions.Logging;

    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    public static class GlobalManager
    {
        static GlobalManager()
        {
            CurrentProcess = Process.GetCurrentProcess();
            ProcessAlignedSecondsStartTime = CurrentProcess
                                                    .StartTime
                                                    .GetAlignSecondsDateTime(1);

            var osPlatform = EnumerableHelper
                                .Range
                                    (
                                        OSPlatform.Linux
                                        , OSPlatform.OSX
                                        , OSPlatform.Windows
                                    )
                                .FirstOrDefault
                                    (
                                        (x) =>
                                        {
                                            return
                                                RuntimeInformation
                                                        .IsOSPlatform(x);
                                        }
                                    );
            if (osPlatform != null)
            {
                OsPlatformName = osPlatform.ToString();
            }
            else
            {
                OsPlatformName = $"Unknown {nameof(OsPlatformName)}:[{ProcessAlignedSecondsStartTime:yyyy-MM-dd HH:mm:ss}]";
            }
        }

        public static LogLevel RequestResponseLoggingLogLevel = LogLevel.Trace;

        public static readonly ILoggerFactory GlobalLoggerFactory =
                            LoggerFactory
                                    .Create
                                        (
                                            (loggingBuilder) =>
                                            {
                                                //loggingBuilder
                                                //            .AddConsole1();

                                                loggingBuilder
                                                        .SetMinimumLevel
                                                                (
                                                                    LogLevel
                                                                            .Information
                                                                )
                                                        .AddConsole();

                                                //        ;
                                                //    .AddFilter("Microsoft", LogLevel.Warning)
                                                //    .AddFilter("System", LogLevel.Warning)
                                                //    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                                                //.AddConsole()
                                                //    .AddEventLog()
                                                //;
                                            }
                                        );

        public static readonly ILogger GlobalLogger = GlobalLoggerFactory
                                                                .CreateLogger("Microshaoft.Logger.Category");





        public static readonly Process CurrentProcess;
        public static readonly DateTime ProcessAlignedSecondsStartTime;
        public static readonly string OsPlatformName;
        public static readonly string OsVersion = Environment
                                                        .OSVersion
                                                        .VersionString?? $"Unknown {nameof(OsVersion)}:[{ProcessAlignedSecondsStartTime:yyyy-MM-dd HH:mm:ss}]";


        public static readonly string FrameworkDescription = RuntimeInformation
                                                                    .FrameworkDescription?? $"Unknown {nameof(FrameworkDescription)}:[{ProcessAlignedSecondsStartTime:yyyy-MM-dd HH:mm:ss}]";

        public static readonly
                    SingleThreadAsyncDequeueProcessorSlim
                        <
                            (
                                string url
                                ,
                                    (
                                        string requestHeaders
                                        , string requestBody
                                        , string requestMethod
                                        , DateTime? requestBeginTime
                                        , long? requestContentLength
                                        , string requestContentType
                                    ) Request
                                ,
                                    (
                                        string responseHeaders
                                        , string responseBody
                                        , int responseStatusCode
                                        , DateTime? responseStartingTime
                                        , long? responseContentLength
                                        , string responseContentType
                                    ) Response
                                ,
                                    (
                                        double? requestResponseTimingInMilliseconds
                                        , double? dbExecutingTimingInMilliseconds
                                    ) Timing
                                ,
                                    (
                                        (
                                            string clientIP
                                            , decimal? locationLongitude
                                            , decimal? locationLatitude
                                        ) Location
                                        , string userID
                                        , string roleID
                                        , string orgUnitID
                                        ,
                                        (
                                            string deviceID
                                            , string deviceInfo
                                        ) Device
                                    ) User

                            )
                        >
                            AsyncRequestResponseLoggingProcessor
                                = new SingleThreadAsyncDequeueProcessorSlim
                                        <
                                            (
                                                string url
                                                ,
                                                    (
                                                        string requestHeaders
                                                        , string requestBody
                                                        , string requestMethod
                                                        , DateTime? requestBeginTime
                                                        , long? requestContentLength
                                                        , string requestContentType
                                                    ) Request
                                                ,
                                                    (
                                                        string responseHeaders
                                                        , string responseBody
                                                        , int responseStatusCode
                                                        , DateTime? responseStartingTime
                                                        , long? responseContentLength
                                                        , string responseContentType
                                                    ) Response
                                                ,
                                                    (
                                                        double? requestResponseTimingInMilliseconds
                                                        , double? dbExecutingTimingInMilliseconds
                                                    ) Timing
                                                ,
                                                    (
                                                        (
                                                            string clientIP
                                                            , decimal? locationLongitude
                                                            , decimal? locationLatitude
                                                        ) Location
                                                        , string userID
                                                        , string roleID
                                                        , string organizationUnitID
                                                        , 
                                                        (
                                                            string deviceID
                                                            , string deviceInfo
                                                        ) Device
                                                    ) User
                                            )
                                        >();



        public static readonly
            ConcurrentDictionary<string, ExecutingInfo>
                ExecutingCachingStore =
                        new ConcurrentDictionary<string, ExecutingInfo>
                                (StringComparer.OrdinalIgnoreCase);

        public static bool OnCaughtExceptionProcessFunc
                (
                    ILogger logger
                    , Exception exception
                    , Exception newException
                    , string innerExceptionMessage
                )
        {
            var rethrow = false;
            logger
                .LogOnDemand
                        (
                            LogLevel
                                    .Error
                            , () =>
                            {
                                return
                                    (
                                        new EventId(-1000)
                                        , exception
                                        , innerExceptionMessage
                                        , null
                                    );
                            }
                        );
            return
                rethrow;
        }

    }
}
