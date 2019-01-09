#if NETCOREAPP2_X
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using System;
    using System.Threading.Tasks;
    public class RequestResponseGuardMiddleware<TInjector>
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
        public async Task Invoke(HttpContext context)
        {
            _onBeforeInvoking?.Invoke(_injector, context);
            context
                .Response
                .OnStarting
                    (
                        () =>
                        {
                            _onAfterInvoked?.Invoke(_injector, context);
                            return
                                Task.CompletedTask;
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
                , Action<T, HttpContext> onBeforeInvokingProcess
                , Action<T, HttpContext> onAfterInvokedProcess
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