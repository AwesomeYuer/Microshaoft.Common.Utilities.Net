namespace Swagger.WebApplication
{
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Hosting;
    public class Program
    {

        public const string defaultSecretKey = "Hello World!Hello World!Hello World!Hello World!Hello World!";

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
