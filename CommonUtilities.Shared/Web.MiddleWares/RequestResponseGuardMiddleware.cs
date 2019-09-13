#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    public class RequestResponseGuardMiddleware<TInjector>
            //竟然没有接口?
    {
        private readonly RequestDelegate _next;
        private readonly TInjector _injector;
        public RequestResponseGuardMiddleware
                (
                    RequestDelegate next
                    , TInjector injector
                    , Action
                        <RequestResponseGuardMiddleware<TInjector>>
                            onInitializeCallbackProcesses = null
                )
        {
            _next = next;
            _injector = injector;
            onInitializeCallbackProcesses?
                                    .Invoke(this);
        }

        public Func<TInjector, HttpContext, string, bool> OnFilterProcessFunc;
        public Func<TInjector, HttpContext, string, Task<bool>> OnInvokingProcessAsync;
        public Action<TInjector, HttpContext, string> OnResponseStartingProcess;
        
        public Action<TInjector, HttpContext, string> OnAfterInvokedNextProcess;
        public Action<TInjector, HttpContext, string> OnResponseCompletedProcess;

        //必须是如下方法(竟然不用接口约束产生编译期错误),否则运行时错误
        public async Task Invoke(HttpContext context)
        {
            var filtered = true;
            bool needNext = true;
            if (OnFilterProcessFunc != null)
            {
                filtered = OnFilterProcessFunc
                                    (
                                        _injector
                                        , context
                                        , nameof(OnFilterProcessFunc)
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
                                                        _injector
                                                        , context
                                                        , nameof(OnResponseStartingProcess)
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
                                                        _injector
                                                        , context
                                                        , nameof(OnResponseCompletedProcess)
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
                                                _injector
                                                , context
                                                , nameof(OnInvokingProcessAsync)
                                            ).Result;
                    } 
                }
            }
            if (needNext)
            {
                await _next(context);
                OnAfterInvokedNextProcess?
                                        .Invoke
                                            (
                                                _injector
                                                , context
                                                , nameof(OnAfterInvokedNextProcess)
                                            );
            }
        }
    }
    public static partial class RequestResponseGuardMiddlewareApplicationBuilderExtensions
    {
        public static IApplicationBuilder UseRequestResponseGuard<TInjector>
                (
                    this IApplicationBuilder target
                    , Action
                        <RequestResponseGuardMiddleware<TInjector>>
                            onInitializeCallbackProcesses = null
                )
        {
            return
                target
                    .UseMiddleware
                        (
                            typeof(RequestResponseGuardMiddleware<TInjector>)
                            , onInitializeCallbackProcesses
                        );
        }
    }
}
#endif