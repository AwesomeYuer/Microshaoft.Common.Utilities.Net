#if NETCOREAPP
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Newtonsoft.Json;
    using System;
    using System.Net;
    using System.Threading.Tasks;
    public class ExceptionGuardMiddleware<TInjector>
    //竟然没有接口?
    {
        private readonly RequestDelegate _next;
        private readonly TInjector _injector;

        public Func
                    <
                        Exception
                        , HttpContext
                        , IConfiguration
                        , ILoggerFactory
                        , ILogger
                        , TInjector
                        ,
                            (
                                bool ReThrow
                                , bool Detail
                                , HttpStatusCode StatusCode
                            )
                    >
                        OnCaughtExceptionProcessFunc;
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
            (
                bool ReThrow
                , bool ResponseDetails
                , HttpStatusCode ResponseStatusCode
            )
                r =
                    (
                        false
                        , false
                        , HttpStatusCode.OK
                    );
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
                    r = OnCaughtExceptionProcessFunc
                                (
                                    exception
                                    , context
                                    , Configuration
                                    , LoggerFactory
                                    , Logger
                                    , _injector

                                );
                }
                var response = context.Response;
                response.StatusCode = (int)r.ResponseStatusCode;
                var errorMessage = "Internal Server Error";
                if (r.ResponseDetails)
                {
                    errorMessage = exception.ToString();
                }
                var jsonResult =
                            new
                            {
                                statusCode = r.ResponseStatusCode
                                ,
                                message = errorMessage
                            };
                var json = JsonConvert
                                    .SerializeObject
                                            (jsonResult);
                await
                    response
                        .WriteAsync(json);
                if (r.ReThrow)
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