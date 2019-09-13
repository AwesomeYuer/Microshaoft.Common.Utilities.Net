namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    public static class AssemblyHelper
    {
        public static IEnumerable<Type> GetAssembliesTypes
                                            (
                                                Func<Type, Assembly, bool> predicateFunc
                                            )
        {
            return
                AppDomain
                    .CurrentDomain
                        .GetAssemblies()
                            .SelectMany
                            (
                                (x) =>
                                {
                                    return
                                        x.GetLoadableTypes()
                                            .Select
                                            (
                                                (xx) =>
                                                {
                                                    return
                                                        //Tuple.Create<Type, Assembly>(xx, x);
                                                        new
                                                        {
                                                            CurrentAssembly = x
                                                            ,
                                                            CurrentType = xx
                                                        };
                                                }
                                            );
                                }
                            )
                            .Where
                            (
                                (x) =>
                                {
                                    return
                                        predicateFunc
                                            (
                                                x.CurrentType
                                                , x.CurrentAssembly
                                            );
                                }
                            )
                            .Select
                            (
                                (x) =>
                                {
                                    return x.CurrentType;
                                }
                            );
        }
        public static IEnumerable<Type> GetLoadableTypes(this Assembly assembly)
        {
            if (assembly == null)
            {
                throw new ArgumentNullException("assembly");
            }
            try
            {
                return assembly.GetTypes();
            }
            catch (ReflectionTypeLoadException e)
            {
                return e.Types.Where
                                (
                                    (x) =>
                                    {
                                        return x != null;
                                    }
                                );
            }
        }
    }
}
