namespace WebApplication.ASPNetCore
{
//    using Google.Protobuf.WellKnownTypes;
    using Microshaoft;
    using Microshaoft.Web;
    using Microshaoft.WebApi.Controllers;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Http;
    using Microsoft.AspNetCore.Http.Features;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Abstractions;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Server.Kestrel.Core;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
 //   using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Options;
    using Microsoft.Net.Http.Headers;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Swashbuckle.AspNetCore.Swagger;
    using System;
    using System.Collections.Generic;
    using System.Data.SqlClient;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Net;
    using System.Reflection;
    using System.Text;
    using System.Threading.Tasks;

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
            //services.AddOptions<ExtendMvcOptions<CsvFormatterOptions>>();
            services
                .Configure<CsvFormatterOptions>
                    (
                        Configuration
                                .GetSection
                                    (
                                        "ExportCsvFormatter"
                                    )
                    );
            //services
            //    .AddSingleton<IConfigureOptions<MvcOptions>, CsvConfigurableMvcOptions>();

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
                .SetCompatibilityVersion
                    (
                        CompatibilityVersion
                            .Version_2_1

                    );
            services
              .AddSingleton
                    <JTokenParametersValidateFilterAttribute>
                        ();

#region 异步批量入库案例专用
            var processor =
                new SingleThreadAsyncDequeueProcessorSlim<JToken>();
            var executor = new MsSqlStoreProceduresExecutor();
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
                            //executor
                            //    .Execute
                            //        (
                            //            sqlConnection
                            //            , "zsp_Test"
                            //            , jo
                            //            , (reader, fieldType, fieldName, rowIndex, columnIndex) =>
                            //            {
                            //                return null;
                            //            }
                            //        );
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
                    //<
                    //     QueuedObjectsPool<Stopwatch>
                    //>
                    (
                        new QueuedObjectsPool<Stopwatch>(1024, true)
                    );

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

#if !NETCOREAPP3_X
            services
                .AddSingleton<IActionSelector, SyncOrAsyncActionSelector>();
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
                            //==================================================
                            // ??????????????????????????????????????????
                            options
                                .OutputFormatters
                                .Add
                                    (
                                        new CsvOutputFormatter
                                                    (
                                                        //csvFormatterOptions
                                                    )
                                    );
                            // ?????????????????????????????????????????
                            //===================================================
                            //options
                            //    .AddCsvOutputFormatter();
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

#if !NETCOREAPP3_X
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
                            , IHostingEnvironment env
                            , IConfiguration configuration
                            , ILoggerFactory loggerFactory
                        )
        {
            var logger = loggerFactory.CreateLogger("Microshaoft.Logger");
            string timingKey = "beginTimestamp";
            
            app
                .UseRequestResponseGuard
                    <QueuedObjectsPool<Stopwatch>>
                        (
                            (middleware) =>
                            {
                                middleware
                                    .OnFilterProcessFunc
                                        = (stopwatchesPool, httpContext, @event) =>
                                        {
                                            Console.WriteLine($"event: {@event}");
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
                                        = async (stopwatchesPool, httpContext, @event) =>
                                        {
                                            Console.WriteLine($"event: {@event}");
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
                                                var errorMessage = $"error in Middleware: [{middleware.GetType().Name}]";
                                                response.StatusCode = errorStatusCode;
                                                var jsonResult =
                                                        new
                                                        {
                                                            StatusCode = errorStatusCode
                                                            , Message = errorMessage
                                                        };
                                                var json = JsonConvert.SerializeObject(jsonResult);
                                                await
                                                    response
                                                        .WriteAsync
                                                                (json);
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
                                        = (stopwatchesPool, httpContext, @event) =>
                                        {
                                            Console.WriteLine($"event: {@event}");
                                            var r = httpContext
                                                        .Items
                                                        .Remove
                                                            (
                                                                timingKey
                                                                , out var removed
                                                            );
                                            if (r)
                                            {
                                                var valueTuple = 
                                                            (
                                                                (ValueTuple<DateTime, long>)
                                                                //(DateTime BeginTime, long BeginTimestamp)
                                                                    removed
                                                            );
                                                removed = null;
                                                var beginTime = valueTuple.Item1;
                                                httpContext
                                                    .Response
                                                    .Headers["X-Request-Receive-BeginTime"]
                                                                = beginTime.ToString(_dateTimeFormat);
                                                httpContext
                                                    .Response
                                                    .Headers["X-Response-Send-BeginTime"]
                                                                = DateTime.Now.ToString(_dateTimeFormat);
                                                //var duration = valueTuple.Item2.GetNowElapsedTime();
                                                httpContext
                                                    .Response
                                                    .Headers["X-Request-Response-Timing-In-Milliseconds"]
                                                                = valueTuple
                                                                        .Item2
                                                                        .GetNowElapsedTime()
                                                                        .TotalMilliseconds
                                                                        .ToString();
                                            }
                                        };
                                middleware
                                    .OnAfterInvokedNextProcess
                                        = (stopwatchesPool, httpContext, @event) =>
                                        {
                                            Console.WriteLine($"event: {@event}");
                                        };
                                middleware
                                    .OnResponseCompletedProcess
                                        = (stopwatchesPool, httpContext, @event) =>
                                        {
                                            Console.WriteLine($"event: {@event}");
                                        };
                            }
                        );
            app.UseCors();
            if (env.IsDevelopment())
            {
                app
                    .UseExceptionGuard<IConfiguration>
                        (
                            (middleware) =>
                            {
                                middleware
                                    .OnCaughtExceptionProcessFunc
                                        = (httpContext, injector, exception) =>
                                        {
                                            var r = 
                                                    (
                                                        false
                                                        , true
                                                        , HttpStatusCode
                                                                .InternalServerError
                                                    );
                                            logger
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
                                                                        exception
                                                                        , "yxy ++++++" + exception.Message
                                                                        , null
                                                                    );
                                                             return
                                                                log;
                                                        }
                                                    );
                                            return r;
                                        };
                            }
                        );
                //app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHsts();
            }
            //app.UseHttpsRedirection();

