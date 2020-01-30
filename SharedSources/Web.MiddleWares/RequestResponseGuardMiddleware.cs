#if NETCOREAPP
namespace Microshaoft.Web
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Logging;
    //using Microsoft.Extensions.Logging;
    using System;
    using System.IO;
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
                                        OnPredicateResponseWorkingStreamProcessFunc;


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
                var needResponseWorkingStreamProcess = true;
                if (OnPredicateResponseWorkingStreamProcessFunc != null)
                {
                    needResponseWorkingStreamProcess =
                        OnPredicateResponseWorkingStreamProcessFunc
                                (
                                    context
                                    , nameof(OnPredicateResponseWorkingStreamProcessFunc)
                                    , _injector1
                                    , _injector2
                                    , _injector3
                                    , _injector4
                                );
                }
                try
                {
                    if (needResponseWorkingStreamProcess)
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