namespace Swagger.WebApplication
{
    using Microsoft.AspNetCore.Authentication.JwtBearer;
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using Microsoft.Extensions.Hosting;
    using Microsoft.IdentityModel.Tokens;
    using Microsoft.OpenApi.Models;
    using System;
    using System.Linq;
    using System.Text;
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

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers()
                .AddNewtonsoftJson()
                ;

            var symmetricSecurityKey = new SymmetricSecurityKey
                                                (
                                                    Encoding
                                                            .UTF8
                                                            .GetBytes
                                                                (
                                                                    Program.defaultSecretKey
                                                                )
                                                );
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
                                            //ValidIssuer = jwtSecurityToken.Issuer,
                                            ValidateIssuer = false,
                                            //ValidAudiences = jwtSecurityToken.Audiences,
                                            ValidateAudience = false,
                                            IssuerSigningKey = symmetricSecurityKey,
                                            ValidateIssuerSigningKey = true,
                                            //ValidateLifetime = validateLifetime,
                                            //ClockSkew = TimeSpan.FromSeconds(clockSkewInSeconds)
                                        };
                            }
                        );

            //services
            //        .AddAuthorization
            //            (
            //                (options) =>
            //                {
            //                    options
            //                        .AddPolicy
            //                            (
            //                                "CheeseburgerPolicy"
            //                                , policy => policy.RequireClaim("icanhazcheeseburger", "true")
            //                            );
            //                }
            //            );

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
                                                    ,
                                            Title = swaggerTitle
                                                    ,
                                            Description = swaggerDescription
                                                    ,
                                            TermsOfService = new Uri("https://github.com/Microshaoft/Microshaoft.Common.Utilities.Net/blob/master/README.md")
                                                    ,
                                            Contact = new OpenApiContact
                                            {
                                                Name = "Microshaoft"
                                                                        ,
                                                Email = "Microshaoft@gmail.com"
                                                                        ,
                                                Url = new Uri("https://github.com/Microshaoft")
                                                                        ,
                                            }
                                                    ,
                                            License = new OpenApiLicense
                                            {
                                                Name = "Use under License"
                                                                        ,
                                                Url = new Uri("https://github.com/Microshaoft/Microshaoft.Common.Utilities.Net/blob/master/License.txt")
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            string SecretKey = "seriouslyneverleavethissittinginyourcode";
            SymmetricSecurityKey signingKey =
                new SymmetricSecurityKey(Encoding.ASCII.GetBytes(SecretKey));

            //loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            //loggerFactory.AddDebug();

            



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


            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
