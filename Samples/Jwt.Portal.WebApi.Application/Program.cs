namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.FileProviders;
    using System;
    using System.IO;
    using System.Linq;
    using System.Runtime.InteropServices;

    //xcopy ..\..\StoreProcedureWebApiExecutorsPlugins\MsSQL.StoreProcedureWebApiExecutor.Plugin\bin\Debug\netcoreapp2.1\*.plugin.* $(TargetDir) CompositionPlugins\ /Y
    //xcopy ..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.StoreProcedureWebApiExecutor.Plugin\bin\Debug\netcoreapp2.1\*.plugin.* $(TargetDir) CompositionPlugins\ /Y


    public class Program
    {
        public static void Main(string[] args)
        {
            OSPlatform OSPlatform
                    = EnumerableHelper
                            .Range
                                (
                                    OSPlatform.Linux
                                    , OSPlatform.OSX
                                    , OSPlatform.Windows
                                )
                            .First
                                (
                                    (x) =>
                                    {
                                        return
                                            RuntimeInformation
                                                .IsOSPlatform(x);
                                    }
                                );
            var s = $"{nameof(RuntimeInformation.FrameworkDescription)}:{RuntimeInformation.FrameworkDescription}";
            s += "\n";
            s += $"{nameof(RuntimeInformation.OSArchitecture)}:{RuntimeInformation.OSArchitecture.ToString()}";
            s += "\n";
            s += $"{nameof(RuntimeInformation.OSDescription)}:{RuntimeInformation.OSDescription}";
            s += "\n";
            s += $"{nameof(RuntimeInformation.ProcessArchitecture)}:{RuntimeInformation.ProcessArchitecture.ToString()}";
            s += "\n";
            s += $"{nameof(OSPlatform)}:{OSPlatform}";
            Console.WriteLine(s);
            var os = Environment.OSVersion;
            Console.WriteLine("Current OS Information:\n");
            Console.WriteLine("Platform: {0:G}", os.Platform);
            Console.WriteLine("Version String: {0}", os.VersionString);
            Console.WriteLine("Version Information:");
            Console.WriteLine("   Major: {0}", os.Version.Major);
            Console.WriteLine("   Minor: {0}", os.Version.Minor);
            Console.WriteLine("Service Pack: '{0}'", os.ServicePack);
            CreateWebHostBuilder
                (args)
                    //.UseKestrel()
                    //.UseContentRoot(Directory.GetCurrentDirectory())
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
                            .AddJsonFile
                                (
                                    path: "JwtValidation.json"
                                    , optional: false
                                    , reloadOnChange: true
                                )
                            .AddJsonFile
                                (
                                    path: "dbConnections.json"
                                    , optional: false
                                    , reloadOnChange: true
                                )
                            .AddJsonFile
                                (
                                    path: "dynamicLoadExecutorsPaths.json"
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