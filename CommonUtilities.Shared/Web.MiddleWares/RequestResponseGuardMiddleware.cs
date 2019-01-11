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
                , Action<TInjector, HttpContext>
                                onBeforeInvokingProcess        = null
                , Action<TInjector, HttpContext>
                                onResponseStartingProcess      = null
                , Action<TInjector, HttpContext>
                                onAfterInvokedProcess          = null
                , Action<TInjector, HttpContext>
                                onResponseCompletedProcess     = null
            )
        {
            _next = next;
            _injector = injector;

            _onBeforeInvokingProcess = onBeforeInvokingProcess;
            _onResponseStartingProcess = onResponseStartingProcess;

            _onAfterInvokedProcess = onAfterInvokedProcess;
            _onResponseCompletedProcess = onResponseCompletedProcess;
        }
        private Action<TInjector, HttpContext> _onBeforeInvokingProcess;
        private Action<TInjector, HttpContext> _onResponseStartingProcess;
        
        private Action<TInjector, HttpContext> _onAfterInvokedProcess;
        private Action<TInjector, HttpContext> _onResponseCompletedProcess;

        //必须是如下方法(竟然不用接口约束产生编译期错误),否则运行时错误
        public async Task Invoke(HttpContext context)
        {
            _onBeforeInvokingProcess?
                        .Invoke
                            (
                                _injector
                                , context
                            );
            if (_onResponseStartingProcess != null)
            {
                context
                    .Response
                    .OnStarting
                        (
                            () =>
                            {
                                _onResponseStartingProcess?
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
            }
            if (_onResponseCompletedProcess != null)
            {
                context
                    .Response
                    .OnCompleted
                        (
                            () =>
                            {
                                _onResponseCompletedProcess?
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
            }
            await _next(context);
            _onAfterInvokedProcess?
                                .Invoke
                                    (
                                        _injector
                                        , context
                                    );
        }
    }
    public static class RequestResponseGuardMiddlewareExtensions
    {
        public static IApplicationBuilder UseRequestResponseGuard<TInjector>
            (
                this IApplicationBuilder target
                //, Action<TInjector, HttpContext> onBeforeInvokingProcess = null
                //, Action<TInjector, HttpContext> onResponseStartingProcess = null
                //, Action<TInjector, HttpContext> onAfterInvokedProcess = null
                //, Action<TInjector, HttpContext> onResponseCompletedProcess = null
                , params Action<TInjector, HttpContext>[] processActions
            )
        {
            return
                target
                    .UseMiddleware
                        (
                            typeof(RequestResponseGuardMiddleware<TInjector>)
                            , processActions
                            //, onBeforeInvokingProcess 
                            //, onResponseStartingProcess
                            //, null // onAfterInvokedProcess
                            //, null //onResponseCompletedProcess
                        );
        }
    }
}
#endif