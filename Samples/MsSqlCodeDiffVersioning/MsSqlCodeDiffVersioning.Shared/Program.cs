namespace WebApplication.ASPNetCore
{
    using Microshaoft;
    using Microsoft.AspNetCore.Hosting;
    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.Hosting;
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
            Console.WriteLine($"{nameof(Environment.Version)}: {Environment.Version}");
            Console.WriteLine($"{nameof(RuntimeInformation.FrameworkDescription)}: {RuntimeInformation.FrameworkDescription}");

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
        public static
#if NETCOREAPP3_X
                IHostBuilder
#elif NETCOREAPP2_X
                IWebHostBuilder
#endif
                        CreateWebHostBuilder(string[] args)
        {
            var executingDirectory = Path
                                        .GetDirectoryName
                                                (
                                                    Assembly
                                                        .GetExecutingAssembly()
                                                        .Location
                                                );
            ////兼容 Linux/Windows wwwroot 路径配置
            var wwwroot = GetExistsPaths
                                (
                                    "wwwrootpaths.json"
                                    , "wwwroot"
                                )
                                .FirstOrDefault();

            return
#if NETCOREAPP2_X
                WebHost
#elif NETCOREAPP3_X
                Host
#endif
                    .CreateDefaultBuilder(args)

#if NETCOREAPP2_X
                    .UseConfiguration(hostingsConfiguration)
#elif NETCOREAPP3_X
                    .ConfigureWebHostDefaults
                        (
                            (webHostBuilder) =>
                            {
                                webHostBuilder
                                        .UseStartup<Startup>();
                                webHostBuilder
                                        .UseWebRoot(wwwroot);
                            }
                        )
#endif
                    //.ConfigureLogging
                    //    (
                    //        (loggingBuilder) =>
                    //        {

                    //            loggingBuilder
                    //                    .SetMinimumLevel(LogLevel.None);
                    //            //loggingBuilder
                    //            //    .AddConsole();
                    //        }
                    //    )
                    .ConfigureAppConfiguration
                        (
                            (hostingContext, configurationBuilder) =>
                            {
                                configurationBuilder
                                        .AddUserSecrets("2cf2bf85-4b45-4fa4-a64c-169d9e832d95");
                                configurationBuilder
                                        .SetBasePath(executingDirectory)
                                        .AddJsonFile
                                            (
                                                path: "dbConnections.json"
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
                                        .AddJsonFile
                                            (
                                                path: "ExportCsvFormatter.json"
                                                , optional: false
                                                , reloadOnChange: true
                                            )
                                        .AddJsonFile
                                            (
                                                path: "useMiddleWares.json"
                                                , optional: false
                                                , reloadOnChange: true
                                            )
                                        .AddJsonFile
                                            (
                                                path: "singleThreadAsyncDequeueProcessors.json"
                                                , optional: false
                                                , reloadOnChange: true
                                            )
                                        .AddJsonFile
                                            (
                                                path: "configurationSwitchAuthorizeFilter.json"
                                                , optional: false
                                                , reloadOnChange: true
                                            )

                                        //.AddJsonFile
                                        //    (
                                        //        path: "appsettings.json"
                                        //        , optional: false
                                        //        , reloadOnChange: true
                                        //    )
                                            
                                        ;
                                if
                                    (
                                        !args
                                            .Any
                                                (
                                                    (x) =>
                                                    {
                                                        return
                                                            (x == "--urls");
                                                    }
                                                )
                                    )
                                {
                                    configurationBuilder
                                                .AddJsonFile
                                                    (
                                                        path: "hostings.json"
                                                        , optional: true
                                                        , reloadOnChange: true
                                                    );
                                }
                                var files = EnumerableHelper
                                                        .Range
                                                            (
                                                                $@"{executingDirectory}\RoutesConfig\"      // for Windows
                                                                , $@"{executingDirectory}/RoutesConfig/"    // for Linux
                                                            )
                                                        .Where
                                                            (
                                                                (directory) =>
                                                                {
                                                                    return
                                                                        Directory
                                                                            .Exists(directory);
                                                                }
                                                            )
                                                        .SelectMany
                                                            (
                                                                (directory) =>
                                                                {
                                                                    return
                                                                        Directory
                                                                            .EnumerateFiles
                                                                                (
                                                                                    directory
                                                                                    , "*.json"
                                                                                );
                                                                }
                                                            )
                                                        .Where
                                                            (
                                                                (file) =>
                                                                {
                                                                    return
                                                                        !file
                                                                            .Contains
                                                                                (
                                                                                    ".development."
                                                                                    , StringComparison
                                                                                            .OrdinalIgnoreCase
                                                                                );
                                                                }
                                                            )
                                                        .Distinct
                                                            (
                                                                new PathFileNameComparer()
                                                            );

                                foreach (var file in files)
                                {
                                    configurationBuilder
                                                .AddJsonFile
                                                        (
                                                            path: file
                                                            , optional: false
                                                            , reloadOnChange: true
                                                        );
                                }

                                var configuration = configurationBuilder
                                                                        .Build();
                                // register change callback
                                ChangeToken
                                        .OnChange<JToken>
                                            (
                                                () =>
                                                {
                                                    return
                                                        configuration
                                                                .GetReloadToken();
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
#if NETCOREAPP2_X
                    //.UseUrls("http://+:5000", "https://+:5001")
                    .UseWebRoot
                        (
                            wwwroot
                        )
                    .UseStartup<Startup>()
#endif
                    ;
        }
        public static IEnumerable<string> GetExistsPaths(string configurationJsonFile, string sectionName)
        {
            var configurationBuilder = new ConfigurationBuilder();
            configurationBuilder
                        .AddJsonFile
                                (
                                    configurationJsonFile
                                );
            var configuration = configurationBuilder.Build();
            var executingDirectory = Path
                                        .GetDirectoryName
                                                (
                                                    Assembly
                                                        .GetExecutingAssembly()
                                                        .Location
                                                );
            var result = configuration
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