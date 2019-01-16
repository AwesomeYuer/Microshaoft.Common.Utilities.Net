//#if NETCOREAPP2_X
//namespace Microshaoft.Web
//{
//    using Microsoft.AspNetCore.Builder;
//    using Microsoft.AspNetCore.Http;
//    using Microsoft.Extensions.Configuration;
//    using Newtonsoft.Json;
//    using System;
//    using System.Collections.Generic;
//    using System.Net;
//    using System.Threading.Tasks;
//    public abstract class AbstractCommonMiddlewareBase
//                            <
//                                TInjector1
//                                , TInjector2
//                                , TInjector3
//                                , TInjector4
//                                //, TInjector5
//                                //, TInjector6
//                                //, TInjector7
//                                //, TInjector8
//                            >
//            //竟然没有接口?
//    {
//        private readonly RequestDelegate _next;
//        private readonly IDictionary<string, object> _injectors
//                    = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);

//        public object this[string key]
//        {
//            get
//            {
//                return _injectors[key];
//            }
//        }


//        public AbstractCommonMiddlewareBase
//            (
//                RequestDelegate next
//                , TInjector1 injector1 = default(TInjector1)
//                , TInjector2 injector2 = default(TInjector2)
//                , TInjector3 injector3 = default(TInjector3)
//                , TInjector4 injector4 = default(TInjector4)
//            //, TInjector5 injector5 = default(TInjector5)
//            //, TInjector6 injector6 = default(TInjector6)
//            //, TInjector7 injector7 = default(TInjector7)
//            //, TInjector8 injector8 = default(TInjector8)
//            )
//        {
//            _next = next;
//            //if (injector1 == default(TInjector1))
//            {
//                _injectors
//                        .TryAdd
//                            (
//                                typeof(TInjector1).Name
//                                , injector1
//                            );
//            }
//            //if (injector2 == default(TInjector2))
//            {
//                _injectors
//                        .TryAdd
//                            (
//                                typeof(TInjector2).Name
//                                , injector2
//                            );
//            }

//        }


//        //必须是如下方法(竟然不用接口约束产生编译期错误),否则运行时错误
//        public virtual async Task Invoke(HttpContext context)
//        {

//        }

        
//    }

//    public static class AbstractCommonMiddlewareBaseExtensions
//    {
//        public static IApplicationBuilder UseCommonMiddleware<TMiddleware>
//            (
//                this IApplicationBuilder target
//                , TMiddleware middleware

//            )
//        {
//            return
//                target
//                    .UseMiddleware
//                        (
//                            typeof(TMiddleware<TInjector>)
//                            , onInitializeCallbackProcesses
//                        );
//        }
//    }

//    public class Middleware1 : AbstractCommonMiddlewareBase<IConfiguration, string, int, object>
//    {
//        public Middleware1(RequestDelegate next, IConfiguration configuration)
//                        : base(next, configuration, "xxx", 100, new object())
//        {


//        }

//    }
//}
//#endif