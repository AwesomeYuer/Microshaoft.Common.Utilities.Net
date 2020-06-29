#if NETCOREAPP
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
                            this ContainerConfiguration @this
                            , string path
                            , string searchPattern = "*.dll"
                            , SearchOption searchOption = SearchOption.TopDirectoryOnly
                        )
        {
            return WithAssembliesByPath(@this, path, searchPattern, searchOption);
        }
        public static ContainerConfiguration WithAssembliesByPath
            (
                this ContainerConfiguration @this
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

            @this = @this
                        .WithAssemblies
                            (
                                assemblies
                                , conventions
                            );
            return @this;
        }
    }
    public static class CompositionHelper
    {
        public static IEnumerable<T> ImportManyExportsComposeParts<T>
                    (
                        string path
                        , string searchPattern = "*.dll"
                        , SearchOption searchOption = SearchOption.TopDirectoryOnly
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