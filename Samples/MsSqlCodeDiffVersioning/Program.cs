namespace WebApplication.ASPNetCore
{
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using System.IO;

    public class Program
    {
        public static void Main(string[] args)
        {
            CreateWebHostBuilder
                (args)
                    //.UseKestrel()
                    .UseContentRoot(Directory.GetCurrentDirectory())
                    //.UseIISIntegration()
                    .Build()
                    .Run();
        }
        public static IWebHostBuilder CreateWebHostBuilder(string[] args)
        {
            var config =
                    new ConfigurationBuilder()
                            .AddJsonFile
                                (
                                    path: "hostings.json"
                                    , optional: false
                                    , reloadOnChange: true
                                )
                            .Build();
            return
                WebHost
                    .CreateDefaultBuilder(args)
                    .UseConfiguration(config)
                    //.UseUrls("http://+:5000")
                    .UseStartup<Startup>();
        }
        
    }
}