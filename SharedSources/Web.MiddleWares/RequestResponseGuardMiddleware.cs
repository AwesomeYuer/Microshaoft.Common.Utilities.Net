#if NETCOREAPP
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    using System;
    using System.Threading.Tasks;
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

        //必须是如下方法(竟然不用接口约束产生编译期错误),否则运行时错误
        public async Task Invoke(HttpContext context)
        {
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
                                        return
                                            Task
                                                .CompletedTask;
                                    }
                                );
                    }
                    if (OnInvokingProcessAsync != null)
                    {
                        needNext = OnInvokingProcessAsync
                                            (
                                                context
                                                , nameof(OnInvokingProcessAsync)
                                                , _injector1
                                                , _injector2
                                                , _injector3
                                                , _injector4
                                            ).Result;
                    }
                }
            }
            if (needNext)
            {
                await
                    _next(context);
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