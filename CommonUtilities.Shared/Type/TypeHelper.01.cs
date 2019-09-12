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

        public static Type GetImplementedInterface(this Type target, Type interfaceDefinition, params Type[] interfaceGenericArguments)
        {
            return target.GetTypeInfo().ImplementedInterfaces.Single(implementedInterface =>
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

        public static Type GetGenericTypeDefinitionIfGeneric(this Type target)
        {
            return target.IsGenericType() ? target.GetGenericTypeDefinition() : target;
        }

        public static Type[] GetGenericParameters(this Type target)
        {
            return target.GetGenericTypeDefinition().GetTypeInfo().GenericTypeParameters;
        }

        public static IEnumerable<ConstructorInfo> GetDeclaredConstructors(this Type target)
        {
            return target.GetTypeInfo().DeclaredConstructors;
        }

#if !PORTABLE && !NETSTANDARD2_X
        public static Type CreateType(this TypeBuilder target)
        {
            return target.CreateTypeInfo().AsType();
        }
#endif

        public static IEnumerable<MemberInfo> GetDeclaredMembers(this Type target)
        {
            return target.GetTypeInfo().DeclaredMembers;
        }

//#if PORTABLE
        public static IEnumerable<MemberInfo> GetAllMembers(this Type target)
        {
            while (true)
            {
                IEnumerable<MemberInfo> declaredMembers = target
                                                            .GetTypeInfo()
                                                            .DeclaredMembers;
                foreach (var memberInfo in declaredMembers)
                {
                    yield
                        return
                      
                        memberInfo;
                }

                target = target.BaseType();

                if (target == null)
                {
                    yield break;
                }
            }
        }

        public static MemberInfo[] GetMember(this Type target, string name)
        {
            return target.GetAllMembers().Where(mi => mi.Name == name).ToArray();
        }
//#endif

        public static IEnumerable<MethodInfo> GetDeclaredMethods(this Type target)
        {
            return target.GetTypeInfo().DeclaredMethods;
        }

//#if PORTABLE
        public static MethodInfo GetMethod(this Type target, string name)
        {
            return target.GetAllMethods().FirstOrDefault(mi => mi.Name == name);
        }

        public static MethodInfo GetMethod(this Type target, string name, Type[] parameters)
        {
            return target
                    .GetAllMethods()
                    .Where(mi => mi.Name == name)
                    .Where(mi => mi.GetParameters().Length == parameters.Length)
                    .FirstOrDefault(mi => mi.GetParameters().Select(pi => pi.ParameterType).SequenceEqual(parameters));
        }
//#endif

        public static IEnumerable<MethodInfo> GetAllMethods(this Type target)
        {
            return target.GetRuntimeMethods();
        }

        public static IEnumerable<PropertyInfo> GetDeclaredProperties(this Type target)
        {
            return target.GetTypeInfo().DeclaredProperties;
        }

//#if PORTABLE
        public static PropertyInfo GetProperty(this Type target, string name)
        {
            return target.GetTypeInfo().DeclaredProperties.FirstOrDefault(mi => mi.Name == name);
        }
//#endif

        public static object[] GetCustomAttributes(this Type target, Type attributeType, bool inherit)
        {
            return target.GetTypeInfo().GetCustomAttributes(attributeType, inherit).ToArray();
        }

        public static bool IsStatic(this FieldInfo target)
        {
            return target?.IsStatic ?? false;
        }

        public static bool IsStatic(this PropertyInfo target)
        {
            return
                target?.GetGetMethod(true)?.IsStatic
                ?? target?.GetSetMethod(true)?.IsStatic
                ?? false;
        }

        public static bool IsStatic(this MemberInfo target)
        {
            return
                (
                    target as FieldInfo).IsStatic()
                    || (target as PropertyInfo).IsStatic()
                    || ((target as MethodInfo)?.IsStatic
                    ?? false
                );
        }

        public static bool IsPublic(this PropertyInfo target)
        {
            return
                (target?.GetGetMethod(true)?.IsPublic ?? false)
                ||
                (target?.GetSetMethod(true)?.IsPublic ?? false);
        }

        public static bool HasAnInaccessibleSetter(this PropertyInfo target)
        {
            var setMethod = target.GetSetMethod(true);
            return setMethod == null || setMethod.IsPrivate || setMethod.IsFamily;
        }

        public static bool IsPublic(this MemberInfo target)
        {
            return (target as FieldInfo)?.IsPublic ?? (target as PropertyInfo).IsPublic();
        }

        public static bool IsAsync(this MethodInfo target)
        {
            Type attributeType = typeof(AsyncStateMachineAttribute);
            var attribute = (AsyncStateMachineAttribute)
                                    target
                                        .GetCustomAttribute(attributeType);
            var r = (attribute != null);
            return r;
        }

        public static bool IsNotPublic(this ConstructorInfo target)
        {
            return
                    target.IsPrivate
                    ||
                    target.IsFamilyAndAssembly
                    ||
                    target.IsFamilyOrAssembly
                    ||
                    target.IsFamily;
        }

        public static Assembly Assembly(this Type target)
        {
            return target.GetTypeInfo().Assembly;
        }

        public static Type BaseType(this Type target)
        {
            return target.GetTypeInfo().BaseType;
        }

//#if PORTABLE
        public static bool IsAssignableFrom(this Type target, Type other)
        {
            return target.GetTypeInfo().IsAssignableFrom(other.GetTypeInfo());
        }
//#endif

        public static bool IsAbstract(this Type target)
        {
            return target.GetTypeInfo().IsAbstract;
        }

        public static bool IsClass(this Type target)
        {
            return target.GetTypeInfo().IsClass;
        }

        public static bool IsEnum(this Type target)
        {
            return target.GetTypeInfo().IsEnum;
        }

        public static bool IsGenericType(this Type target)
        {
            return target.GetTypeInfo().IsGenericType;
        }

        public static bool IsGenericTypeDefinition(this Type target)
        {
            return target.GetTypeInfo().IsGenericTypeDefinition;
        }

        public static bool IsInterface(this Type target)
        {
            return target.GetTypeInfo().IsInterface;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static bool IsSealed(this Type target)
        {
            return target.GetTypeInfo().IsSealed;
        }

        public static bool IsValueType(this Type target)
        {
            return target.GetTypeInfo().IsValueType;
        }

        public static bool IsInstanceOfType(this Type target, object o)
        {
            return o != null && target.IsAssignableFrom(o.GetType());
        }

        public static ConstructorInfo[] GetConstructors(this Type target)
        {
            return target.GetTypeInfo().DeclaredConstructors.ToArray();
        }

        public static MethodInfo GetGetMethod(this PropertyInfo target, bool ignored)
        {
            return target.GetMethod;
        }

        public static MethodInfo GetSetMethod(this PropertyInfo target, bool ignored)
        {
            return target.SetMethod;
        }

        public static FieldInfo GetField(this Type target, string name)
        {
            return target.GetRuntimeField(name);
        }
    }
}
//#endif
