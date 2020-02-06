namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.Controllers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    //using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.IO;
    using System.Net;
    using System.Text.Json;
    using System.Threading.Tasks;
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

            var requestResponseTimingItemKey = string.Empty;
            requestResponseTimingItemKey = nameof(requestResponseTimingItemKey);
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
                                    //onInitializeCallbackProcesses
                                    var middlewareTypeName = middleware.GetType().Name;

                                    #region middleware.OnFilterProcessFunc
                                    middleware
                                        .OnFilterProcessFunc
                                            = 
                                            (
                                                httpContext
                                                , @event
                                                , stopwatchesPool
                                                , xConfiguration
                                                , xLoggerFactory
                                                , xLogger
                                            )
                                                =>
                                            {
                                                if
                                                    (
                                                        requestResponseTimingItemKey
                                                                        .IsNullOrEmptyOrWhiteSpace()
                                                    )
                                                {
                                                    return false;
                                                }
                                                var request = httpContext.Request;
                                                request.EnableBuffering();
                                                //xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
                                                var httpRequestFeature = httpContext.Features.Get<IHttpRequestFeature>();
                                                var url = httpRequestFeature.RawTarget;
                                                httpRequestFeature = null;
                                                var r = url.Contains("/api/", StringComparison.OrdinalIgnoreCase);
                                                if (r)
                                                {
                                                    httpContext
                                                            .Items
                                                            .TryAdd
                                                                (
                                                                    requestResponseTimingItemKey
                                                                    ,
                                                                        (
                                                                            BeginTime: DateTime.Now
                                                                            , BeginTimestamp: Stopwatch.GetTimestamp()
                                                                        )
                                                                );
                                                }
                                                var requestBody = string.Empty;
                                                //should not use using 
                                                var requestBodyStream = request.Body;
                                                if
                                                    (
                                                        requestBodyStream
                                                                        .CanRead
                                                        &&
                                                        requestBodyStream
                                                                        .CanSeek
                                                    )
                                                {
                                                    requestBodyStream.Position = 0;
                                                    //should not use using
                                                    var streamReader = new StreamReader(requestBodyStream);
                                                    requestBody = streamReader.ReadToEnd();
                                                    requestBodyStream.Position = 0;
                                                    httpContext
                                                            .Items
                                                            .TryAdd
                                                                (
                                                                    nameof(requestBody)
                                                                    , requestBody
                                                                );
                                                }
                                                return r;
                                            };
                                    #endregion

                                    #region middleware.OnPredicateResponseBodyWorkingStreamProcessFunc
                                    middleware
                                        .OnPredicateResponseBodyWorkingStreamProcessFunc
                                            =
                                            (
                                                httpContext
                                                , @event
                                                , stopwatchesPool
                                                , xConfiguration
                                                , xLoggerFactory
                                                , xLogger
                                            )
                                                =>
                                            {
                                                var request = httpContext
                                                                        .Request;
                                                var r = !request
                                                                .Path
                                                                .Value
                                                                .Contains
                                                                    (
                                                                        "export"
                                                                        , StringComparison
                                                                                .OrdinalIgnoreCase
                                                                    );
                                                return r;
                                            };
                                    #endregion

                                    #region middleware.OnInvokingProcessAsync
                                    middleware
                                        .OnInvokingProcessAsync
                                            =
                                                async
                                                (
                                                    httpContext
                                                    , @event
                                                    , stopwatchesPool
                                                    , xConfiguration
                                                    , xLoggerFactory
                                                    , xLogger
                                                )
                                                    =>
                                                {
                                                    //xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
                                                    var httpRequestFeature = httpContext
                                                                                    .Features
                                                                                    .Get<IHttpRequestFeature>();
                                                    var url = httpRequestFeature.RawTarget;
                                                    httpRequestFeature = null;
                                                    var result = false;
                                                    if
                                                        (
                                                            //request.ContentType == "image/jpeg"
                                                            url.EndsWith("error.js")
                                                        )
                                                    {
                                                        var response = httpContext.Response;
                                                        var errorStatusCode = 500;
                                                        var errorMessage = $"error in Middleware: [{middlewareTypeName}]";
                                                        response.StatusCode = errorStatusCode;
                                                        var jsonResult = new
                                                        {
                                                            statusCode = errorStatusCode
                                                            , resultCode = -1 * errorStatusCode
                                                            , message = errorMessage
                                                        };
                                                        using var responseBodyStream = response.Body;
                                                        await
                                                            JsonSerializer
                                                                        .SerializeAsync
                                                                                (
                                                                                    responseBodyStream
                                                                                    , jsonResult
                                                                                );
                                                        result = false;
                                                    }
                                                    else
                                                    {
                                                        result = true;
                                                    }
                                                    return
                                                        await
                                                            Task
                                                                .FromResult(result);
                                                };
                                    #endregion

                                    #region middleware.OnResponseStartingProcess
                                    middleware
                                        .OnResponseStartingProcess
                                            =
                                                // don't support async 
                                                (
                                                    httpContext
                                                    , @event
                                                    , stopwatchesPool
                                                    , xConfiguration
                                                    , xLoggerFactory
                                                    , xLogger
                                                )
                                                    =>
                                                {
                                                    //xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
                                                    //return;
                                                    xLogger
                                                        .LogOnDemand
                                                            (
                                                                //LogLevel.Trace
                                                                GlobalManager
                                                                        .RequestResponseLoggingLogLevel
                                                                , () =>
                                                                {
                                                                    #region Request
                                                                    var httpRequestFeature = httpContext
                                                                                                    .Features
                                                                                                    .Get<IHttpRequestFeature>();
                                                                    var url = httpRequestFeature.RawTarget;
                                                                    var request = httpContext.Request;
                                                                    //
                                                                    using var requestBodyStream = request.Body;
                                                                    var requestBody = string.Empty;
                                                                    if
                                                                        (
                                                                            httpContext
                                                                                    .Items
                                                                                    .Remove
                                                                                        (
                                                                                            nameof(requestBody)
                                                                                            , out var removedRequestBody
                                                                                        )
                                                                        )
                                                                    {
                                                                        requestBody = (string) removedRequestBody;
                                                                    }
                                                                    else
                                                                    {
                                                                        if
                                                                            (
                                                                                requestBodyStream
                                                                                                .CanRead
                                                                                &&
                                                                                requestBodyStream
                                                                                                .CanSeek
                                                                            )
                                                                        {
                                                                            requestBodyStream.Position = 0;
                                                                            //
                                                                            using var streamReader = new StreamReader(requestBodyStream);
                                                                            requestBody = new StreamReader(requestBodyStream).ReadToEnd();
                                                                        }
                                                                    }
                                                                    var requestHeaders = Newtonsoft
                                                                                                .Json
                                                                                                .JsonConvert
                                                                                                .SerializeObject
                                                                                                        (
                                                                                                            request
                                                                                                                .Headers
                                                                                                        );
                                                                    var requestPath = request.Path;
                                                                    #endregion

                                                                    #region Response
                                                                    var response = httpContext.Response;
                                                                    using var responseBodyStream = response.Body;
                                                                    var responseBody = string.Empty;
                                                                    if
                                                                        (
                                                                            responseBodyStream
                                                                                            .CanRead
                                                                            &&
                                                                            responseBodyStream
                                                                                            .CanSeek
                                                                        )
                                                                    {
                                                                        responseBodyStream.Position = 0;
                                                                        //
                                                                        using var streamReader = new StreamReader(responseBodyStream);
                                                                        responseBody = streamReader.ReadToEnd();
                                                                        //Console.WriteLine(responseBody.Length);
                                                                    }
                                                                    var r = httpContext
                                                                                    .Items
                                                                                    .Remove
                                                                                        (
                                                                                            "dbExecutingDuration"
                                                                                            , out var removed
                                                                                        );
                                                                    double? dbExecutingTimingInMilliseconds = null;
                                                                    if (r)
                                                                    {
                                                                        TimeSpan? timespan = removed as TimeSpan?;
                                                                        if (timespan != null)
                                                                        {
                                                                            if (timespan.HasValue)
                                                                            {
                                                                                dbExecutingTimingInMilliseconds =
                                                                                        timespan.Value.TotalMilliseconds;
                                                                            }
                                                                        }
                                                                    }
                                                                    removed = null;
                                                                    r = httpContext
                                                                                .Items
                                                                                .Remove
                                                                                    (
                                                                                        requestResponseTimingItemKey
                                                                                        , out removed
                                                                                    );
                                                                    double? requestResponseTimingInMilliseconds = null;
                                                                    DateTime? requestBeginTime = null;
                                                                    DateTime? responseStartingTime = null;
                                                                    if (r)
                                                                    {
                                                                        var
                                                                        (
                                                                            beginTime
                                                                            , beginTimeStamp
                                                                        )
                                                                        =
                                                                        (ValueTuple<DateTime, long>)removed;
                                                                        removed = null;
                                                                        requestBeginTime = beginTime;
                                                                        response
                                                                            .Headers["X-Request-Receive-BeginTime"]
                                                                                        = beginTime
                                                                                                    .ToString(defaultDateTimeFormat);
                                                                        responseStartingTime = DateTime.Now;
                                                                        response
                                                                            .Headers["X-Response-Send-BeginTime"]
                                                                                        = responseStartingTime
                                                                                                    .Value
                                                                                                    .ToString(defaultDateTimeFormat);

                                                                        requestResponseTimingInMilliseconds
                                                                                        = beginTimeStamp
                                                                                                    .GetElapsedTimeToNow()
                                                                                                    .TotalMilliseconds;
                                                                        response
                                                                            .Headers["X-Request-Response-Timing-In-Milliseconds"]
                                                                                        = requestResponseTimingInMilliseconds
                                                                                                .ToString();
                                                                    }
                                                                    var responseHeaders = Newtonsoft
                                                                                                    .Json
                                                                                                    .JsonConvert
                                                                                                    .SerializeObject
                                                                                                            (
                                                                                                                response
                                                                                                                    .Headers
                                                                                                            );
                                                                    var responseContentLength = response
                                                                                                    .ContentLength;
                                                                    #endregion

                                                                    #region Claims
                                                                    var roleID = string.Empty;
                                                                    roleID = httpContext
                                                                                        .User
                                                                                        .GetClaimTypeValueOrDefault
                                                                                            (
                                                                                                nameof(roleID)
                                                                                                , "AnonymousRole"
                                                                                            );
                                                                    var orgUnitID = string.Empty;
                                                                    orgUnitID = httpContext
                                                                                        .User
                                                                                        .GetClaimTypeValueOrDefault
                                                                                            (
                                                                                                nameof(orgUnitID)
                                                                                                , "AnonymousOrgUnit"
                                                                                            );
                                                                    var clientIP = httpContext
                                                                                        .Connection
                                                                                        .RemoteIpAddress
                                                                                        .ToString();
                                                                    var userID = httpContext
                                                                                        .User
                                                                                        .Identity
                                                                                        .Name ?? "AnonymousUser";
                                                                    var deviceID = string.Empty;
                                                                    deviceID = httpContext
                                                                                        .User
                                                                                        .GetClaimTypeValueOrDefault
                                                                                            (
                                                                                                nameof(deviceID)
                                                                                                , "AnonymousDevice"
                                                                                            );
                                                                    var deviceInfo = string.Empty;
                                                                    deviceInfo = httpContext
                                                                                        .User
                                                                                        .GetClaimTypeValueOrDefault
                                                                                            (
                                                                                                nameof(deviceInfo)
                                                                                                , "UnknownDevice"
                                                                                            );
                                                                    #endregion

                                                                    #region GlobalManager.AsyncRequestResponseLoggingProcessor.Enqueue
                                                                    GlobalManager
                                                                            .AsyncRequestResponseLoggingProcessor
                                                                            .Enqueue
                                                                                (
                                                                                    (
                                                                                        url
                                                                                        ,
                                                                                            (
                                                                                                requestHeaders
                                                                                                , requestBody
                                                                                                , request.Method
                                                                                                , requestBeginTime
                                                                                                , request.ContentLength
                                                                                                , request.ContentType
                                                                                            )
                                                                                        ,
                                                                                            (
                                                                                                responseHeaders
                                                                                                , responseBody
                                                                                                , response.StatusCode
                                                                                                , responseStartingTime
                                                                                                , response.ContentLength
                                                                                                , response.ContentType
                                                                                            )
                                                                                        ,
                                                                                            (
                                                                                                requestResponseTimingInMilliseconds
                                                                                                , dbExecutingTimingInMilliseconds
                                                                                            )
                                                                                        ,
                                                                                            (
                                                                                                (
                                                                                                    clientIP
                                                                                                    , decimal.Parse("1.0")
                                                                                                    , new decimal(1.0)
                                                                                                ) //Location
                                                                                                , userID
                                                                                                , roleID
                                                                                                , orgUnitID
                                                                                                ,
                                                                                                (
                                                                                                    deviceID
                                                                                                    , deviceInfo
                                                                                                ) // Device
                                                                                            ) //User
                                                                                    )
                                                                                );
                                                                    #endregion
                                                                }
                                                            );
                                                };
                                    #endregion

                                    #region middleware.OnAfterInvokedNextProcess
                                    middleware
                                        .OnAfterInvokedNextProcess
                                            =
                                                (
                                                    httpContext
                                                    , @event
                                                    , stopwatchesPool
                                                    , xConfiguration
                                                    , xLoggerFactory
                                                    , xLogger
                                                )
                                                    =>
                                                {
                                                    //Console.WriteLine($"event: {@event} @ {middlewareTypeName}");
                                                    //xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
                                                };
                                    #endregion

                                    #region middleware.OnResponseCompletedProcess
                                    middleware
                                        .OnResponseCompletedProcess
                                            =
                                                // don't support async
                                                (
                                                    httpContext
                                                    , @event
                                                    , stopwatchesPool
                                                    , xConfiguration
                                                    , xLoggerFactory
                                                    , xLogger
                                                )
                                                    =>
                                                {
                                                    //throw new Exception();
                                                    if
                                                        (
                                                            httpContext
                                                                    .Items
                                                                    .Remove
                                                                        (
                                                                            AbstractStoreProceduresExecutorControllerBase
                                                                                .requestJTokenParametersItemKey
                                                                            , out var removed
                                                                        )
                                                        )
                                                    {
                                                        if (removed is JToken parameters)
                                                        {
                                                            parameters = null;
                                                        }
                                                        removed = null;
                                                    }
                                                    //xLogger
                                                    //    .LogInformation($"event: {@event} @ {middlewareTypeName}");
                                                }; 
                                    #endregion
                                }
                            );
                #endregion
            }
            needUseMiddleware = configuration.GetValue("useExceptionGuard", false);
            if (needUseMiddleware)
            {
                #region ExceptionGuard
                app
                    .UseExceptionGuard<string>
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
                                            , xTInjector
                                        )
                                            =>
                                        {
                                            var reThrow = false;
                                            var errorDetails = true;
                                            var errorStatusCode = HttpStatusCode
                                                                            .InternalServerError;
                                            var errorResultCode = -1 * (int)errorStatusCode;
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
                                         (xHttpContext, xConfiguration, xCaughtException) =>
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

#if NETCOREAPPX_X //only for NETCOREAPP2_X
            #region SyncAsyncActionSelector 拦截处理
            app
                .UseCustomActionSelector<SyncOrAsyncActionSelector>
                    (
                        (actionSelector) =>
                        {
                            actionSelector
                                .OnSelectSyncOrAsyncActionCandidate =
                                    (routeContext, candidatesPair, _) =>
                                    {
                                        ActionDescriptor candidate = null;
                                        var type = typeof(AbstractStoreProceduresExecutorControllerBase);
                                        var asyncCandidate = candidatesPair.AsyncCandidate;
                                        var syncCandidate = candidatesPair.SyncCandidate;
                                        var r = type
                                                    .IsAssignableFrom
                                                        (
                                                            ((ControllerActionDescriptor) asyncCandidate)
                                                                .ControllerTypeInfo
                                                                .UnderlyingSystemType
                                                        );
                                        if (r)
                                        {
                                            r = type
                                                    .IsAssignableFrom
                                                        (
                                                            ((ControllerActionDescriptor) syncCandidate)
                                                                .ControllerTypeInfo
                                                                .UnderlyingSystemType
                                                        );
                                        }
                                        if (r)
                                        {
                                            var httpContext = routeContext
                                                                .HttpContext;
                                            var request = httpContext
                                                                .Request;
                                            var routeName = routeContext
                                                                .RouteData
                                                                .Values["routeName"]
                                                                .ToString();
                                            var httpMethod = $"Http{request.Method}";
                                            var isAsyncExecuting = false;

                                            var accessingConfigurationKey = "DefaultAccessing";
                                            if (request.Path.ToString().Contains("/export/", StringComparison.OrdinalIgnoreCase))
                                            {
                                                accessingConfigurationKey = "exporting";
                                            }
                                            var isAsyncExecutingConfiguration =
                                                        configuration
                                                            .GetSection($"Routes:{routeName}:{httpMethod}:{accessingConfigurationKey}:isAsyncExecuting");

                                            if (isAsyncExecutingConfiguration.Exists())
                                            {
                                                isAsyncExecuting = isAsyncExecutingConfiguration.Get<bool>();
                                            }
                                            candidate = (isAsyncExecuting ? asyncCandidate : syncCandidate);
                                        }
                                        return candidate;
                                    };
                        }
                    );
#endregion
#endif
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
            // Enable middleware to serve generated Swagger as a JSON endpoint.
            app
                .UseSwagger();
            // Enable middleware to serve swagger-ui (HTML, JS, CSS, etc.),
            // specifying the Swagger JSON endpoint.
            app
                .UseSwaggerUI
                    (
                        (c) =>
                        {
                            c
                                .SwaggerEndpoint
                                    (
                                        $"/swagger/{swaggerVersion}/swagger.json"
                                        , swaggerTitle
                                    );
                        }
                    ); 
            #endregion

            //app.UseEndpoints(endpoints =>
            //    {
            //        endpoints.MapControllers();
            //    });

            //app.UseHttpsRedirection();
        }
    }
}
