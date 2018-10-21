namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microshaoft.Web;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Cors.Infrastructure;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Server.IISIntegration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.FileProviders;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;

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
            // 允许跨域
            services.AddCors
                        (
                            options =>
                                options
                                    .AddPolicy
                                        (
                                            "AllowAllOrigins"
                                            , builder =>
                                                builder
                                                    //.WithOrigins("127.0.0.1:52184")
                                                    .AllowAnyMethod()
                                                    .AllowAnyHeader()
                                                    .AllowAnyOrigin()
                                                    .AllowCredentials()
                                                    // 允许浏览器添加的 http request header
                                                    .WithExposedHeaders
                                                        (
                                                            //"Microshaoft-Authorization-Bearer",
                                                            "*"
                                                        )
                                        )
            );

            services.AddAuthentication(IISDefaults.AuthenticationScheme);
            services
                .AddMvc()
                .SetCompatibilityVersion
                    (
                        CompatibilityVersion
                            .Version_2_1
                    );
            services
                //.AddTransient
                .AddSingleton
                    <
                        IStoreProceduresWebApiService
                        , StoreProceduresExecuteService
                    >
                    ();

            //services
            //    .Add
            //        (
            //            ServiceDescriptor
            //                .Transient<ICorsService, WildcardCorsService>()
            //        );

            //services
            //    .AddCors
            //        (
            //            (options) =>
            //            {
            //                options
            //                    .AddPolicy
            //                        (
            //                            "SPE"
            //                            , (builder) =>
            //                            {
            //                                builder
            //                                    .WithOrigins
            //                                        (
            //                                            "*.microshaoft.com"
            //                                        );
            //                            }
            //                        );
            //                // BEGIN02
            //                options
            //                    .AddPolicy
            //                        (
            //                            "AllowAllOrigins"
            //                            ,
            //                            (builder) =>
            //                            {
            //                                builder.AllowAnyOrigin();
            //                            }
            //                        );

            //            }
            //      );
            //services.AddResponseCaching();
        }
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // 允许的跨域策略
            app.UseCors("AllowAllOrigins");
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHsts();
            }
            //app.UseHttpsRedirection();
            app.UseMvc(
                    routes =>
                    {
                        routes.MapRoute(
                            name: "default",
                            template: "{controller=Home}/{action=Index}/{id?}");
                    }
                );
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
                                ,
                                RequestPath = ""
                            }
                        );
            }
            //app.Use(async (context, next) =>
            //{
            //    context.Response.GetTypedHeaders().CacheControl =
            //        new Microsoft.Net.Http.Headers.CacheControlHeaderValue()
            //        {
            //            Public = true,
            //            MaxAge = TimeSpan.FromSeconds(10)
            //        };
            //    context.Response.Headers[Microsoft.Net.Http.Headers.HeaderNames.Vary] =
            //        new string[] { "Accept-Encoding" };
            //    await next();
            //});
            //app.UseResponseCaching();
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
