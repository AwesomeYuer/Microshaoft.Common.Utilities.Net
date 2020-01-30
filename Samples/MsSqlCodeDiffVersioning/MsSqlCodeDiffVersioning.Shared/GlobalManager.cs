using Microshaoft;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace WebApplication.ASPNetCore
{
    public static class GlobalManager
    {
        public static readonly string OsPlatformName =
                                        EnumerableHelper
                                                    .Range
                                                        (
                                                            OSPlatform.Linux
                                                            , OSPlatform.OSX
                                                            , OSPlatform.Windows
                                                        )
                                                    .First
                                                        (
                                                            (x) =>
                                                            {
                                                                return
                                                                    RuntimeInformation
                                                                            .IsOSPlatform(x);
                                                            }
                                                        )
                                                    .ToString();

        public static readonly string OsVersion = Environment
                                                        .OSVersion
                                                        .VersionString;


        public static readonly string FrameworkDescription = RuntimeInformation
                                                                    .FrameworkDescription;

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
                                ,
                                    (
                                        string osPlatformName
                                        , string osVersion
                                        , string frameworkDescription
                                    ) ServerHost
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
                                                ,
                                                    (
                                                        string osPlatformName
                                                        , string osVersion
                                                        , string frameworkDescription
                                                    ) ServerHost
                                            )
                                        >();



        public static readonly
            ConcurrentDictionary<string, ExecutingInfo>
                ExecutingCachingStore =
                        new ConcurrentDictionary<string, ExecutingInfo>
                                (StringComparer.OrdinalIgnoreCase);



    }
}
