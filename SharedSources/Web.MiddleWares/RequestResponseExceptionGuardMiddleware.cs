#if NETCOREAPP
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    //using Newtonsoft.Json;
    using System;
    using System.Net;
    using System.Threading.Tasks;
    using SystemJsonSerializer = System.Text.Json.JsonSerializer;
    using System.Text.Json;
    using Microsoft.AspNetCore.Http.Features;

    public class RequestResponseExceptionGuardMiddleware<TInjector>
    //竟然没有接口?
    {
        private readonly RequestDelegate _next;
        private readonly TInjector _injector;

        private const string defaultErrorResponseContentType = "application/json";
        private const string defaultErrorMessage = nameof(HttpStatusCode.InternalServerError);
        private readonly JsonSerializerOptions defaultJsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
        public Func
                    <
                        HttpContext
                        , IConfiguration
                        , Exception
                        , ILoggerFactory
                        , ILogger

                        , DateTime?         // time
                        , string            // source
                        , Guid?             // traceID

                        , TInjector
                        ,
                            (
                                bool                // reThrow
                                , bool              // error Details
                                , HttpStatusCode
                                , int               // error Result Code
                                , string            // error Message

                            )
                    >
                        OnCaughtExceptionProcessFunc;

        public
            Action
                <
                    bool                    // caught Exeption
                    , Exception
                    , HttpContext
                    , IConfiguration
                    , ILoggerFactory
                    , ILogger
                    , TInjector
                >
                    OnFinallyProcessAction;

        private RequestResponseExceptionGuardMiddleware
                    (
                        RequestDelegate
                                next
                        , Action
                            <
                                RequestResponseExceptionGuardMiddleware
                                    <TInjector>
                            >
                                onInitializeCallbackProcesses = null
                    )
        {
            _next = next;
            onInitializeCallbackProcesses?
                                    .Invoke(this);
        }

        public readonly IConfiguration Configuration;
        public readonly ILoggerFactory LoggerFactory;
        public readonly ILogger Logger;

        public RequestResponseExceptionGuardMiddleware
            (
                RequestDelegate next
                , IConfiguration configuration
                , ILoggerFactory loggerFactory
                , ILogger logger
                , TInjector injector
                , Action
                    <
                        RequestResponseExceptionGuardMiddleware<TInjector>
                    >
                        onInitializeCallbackProcesses = default
            ) : this(next, onInitializeCallbackProcesses)
        {
            _injector = injector;
            Configuration = configuration;
            LoggerFactory = loggerFactory;
            Logger = logger;
        }

        //必须是如下方法(竟然不用接口约束产生编译期错误),否则运行时错误
        public async Task Invoke(HttpContext context)
        {
            var caughtException = false;
            Exception exception = null;
            
            bool reThrow = false;
            bool errorDetails = false;
            HttpStatusCode errorStatusCode = HttpStatusCode.InternalServerError;
            int errorResultCode = -1 * (int) errorStatusCode;
            var errorMessage = defaultErrorMessage;

            try
            {
                await _next(context);
            }
            catch (Exception e)
            {
                
                exception = e;
                caughtException = true;

                if (OnCaughtExceptionProcessFunc != null)
                {
                    DateTime? errorTime = DateTime.Now;
                    var httpRequestFeature = context
                                                    .Features
                                                    .Get<IHttpRequestFeature>();
                    string requestResponseTimingItemKey = nameof(requestResponseTimingItemKey);
                    DateTime beginTime = default;
                    long beginTimeStamp = default;
                    Guid? traceID = default;
                    if
                        (
                            context
                                .Items
                                .TryGetValue
                                    (
                                        requestResponseTimingItemKey
                                        , out var v
                                    )
                        )
                    {
                        (
                            beginTime
                            , beginTimeStamp
                            , traceID
                        )
                        =
                            (ValueTuple<DateTime, long, Guid?>) v;
                        
                    }
                    var requestUrl = httpRequestFeature.RawTarget;
                    (
                        reThrow
                        , errorDetails
                        , errorStatusCode
                        , errorResultCode
                        , errorMessage
                    )
                    = OnCaughtExceptionProcessFunc
                                (
                                    context
                                    , Configuration
                                    , exception
                                    , LoggerFactory
                                    , Logger

                                    , errorTime
                                    , requestUrl
                                    , traceID
                                    , _injector
                                );
                }
                var response = context.Response;
                response.StatusCode = (int) errorStatusCode;
                response.ContentType = defaultErrorResponseContentType;
                if (errorDetails && errorMessage.IsNullOrEmptyOrWhiteSpace())
                {
                    errorMessage = exception.ToString();
                }
                var jsonResult =
                            new
                            {
                                statusCode = errorStatusCode
                                , resultCode = errorResultCode
                                , message = errorMessage
                            };
                await
                        SystemJsonSerializer
                            .SerializeAsync
                                    (response.Body, jsonResult, defaultJsonSerializerOptions);
                //await
                //    response
                //        .WriteAsync(json);
                if (reThrow)
                {
                    throw;
                }
            }
            finally
            {
                OnFinallyProcessAction?
                                    .Invoke
                                        (
                                            caughtException
                                            , exception
                                            , context
                                            , Configuration
                                            , LoggerFactory
                                            , Logger
                                            , _injector
                                        );
            }
        }
    }

    public static partial class RequestResponseGuardMiddwareExtensions
    {
        public static IApplicationBuilder UseRequestResponseExceptionGuard<TInjector>
            (
                this IApplicationBuilder
                        target
                , Action
                    <RequestResponseExceptionGuardMiddleware<TInjector>>
                        onInitializeCallbackProcesses = default
            )
        {
            return
                target
                    .UseMiddleware
                        (
                            typeof
                                (
                                    RequestResponseExceptionGuardMiddleware
                                                        <TInjector>
                                )
                            , onInitializeCallbackProcesses
                        );
        }
    }
}
#endif