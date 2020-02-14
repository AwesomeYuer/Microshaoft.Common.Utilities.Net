namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Mvc.ApplicationModels;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.DependencyInjection.Extensions;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.Net.Http.Headers;
    //using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Data;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.Text;
    using System.Web;
#if NETCOREAPP2_X
    using Microsoft.AspNetCore.Hosting;
#else
#endif
    public partial class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }
        public IConfiguration Configuration
        {
            get;
        }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            ConfigurationHelper
                        .Load(Configuration);
#if NETCOREAPP3_X

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
            //var loggerFactory = LoggerFactory.Create
            //                                    (
            //                                        (loggingBuilder) =>
            //                                        {
            //                                            loggingBuilder
            //                                                .SetMinimumLevel(LogLevel.Debug)
            //                                                .AddConsole1();

            //                                        }
            //                                    );
            //loggerFactory.AddProvider(new LightConsoleLoggerProvider(null));

            #region Logging
            services
             .AddSingleton
                     (
                         GlobalManager.GlobalLoggerFactory
                     );
            services
                    .AddSingleton
                            (
                                GlobalManager.GlobalLogger
                            ); 
            #endregion

            services
                    .AddSingleton<ConfigurationSwitchAuthorizeFilter>();

            #region SwaggerGen
            services
                .AddSwaggerGenDefault(); 
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
                .OnCaughtException += 
                        (
                            sender
                            , exception
                            , newException
                            , innerExceptionMessage
                        )
                            =>
                        {
                            return
                                GlobalManager
                                        .OnCaughtExceptionProcessFunc
                                            (
                                                GlobalManager
                                                        .GlobalLogger
                                               , exception
                                               , newException
                                               , innerExceptionMessage
                                            );
                        };

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
            string connectionString = nameof(connectionString);
            connectionString = Configuration.GetValue<string>($"connections:{connectionID}:{connectionString}");
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
                            string serverHost = nameof(serverHost);
                            try
                            {
                                msSqlStoreProceduresExecutor
                                    .ExecuteJsonResults
                                        (
                                            sqlConnection
                                            , "zsp_Logging"
                                            , new JObject
                                                {
                                                      { $"{serverHost}{nameof(GlobalManager.OsPlatformName)}"                       , GlobalManager.OsPlatformName                  }
                                                    , { $"{serverHost}{nameof(GlobalManager.OsVersion)}"                            , GlobalManager.OsVersion                       }
                                                    , { $"{serverHost}{nameof(GlobalManager.FrameworkDescription)}"                 , GlobalManager.FrameworkDescription            }
                                                    , { $"{serverHost}{nameof(Environment.MachineName)}"                            , Environment.MachineName                       }
                                                    , { $"{serverHost}ProcessId"                                                    , GlobalManager.CurrentProcess.Id               }
                                                    , { $"{serverHost}{nameof(GlobalManager.CurrentProcess.ProcessName)}"           , GlobalManager.CurrentProcess.ProcessName      }
                                                    , { $"{serverHost}ProcessStartTime"                                             , GlobalManager.CurrentProcess.StartTime        }
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
                    .AddSingleton(Configuration);

            services
                    .AddSingleton("Inject String");

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

            #region JwtBearer Authentication
            services
                    .AddAuthentication
                        (
                            //(options) =>
                            //{
                            //    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                            //    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
                            //}
                            JwtBearerDefaults
                                        .AuthenticationScheme
                        )
                    .AddJwtBearer
                        (
                            (options) =>
                            {
                                options
                                    .TokenValidationParameters =
                                        new TokenValidationParameters()
                                        {
                                            //ValidIssuer = jwtSecurityToken.Issuer
                                            ValidateIssuer = false
                                            //, ValidAudiences = jwtSecurityToken.Audiences
                                            , ValidateAudience = false
                                            , IssuerSigningKey = GlobalManager.jwtSymmetricSecurityKey
                                            , ValidateIssuerSigningKey = true
                                            , ValidateLifetime = false
                                            //, ClockSkew = TimeSpan.FromSeconds(clockSkewInSeconds)
                                        };
                            }
                        ); 
            #endregion
        }
    }
}


