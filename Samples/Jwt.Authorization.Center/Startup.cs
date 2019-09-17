namespace JwtAuthorizationCenterServer
{
    using Microsoft.AspNetCore.Builder;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.Server.IISIntegration;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
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
                                        )
            );
            services.AddAuthentication(IISDefaults.AuthenticationScheme);
            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                //app.UseHsts();
            }
            //app.UseAuthentication();
            //app.UseHttpsRedirection();

            app.UseCors("AllowAllOrigins");

            app
                .UseMvc
                    (
                        routes =>
                        {
                            routes.MapRoute(
                                name: "default",
                                template: "{controller=Home}/{action=Index}/{id?}");
                        }
                    );
        }
    }
}
