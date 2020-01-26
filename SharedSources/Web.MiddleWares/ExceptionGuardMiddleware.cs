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
    using System.Text.Json;
    public class ExceptionGuardMiddleware<TInjector>
    //竟然没有接口?
    {
        private readonly RequestDelegate _next;
        private readonly TInjector _injector;

        private const string defaultErrorResponseContentType = "application/json";
        private const string defaultErrorMessage = nameof(HttpStatusCode.InternalServerError);
        private readonly JsonSerializerOptions defaultJsonSerializerOptions = new JsonSerializerOptions { WriteIndented = true };
        public Func<HttpContext, IConfiguration, Exception, ILoggerFactory, ILogger, TInjector,(bool, bool, HttpStatusCode, int, string)> OnCaughtExceptionProcessFunc;

        public
            Action
                <
                    bool
                    , Exception
                    , HttpContext
                    , IConfiguration
                    , ILoggerFactory
                    , ILogger
                    , TInjector
                >
                    OnFinallyProcessAction;

        private ExceptionGuardMiddleware
                    (
                        RequestDelegate
                                next
                        , Action
                            <
                                ExceptionGuardMiddleware
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

        public ExceptionGuardMiddleware
            (
                RequestDelegate next
                , IConfiguration configuration
                , ILoggerFactory loggerFactory
                , ILogger logger
                , TInjector injector
                , Action
                    <
                        ExceptionGuardMiddleware<TInjector>
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
            int errorResultCode = -1 * (int)errorStatusCode;
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
                        JsonSerializer
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
        public static IApplicationBuilder UseExceptionGuard<TInjector>
            (
                this IApplicationBuilder
                        target
                , Action
                    <ExceptionGuardMiddleware<TInjector>>
                        onInitializeCallbackProcesses = default
            )
        {
            return
                target
                    .UseMiddleware
                        (
                            typeof
                                (
                                    ExceptionGuardMiddleware
                                                        <TInjector>
                                )
                            , onInitializeCallbackProcesses
                        );
        }
    }
}
#endif