#if !NETCOREAPP3_X
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

            app.UseDefaultFiles
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

#if !NETCOREAPP3_X
            app.UseSwagger();
            app
                .UseSwaggerUI
                (
                    c =>
                    {
                        c
                            .SwaggerEndpoint
                                (
                                    "/swagger/v1/swagger.json"
                                    , "My API V1"
                                );
                    }
                );
#endif
            //app.UseHttpsRedirection();
        }
        
    }

    ////====================================================================================
    //public class ExtendMvcOptions<T> : MvcOptions
    //            where
    //                T : class, new()
    //{
    //    private T _options;
    //    public T Options
    //    { 
    //        get => _options;
    //        set => _options = value;
    //    }

    //    public ExtendMvcOptions(IOptions<T> iOptions)
    //    {
    //        _options = iOptions.Value;
    //    }
    //}
    //public static class MvcOptionsExtensions
    //{
    //    public static void AddCsvOutputFormatter(this MvcOptions target)
    //    {

    //        var extendMvcOptions =
    //                                (
    //                                    (ExtendMvcOptions<CsvFormatterOptions>)
    //                                        target
    //                                );
    //        extendMvcOptions
    //                    .OutputFormatters
    //                    .Add
    //                        (
    //                            new CsvOutputFormatter(extendMvcOptions.Options)        
    //                        );
    //    }
    //}
    //public class CsvConfigurableMvcOptions
    //                    : AbstractConfigurableMvcOptions<CsvFormatterOptions>
    //{
    //    public CsvConfigurableMvcOptions(IOptions<CsvFormatterOptions> iOptions)
    //                    : base(iOptions)
    //    { 
        
    //    }

    //    public override void OnConfigureProcess
    //            (
    //                AbstractConfigurableMvcOptions<CsvFormatterOptions> sender
    //                , MvcOptions mvcOptions
    //                , CsvFormatterOptions options
    //            )
    //    {
    //        mvcOptions
    //            .OutputFormatters
    //            .Add
    //                (
    //                    new CsvOutputFormatter
    //                    (
    //                        options    
    //                    )
    //                );
    //    }
    //}

         
    //public abstract class
    //            AbstractConfigurableMvcOptions<TOptions>
    //                                : IConfigureOptions<MvcOptions>
    //                                        where
    //                                            TOptions : class, new()
    //{
    //    public readonly TOptions Options;

    //    public AbstractConfigurableMvcOptions(IOptions<TOptions> iOptions)
    //    {
    //        Options = iOptions.Value;
    //    }

    //    public void Configure(MvcOptions mvcOptions)
    //    {
    //        OnConfigureProcess
    //                (
    //                    this
    //                    , mvcOptions
    //                    , Options
    //                );
    //    }

    //    public abstract void OnConfigureProcess
    //                    (
    //                        AbstractConfigurableMvcOptions<TOptions>  sender
    //                        , MvcOptions mvcOptions
    //                        , TOptions options
    //                    );
    //}

    
}
