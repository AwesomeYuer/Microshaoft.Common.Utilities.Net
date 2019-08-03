#if NETCOREAPP2_X
namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Composition.Convention;
    using System.Composition.Hosting;
    using System.IO;
    using System.Linq;
    using System.Runtime.Loader;

    public static class ContainerConfigurationExtensions
    {
        public static ContainerConfiguration WithAssembliesByPath
                        (
                            this ContainerConfiguration configuration
                            , string path
                            , string searchPattern = "*.dll"
                            , SearchOption searchOption = SearchOption.TopDirectoryOnly
                        )
        {
            return WithAssembliesByPath(configuration, path, searchPattern, searchOption);
        }
        public static ContainerConfiguration WithAssembliesByPath
            (
                this ContainerConfiguration configuration
                , string path
                , AttributedModelProvider conventions
                , string searchPattern = "*.dll"
                , SearchOption searchOption = SearchOption.TopDirectoryOnly
            )
        {
            var assemblies =
                    Directory
                        .GetFiles
                            (
                                path
                                , searchPattern
                                , searchOption
                            )
                        //.Select
                        //    (
                        //        (x) =>
                        //        {
                        //            return
                        //            AssemblyLoadContext.GetAssemblyName(x);
                        //        }
                        //    )
                        .Select
                            (
                                (x) =>
                                {
                                    return
                                        AssemblyLoadContext
                                            .Default
                                            .LoadFromAssemblyPath(x);
                                }
                            )
                        .ToList();

            configuration = configuration
                                .WithAssemblies
                                    (
                                        assemblies
                                        , conventions
                                    );
            return configuration;
        }
    }
    public static class CompositionHelper
    {
        public static IEnumerable<T> ImportManyExportsComposeParts<T>
                    (
                        string path
                        , string searchPattern = "*.dll"
                        , SearchOption searchOption = SearchOption.AllDirectories
                    )
        {
            IEnumerable<T> result = null;
            var assemblies = 
                        Directory
                            .GetFiles
                                (
                                    path
                                    , searchPattern
                                    , searchOption
                                )
                            .Select
                                (
                                    AssemblyLoadContext
                                        .Default
                                        .LoadFromAssemblyPath
                                )
                            .ToList();
            var configuration = new ContainerConfiguration()
                                        .WithAssemblies
                                            (
                                                assemblies
                                            );
            using (var container = configuration.CreateContainer())
            {
                result = container.GetExports<T>();
            }
            return result;
        }
    }
}
#endif