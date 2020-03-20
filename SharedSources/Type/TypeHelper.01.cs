//#if NETFRAMEWORK4_X
namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    //using System.Linq.Expressions;
    using System.Reflection;
#if !PORTABLE
    using System.Reflection.Emit;
    using System.Runtime.CompilerServices;
#endif

    public static partial class TypeHelper
    {
      

        /// <param name="type">The type to construct.</param>
        /// <param name="getClosedGenericInterfaceType">
        /// For generic interfaces, the only way to reliably determine the implementing type's generic type arguments
        /// is to know the closed type of the desired interface implementation since there may be multiple implementations
        /// of the same generic interface on this type.
        /// </param>
        //public static Func<ResolutionContext, TServiceType> BuildCtor<TServiceType>(this Type type, Func<ResolutionContext, Type> getClosedGenericInterfaceType = null)
        //{
        //    return context =>
        //    {
        //        if (type.IsGenericTypeDefinition())
        //        {
        //            if (getClosedGenericInterfaceType == null) throw new ArgumentNullException(nameof(getClosedGenericInterfaceType), "For generic interfaces, the desired closed interface type must be known.");
        //            var closedInterfaceType = getClosedGenericInterfaceType.Invoke(context);
        //            var implementationTypeArguments = type.GetImplementedInterface(closedInterfaceType.GetGenericTypeDefinition(), closedInterfaceType.GenericTypeArguments).GenericTypeArguments;

        //            var genericParameters = type.GetTypeInfo().GenericTypeParameters;
        //            var deducedTypeArguments = new Type[genericParameters.Length];
        //            DeduceGenericArguments(genericParameters, deducedTypeArguments, implementationTypeArguments[0], context.SourceType);
        //            DeduceGenericArguments(genericParameters, deducedTypeArguments, implementationTypeArguments[1], context.DestinationType);

        //            if (deducedTypeArguments.Any(_ => _ == null)) throw new InvalidOperationException($"One or more type arguments to {type.Name} cannot be determined.");
        //            type = type.MakeGenericType(deducedTypeArguments);
        //        }

        //        var obj = context.Options.ServiceCtor.Invoke(type);

        //        return (TServiceType)obj;
        //    };
        //}

        private static void DeduceGenericArguments(Type[] genericParameters, Type[] deducedGenericArguments, Type typeUsingParameters, Type typeUsingArguments)
        {
            if (typeUsingParameters.IsByRef)
            {
                DeduceGenericArguments(genericParameters, deducedGenericArguments, typeUsingParameters.GetElementType(), typeUsingArguments.GetElementType());
                return;
            }

            var index = Array.IndexOf(genericParameters, typeUsingParameters);
            if (index != -1)
            {
                if (deducedGenericArguments[index] == null)
                {
                    deducedGenericArguments[index] = typeUsingArguments;
                }
                else if (deducedGenericArguments[index] != typeUsingArguments)
                {
                    throw new NotImplementedException("Generic variance is not implemented.");
                }
            }
            else if (typeUsingParameters.IsGenericType() && typeUsingArguments.IsGenericType())
            {
                var childArgumentsUsingParameters = typeUsingParameters.GenericTypeArguments;
                var childArgumentsUsingArguments = typeUsingArguments.GenericTypeArguments;
                for (var i = 0; i < childArgumentsUsingParameters.Length; i++)
                {
                    DeduceGenericArguments(genericParameters, deducedGenericArguments, childArgumentsUsingParameters[i], childArgumentsUsingArguments[i]);
                }
            }
        }

        public static Type GetImplementedInterface(this Type @this, Type interfaceDefinition, params Type[] interfaceGenericArguments)
        {
            return @this.GetTypeInfo().ImplementedInterfaces.Single(implementedInterface =>
            {
                if (implementedInterface.GetGenericTypeDefinition() != interfaceDefinition)
                {
                    return false;
                }
                var implementedInterfaceArguments = implementedInterface.GenericTypeArguments;
                for (var i = 0; i < interfaceGenericArguments.Length; i++)
                {
                    // This assumes the interface type parameters are not covariant or contravariant
                    if (implementedInterfaceArguments[i].GetGenericTypeDefinitionIfGeneric() != interfaceGenericArguments[i].GetGenericTypeDefinitionIfGeneric())
                    {
                        return false;
                    }
                }

                return true;
            });
        }

        public static Type GetGenericTypeDefinitionIfGeneric(this Type @this)
        {
            return @this.IsGenericType() ? @this.GetGenericTypeDefinition() : @this;
        }

        public static Type[] GetGenericParameters(this Type @this)
        {
            return @this.GetGenericTypeDefinition().GetTypeInfo().GenericTypeParameters;
        }

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredConstructors;
        }

#if !PORTABLE && !NETSTANDARD2_X
        public static Type CreateType(this TypeBuilder @this)
        {
            return @this.CreateTypeInfo().AsType();
        }
#endif

        public static IEnumerable<MemberInfo> GetDeclaredMembers(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredMembers;
        }

