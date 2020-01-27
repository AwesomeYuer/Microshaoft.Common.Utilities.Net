namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.Controllers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.ModelBinding.Metadata;
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
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Text.Json;
    using System.Threading.Tasks;
#if NETCOREAPP2_X
    using Microsoft.AspNetCore.Hosting;
#else
#endif


    public class Startup
    {
        private const string _dateTimeFormat = "yyyy-MM-dd HH:mm:ss.fffff";
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

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Version = "v1",
                    Title = "Microshaoft Store Procedures Executors API",
                    Description = "Microshaoft Store Procedures Executors API",
                    TermsOfService = new Uri("https://github.com/Microshaoft/Microshaoft.Common.Utilities.Net/blob/master/README.md"),
                    Contact = new OpenApiContact
                    {
                        Name = "Microshaoft",
                        Email = "Microshaoft@gmail.com",
                        Url = new Uri("https://github.com/Microshaoft"),
                    },
                    License = new OpenApiLicense
                    {
                        Name = "Use under License",
                        Url = new Uri("https://github.com/Microshaoft/Microshaoft.Common.Utilities.Net/blob/master/License.txt"),
                    }
                });

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
            });

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
                        option.EnableEndpointRouting = false;
                    }
#endif
                )
#if NETCOREAPP3_X
                .AddNewtonsoftJson()
#endif
                //.SetCompatibilityVersion
                //    (
                //        CompatibilityVersion
                //            .Version_2_1
                //    )
                ;

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

            //services
            //  .AddSingleton
            //        <JTokenParametersValidateFilterAttribute>
            //            ();

#region 异步批量入库案例专用
            var processor =
                new SingleThreadAsyncDequeueProcessorSlim<JToken>();
            ConcurrentDictionary<string, ExecutingInfo>
                        executingCachingStore
                                = new ConcurrentDictionary<string, ExecutingInfo>();
            services
                    .AddSingleton(executingCachingStore);
            var executor = new MsSqlStoreProceduresExecutor(executingCachingStore);
            processor
                .StartRunDequeueThreadProcess
                    (
                        (i, data) =>
                        {
                            //Debugger.Break();
                            var ja = new JArray(data);
                            var jo = new JObject
                            {
                                ["udt_vcidt"] = ja
                            };
                            var sqlConnection = new SqlConnection("Initial Catalog=test;Data Source=localhost;User=sa;Password=!@#123QWE");
                            executor
                                .ExecuteJsonResults
                                    (
                                        sqlConnection
                                        , "zsp_Test"
                                        , jo
                                        ,
                                            (
                                                resultSetIndex
                                                , reader
                                                , rowIndex
                                                , columnIndex
                                                , fieldType
                                                , fieldName
                                            )
                                        =>
                                            {
                                                return (true, null);
                                            }
                                    );
                        }
                        , null
                        , 1000
                        , 10 * 1000
                    );
            services
                .AddSingleton
                    //<SingleThreadAsyncDequeueProcessorSlim<JToken>>
                    (
                        processor
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
                    <
                       QueuedObjectsPool<Stopwatch>
                    >
                    (
                            new QueuedObjectsPool<Stopwatch>(1024, true)
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
            services.AddSingleton(loggerFactory);
            services.AddSingleton(logger);



            services.AddSingleton<string>("Inject String");

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
            //services
            //    .AddSingleton<IActionSelector, SyncOrAsyncActionSelector>();
#endif

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

#if NETCOREAPP2_X
            services
                .AddSwaggerGen
                    (
                        c =>
                        {
                            c
                                .SwaggerDoc
                                    (
                                        "v1"
                                        , new Info
                                        {
                                            Title = "My API"
                                            , Version = "v1"
                                        }
                                    );
                        }
                    );
#endif
            services
                .Configure<KestrelServerOptions>
                    (
                        (options) =>
                        {
                            options.AllowSynchronousIO = true;
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
                        //, ILogger logger
                        )
        {

            string timingKey = "beginTimestamp";
            timingKey = string.Empty;
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
                                            if (timingKey.IsNullOrEmptyOrWhiteSpace())
                                            {
                                                return false;
                                            }

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
                                                                timingKey
                                                                ,
                                                                    (
                                                                        BeginTime : DateTime.Now
                                                                        , BeginTimestamp : Stopwatch.GetTimestamp()
                                                                    )
                                                            );
                                            }
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
                                                        , resultCode = -1 * errorStatusCode
                                                        , message = errorMessage
                                                    };
                                                    await
                                                        JsonSerializer
                                                                .SerializeAsync
                                                                        (
                                                                            response.Body
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
                                                var r = httpContext
                                                                .Items
                                                                .Remove
                                                                    (
                                                                        timingKey
                                                                        , out var removed
                                                                    );
                                                if (r)
                                                {
                                                    (
                                                        DateTime beginTime
                                                        , long beginTimeStamp
                                                    )
                                                        = (ValueTuple<DateTime, long>) removed;
                                                    removed = null;
                                                    httpContext
                                                        .Response
                                                        .Headers["X-Request-Receive-BeginTime"]
                                                                    = beginTime.ToString(_dateTimeFormat);
                                                    httpContext
                                                        .Response
                                                        .Headers["X-Response-Send-BeginTime"]
                                                                    = DateTime.Now.ToString(_dateTimeFormat);
                                                    httpContext
                                                        .Response
                                                        .Headers["X-Request-Response-Timing-In-Milliseconds"]
                                                                    = beginTimeStamp
                                                                            .GetElapsedTimeToNow()
                                                                            .TotalMilliseconds
                                                                            .ToString();
                                                }
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
                                                xLogger
                                                    .LogInformation($"event: {@event} @ {middlewareTypeName}");
                                            };
                            }
                        );
            app.UseCors();
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
                                        var errorResultCode = -1 * (int) errorStatusCode;
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

            if (1 == 0)
            {
                app.UseExceptionHandler
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
                                                , -1 * (int) HttpStatusCode
                                                                    .InternalServerError
                                                , errorMessage
                                             );
                                     }
                                }.Invoke
                            }
                        );
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
                                        "/swagger/v1/swagger.json"
                                        , "My API V1"
                                    );
                        }
                    );
            //app.UseEndpoints(endpoints =>
            //    {
            //        endpoints.MapControllers();
            //    });

            //app.UseHttpsRedirection();
        }
    }
}
