namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.Controllers;
    using Microsoft.AspNetCore.Cors.Infrastructure;
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
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Web;
#if NETCOREAPP2_X
    using Microsoft.AspNetCore.Hosting;
#else
#endif
    public partial class Startup
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
            //Console.WriteLine($"Startup: {nameof(Thread.CurrentThread.ManagedThreadId)}:{Thread.CurrentThread.ManagedThreadId}");
            GlobalManager
                .AsyncRequestResponseLoggingProcessor
                .OnCaughtException += AsyncRequestResponseLoggingProcessor_OnCaughtException;

            var asyncProcessorConfigurationPrefixKeys = $"SingleThreadAsyncDequeueProcessors:AsyncRequestResponseLoggingProcessor";
            int sleepInMilliseconds = Configuration
                                            .GetValue
                                                (
                                                    $"{asyncProcessorConfigurationPrefixKeys}:{nameof(sleepInMilliseconds)}"
                                                    , 1000
                                                );
            int waitOneBatchTimeOutInMilliseconds =
                                    Configuration
                                            .GetValue
                                                (
                                                    $"{asyncProcessorConfigurationPrefixKeys}:{nameof(waitOneBatchTimeOutInMilliseconds)}"
                                                    , 1000
                                                );
            int waitOneBatchMaxDequeuedTimes =
                                    Configuration
                                            .GetValue
                                                (
                                                    $"{asyncProcessorConfigurationPrefixKeys}:{nameof(waitOneBatchMaxDequeuedTimes)}"
                                                    , 100
                                                );
            var msSqlStoreProceduresExecutor =
                    new MsSqlStoreProceduresExecutor(GlobalManager.ExecutingCachingStore)
                        {
                            CachedParametersDefinitionExpiredInSeconds
                                = Configuration
                                            .GetValue
                                                (
                                                    "CachedParametersDefinitionExpiredInSeconds"
                                                    , 3600
                                                )
                        };
            var connectionID = "c1";
            var connectionString = string.Empty;
            connectionString = Configuration.GetValue<string>($"connections:{connectionID}:{nameof(connectionString)}");
            // there are only one Thread that's DequeueThread write it, so it's security
            var jArrayData = new JArray();
            GlobalManager
                .AsyncRequestResponseLoggingProcessor
                .StartRunDequeueThreadProcess
                    (
                        (dequeued, batch, indexInBatch, queueElement) =>
                        {
                            //Console.WriteLine($"Dequeue Once: {nameof(Thread.CurrentThread.ManagedThreadId)}:{Thread.CurrentThread.ManagedThreadId}");
                            var (url, Request, Response, Timing, User) = queueElement.Element;
                            var enqueueTimestamp = queueElement
                                                            .Timing
                                                            .EnqueueTimestamp;
                            double? queueTimingInMilliseconds = null;
                            if (enqueueTimestamp.HasValue)
                            {
                                queueTimingInMilliseconds =
                                                enqueueTimestamp
                                                                .Value
                                                                .GetElapsedTimeToNow()
                                                                .TotalMilliseconds;
                            }
                            jArrayData
                                .Add
                                    (
                                        new JObject
                                        {
                                              { nameof(queueElement.ID)                             , queueElement.ID                                   }
                                            //======================================================================
                                            // Queue:
                                            , { nameof(queueElement.Timing.EnqueueTime)             , queueElement.Timing.EnqueueTime                   }
                                            , { nameof(queueElement.Timing.DequeueTime)             , queueElement.Timing.DequeueTime                   }
                                            , { nameof(queueTimingInMilliseconds)                   , queueTimingInMilliseconds                         }
                                            //=====================================================================
                                            // common
                                            , { nameof(url)                                         , url                                               }
                                            //=====================================================================
                                            // request:
                                            , { nameof(Request.requestHeaders)                      , Request.requestHeaders                            }
                                            , { nameof(Request.requestBody)                         , HttpUtility.UrlDecode(Request.requestBody)        }
                                            , { nameof(Request.requestMethod)                       , Request.requestMethod                             }
                                            , { nameof(Request.requestBeginTime)                    , Request.requestBeginTime                          }
                                            , { nameof(Request.requestContentLength)                , Request.requestContentLength                      }
                                            , { nameof(Request.requestContentType)                  , Request.requestContentType                        }

                                            //======================================================================
                                            // response:
                                            , { nameof(Response.responseHeaders)                    , Response.responseHeaders                          }
                                            , { nameof(Response.responseBody)                       , HttpUtility.UrlDecode(Response.responseBody)      }
                                            , { nameof(Response.responseStatusCode)                 , Response.responseStatusCode                       }
                                            , { nameof(Response.responseStartingTime)               , Response.responseStartingTime                     }
                                            , { nameof(Response.responseContentLength)              , Response.responseContentLength                    }
                                            , { nameof(Response.responseContentType)                , Response.responseContentType                      }
                                            
                                            //======================================================================
                                            // Timing :
                                            , { nameof(Timing.requestResponseTimingInMilliseconds)  , Timing.requestResponseTimingInMilliseconds        }
                                            , { nameof(Timing.dbExecutingTimingInMilliseconds)      , Timing.dbExecutingTimingInMilliseconds            }

                                            //======================================================================
                                            // Location:
                                            , { nameof(User.Location.clientIP)                      , User.Location.clientIP                            }
                                            , { nameof(User.Location.locationLongitude)             , User.Location.locationLongitude                   }
                                            , { nameof(User.Location.locationLatitude)              , User.Location.locationLatitude                    }
                                            
                                            //=======================================================================
                                            // user:
                                            , { nameof(User.userID)                                 , User.userID                                       }
                                            , { nameof(User.roleID)                                 , User.roleID                                       }
                                            , { nameof(User.orgUnitID)                              , User.orgUnitID                                    }
                                            , { nameof(User.Device.deviceID)                        , User.Device.deviceID                              }
                                            , { nameof(User.Device.deviceInfo)                      , User.Device.deviceInfo                            }
                                        }
                                    );
                        }
                        , (dequeued, batch, indexInBatch) =>
                        {
                            //Console.WriteLine($"Dequeue Batch: {nameof(Thread.CurrentThread.ManagedThreadId)}:{Thread.CurrentThread.ManagedThreadId}");
                            // sql Connection should be here avoid cross threads
                            var sqlConnection = new SqlConnection(connectionString);
                            var serverHost = string.Empty;
                            try
                            {
                                msSqlStoreProceduresExecutor
                                    .ExecuteJsonResults
                                        (
                                            sqlConnection
                                            , "zsp_Logging"
                                            , new JObject
                                                {
                                                      { $"{nameof(serverHost)}{nameof(GlobalManager.OsPlatformName)}"                       , GlobalManager.OsPlatformName                  }
                                                    , { $"{nameof(serverHost)}{nameof(GlobalManager.OsVersion)}"                            , GlobalManager.OsVersion                       }
                                                    , { $"{nameof(serverHost)}{nameof(GlobalManager.FrameworkDescription)}"                 , GlobalManager.FrameworkDescription            }
                                                    , { $"{nameof(serverHost)}{nameof(Environment.MachineName)}"                            , Environment.MachineName                       }
                                                    , { $"{nameof(serverHost)}ProcessId"                                                    , GlobalManager.CurrentProcess.Id               }
                                                    , { $"{nameof(serverHost)}{nameof(GlobalManager.CurrentProcess.ProcessName)}"           , GlobalManager.CurrentProcess.ProcessName      }
                                                    , { $"{nameof(serverHost)}ProcessStartTime"                                             , GlobalManager.CurrentProcess.StartTime        }
                                                    , { "data"                                                                              , jArrayData                                    }
                                                }
                                        );
                            }
                            finally
                            {
                                if (sqlConnection.State != ConnectionState.Closed)
                                {
                                    sqlConnection.Close();
                                }
                                //dataTable.Clear();
                                //should be clear correctly!!!!
                                jArrayData.Clear();
                            }
                        }
                        , sleepInMilliseconds
                        , waitOneBatchTimeOutInMilliseconds
                        , waitOneBatchMaxDequeuedTimes
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

        private bool AsyncRequestResponseLoggingProcessor_OnCaughtException
                        (
                            SingleThreadAsyncDequeueProcessorSlim<(string url, (string requestHeaders, string requestBody, string requestMethod, DateTime? requestBeginTime, long? requestContentLength, string requestContentType) Request, (string responseHeaders, string responseBody, int responseStatusCode, DateTime? responseStartingTime, long? responseContentLength, string responseContentType) Response, (double? requestResponseTimingInMilliseconds, double? dbExecutingTimingInMilliseconds) Timing, ((string clientIP, decimal? locationLongitude, decimal? locationLatitude) Location, string userID, string roleID, string orgUnitID, (string deviceID, string deviceInfo) Device) User)> sender
                            , Exception exception
                            , Exception newException
                            , string innerExceptionMessage
                        )
        {
            var rethrow = false;
            _logger
                .LogOnDemand
                        (
                            LogLevel
                                    .Error
                            , () =>
                            {
                                return
                                    (
                                        new EventId(-1000)
                                        , exception
                                        , $"Caught Exception on {nameof(GlobalManager.AsyncRequestResponseLoggingProcessor)} onBatchDequeuesProcessAction"
                                        , null
                                    );
                            }
                        );
            return rethrow;
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.

    }
}
