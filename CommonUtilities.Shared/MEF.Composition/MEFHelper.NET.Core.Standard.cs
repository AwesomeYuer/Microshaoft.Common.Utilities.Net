#if NETCOREAPP2_X
namespace Microshaoft
{
    using System.Composition.Convention;
    using System.Composition.Hosting;
    using System.IO;
    using System.Reflection;
    using System.Runtime.Loader;
    using System.Linq;
    using System.Collections.Generic;

    public static class ContainerConfigurationExtensions
    {



        public static ContainerConfiguration WithAssembliesByPath(this ContainerConfiguration configuration, string path, SearchOption searchOption = SearchOption.TopDirectoryOnly)
        {
            return WithAssembliesByPath(configuration, path, null, searchOption);
        }

        public static ContainerConfiguration WithAssembliesByPath
            (
                this ContainerConfiguration configuration
                , string path
                , AttributedModelProvider conventions
                , SearchOption searchOption = SearchOption.TopDirectoryOnly
            )
        {
            var assemblies = Directory
                .GetFiles(path, "*.dll", searchOption)
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

            configuration = configuration.WithAssemblies(assemblies, conventions);

            return configuration;
        }
    }
    public static class CompositionHelper
    {
        
        public static IEnumerable<T> ImportManyExportsComposeParts<T>
            //(string path,  T attributedPart)
            (
                //this ContainerConfiguration configuration
                string path
                
                , SearchOption searchOption = SearchOption.TopDirectoryOnly
            )
        {
            IEnumerable<T> result = null;
           
            var assemblies = Directory
                        .GetFiles(path, "*.dll", SearchOption.AllDirectories)
                        .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                        .ToList();
            var configuration = new ContainerConfiguration()
                .WithAssemblies(assemblies);
            using (var container = configuration.CreateContainer())
            {
                result = container.GetExports<T>();
            }
            return result;
            //var containerConfiguration = new ContainerConfiguration();
            //var conventions = new ConventionBuilder();
            //conventions
            //    .ForTypesDerivedFrom<T>()
            //    .Export<T>()
            //    .Shared();
            //var configuration = new ContainerConfiguration()
            //    .WithAssembliesByPath(path, conventions, searchOption);

            //var conventions = new ConventionBuilder();
            //conventions
            //    .ForTypesDerivedFrom<T>()
            //    .Export<T>()
            //    .Shared();

            //var assemblies = new[] { typeof(T).GetTypeInfo().Assembly };

            //var configuration = new ContainerConfiguration()
            //    .WithAssemblies(assemblies, conventions);
        }
    }
}

#endif