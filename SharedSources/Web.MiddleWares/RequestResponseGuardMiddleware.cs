#if NETCOREAPP
namespace Microshaoft.Web
{
    using Microshaoft.WebApi.Controllers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json.Linq;
    //using Microsoft.Extensions.Logging;
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.IO;
    using System.Threading.Tasks;
    using SystemJsonSerializer = System.Text.Json.JsonSerializer;
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


    public class RequestResponseGuardMiddleware<TInjector1, TInjector2, TInjector3, TInjector4>
    //竟然没有接口?
    {
        private readonly RequestDelegate _next;
        private readonly TInjector1 _injector1;
        private readonly TInjector2 _injector2;
        private readonly TInjector3 _injector3;
        private readonly TInjector4 _injector4;


        //private readonly ILogger _logger;

        public RequestResponseGuardMiddleware
                (
                    RequestDelegate next
                    , TInjector1 injector1 = default
                    , TInjector2 injector2 = default
                    , TInjector3 injector3 = default
                    , TInjector4 injector4 = default
                    , Action
                        <RequestResponseGuardMiddleware<TInjector1, TInjector2, TInjector3, TInjector4>>
                            onInitializeCallbackProcesses = default
                    , Func
                        <
                            RequestResponseGuardMiddleware<TInjector1, TInjector2, TInjector3, TInjector4>
                            , HttpContext
                            , Exception
                            , bool
                        >
                            onCaughtExceptionProcessFunc = default
                )
        {
            _next = next;
            //_logger = logger;
            _injector1 = injector1;
            _injector2 = injector2;
            _injector3 = injector3;
            _injector4 = injector4;
            onInitializeCallbackProcesses?
                                    .Invoke(this);
            OnCaughtExceptionProcessFunc = onCaughtExceptionProcessFunc;
        }

        public
            Func<HttpContext, string, TInjector1, TInjector2, TInjector3, TInjector4, bool>
                                        OnFilterProcessFunc;
        public
            Func<HttpContext, string, TInjector1, TInjector2, TInjector3, TInjector4, Task<bool>>
                                        OnInvokingProcessAsync;
        public
            Action<HttpContext, string, TInjector1, TInjector2, TInjector3, TInjector4>
                                        OnResponseStartingProcess;

        public
            Action<HttpContext, string, TInjector1, TInjector2, TInjector3, TInjector4>
                                        OnAfterInvokedNextProcess;
        public
            Action<HttpContext, string, TInjector1, TInjector2, TInjector3, TInjector4>
                                        OnResponseCompletedProcess;

        //public
        //    Action<HttpContext, string, TInjector1, TInjector2, TInjector3, TInjector4>
        //                        OnResponseBodyStreamProcess;

        public
            Func<HttpContext, string, TInjector1, TInjector2, TInjector3, TInjector4, bool>
                                        OnPredicateResponseBodyWorkingStreamProcessFunc;


        public readonly
            Func
                <
                    RequestResponseGuardMiddleware<TInjector1, TInjector2, TInjector3, TInjector4>
                    , HttpContext
                    , Exception
                    , bool
                >
                OnCaughtExceptionProcessFunc;

        //必须是如下方法(竟然不用接口约束产生编译期错误),否则运行时错误
        public async Task Invoke(HttpContext context)
        {
            //throw new Exception();
            var filtered = true;
            bool needNext = true;
            if (OnFilterProcessFunc != null)
            {
                filtered = OnFilterProcessFunc
                                    (
                                        context
                                        , nameof(OnFilterProcessFunc)
                                        , _injector1
                                        , _injector2
                                        , _injector3
                                        , _injector4
                                    );
                if (filtered)
                {
                    if (OnResponseStartingProcess != null)
                    {
                        context
                            .Response
                            .OnStarting
                                (
                                    () =>
                                    {

                                        try
                                        {
                                            OnResponseStartingProcess?
                                                                .Invoke
                                                                    (
                                                                        context
                                                                        , nameof(OnResponseStartingProcess)
                                                                        , _injector1
                                                                        , _injector2
                                                                        , _injector3
                                                                        , _injector4
                                                                    );
                                        }
                                        catch (Exception e)
                                        {
                                            var reThrow = false;
                                            if (OnCaughtExceptionProcessFunc != null)
                                            {
                                                reThrow = OnCaughtExceptionProcessFunc(this, context, e);
                                            }
                                            if (reThrow)
                                            {
                                                throw;
                                            }
                                        }
                                        return
                                                Task
                                                    .CompletedTask;
                                    }
                                );
                    }
                    if (OnResponseCompletedProcess != null)
                    {
                        context
                            .Response
                            .OnCompleted
                                (
                                    () =>
                                    {
                                        try
                                        {
                                            OnResponseCompletedProcess?
                                                                    .Invoke
                                                                        (
                                                                            context
                                                                            , nameof(OnResponseCompletedProcess)
                                                                            , _injector1
                                                                            , _injector2
                                                                            , _injector3
                                                                            , _injector4
                                                                        );
                                        }
                                        catch (Exception e)
                                        {
                                            var reThrow = false;
                                            if (OnCaughtExceptionProcessFunc != null)
                                            {
                                                reThrow = OnCaughtExceptionProcessFunc(this, context, e);
                                            }
                                            if (reThrow)
                                            {
                                                throw;
                                            }
                                        }
                                        return
                                                Task
                                                    .CompletedTask;
                                                                                
                                    }
                                );
                    }
                    if (OnInvokingProcessAsync != null)
                    {
                        try
                        {
                            needNext = OnInvokingProcessAsync
                                                    (
                                                        context
                                                        , nameof(OnInvokingProcessAsync)
                                                        , _injector1
                                                        , _injector2
                                                        , _injector3
                                                        , _injector4
                                                    )
                                                    .Result;
                        }
                        catch (Exception e)
                        {
                            var reThrow = false;
                            if (OnCaughtExceptionProcessFunc != null)
                            {
                                reThrow = OnCaughtExceptionProcessFunc(this, context, e);
                            }
                            if (reThrow)
                            {
                                throw;
                            }
                        }
                    }
                }
            }
            if (needNext)
            {
                var needResponseBodyWorkingStreamProcess = true;
                if (OnPredicateResponseBodyWorkingStreamProcessFunc != null)
                {
                    needResponseBodyWorkingStreamProcess =
                        OnPredicateResponseBodyWorkingStreamProcessFunc
                                (
                                    context
                                    , nameof(OnPredicateResponseBodyWorkingStreamProcessFunc)
                                    , _injector1
                                    , _injector2
                                    , _injector3
                                    , _injector4
                                );
                }
                try
                {
                    if (needResponseBodyWorkingStreamProcess)
                    {
                        var response = context.Response;
                        var request = context.Request;
                        var originalResponseBodyStream = response.Body;
                        try
                        {
                            using var workingStream = new MemoryStream();
                            response
                                    .Body = workingStream;
                            await
                                _next(context);
                            workingStream
                                    .Position = 0;
                            await
                                workingStream
                                        .CopyToAsync
                                                (
                                                    originalResponseBodyStream
                                                );
                        }
                        finally
                        {
                            response
                                    .Body = originalResponseBodyStream;
                        }
                    }
                    else
                    {
                        await
                            _next(context);
                    }
                    OnAfterInvokedNextProcess?
                                            .Invoke
                                                (
                                                    context
                                                    , nameof(OnAfterInvokedNextProcess)
                                                    , _injector1
                                                    , _injector2
                                                    , _injector3
                                                    , _injector4
                                                );
                }
                catch (Exception e)
                {
                    var reThrow = false;
                    if (OnCaughtExceptionProcessFunc != null)
                    {
                        reThrow = OnCaughtExceptionProcessFunc(this, context, e);
                    }
                    if (reThrow)
                    {
                        throw;
                    }
                }
            }



        }
        public void InitializeLoggingProcesses
                        (
                            SingleThreadAsyncDequeueLoggingProcessor
                                asyncRequestResponseLoggingProcessor
                            , Func<LogLevel>
                                getLoggingLogLevelProcessFunc
                            , string defaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fffff"
                        )
        {
            string requestResponseTimingItemKey = nameof(requestResponseTimingItemKey);
            var middleware = this;
            var middlewareTypeName = middleware.GetType().Name;

            #region middleware.OnFilterProcessFunc
            middleware
                .OnFilterProcessFunc
                    =
                    (
                        httpContext
                        , @event
                        , stopwatchesPool
                        , xConfiguration
                        , xLoggerFactory
                        , xLogger
                    )
                        =>
                    {
                        if
                            (
                                requestResponseTimingItemKey
                                                .IsNullOrEmptyOrWhiteSpace()
                            )
                        {
                            return false;
                        }
                        var request = httpContext.Request;                
                        request.EnableBuffering();
                        //xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
                        var httpRequestFeature = httpContext.Features.Get<IHttpRequestFeature>();
                        var url = httpRequestFeature.RawTarget;
                        httpRequestFeature = null;
                        var r = url.Contains("/api/", StringComparison.OrdinalIgnoreCase);
                        if (r)
                        {
                            httpContext
                                    .Items
                                    .TryAdd
                                        (
                                            requestResponseTimingItemKey
                                            ,
                                                (
                                                    BeginTime: DateTime.Now
                                                    , BeginTimestamp: Stopwatch.GetTimestamp()
                                                )
                                        );
                        }
                        var requestBody = string.Empty;
                                                //should not use using 
                                                var requestBodyStream = request.Body;
                        if
                            (
                                requestBodyStream
                                                .CanRead
                                &&
                                requestBodyStream
                                                .CanSeek
                            )
                        {
                            requestBodyStream.Position = 0;
                                                    //should not use using
                                                    var streamReader = new StreamReader(requestBodyStream);
                            requestBody = streamReader.ReadToEnd();
                            requestBodyStream.Position = 0;
                            httpContext
                                    .Items
                                    .TryAdd
                                        (
                                            nameof(requestBody)
                                            , requestBody
                                        );
                        }
                        return r;
                    };
            #endregion

            #region middleware.OnPredicateResponseBodyWorkingStreamProcessFunc
            middleware
                .OnPredicateResponseBodyWorkingStreamProcessFunc
                    =
                    (
                        httpContext
                        , @event
                        , stopwatchesPool
                        , xConfiguration
                        , xLoggerFactory
                        , xLogger
                    )
                        =>
                    {
                        var request = httpContext
                                                .Request;
                        var r = !request
                                        .Path
                                        .Value
                                        .Contains
                                            (
                                                "export"
                                                , StringComparison
                                                        .OrdinalIgnoreCase
                                            );
                        return r;
                    };
            #endregion

            #region middleware.OnInvokingProcessAsync
            middleware
                .OnInvokingProcessAsync
                    =
                        async
                        (
                            httpContext
                            , @event
                            , stopwatchesPool
                            , xConfiguration
                            , xLoggerFactory
                            , xLogger
                        )
                            =>
                        {
                            //xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
                            var httpRequestFeature = httpContext
                                    .Features
                                    .Get<IHttpRequestFeature>();
                            var url = httpRequestFeature.RawTarget;
                            httpRequestFeature = null;
                            var result = false;
                            if
                                (
                                    //request.ContentType == "image/jpeg"
                                    url.EndsWith("error.js")
                                )
                            {
                                var response = httpContext.Response;
                                var errorStatusCode = 500;
                                var errorMessage = $"error in Middleware: [{middlewareTypeName}]";
                                response.StatusCode = errorStatusCode;
                                var jsonResult = new
                                {
                                    statusCode = errorStatusCode
                                    ,
                                    resultCode = -1 * errorStatusCode
                                    ,
                                    message = errorMessage
                                };
                                using var responseBodyStream = response.Body;
                                await
                                    SystemJsonSerializer
                                                .SerializeAsync
                                                        (
                                                            responseBodyStream
                                                            , jsonResult
                                                        );
                                result = false;
                            }
                            else
                            {
                                result = true;
                            }
                            return
                                await
                                    Task
                                        .FromResult(result);
                        };
            #endregion

            #region middleware.OnResponseStartingProcess
            middleware
                .OnResponseStartingProcess
                    =
                        // don't support async 
                        (
                            httpContext
                            , @event
                            , stopwatchesPool
                            , xConfiguration
                            , xLoggerFactory
                            , xLogger
                        )
                            =>
                        {
                            //xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
                            //return;
                            //bool supportTrailers = httpContext.Response.SupportsTrailers();
                            if (xLogger is ILogger logger)
                            {
                                var logLevel = getLoggingLogLevelProcessFunc();
                                logger
                                    .LogOnDemand
                                        (
                                            //LogLevel.Trace
                                            logLevel
                                            , () =>
                                            {
                                                #region Request
                                                var httpRequestFeature = httpContext
                                                        .Features
                                                        .Get<IHttpRequestFeature>();
                                                var requestUrl = httpRequestFeature.RawTarget;
                                                var request = httpContext.Request;
                                                var requestPath = request.Path;
                                                var requestPathBase = request.PathBase.Value;
                                                var requestActionRoutPath = request.GetActionRoutePathOrDefault("Unknown");
                                                //
                                                using var requestBodyStream = request.Body;
                                                var requestBody = string.Empty;
                                                if
                                                    (
                                                        httpContext
                                                                .Items
                                                                .Remove
                                                                    (
                                                                        nameof(requestBody)
                                                                        , out var removedRequestBody
                                                                    )
                                                    )
                                                {
                                                    requestBody = (string)removedRequestBody;
                                                }
                                                else
                                                {
                                                    if
                                                        (
                                                            requestBodyStream
                                                                            .CanRead
                                                            &&
                                                            requestBodyStream
                                                                            .CanSeek
                                                        )
                                                    {
                                                        requestBodyStream.Position = 0;
                                                        //
                                                        using var streamReader = new StreamReader(requestBodyStream);
                                                        requestBody = new StreamReader(requestBodyStream).ReadToEnd();
                                                    }
                                                }
                                                var requestHeaders = Newtonsoft
                                                                            .Json
                                                                            .JsonConvert
                                                                            .SerializeObject
                                                                                    (
                                                                                        request
                                                                                            .Headers
                                                                                    );
                                                #endregion

                                                #region Response
                                                var response = httpContext.Response;
                                                using var responseBodyStream = response.Body;
                                                var responseBody = string.Empty;
                                                if
                                                    (
                                                        responseBodyStream
                                                                        .CanRead
                                                        &&
                                                        responseBodyStream
                                                                        .CanSeek
                                                    )
                                                {
                                                    responseBodyStream.Position = 0;
                                                    //
                                                    using var streamReader = new StreamReader(responseBodyStream);
                                                    responseBody = streamReader.ReadToEnd();
                                                    //Console.WriteLine(responseBody.Length);
                                                }
                                                var r = httpContext
                                                                .Items
                                                                .Remove
                                                                    (
                                                                        "dbExecutingDuration"
                                                                        , out var removed
                                                                    );
                                                double? dbExecutingTimingInMilliseconds = null;
                                                if (r)
                                                {
                                                    TimeSpan? timespan = removed as TimeSpan?;
                                                    if (timespan != null)
                                                    {
                                                        if (timespan.HasValue)
                                                        {
                                                            dbExecutingTimingInMilliseconds =
                                                                    timespan.Value.TotalMilliseconds;
                                                        }
                                                    }
                                                }
                                                removed = null;
                                                r = httpContext
                                                            .Items
                                                            .Remove
                                                                (
                                                                    requestResponseTimingItemKey
                                                                    , out removed
                                                                );
                                                double? requestResponseTimingInMilliseconds = null;
                                                DateTime? requestBeginTime = null;
                                                DateTime? responseStartingTime = null;
                                                if (r)
                                                {
                                                    var
                                                    (
                                                        beginTime
                                                        , beginTimeStamp
                                                    )
                                                    =
                                                    (ValueTuple<DateTime, long>) removed;
                                                    removed = null;
                                                    requestBeginTime = beginTime;
                                                    response
                                                        .Headers["X-Request-Receive-BeginTime"]
                                                                    = beginTime
                                                                                .ToString(defaultDateTimeFormat);
                                                    responseStartingTime = DateTime.Now;
                                                    response
                                                        .Headers["X-Response-Send-BeginTime"]
                                                                    = responseStartingTime
                                                                                .Value
                                                                                .ToString(defaultDateTimeFormat);

                                                    requestResponseTimingInMilliseconds
                                                                    = beginTimeStamp
                                                                                .GetElapsedTimeToNow()
                                                                                .TotalMilliseconds;
                                                    response
                                                        .Headers["X-Request-Response-Timing-In-Milliseconds"]
                                                                    = requestResponseTimingInMilliseconds
                                                                            .ToString();
                                                }
                                                var responseHeaders = Newtonsoft
                                                                                .Json
                                                                                .JsonConvert
                                                                                .SerializeObject
                                                                                        (
                                                                                            response
                                                                                                .Headers
                                                                                        );
                                                var responseContentLength = response
                                                                                .ContentLength;
                                                #endregion

                                                #region Claims
                                                string roleID = nameof(roleID);
                                                roleID = httpContext
                                                                    .User
                                                                    .GetClaimTypeValueOrDefault
                                                                        (
                                                                            roleID
                                                                            , "AnonymousRole"
                                                                        );
                                                string orgUnitID = nameof(orgUnitID);
                                                orgUnitID = httpContext
                                                                    .User
                                                                    .GetClaimTypeValueOrDefault
                                                                        (
                                                                            orgUnitID
                                                                            , "AnonymousOrgUnit"
                                                                        );
                                                var clientIP = httpContext
                                                                    .Connection
                                                                    .RemoteIpAddress
                                                                    .ToString();
                                                var userID = httpContext
                                                                    .User
                                                                    .Identity
                                                                    .Name ?? "AnonymousUser";
                                                string deviceID = nameof(deviceID);
                                                deviceID = httpContext
                                                                    .User
                                                                    .GetClaimTypeValueOrDefault
                                                                        (
                                                                            deviceID
                                                                            , "AnonymousDevice"
                                                                        );
                                                string deviceInfo = nameof(deviceInfo);
                                                deviceInfo = httpContext
                                                                    .User
                                                                    .GetClaimTypeValueOrDefault
                                                                        (
                                                                            deviceInfo
                                                                            , "UnknownDevice"
                                                                        );
                                                #endregion

                                                #region GlobalManager.AsyncRequestResponseLoggingProcessor.Enqueue
                                                // GlobalManager
                                                asyncRequestResponseLoggingProcessor
                                                        .Enqueue
                                                            (
                                                                (
                                                                        (
                                                                            requestUrl
                                                                            , requestPath
                                                                            , requestPathBase
                                                                            , requestActionRoutPath
                                                                        )
                                                                    ,
                                                                        (
                                                                            requestHeaders
                                                                            , requestBody
                                                                            , request.Method
                                                                            , requestBeginTime
                                                                            , request.ContentLength
                                                                            , request.ContentType
                                                                        )
                                                                    ,
                                                                        (
                                                                            responseHeaders
                                                                            , responseBody
                                                                            , response.StatusCode
                                                                            , responseStartingTime
                                                                            , response.ContentLength
                                                                            , response.ContentType
                                                                        )
                                                                    ,
                                                                        (
                                                                            requestResponseTimingInMilliseconds
                                                                            , dbExecutingTimingInMilliseconds
                                                                        )
                                                                    ,
                                                                        (
                                                                            (
                                                                                clientIP
                                                                                , decimal.Parse("1.0")
                                                                                , new decimal(1.0)
                                                                            ) //Location
                                                                            , userID
                                                                            , roleID
                                                                            , orgUnitID
                                                                            ,
                                                                            (
                                                                                deviceID
                                                                                , deviceInfo
                                                                            ) // Device
                                                                        ) //User
                                                                )
                                                            );
                                                #endregion
                                            }
                                        );
                            }
                        };
            #endregion

            #region middleware.OnAfterInvokedNextProcess
            middleware
                .OnAfterInvokedNextProcess
                    =
                        (
                            httpContext
                            , @event
                            , stopwatchesPool
                            , xConfiguration
                            , xLoggerFactory
                            , xLogger
                        )
                            =>
                        {
                            //Console.WriteLine($"event: {@event} @ {middlewareTypeName}");
                            //xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
                        };
            #endregion

            #region middleware.OnResponseCompletedProcess
            middleware
                .OnResponseCompletedProcess
                    =
                        // don't support async
                        (
                            httpContext
                            , @event
                            , stopwatchesPool
                            , xConfiguration
                            , xLoggerFactory
                            , xLogger
                        )
                            =>
                        {
                            //throw new Exception();
                            if
                                (
                                    httpContext
                                            .Items
                                            .Remove
                                                (
                                                    AbstractStoreProceduresExecutorControllerBase
                                                        .requestJTokenParametersItemKey
                                                    , out var removed
                                                )
                                )
                            {
                                if (removed is JToken parameters)
                                {
                                    parameters = null;
                                }
                                removed = null;
                            }
                                                    //xLogger
                                                    //    .LogInformation($"event: {@event} @ {middlewareTypeName}");
                                                };
            #endregion

        }
    }
    public static partial class RequestResponseGuardMiddlewareApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestResponseGuard<TInjector1, TInjector2, TInjector3, TInjector4>
                (
                    this IApplicationBuilder
                            target

                    , Action
                        <RequestResponseGuardMiddleware<TInjector1, TInjector2, TInjector3, TInjector4>>
                            onInitializeCallbackProcesses = default
                )
        {
            return
                target
                    .UseMiddleware
                        (
                            typeof(RequestResponseGuardMiddleware<TInjector1, TInjector2, TInjector3, TInjector4>)
                            , onInitializeCallbackProcesses
                        );
        }
    }
}
#endif