//rem only for Windows/dos cmd
//rem xcopy ..\..\StoreProcedureWebApiExecutorsPlugins\MsSQL.StoreProcedureWebApiExecutor.Plugin\bin\Debug\netcoreapp2.2\*plugin* $(TargetDir)CompositionPlugins\ /Y
//rem xcopy ..\..\StoreProcedureWebApiExecutorsPlugins\MySQL.StoreProcedureWebApiExecutor.Plugin\bin\Debug\netcoreapp2.2\*plugin* $(TargetDir)CompositionPlugins\ /Y
//rem xcopy ..\..\JTokenModelParameterValidatorsPlugins\JTokenModelParameterValidatorSamplePlugin\bin\Debug\netcoreapp2.2\*plugin* $(TargetDir)CompositionPlugins\ /Y

namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microsoft.AspNetCore;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Logging;
    using Microsoft.Extensions.Primitives;
    using Newtonsoft.Json.Linq;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using System.Runtime.InteropServices;
    public class Program
    { 
        public static void Main(string[] args)
        {
            if (args != null)
            {
                if (args.Length > 0)
                {
                    if (args[0] == "/wait")
                    {
                        Console.WriteLine("Waiting ... ...");
                        Console.WriteLine("Press any key to continue ...");
                        Console.ReadLine();
                        Console.WriteLine("Continue ... ...");
                    }
                }
            }
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
            var executingDirectory = Path
                                        .GetDirectoryName
                                                (
                                                    Assembly
                                                        .GetExecutingAssembly()
                                                        .Location
                                                );
            var hostingsConfiguration = new ConfigurationBuilder()
                                                            .AddJsonFile
                                                                (
                                                                    "hostings.json"
                                                                    , optional: false
                                                                )
                                                            .Build();


            //兼容 Linux/Windows wwwroot 路径配置
            var wwwroot = GetExistsPaths
                                (
                                    "wwwrootpaths.json"
                                    , "wwwroot"
                                )
                                .FirstOrDefault();

            return
                WebHost
                    .CreateDefaultBuilder(args)
                    .UseConfiguration(hostingsConfiguration)
                    .ConfigureLogging
                        (
                            builder =>
                            {
                                builder
                                    .SetMinimumLevel(LogLevel.Error);
                                builder
                                    .AddConsole();
                            }
                        )
                    .ConfigureAppConfiguration
                        (
                            (hostingContext, configurationBuilder) =>
                            {
                                var configuration = configurationBuilder
                                                        .SetBasePath(executingDirectory)
                                                        //.AddJsonFile
                                                        //    (
                                                        //        path: "hostings.json"
                                                        //        , optional: true
                                                        //        , reloadOnChange: true
                                                        //    )
                                                        .AddJsonFile
                                                            (
                                                                path: "dbConnections.json"
                                                                , optional: false
                                                                , reloadOnChange: true
                                                            )
                                                        .AddJsonFile
                                                            (
                                                                path: "routes.json"
                                                                , optional: false
                                                                , reloadOnChange: true
                                                            )
                                                        .AddJsonFile
                                                            (
                                                                path: "dynamicCompositionPluginsPaths.json"
                                                                , optional: false
                                                                , reloadOnChange: true
                                                            )
                                                        .AddJsonFile
                                                            (
                                                                path: "JwtValidation.json"
                                                                , optional: false
                                                                , reloadOnChange: true
                                                            )
                                                        .Build();
                                // register change callback
                                ChangeToken
                                        .OnChange<JToken>
                                            (
                                                () =>
                                                {
                                                    return
                                                        configuration.GetReloadToken();
                                                }
                                                , (x) =>
                                                {
                                                    Console.WriteLine("Configuration changed");
                                                    configuration
                                                        .AsEnumerable()
                                                        .Select
                                                            (
                                                                (kvp) =>
                                                                {
                                                                    Console.WriteLine($"Key:{kvp.Key}, Value:{kvp.Value}");
                                                                    return
                                                                        kvp;
                                                                }
                                                            )
                                                        .ToArray();
                                                }
                                                , new JObject()
                                            );
                            }
                        )
                    //.UseUrls("http://+:5000", "https://+:5001")
                    .UseWebRoot
                        (
                            wwwroot
                        )
                    .UseStartup<Startup>();
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
                                                &&
                                                !r.StartsWith("..")
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