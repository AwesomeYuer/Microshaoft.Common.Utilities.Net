namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.Controllers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.Extensions.Logging;
    using Microsoft.Net.Http.Headers;
    using Microsoft.OpenApi.Models;
    //using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Text;
    using System.Text.Json;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
#if NETCOREAPP2_X
    using Microsoft.AspNetCore.Hosting;
#else
#endif
    public class Startup
    {
        
        private const string defaultDateTimeFormat = "yyyy-MM-dd HH:mm:ss.fffff";
        private const string swaggerVersion = "v3.1.101";
        private const string swaggerTitle = "Microshaoft Store Procedures Executors API";
        private const string swaggerDescription = "Powered By Microshaoft.Common.Utilities.Net";

        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration
        {
            get;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            ConfigurationHelper
                            .Load(Configuration);

            #region SwaggerGen
            services
                .AddSwaggerGen
                    (
                        (c) =>
                        {
                            c
                                .SwaggerDoc
                                    (
                                        swaggerVersion
                                        , new OpenApiInfo
                                                {
                                                    Version = swaggerVersion
                                                    , Title = swaggerTitle
                                                    , Description = swaggerDescription
                                                    , TermsOfService = new Uri("https://github.com/Microshaoft/Microshaoft.Common.Utilities.Net/blob/master/README.md")
                                                    , Contact = new OpenApiContact
                                                                    {
                                                                        Name = "Microshaoft"
                                                                        , Email = "Microshaoft@gmail.com"
                                                                        , Url = new Uri("https://github.com/Microshaoft")
                                                                        ,
                                                                    }
                                                    , License = new OpenApiLicense
                                                                    {
                                                                        Name = "Use under License"
                                                                        , Url = new Uri("https://github.com/Microshaoft/Microshaoft.Common.Utilities.Net/blob/master/License.txt")
                                                                        ,
                                                                    }
                                                }
                                    );
                                    // Set the comments path for the Swagger JSON and UI.
                                    //var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                                    //var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                                    //c.IncludeXmlComments(xmlPath);
                            c
                                .ResolveConflictingActions
                                    (
                                        (xApiDescriptions) =>
                                        {
                                            return
                                                xApiDescriptions
                                                            .First();
                                        }
                                    );
                        }
                    ); 
            #endregion

            services
                .Configure<CsvFormatterOptions>
                    (
                        Configuration
                                .GetSection
                                    (
                                        "ExportCsvFormatter"
                                    )
                    );

            services
                .AddMvc
                (
#if NETCOREAPP3_X
                    (option) =>
                    {
                        option
                            .EnableEndpointRouting = false;
                    }
#endif
                )
#if NETCOREAPP3_X
                .AddNewtonsoftJson()
#endif
                ;

            #region ConfigurableActionConstrainedRouteApplicationModelProvider
            // for both NETCOREAPP2_X and NETCOREAPP3_X
            // for Sync or Async Action Selector
            services
                .TryAddEnumerable
                    (
                        ServiceDescriptor
                            .Singleton
                                <
                                    IApplicationModelProvider
                                    , ConfigurableActionConstrainedRouteApplicationModelProvider
                                                                            <ConstrainedRouteAttribute>
                                >
                            (
                                (x) =>
                                {
                                    return
                                        new ConfigurableActionConstrainedRouteApplicationModelProvider
                                                                                <ConstrainedRouteAttribute>
                                                (
                                                    Configuration
                                                    , (attribute) =>
                                                    {
                                                        return
                                                            new ConfigurableActionConstraint
                                                                        <ConstrainedRouteAttribute>
                                                                    (
                                                                        attribute
                                                                        , (actionConstraintContext, constrainedRouteAttribute) =>
                                                                        {
                                                                            var r = (actionConstraintContext.Candidates.Count == 1);
                                                                            if (!r)
                                                                            {
                                                                                var routeContext = actionConstraintContext.RouteContext;
                                                                                var httpContext = routeContext
                                                                                                        .HttpContext;
                                                                                var request = httpContext
                                                                                                    .Request;
                                                                                var type = typeof(AbstractStoreProceduresExecutorControllerBase);
                                                                                var currentCandidateAction = actionConstraintContext
                                                                                                                        .CurrentCandidate
                                                                                                                        .Action;

                                                                                var isAsyncExecuting = ((ControllerActionDescriptor)currentCandidateAction)
                                                                                                                    .MethodInfo
                                                                                                                    .IsAsync();
                                                                                var routeName = routeContext
                                                                                                        .RouteData
                                                                                                        .Values["routeName"]
                                                                                                        .ToString();
                                                                                var httpMethod = $"Http{request.Method}";
                                                                                var isAsyncExecutingInConfiguration = false;

                                                                                var accessingConfigurationKey = "DefaultAccessing";
                                                                                if (request.Path.ToString().Contains("/export/", StringComparison.OrdinalIgnoreCase))
                                                                                {
                                                                                    accessingConfigurationKey = "exporting";
                                                                                }

                                                                                if
                                                                                    (
                                                                                        constrainedRouteAttribute
                                                                                                        .Configuration
                                                                                                        .TryGetSection
                                                                                                            (
                                                                                                                $"Routes:{routeName}:{httpMethod}:{accessingConfigurationKey}:isAsyncExecuting"
                                                                                                                , out var isAsyncExecutingConfiguration
                                                                                                            )
                                                                                    )
                                                                                {
                                                                                    isAsyncExecutingInConfiguration =
                                                                                        isAsyncExecutingConfiguration
                                                                                                                .Get<bool>();
                                                                                }
                                                                                r =
                                                                                    (
                                                                                        isAsyncExecutingInConfiguration
                                                                                        ==
                                                                                        isAsyncExecuting
                                                                                    );
                                                                            }
                                                                            return r;
                                                                        }
                                                                    );
                                                    }
                                                );
                                }
                            )
                    );
            #endregion

            services
                    .AddSingleton
                        (
                            GlobalManager
                                .ExecutingCachingStore
                        );

            #region asyncRequestResponseLoggingProcessor
            services
                    .AddSingleton
                        (
                            GlobalManager
                                    .AsyncRequestResponseLoggingProcessor
                        );
            // there are only one Thread that's DequeueThread write it, so it's security
            var jArrayData = new JArray();
            var msSqlStoreProceduresExecutor =
                    new MsSqlStoreProceduresExecutor(GlobalManager.ExecutingCachingStore)
                    { 
                        CachedParametersDefinitionExpiredInSeconds = 10
                    };
            Console.WriteLine($"Startup: {nameof(Thread.CurrentThread.ManagedThreadId)}:{Thread.CurrentThread.ManagedThreadId}");
            GlobalManager
                .AsyncRequestResponseLoggingProcessor
                .StartRunDequeueThreadProcess
                    (
                        (dequeued, batch, indexInBatch, queueElement) =>
                        {
                            Console.WriteLine($"Dequeue Once: {nameof(Thread.CurrentThread.ManagedThreadId)}:{Thread.CurrentThread.ManagedThreadId}");
                            var element = queueElement.Element;
                            var enqueueTimestamp = queueElement.Timing.EnqueueTimestamp;
                            var queueTimingInMilliseconds =
                                                    (
                                                        enqueueTimestamp.HasValue
                                                        ?
                                                        enqueueTimestamp.Value.GetElapsedTimeToNow().TotalMilliseconds
                                                        :
                                                        -1
                                                    );
                            jArrayData
                                .Add
                                    (
                                        new JObject
                                        {
                                              { nameof(queueElement.ID)                                 , queueElement.ID                                           }
                                            //======================================================================
                                            // Queue:
                                            , { nameof(queueElement.Timing.EnqueueTime)                 , queueElement.Timing.EnqueueTime                           }
                                            , { nameof(queueElement.Timing.DequeueTime)                 , queueElement.Timing.DequeueTime                           }
                                            , { nameof(queueTimingInMilliseconds)                       , queueTimingInMilliseconds                                 }
                                            //=====================================================================
                                            // common
                                            , { nameof(element.url)                                     , element.url                                               }
                                            //=====================================================================
                                            // request:
                                            , { nameof(element.Request.requestHeaders)                  , element.Request.requestHeaders                            }
                                            , { nameof(element.Request.requestBody)                     , HttpUtility.UrlDecode(element.Request.requestBody)        }
                                            , { nameof(element.Request.requestMethod)                   , element.Request.requestMethod                             }
                                            , { nameof(element.Request.requestBeginTime)                , element.Request.requestBeginTime                          }
                                            , { nameof(element.Request.requestContentLength)            , element.Request.requestContentLength                      }
                                            , { nameof(element.Request.requestContentType)              , element.Request.requestContentType                        }

                                            //======================================================================
                                            // response:
                                            , { nameof(element.Response.responseHeaders)                , element.Response.responseHeaders                          }
                                            , { nameof(element.Response.responseBody)                   , HttpUtility.UrlDecode(element.Response.responseBody)      }
                                            , { nameof(element.Response.responseStatusCode)             , element.Response.responseStatusCode                       }
                                            , { nameof(element.Response.responseStartingTime)           , element.Response.responseStartingTime                     }
                                            , { nameof(element.Response.responseContentLength)          , element.Response.responseContentLength                    }
                                            , { nameof(element.Response.responseContentType)            , element.Response.responseContentType                      }
                                            //======================================================================
                                            // Timing :
                                            , { nameof(element.Timing.requestResponseTimingInMilliseconds)  , element.Timing.requestResponseTimingInMilliseconds    }
                                            , { nameof(element.Timing.dbExecutingTimingInMilliseconds)      , element.Timing.dbExecutingTimingInMilliseconds        }

                                            //======================================================================
                                            // Location:
                                            , { nameof(element.User.Location.clientIP)                  , element.User.Location.clientIP                            }
                                            , { nameof(element.User.Location.locationLongitude)         , element.User.Location.locationLongitude                   }
                                            , { nameof(element.User.Location.locationLatitude)          , element.User.Location.locationLatitude                    }
                                            //=======================================================================
                                            // user:
                                            , { nameof(element.User.userID)                             , element.User.userID                                       }
                                            , { nameof(element.User.roleID)                             , element.User.roleID                                       }
                                            , { nameof(element.User.orgUnitID)                          , element.User.orgUnitID                                    }
                                            , { nameof(element.User.Device.deviceID)                    , element.User.Device.deviceID                              }
                                            , { nameof(element.User.Device.deviceInfo)                  , element.User.Device.deviceInfo                            }
                                        }
                                    );
                        }
                        , (dequeued, batch, indexInBatch) =>
                        {
                            Console.WriteLine($"Dequeue Batch: {nameof(Thread.CurrentThread.ManagedThreadId)}:{Thread.CurrentThread.ManagedThreadId}");
                            try
                            {
                                // sql Connection should be here avoid cross threads
                                var sqlConnection = new SqlConnection("Initial Catalog=test;Data Source=gateway.hyper-v.internal\\sql2019,11433;User=sa;Password=!@#123QWE");

                                try
                                {
                                    msSqlStoreProceduresExecutor
                                        .ExecuteJsonResults
                                            (
                                                sqlConnection
                                                , "zsp_Logging"
                                                , new JObject
                                                    {
                                                          { "serverHostOsPlatformName"              , GlobalManager.OsPlatformName                  }
                                                        , { "serverHostOsVersion"                   , GlobalManager.OsVersion                       }
                                                        , { "serverHostFrameworkDescription"        , GlobalManager.FrameworkDescription            }
                                                        , { "serverHostMachineName"                 , Environment.MachineName                       }
                                                        , { "serverHostProcessId"                   , GlobalManager.CurrentProcess.Id               }
                                                        , { "serverHostProcessName"                 , GlobalManager.CurrentProcess.ProcessName      }
                                                        , { "serverHostProcessStartTime"            , GlobalManager.CurrentProcess.StartTime        }
                                                        , { "data"                                  , jArrayData                                    }
                                                    }
                                            );
                                }
                                finally
                                {
                                    if (sqlConnection.State != ConnectionState.Closed)
                                    {
                                        sqlConnection.Close();
                                    }
                                }
                            }
                            finally
                            {
                                //dataTable.Clear();
                                //should be clear correctly!!!!
                                jArrayData.Clear();
                            }
                        }
                        , 200
                        , 5000
                        , 10
                    );
            #endregion

            services
                .AddSingleton
                    <
                        AbstractStoreProceduresService
                        , StoreProceduresExecuteService
                    >
                    ();

            services
                    .AddSingleton
                        (
                            new QueuedObjectsPool<Stopwatch>(16, true)
                        );
            services
                    .AddSingleton
                        (
                               Configuration
                        );

#if NETCOREAPP3_X
            var loggerFactory = LoggerFactory
                                            .Create
                                                (
                                                    builder =>
                                                    {
                                                        builder
                                                        //    .AddFilter("Microsoft", LogLevel.Warning)
                                                        //    .AddFilter("System", LogLevel.Warning)
                                                        //    .AddFilter("LoggingConsoleApp.Program", LogLevel.Debug)
                                                            .AddConsole()
                                                        //    .AddEventLog()
                                                            ;
                                                    }
                                                );
#else
            services
                .AddLogging
                (
                    builder =>
                    {
                        builder
                            .AddConsole()
                            //.AddFilter(level => level >= LogLevel.Information)
                            ;
                    }
                );
            var loggerFactory = services
                                    .BuildServiceProvider()
                                    .GetService<ILoggerFactory>();
#endif
            
            ILogger logger = loggerFactory.CreateLogger("Microshaoft.Logger");
            services
                    .AddSingleton(loggerFactory);
            services
                    .AddSingleton(logger);

            services
                    .AddSingleton<string>("Inject String");

            #region 跨域策略
            services
                    .Add
                        (
                            ServiceDescriptor
                                .Transient<ICorsService, WildcardCorsService>()
                        );
            services
                .AddCors
                    (
                        (options) =>
                        {
                            options
                                .AddPolicy
                                    (
                                        "SPE"
                                        , (builder) =>
                                        {
                                            builder
                                                .WithOrigins
                                                    (
                                                        "*.microshaoft.com"
                                                    );
                                        }
                                    );
                            // BEGIN02
                            options
                                .AddPolicy
                                    (
                                        "AllowAllAny"
                                        , (builder) =>
                                        {
                                            builder
                                                .AllowAnyOrigin()
                                                .AllowAnyHeader()
                                                .AllowAnyMethod()
                                                .WithExposedHeaders("*");
                                        }
                                    );
                        }
                  );
            #endregion

            services
                    .AddResponseCaching();

#if NETCOREAPP2_X
            //for NETCOREAPP2_X only
            services
                  .AddSingleton<IActionSelector, SyncOrAsyncActionSelector>();
#endif

            #region Output CsvFormatterOptions
            services
                    .AddMvc
                        (
                            (options) =>
                            {
                                var csvFormatterOptions = new CsvFormatterOptions
                                {
                                    CsvColumnsDelimiter = ",",
                                    IncludeExcelDelimiterHeader = false,
                                    UseSingleLineHeaderInCsv = true
                                };
                                if
                                    (
                                        Configuration
                                                    .TryGetSection
                                                        (
                                                            "ExportCsvFormatter"
                                                            , out var exportCsvFormatterConfiguration
                                                        )
                                    )
                                {
                                    IConfigurationSection section;
                                    if
                                        (
                                            exportCsvFormatterConfiguration
                                                    .TryGetSection
                                                        (
                                                            nameof(csvFormatterOptions.CsvColumnsDelimiter)
                                                            , out section
                                                        )
                                        )
                                    {
                                        csvFormatterOptions
                                                .CsvColumnsDelimiter = section.Value;
                                    }
                                    if
                                        (
                                            exportCsvFormatterConfiguration
                                                    .TryGetSection
                                                        (
                                                            nameof(csvFormatterOptions.DateFormat)
                                                            , out section
                                                        )
                                        )
                                    {
                                        csvFormatterOptions
                                                .DateFormat = section.Value;
                                    }
                                    if
                                        (
                                            exportCsvFormatterConfiguration
                                                    .TryGetSection
                                                        (
                                                            nameof(csvFormatterOptions.DateTimeFormat)
                                                            , out section
                                                        )

                                        )
                                    {
                                        csvFormatterOptions
                                                .DateTimeFormat = section.Value;
                                    }
                                    if
                                        (
                                            exportCsvFormatterConfiguration
                                                    .TryGetSection
                                                        (
                                                            nameof(csvFormatterOptions.DigitsTextSuffix)
                                                            , out section
                                                        )
                                        )
                                    {
                                        csvFormatterOptions
                                                .DigitsTextSuffix = section.Value;
                                    }
                                    if
                                        (
                                            exportCsvFormatterConfiguration
                                                    .TryGetSection
                                                        (
                                                            nameof(csvFormatterOptions.MinExclusiveLengthDigitsTextSuffix)
                                                            , out section
                                                        )
                                        )
                                    {
                                        csvFormatterOptions
                                                .MinExclusiveLengthDigitsTextSuffix = section.Get<int>();
                                    }
                                    if
                                        (
                                            exportCsvFormatterConfiguration
                                                    .TryGetSection
                                                        (
                                                            nameof(csvFormatterOptions.Encoding)
                                                            , out section
                                                        )
                                        )
                                    {
                                        csvFormatterOptions
                                                .Encoding = Encoding.GetEncoding(section.Value);
                                    }
                                    if
                                        (
                                            exportCsvFormatterConfiguration
                                                    .TryGetSection
                                                        (
                                                            nameof(csvFormatterOptions.IncludeExcelDelimiterHeader)
                                                            , out section
                                                        )
                                        )
                                    {
                                        csvFormatterOptions
                                                .IncludeExcelDelimiterHeader = section.Get<bool>();
                                    }
                                    if
                                        (
                                            exportCsvFormatterConfiguration
                                                    .TryGetSection
                                                        (
                                                            nameof(csvFormatterOptions.UseSingleLineHeaderInCsv)
                                                            , out section
                                                        )
                                        )
                                    {
                                        csvFormatterOptions
                                                 .UseSingleLineHeaderInCsv = section.Get<bool>();
                                    }
                                }
                                //options.InputFormatters.Add(new CsvInputFormatter(csvFormatterOptions));
                                options
                                    .OutputFormatters
                                    .Add
                                        (
                                            new CsvOutputFormatter()
                                        );
                                options
                                    .FormatterMappings
                                    .SetMediaTypeMappingForFormat
                                        (
                                            "csv"
                                            , MediaTypeHeaderValue
                                                    .Parse
                                                        (
                                                            "text/csv"
                                                        )
                                        );
                            }
                        ); 
            #endregion

            services
                    .Configure<KestrelServerOptions>
                        (
                            (options) =>
                            {
                                options
                                        .AllowSynchronousIO = true;
                            }
                        );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

        public void Configure
                        (
                            IApplicationBuilder app
                            ,
#if NETCOREAPP2_X
                            IHostingEnvironment
#else
                            IWebHostEnvironment
#endif
                                env
                            , IConfiguration configuration
                            , ILoggerFactory loggerFactory

                            ,
                                ConcurrentDictionary<string, ExecutingInfo>
                                                                executingCachingStore
                                //, ILogger logger
                        )
        {


            string requestResponseTimingLoggingItemKey = "beginTimestamp";
            //timingKey = string.Empty;

            var needUse = configuration.GetValue("useRequestResponseGuard", false);
            if (needUse)
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
                                        middleware
                                            .OnFilterProcessFunc
                                                = (httpContext, @event, stopwatchesPool, xConfiguration, xLoggerFactory, xLogger) =>
                                                {
                                                    if
                                                        (
                                                            requestResponseTimingLoggingItemKey
                                                                            .IsNullOrEmptyOrWhiteSpace()
                                                        )
                                                    {
                                                        return false;
                                                    }
                                                    var request = httpContext.Request;
                                                    request.EnableBuffering();
                                                    xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
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
                                                                        requestResponseTimingLoggingItemKey
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
                                                        xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
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
                                                                ,
                                                                resultCode = -1 * errorStatusCode
                                                                ,
                                                                message = errorMessage
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
                                                       xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");

                                                   //return;
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
                                                           requestBody = (string)removedRequestBody;
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
                                                           Console.WriteLine(responseBody.Length);
                                                       }
                                                       var r = httpContext
                                                               .Items
                                                               .Remove
                                                                   (
                                                                       "dbExecutingDuration"
                                                                       , out var removed

                                                                   );
                                                       double dbExecutingTimingInMilliseconds = -1;
                                                       if (r)
                                                       {
                                                           TimeSpan? timespan = removed as TimeSpan?;
                                                           if (timespan != null)
                                                           {
                                                               if (timespan.HasValue)
                                                               {
                                                                   dbExecutingTimingInMilliseconds = timespan.Value.TotalMilliseconds;
                                                               }
                                                           }




                                                       }
                                                       removed = null;
                                                       r = httpContext
                                                                       .Items
                                                                       .Remove
                                                                           (
                                                                               requestResponseTimingLoggingItemKey
                                                                               , out removed
                                                                           );
                                                       double requestResponseTimingInMilliseconds = -1;
                                                       DateTime? requestBeginTime = null;
                                                       DateTime? responseStartingTime = null;
                                                       if (r)
                                                       {
                                                           (
                                                               DateTime beginTime
                                                               , long beginTimeStamp
                                                           )
                                                               = (ValueTuple<DateTime, long>)removed;
                                                           removed = null;
                                                           requestBeginTime = beginTime;
                                                           response
                                                               .Headers["X-Request-Receive-BeginTime"]
                                                                           = beginTime.ToString(defaultDateTimeFormat);

                                                           responseStartingTime = DateTime.Now;
                                                           response
                                                               .Headers["X-Response-Send-BeginTime"]
                                                                           = responseStartingTime
                                                                                               .Value
                                                                                               .ToString(defaultDateTimeFormat);

                                                           requestResponseTimingInMilliseconds = beginTimeStamp
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
                                                   };
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
                                                    xLogger.LogInformation($"event: {@event} @ {middlewareTypeName}");
                                                    };

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
                                                       xLogger
                                                           .LogInformation($"event: {@event} @ {middlewareTypeName}");
                                                   };
                                    }
                                );
                #endregion
            }
            needUse = configuration.GetValue("useExceptionGuard", false);
            if (needUse)
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
                                                xLogger
                                                    .LogError($"event: exception @ {middlewareTypeName}");
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

                                                xLogger
                                                    .LogOnDemand
                                                        (
                                                            LogLevel.Error
                                                            , () =>
                                                            {
                                                                (
                                                                    Exception LoggingException
                                                                    , string LoggingMessage
                                                                    , object[] LoggingArguments
                                                                )
                                                                    log =
                                                                        (
                                                                            xException
                                                                            , $"LogOnDemand : {xException.ToString()}"
                                                                            , null
                                                                        );
                                                                return
                                                                       log;
                                                            }
                                                        );
                                            //Console.WriteLine($"event: exception @ {middlewareTypeName}");

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

            app.UseCors();

            if (1 == 1)
            {
                #region ExceptionHandler
                app
                    .UseExceptionHandler
                            (
                                new ExceptionHandlerOptions()
                                {
                                    ExceptionHandler = new ExceptionOnDemandHandlerMiddleware
                                                            (
                                                                env
                                                                , configuration
                                                            )
                                    {
                                        OnCaughtExceptionHandleProcess =
                                         (xHttpContext, xConfiguration, xCaughtException) =>
                                         {
                                             var errorMessage = nameof(HttpStatusCode.InternalServerError);
                                             var errorDetails = true;
                                             if (errorDetails)
                                             {
                                                 errorMessage = xCaughtException.ToString();
                                             }
                                             return
                                                 (
                                                    errorDetails
                                                    , HttpStatusCode
                                                                .InternalServerError
                                                    , -1 * (int)HttpStatusCode
                                                                        .InternalServerError
                                                    , errorMessage
                                                 );
                                         }
                                    }.Invoke
                                }
                            ); 
                #endregion
                //app.UseDeveloperExceptionPage();
            }
            //else
            {
                //app.UseHsts();
            }
            //app.UseHttpsRedirection();

#if NETCOREAPPX_X
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
