namespace WebApplication.ASPNetCore
{
    using Microshaoft;
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
                                , double? requestResponseTimingInMilliseconds

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
                                                , double? requestResponseTimingInMilliseconds
                                                //,
                                                //    (
                                                //        string osPlatformName
                                                //        , string osVersion
                                                //        , string frameworkDescription
                                                //        , string machineHostName
                                                //    ) ServerHost
                                                //,
                                                //    (
                                                //        int processId
                                                //        , DateTime? processStartTime
                                                //        , string processName
                                                //    ) Process
                                            )
                                        >();



        public static readonly
            ConcurrentDictionary<string, ExecutingInfo>
                ExecutingCachingStore =
                        new ConcurrentDictionary<string, ExecutingInfo>
                                (StringComparer.OrdinalIgnoreCase);



    }
}
