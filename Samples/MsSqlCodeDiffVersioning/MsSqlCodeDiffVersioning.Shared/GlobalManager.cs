using Microshaoft;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace WebApplication.ASPNetCore
{
    public static class GlobalManager
    {
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
                            AsyncRequestResponseLoggingProcessor = 
                                new SingleThreadAsyncDequeueProcessorSlim
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
                                        >();


        public static readonly
            ConcurrentDictionary<string, ExecutingInfo>
                ExecutingCachingStore =
                        new ConcurrentDictionary<string, ExecutingInfo>
                                (StringComparer.OrdinalIgnoreCase);



    }
}
