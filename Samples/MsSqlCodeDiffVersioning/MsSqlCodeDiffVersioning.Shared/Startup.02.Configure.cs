namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    //using Newtonsoft.Json;
    using System;
    using System.Collections.Concurrent;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
#if NETCOREAPP2_X
    using Microsoft.AspNetCore.Hosting;
#else
#endif
    public partial class Startup
    {
        private ILogger _logger;

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure
                        (
                            IApplicationBuilder
                                        app
                            ,
#if NETCOREAPP2_X
                                IHostingEnvironment
#else
                                IWebHostEnvironment
#endif
                                        environment
                            , IConfiguration
                                        configuration
                            , ILoggerFactory
                                        loggerFactory
                            , ConcurrentDictionary<string, ExecutingInfo>
                                        executingCachingStore
                            , ILogger
                                        logger
                        )
        {
            _logger = logger;
            app.UseCors();

            string requestResponseTimingItemKey = nameof(requestResponseTimingItemKey);
            //timingKey = string.Empty;
            var needUseMiddleware = configuration.GetValue("useRequestResponseGuard", false);
            if (needUseMiddleware)
            {
                #region RequestResponseGuard
                app
                    .UseRequestResponseGuard
                        <
                            QueuedObjectsPool<Stopwatch>
                            , IConfiguration
                            , ILoggerFactory
                            , ILogger
                        >
                            (
                                (middleware) =>
                                {
                                    middleware
                                        .InitializeLoggingProcesses
                                            (
                                                GlobalManager.RequestResponseLoggingProcessor
                                                , () => GlobalManager.RequestResponseLoggingLogLevel
                                            );
                                }
                            );
                #endregion
            }
            needUseMiddleware = configuration.GetValue("useRequestResponseExceptionGuard", false);
            if (needUseMiddleware)
            {
                var errorDetails = true;

                #region UseRequestResponseExceptionGuard
                app
                    .UseRequestResponseExceptionGuard<string>
                        (
                            (middleware) =>
                            {
                                //onInitializeCallbackProcesses
                                var middlewareTypeName = middleware.GetType().Name;
                                middleware
                                    .OnCaughtExceptionProcessFunc
                                        =
                                        (
                                            xHttpContext
                                            , xConfiguration
                                            , xException
                                            , xLoggerFactory
                                            , xLogger

                                            , xErrorTime
                                            , xErrorSource
                                            , xTraceID

                                            , xTInjector
                                        )
                                            =>
                                        {
                                            var reThrow = false;
                                            
                                            var errorStatusCode = HttpStatusCode
                                                                            .InternalServerError;
                                            var errorResultCode = -1 * (int) errorStatusCode;
                                            var errorMessage = nameof(HttpStatusCode.InternalServerError);

                                            if (errorDetails)
                                            {
                                                errorMessage = xException.ToString();
                                            }
                                            reThrow = GlobalManager
                                                                .OnCaughtExceptionProcessFunc
                                                                    (
                                                                        logger
                                                                        , xException
                                                                        , xException
                                                                        , $"event: exception @ middleware : {middlewareTypeName}"

                                                                        , xErrorTime
                                                                        , xErrorSource
                                                                        , xTraceID
                                                                    );
                                            return
                                                (
                                                    reThrow
                                                    , errorDetails
                                                    , errorStatusCode
                                                    , errorResultCode
                                                    , errorMessage
                                                );
                                        };
                            }
                        );
                #endregion
            }
            needUseMiddleware = configuration.GetValue("useExceptionHandler", false);
            if (needUseMiddleware)
            {
                #region ExceptionHandler
                app
                    .UseExceptionHandler
                            (
                                new ExceptionHandlerOptions()
                                {
                                    ExceptionHandler = new ExceptionOnDemandHandlerMiddleware
                                                            (
                                                                environment
                                                                , configuration
                                                            )
                                    {
                                        OnCaughtExceptionHandleProcess =
                                         (xHttpContext, xConfiguration, xCaughtException, xExceptionTime, xExceptionSource, xTraceID) =>
                                         {
                                             var middlewareTypeName = typeof(ExceptionOnDemandHandlerMiddleware).Name;
                                             var errorMessage = nameof(HttpStatusCode.InternalServerError);
                                             var errorDetails = true;
                                             if (errorDetails)
                                             {
                                                 errorMessage = xCaughtException.ToString();
                                             }
                                             var reThrow = GlobalManager
                                                                .OnCaughtExceptionProcessFunc
                                                                    (
                                                                        logger
                                                                        , xCaughtException
                                                                        , xCaughtException
                                                                        , $"event: exception @ middleware : {middlewareTypeName}"
                                                                        , xExceptionTime
                                                                        , xExceptionSource
                                                                        , xTraceID
                                                                    );
                                             return
                                                 (
                                                    errorDetails
                                                    , HttpStatusCode
                                                                .InternalServerError
                                                    , -1 * (int) HttpStatusCode
                                                                        .InternalServerError
                                                    , errorMessage
                                                 );
                                         }
                                    }.Invoke
                                }
                            ) ; 
                #endregion
                //app.UseDeveloperExceptionPage();
            }

            {
                //app.UseHsts();
            }
            //app.UseHttpsRedirection();

            //should before app.UseMvc();
            app.UseAuthentication();
            app.UseAuthorization();
            
            app.UseMvc();

            Console.WriteLine(Directory.GetCurrentDirectory());

            #region StaticFiles
            app
                .UseDefaultFiles
                    (
                        new DefaultFilesOptions()
                        {
                            DefaultFileNames =
                                {
                                    "index.html"
                                }
                        }
                    );
            app.UseStaticFiles();
            #endregion

            #region Swagger
            app.UseSwaggerDefault();
            #endregion

            //app.UseEndpoints(endpoints =>
            //    {
            //        endpoints.MapControllers();
            //    });

            //app.UseHttpsRedirection();
        }
    }
}