//#if PORTABLE
        public static IEnumerable<MemberInfo> GetAllMembers(this Type @this)
        {
            while (true)
            {
                IEnumerable<MemberInfo> declaredMembers = @this
                                                            .GetTypeInfo()
                                                            .DeclaredMembers;
                foreach (var memberInfo in declaredMembers)
                {
                    yield
                        return
                      
                        memberInfo;
                }

                @this = @this.BaseType();

                if (@this == null)
                {
                    yield break;
                }
            }
        }

        public static MemberInfo[] GetMember(this Type @this, string name)
        {
            return @this.GetAllMembers().Where(mi => mi.Name == name).ToArray();
        }
//#endif

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredMethods;
        }

//#if PORTABLE
        public static MethodInfo GetMethod(this Type @this, string name)
        {
            return @this.GetAllMethods().FirstOrDefault(mi => mi.Name == name);
        }

        public static MethodInfo GetMethod(this Type @this, string name, Type[] parameters)
        {
            return @this
                    .GetAllMethods()
                    .Where(mi => mi.Name == name)
                    .Where(mi => mi.GetParameters().Length == parameters.Length)
                    .FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameters));
        }
//#endif

        public static IEnumerable<MethodInfo> GetAllMethods(this Type @this)
        {
            return @this.GetRuntimeMethods();
        }

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredProperties;
        }

//#if PORTABLE
        public static PropertyInfo GetProperty(this Type @this, string name)
        {
            return @this.GetTypeInfo().DeclaredProperties.FirstOrDefault(mi => mi.Name == name);
        }
//#endif

        public static object[] GetCustomAttributes(this Type @this, Type attributeType, bool inherit)
        {
            return @this.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }

        public static bool IsStatic(this FieldInfo @this)
        {
            return @this?.IsStatic ?? false;
        }

        public static bool IsStatic(this PropertyInfo @this)
        {
            return
                @this?.GetGetMethod(true)?.IsStatic
                ?? @this?.GetSetMethod(true)?.IsStatic
                ?? false;
        }

        public static bool IsStatic(this MemberInfo @this)
        {
            return
                (
                    @this as FieldInfo).IsStatic()
                    || (@this as PropertyInfo).IsStatic()
                    || ((@this as MethodInfo)?.IsStatic
                    ?? false
                );
        }

        public static bool IsPublic(this PropertyInfo @this)
        {
            return
                (@this?.GetGetMethod(true)?.IsPublic ?? false)
                ||
                (@this?.GetSetMethod(true)?.IsPublic ?? false);
        }

        public static bool HasAnInaccessibleSetter(this PropertyInfo @this)
        {
            var setMethod = @this.GetSetMethod(true);
            return setMethod == null || setMethod.IsPrivate || setMethod.IsFamily;
        }

        public static bool IsPublic(this MemberInfo @this)
        {
            return (@this as FieldInfo)?.IsPublic ?? (@this as PropertyInfo).IsPublic();
        }

        public static bool IsAsync(this MethodInfo @this)
        {
            Type attributeType = typeof(AsyncStateMachineAttribute);
            var attribute = (AsyncStateMachineAttribute)
                                    @this
                                        .GetCustomAttribute(attributeType);
            var r = (attribute != null);
            return r;
        }

        public static bool IsNotPublic(this ConstructorInfo @this)
        {
            return
                    @this.IsPrivate
                    ||
                    @this.IsFamilyAndAssembly
                    ||
                    @this.IsFamilyOrAssembly
                    ||
                    @this.IsFamily;
        }

        public static Assembly Assembly(this Type @this)
        {
            return @this.GetTypeInfo().Assembly;
        }

        public static Type BaseType(this Type @this)
        {
            return @this.GetTypeInfo().BaseType;
        }

//#if PORTABLE
        public static bool IsAssignableFrom(this Type @this, Type other)
        {
            return @this.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        }
//#endif

        public static bool IsAbstract(this Type @this)
        {
            return @this.GetTypeInfo().IsAbstract;
        }

        public static bool IsClass(this Type @this)
        {
            return @this.GetTypeInfo().IsClass;
        }

        public static bool IsEnum(this Type @this)
        {
            return @this.GetTypeInfo().IsEnum;
        }

        public static bool IsGenericType(this Type @this)
        {
            return @this.GetTypeInfo().IsGenericType;
        }

        public static bool IsGenericTypeDefinition(this Type @this)
        {
            return @this.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static bool IsInterface(this Type @this)
        {
            return @this.GetTypeInfo().IsInterface;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static bool IsSealed(this Type @this)
        {
            return @this.GetTypeInfo().IsSealed;
        }

        public static bool IsValueType(this Type @this)
        {
            return @this.GetTypeInfo().IsValueType;
        }

        public static bool IsInstanceOfType(this Type @this, object o)
        {
            return o != null && @this.IsAssignableFrom(o.GetType());
        }

        public static ConstructorInfo[] GetConstructors(this Type @this)
        {
            return @this.GetTypeInfo().DeclaredConstructors.ToArray();
        }

        public static MethodInfo GetGetMethod(this PropertyInfo @this, bool ignored)
        {
            return @this.GetMethod;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo @this, bool ignored)
        {
            return @this.SetMethod;
        }

        public static FieldInfo GetField(this Type @this, string name)
        {
            return @this.GetRuntimeField(name);
        }
    }
}
//#endif
