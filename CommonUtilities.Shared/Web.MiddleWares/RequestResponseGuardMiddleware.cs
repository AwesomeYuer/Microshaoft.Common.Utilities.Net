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
                        , Action<TInjector, HttpContext> onBeforeInvokingProcess = null
                        , Action<TInjector, HttpContext> onAfterInvokedProcess = null
                    )
        {
            _next = next;
            _injector = injector;
            _onBeforeInvoking = onBeforeInvokingProcess;
            _onAfterInvoked = onAfterInvokedProcess;
        }
        private Action<TInjector, HttpContext> _onAfterInvoked;
        private Action<TInjector, HttpContext> _onBeforeInvoking;
        //必须是如下方法(竟然不用接口约束产生编译期错误),否则运行时错误
        public async Task Invoke(HttpContext context)
        {
            _onBeforeInvoking?
                        .Invoke
                            (
                                _injector
                                , context
                            );
            context
                .Response
                .OnStarting
                    (
                        () =>
                        {
                            _onAfterInvoked?
                                    .Invoke
                                        (
                                            _injector
                                            , context
                                        );
                            return
                                Task
                                    .CompletedTask;
                        }
                    );
            await _next(context);
        }
    }
    public static class RequestResponseGuardMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseGuard<T>
            (
                this IApplicationBuilder target
                , Action<T, HttpContext> onBeforeInvokingProcess = null
                , Action<T, HttpContext> onAfterInvokedProcess = null
            )
        {
            return
                target
                    .UseMiddleware
                        (
                            typeof(RequestResponseGuardMiddleware<T>)
                            , onBeforeInvokingProcess
                            , onAfterInvokedProcess
                        );
        }
    }
}
#endif