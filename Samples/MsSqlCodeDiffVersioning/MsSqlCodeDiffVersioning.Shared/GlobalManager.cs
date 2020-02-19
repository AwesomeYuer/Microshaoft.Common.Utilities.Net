namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.IdentityModel.Tokens;
    using System;
    using System.Collections.Concurrent;
    using System.Diagnostics;
    using System.Linq;
    using System.Runtime.InteropServices;
    using System.Text;
    using SingleThreadAsyncDequeueLoggingProcessor
            = Microshaoft
                    .SingleThreadAsyncDequeueProcessorSlim
                        <
                            (
                                    (
                                          string requestUrl
                                        , string requestPath
                                        , string requestPathBase
                                        , string requestActionRoutePath
                                    ) Url
                                ,
                                    (
                                          string requestHeaders
                                        , string requestBody
                                        , string requestMethod
                                        , System.DateTime? requestBeginTime
                                        , long? requestContentLength
                                        , string requestContentType
                                    ) Request
                                ,
                                    (
                                          string responseHeaders
                                        , string responseBody
                                        , int responseStatusCode
                                        , System.DateTime? responseStartingTime
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
                        >;
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

        public static LogLevel RequestResponseLoggingLogLevel = 
                            Enum
                                .Parse<LogLevel>
                                    (
                                        ConfigurationHelper
                                                    .Configuration
                                                    .GetValue
                                                        (
                                                            "RequestResponseLoggingLogLevel"
                                                            , nameof(LogLevel.Trace)
                                                        )
                                        , true
                                    );

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

        public static readonly SymmetricSecurityKey jwtSymmetricSecurityKey = new SymmetricSecurityKey
                                        (
                                            Encoding
                                                    .UTF8
                                                    .GetBytes
                                                        (
                                                            ConfigurationHelper.Configuration.GetValue<string>("SecretKey")
                                                        )
                                        );
        public static readonly SigningCredentials jwtSigningCredentials = new SigningCredentials
                                (
                                    jwtSymmetricSecurityKey
                                    , SecurityAlgorithms.HmacSha256Signature
                                    , SecurityAlgorithms.Sha256Digest
                                );


        public static readonly Process CurrentProcess;
        public static readonly DateTime ProcessAlignedSecondsStartTime;
        public static readonly string OsPlatformName;
        public static readonly string OsVersion = Environment
                                                        .OSVersion
                                                        .VersionString?? $"Unknown {nameof(OsVersion)}:[{ProcessAlignedSecondsStartTime:yyyy-MM-dd HH:mm:ss}]";


        public static readonly string FrameworkDescription = RuntimeInformation
                                                                    .FrameworkDescription?? $"Unknown {nameof(FrameworkDescription)}:[{ProcessAlignedSecondsStartTime:yyyy-MM-dd HH:mm:ss}]";

        public static readonly
                    SingleThreadAsyncDequeueLoggingProcessor
                            AsyncRequestResponseLoggingProcessor
                                = new SingleThreadAsyncDequeueLoggingProcessor();


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
            var reThrow = false;
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
                reThrow;
        }

    }
}
