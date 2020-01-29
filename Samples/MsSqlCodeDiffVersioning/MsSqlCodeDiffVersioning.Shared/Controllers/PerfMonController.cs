namespace Microshaoft.WebApi.Controllers
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.ModelBinders;
    using Microsoft.AspNetCore.Mvc;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;

    [Route("api/[controller]")]
    [ApiController]
    public class PerfMonController : ControllerBase
    {
        private readonly ConcurrentDictionary<string, ExecutingInfo>
                                        _executingCachingStore;

        private readonly SingleThreadAsyncDequeueProcessorSlim
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
                                        _asyncRequestResponseLoggingProcessor;
        public PerfMonController
                        (
                            ConcurrentDictionary<string, ExecutingInfo>
                                        executingCachingStore
                            ,
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
                                        asyncRequestResponseLoggingProcessor
                        )
        {
            _executingCachingStore = executingCachingStore;
            _asyncRequestResponseLoggingProcessor = asyncRequestResponseLoggingProcessor;
        }

        [HttpGet]
        [Route("ExecutingCachingStore")]
        public ActionResult<ConcurrentDictionary<string, ExecutingInfo>>
                        ExecutingCachingStore
                            (
                    
                            )
        {
            return
                _executingCachingStore;
        }

        [HttpGet]
        [Route("RequestResponseLoggingProcessor")]
        public ActionResult
                    <
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
                    >

                       RequestResponseLoggingProcessor
                           (

                           )
        {
            return
                _asyncRequestResponseLoggingProcessor;
                  
                
        }


    }
}