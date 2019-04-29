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
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Mvc.Controllers;
    using Microsoft.AspNetCore.Mvc.Infrastructure;
    using Microsoft.AspNetCore.Mvc.Internal;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using Microsoft.Extensions.Logging;
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
    using System.Threading.Tasks;

    public class Startup
    {
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
            services
                .AddMvc()
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
                            var jo = new JObject();
                            jo["udt_vcidt"] = ja;
                            var sqlConnection = new SqlConnection("Initial Catalog=test;Data Source=localhost;User=sa;Password=!@#123QWE");
                            executor
                                .Execute
                                    (
                                        sqlConnection
                                        , "zsp_Test"
                                        , jo
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
                                        ,
                                        (builder) =>
                                        {
                                            builder
                                                .AllowAnyOrigin()
                                                .AllowAnyHeader()
                                                .AllowAnyMethod();
                                        }
                                    );

                        }
                  );
            #endregion

            services.AddResponseCaching();

            services
                .AddSingleton<IActionSelector, SyncAsyncActionSelector>();

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
            string timingKey = nameof(timingKey);

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
                                                if (!httpContext.Items.ContainsKey(timingKey))
                                                {
                                                    stopwatchesPool.TryGet(out var stopwatch);
                                                    stopwatch.Start();
                                                    httpContext.Items[timingKey] = stopwatch;
                                                    stopwatch = null;
                                                }
                                            }
                                            return r;
                                        };
                                middleware
                                    .OnInvokingProcessAsync
                                        = async (stopwatchesPool, httpContext, @event) =>
                                        {
                                            Console.WriteLine($"event: {@event}");
                                            if (!httpContext.Items.ContainsKey(timingKey))
                                            {
                                                stopwatchesPool.TryGet(out var stopwatch);
                                                stopwatch.Start();
                                                httpContext.Items[timingKey] = stopwatch;
                                                stopwatch = null;
                                            }
                                            //var request = httpContext.Request;
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
                                                            ,
                                                            Message = errorMessage
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
                                                    Task.FromResult(result);
                                        };
                                middleware
                                    .OnResponseStartingProcess
                                        = (stopwatchesPool, httpContext, @event) =>
                                        {
                                            Console.WriteLine($"event: {@event}");
                                            var stopwatch = httpContext
                                                                .Items[timingKey] as Stopwatch;
                                            if (stopwatch != null)
                                            {
                                                stopwatch.Stop();
                                                var duration = stopwatch.ElapsedMilliseconds;
                                                httpContext
                                                    .Response
                                                    .Headers["X-Request-Response-Timing"]
                                                                = duration.ToString() + "ms";
                                                stopwatch.Reset();
                                                stopwatchesPool.TryPut(stopwatch);
                                                stopwatch = null;
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
                                            if
                                                (
                                                    httpContext
                                                        .Items
                                                        .Remove(timingKey, out var removed)
                                                )
                                            {
                                                removed = null;
                                            }
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
                app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseMvc();

            #region SyncAsyncActionSelector 拦截处理
            var actionSelector = (SyncAsyncActionSelector)
                                        app
                                            .ApplicationServices
                                            .GetServices<IActionSelector>()
                                            .First
                                                (
                                                    (x) =>
                                                    {
                                                        return
                                                            (
                                                                x.GetType()
                                                                ==
                                                                typeof(SyncAsyncActionSelector)
                                                            );
                                                    }
                                                );
            actionSelector
                .FilterControllerNamePrefixs = new string[] { "storeProcedureExecutor" };
            actionSelector
                .OnSelectSyncAsyncActionCandidate = (routeContext, candidates, _) =>
                {
                    var r = candidates
                                .All
                                    (
                                        (x) =>
                                        {
                                            var controllerActionDescriptor = (ControllerActionDescriptor)x;
                                            var rr = typeof(AbstractStoreProceduresExecutorControllerBase)
                                                        .IsAssignableFrom
                                                            (
                                                                controllerActionDescriptor
                                                                    .ControllerTypeInfo
                                                                    .UnderlyingSystemType
                                                            );
                                            return rr;
                                        }
                                    );
                    if (r)
                    {
                        var httpContext = routeContext.HttpContext;
                        var request = httpContext.Request;
                        var routeName = routeContext.RouteData.Values["routeName"].ToString();
                        var httpMethod = $"Http{request.Method}";
                        var isExecuteAsync = false;
                        var isExecuteAsyncConfiguration =
                                    configuration
                                        .GetSection($"Routes:{routeName}:{httpMethod}:IsExecuteAsync");
                        if (isExecuteAsyncConfiguration.Exists())
                        {
                            isExecuteAsync = isExecuteAsyncConfiguration.Get<bool>();
                        }
                        if (isExecuteAsync)
                        {
                            candidates = candidates
                                                .Where
                                                    (
                                                        (x) =>
                                                        {
                                                            return
                                                                x
                                                                    .RouteValues["action"]
                                                                    .EndsWith
                                                                        (
                                                                            "async"
                                                                            , StringComparison
                                                                                    .OrdinalIgnoreCase
                                                                        );
                                                        }
                                                    )
                                                .ToList()
                                                .AsReadOnly();
                        }
                        else
                        {
                            candidates = candidates
                                                .Where
                                                    (
                                                        (x) =>
                                                        {
                                                            return
                                                                !x
                                                                    .RouteValues["action"]
                                                                    .EndsWith
                                                                        (
                                                                            "async"
                                                                            , StringComparison
                                                                                .OrdinalIgnoreCase
                                                                        );
                                                        }
                                                    )
                                                .ToList()
                                                .AsReadOnly();
                        }
                    }
                    return candidates;
                }; 
            #endregion

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
            //兼容 Linux/Windows wwwroot 路径配置
            var wwwroot = GetExistsPaths
                                (
                                    "wwwrootpaths.json"
                                    , "wwwroot"
                                )
                                .FirstOrDefault();
            if (wwwroot.IsNullOrEmptyOrWhiteSpace())
            {
                app.UseStaticFiles();
            }
            else
            {
                app
                    .UseStaticFiles
                        (
                            new StaticFileOptions()
                            {
                                FileProvider = new PhysicalFileProvider
                                                        (
                                                            wwwroot
                                                        )
                                , RequestPath = ""
                            }
                        );
            }
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
            app.UseHttpsRedirection();
        }
        private static IEnumerable<string> GetExistsPaths(string configurationJsonFile, string sectionName)
        {
            var configurationBuilder =
                        new ConfigurationBuilder()
                                .AddJsonFile(configurationJsonFile);
            var configuration = configurationBuilder.Build();

            var executingDirectory =
                        Path
                            .GetDirectoryName
                                    (
                                        Assembly
                                            .GetExecutingAssembly()
                                            .Location
                                    );
            //executingDirectory = AppContext.BaseDirectory;
            var result =
                    configuration
                        .GetSection(sectionName)
                        .AsEnumerable()
                        .Select
                            (
                                (x) =>
                                {
                                    var r = x.Value;
                                    if (!r.IsNullOrEmptyOrWhiteSpace())
                                    {
                                        if
                                            (
                                                r.StartsWith(".")
                                            )
                                        {
                                            r = r.TrimStart('.', '\\', '/');
                                        }
                                        r = Path
                                                .Combine
                                                    (
                                                        executingDirectory
                                                        , r
                                                    );
                                    }
                                    return r;
                                }
                            )
                        .Where
                            (
                                (x) =>
                                {
                                    return
                                        (
                                            !x
                                                .IsNullOrEmptyOrWhiteSpace()
                                            &&
                                            Directory
                                                .Exists(x)
                                        );
                                }
                            );
            return result;
        }
    }
}
