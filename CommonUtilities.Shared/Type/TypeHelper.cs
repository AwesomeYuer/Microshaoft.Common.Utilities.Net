namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.ComponentModel;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Reflection;
    using System.Threading.Tasks;
    public class MemberAccessor//<TContext>
    {
        public Func<object, object> Getter;
        public Action<object, object> Setter;
        public MemberInfo Member;
        public MemberTypes? Types;
        public MemberAdditionalDefinitionAttribute DefinitionAttribute;
        public string Name;
        public string Key;
        public Type MemberType;
    }

    public static partial class TypeHelper
    {
        private static IEnumerable<Type>
                            _typesWhiteList
                                = new List<Type>()
                                            {
                                                typeof(int)
                                                //, typeof(int?)
                                                , typeof(long)
                                                //, typeof(long?)
                                                , typeof(string)
                                                , typeof(DateTime)
                                                , typeof(Guid)
                                                , typeof(bool)
                                                , typeof(double)
                                                , typeof(float)
                                                , typeof(uint)
                                                //, typeof(DateTime?)
                                            }
                                        .Concat
                                            (
                                                AssemblyHelper
                                                    .GetAssembliesTypes
                                                        (
                                                            (x, y) =>
                                                            {
                                                                return
                                                                    (
                                                                        x.Namespace == "System.Data.SqlTypes"
                                                                        &&
                                                                        x.IsValueType
                                                                        &&
                                                                        typeof(INullable).IsAssignableFrom(x)
                                                                    );
                                                            }
                                                        )
                                            );

        public static IEnumerable<MemberInfo> GetMembersByMemberType<TTarget, TMember>()
        {
            var targetType = typeof(TTarget);
            var memberType = typeof(TMember);
            return
                GetMembersByMemberType
                        (
                            targetType
                            , memberType
                        );
        }
        public static IEnumerable<MemberInfo> GetMembersByMemberType(Type targetType, Type memberType, bool includeIndexer = false)
        {
            return
                targetType
                    .GetMembers()
                    .Where
                        (
                            (x) =>
                            {
                                Type type = null;
                                var r = false;
                                if (x is PropertyInfo)
                                {
                                    var propertyInfo = x as PropertyInfo;
                                    type = propertyInfo.PropertyType;
                                    var parameters = propertyInfo.GetIndexParameters();
                                    if (!includeIndexer)
                                    {
                                        if (parameters != null && parameters.Length > 0)
                                        {
                                            type = null;
                                        }
                                    }
                                }
                                else if (x is FieldInfo)
                                {
                                    var fieldInfo = x as FieldInfo;
                                    type = fieldInfo.FieldType;
                                }
                                if (type != null)
                                {
                                    r =
                                        (
                                            type
                                            ==
                                            memberType
                                        );
                                }
                                return r;
                            }
                        );
            }

        public static IEnumerable<Type> ModelMemberTypes
        {
            get
            {
                return _typesWhiteList;
            }

            //set
            //{
            //    _typesWhiteList = value;
            //}
        }

        public static IEnumerable<MemberInfo>
                       GetModelMembers
                               (
                                   Type type
                                )
        {
            return
                type
                    .GetMembers()
                    .Where
                        (
                            (x) =>
                            {
                                var r = false;
                                Type memberType = null;
                                if (x is FieldInfo)
                                {
                                    var fieldInfo = x as FieldInfo;
                                    memberType = fieldInfo.FieldType;
                                    r = true;
                                }
                                else if (x is PropertyInfo)
                                {
                                    var propertyInfo = x as PropertyInfo;
                                    memberType = propertyInfo.PropertyType;
                                    r = true;
                                }
                                else
                                {
                                    r = false;
                                }
                                if (r)
                                {
                                    if (memberType.IsNullableType())
                                    {
                                        memberType = memberType.GetNullableUnderlyingType();
                                    }
                                    r = ModelMemberTypes
                                            .Any
                                                (
                                                    (xx) =>
                                                    {
                                                        return
                                                            (xx == memberType);
                                                    }
                                                );
                                }
                                return r;
                            }
                        );
        }
        public static IEnumerable<MemberInfo>
                       GetCustomAttributedMembers<TAttribute>
                               (
                                   Type type
                                   , Func<MemberTypes, MemberInfo, TAttribute, bool> onAttributeProcessFunc = null
                                   //, BindingFlags bindingFlags = BindingFlags.
                               )
                            where TAttribute : Attribute

        {
            return
                type
                    .GetFields()
                    .Where
                        (
                            (x) =>
                            {
                                var rr = false;
                                var attribute = x
                                                .GetCustomAttributes
                                                    (typeof(TAttribute), true)
                                                .FirstOrDefault() as TAttribute;
                                if (attribute != null)
                                {
                                    rr = true;
                                    if (onAttributeProcessFunc != null)
                                    {
                                        rr = onAttributeProcessFunc(MemberTypes.Field, x, attribute);
                                    }
                                }
                                return rr;
                            }
                        )
                    .Select
                        (
                            (x) =>
                            {
                                return x as MemberInfo;
                            }
                        )
                    .Concat
                        (
                            type
                                .GetProperties()
                                .Where
                                    (
                                        (x) =>
                                        {
                                            var rr = false;
                                            var attribute = x
                                                            .GetCustomAttributes
                                                                (typeof(TAttribute), true)
                                                            .FirstOrDefault() as TAttribute;
                                            if (attribute != null)
                                            {
                                                rr = true;
                                                if (onAttributeProcessFunc != null)
                                                {
                                                    rr = onAttributeProcessFunc(MemberTypes.Field, x, attribute);
                                                }
                                            }
                                            return rr;
                                        }
                                    )
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                return x as MemberInfo;
                                            }
                                        )
                        );
        }

        public static IEnumerable<MemberAccessor>
                        GetModelMembersAccessors
                                (
                                    Type targetType
                                    , bool needDefinitionAttributeProcess = false
                                )
                                
        {
            var members = TypeHelper
                                .GetModelMembers
                                    (
                                        targetType
                                    );
            foreach (var member in members)
            {
                var memberName = member.Name;

                Type memberType = null;
                MemberTypes? memberTypes = null;
                if (member is FieldInfo)
                {
                    var fieldInfo = member as FieldInfo;
                    memberType = fieldInfo.FieldType;
                }
                else if (member is PropertyInfo)
                {
                    var propertyInfo = member as PropertyInfo;
                    memberType = propertyInfo.PropertyType;
                }

                var accessor = new MemberAccessor()
                {
                    Getter = DynamicExpressionTreeHelper
                                .CreateMemberGetter(targetType, memberName)
                    ,
                    Setter = DynamicExpressionTreeHelper
                                .CreateMemberSetter(targetType, memberName)
                    ,
                    Member = member
                    ,
                    Types = memberTypes
                 
                    ,
                    Name = memberName
                    ,
                    Key = memberName
                    ,
                    MemberType = GetNullableUnderlyingType(memberType)
                };
                if (needDefinitionAttributeProcess)
                {
                    var attribute = member
                                        .GetCustomAttributes
                                            (
                                                typeof(MemberAdditionalDefinitionAttribute)
                                                , true
                                            )
                                            .FirstOrDefault(); //as DataTableColumnIDAttribute;
                    if (attribute != null)
                    {
                        var asAttribute =
                                        attribute as MemberAdditionalDefinitionAttribute;
                        if (asAttribute != null)
                        {
                            //if (asAttribute.DataTableColumnDataType != null)
                            //{
                            //    accessor
                            //        .MemberType = asAttribute
                            //                                    .DataTableColumnDataType;
                            //}
                            accessor
                                    .DefinitionAttribute = asAttribute;
                            if
                                (
                                    !asAttribute
                                            .DataTableColumnName
                                            .IsNullOrEmptyOrWhiteSpace()
                                )
                            {
                                accessor
                                    .Key
                                            = asAttribute
                                                    .DataTableColumnName;
                            }
                        }
                    }
                }
                yield
                    return
                        accessor;
            }

            //var properties = type.GetProperties();
            //foreach (var property in properties)
            //{
            //    if 
            //        (
            //            ModelMemberTypes
            //                .Any
            //                    (
            //                        (x) =>
            //                        {
            //                            var r = false;
            //                            var propertyType = property.PropertyType;
            //                            if (x == propertyType)
            //                            {
            //                                r = true;
            //                            }
            //                            if (!r)
            //                            {
            //                                if
            //                                    (
            //                                        propertyType
            //                                            .IsGenericType
            //                                        &&
            //                                        propertyType
            //                                            .GetGenericTypeDefinition()
            //                                            .Equals
            //                                                (
            //                                                    typeof(Nullable<>)
            //                                                )
            //                                    )
            //                                {
            //                                    if
            //                                        (
            //                                            x
            //                                            ==
            //                                            GetNullableUnderlyingType
            //                                                    (propertyType)
            //                                        )
            //                                    {
            //                                        r = true;
            //                                    }
            //                                }
            //                            }
            //                            return r;
            //                        }
            //                    )
            //        )
            //    {
            //        var propertyName = property.Name;
            //        var propertyType = property.PropertyType;
            //        var accessor = new MemberAccessor()
            //        {
            //            Getter = DynamicPropertyAccessor
            //                        .CreateGetPropertyValueFunc(type, propertyName)
            //            , Setter = DynamicPropertyAccessor
            //                        .CreateSetPropertyValueAction(type, propertyName)
            //            , Member = property
            //            , Name = property.Name
            //            , Key = property.Name
            //            , MemberType = GetNullableUnderlyingType(propertyType)
            //        };
            //        if (needDefinitionAttributeProcess)
            //        {
            //            var attribute = property
            //                                .GetCustomAttributes
            //                                    (
            //                                        typeof(MemberAdditionalDefinitionAttribute)
            //                                        , true
            //                                    )
            //                                    .FirstOrDefault(); //as DataTableColumnIDAttribute;
            //            if (attribute != null)
            //            {
            //                var asAttribute =
            //                                attribute as MemberAdditionalDefinitionAttribute;
            //                if (asAttribute != null)
            //                {
            //                    if (asAttribute.DataTableColumnDataType != null)
            //                    {
            //                        accessor
            //                            .MemberType = asAttribute
            //                                                        .DataTableColumnDataType; 
            //                    }
            //                    accessor
            //                            .DefinitionAttribute = asAttribute;
            //                    if
            //                        (
            //                            !asAttribute
            //                                    .DataTableColumnName
            //                                    .IsNullOrEmptyOrWhiteSpace()
            //                        )
            //                    {
            //                        accessor
            //                            .Key
            //                                    = asAttribute
            //                                            .DataTableColumnName;
            //                    }
            //                }
            //            }
            //        }
            //        yield
            //            return
            //                accessor;
            //    }
            //}
            //);
            //return dictionary;
        }
        public static Dictionary<string, MemberAccessor>
                                GenerateTypeKeyedCachedMembersAccessors
                                        (
                                            Type type
                                            , bool needDefinitionAttributeProcess = false
                                        )
        {
            Dictionary<string, MemberAccessor> dictionary = null;
            var result = GetModelMembersAccessors(type, needDefinitionAttributeProcess);
            foreach (var x in result)
            {
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, MemberAccessor>();
                }
                dictionary
                        .Add
                            (
                                x.Key
                                , x
                            );
            }
            return dictionary;
        }
        public static bool IsNullableType(Type type)
        {
            return
                (
                    (
                        type.IsGenericType
                        &&
                        type.GetGenericTypeDefinition() == typeof(Nullable<>)
                    )
                );
        }
        public static Type GetNullableUnderlyingType(Type type)
        {
            //Type r = null;
            if (IsNullableType(type))
            {
                type = Nullable.GetUnderlyingType(type); 
            }
            return type;
        }
        public static bool IsNumericType(Type type)
        {
            var typeCode = Type.GetTypeCode(type);
            return
                (
                    (
                        type.IsPrimitive
                        &&
                        type.IsValueType
                        && !type.IsEnum
                        && typeCode != TypeCode.Char
                        && typeCode != TypeCode.Boolean
                    )
                    ||
                    typeCode == TypeCode.Decimal
                );
        }
        public static bool IsNumericOrNullableNumericType(Type type)
        {
            return
                (
                    IsNumericType(type)
                    ||
                    (
                        IsNullableType(type)
                        && 
                        IsNumericType
                                (
                                    //type.GetGenericArguments()[0]
                                    Nullable.GetUnderlyingType(type)
                                )
                    )
                );
        }
        private static readonly Type TaskGenericType = typeof(Task<>);

        

        public static Type GetTaskInnerTypeOrNull(Type type)
        {
            //Contract.Assert(type != null);
            if (type.IsGenericType && !type.IsGenericTypeDefinition)
            {
                Type genericTypeDefinition = type.GetGenericTypeDefinition();

                if (TaskGenericType == genericTypeDefinition)
                {
                    return type.GetGenericArguments()[0];
                }
            }

            return null;
        }

        public static Type[] GetTypeArgumentsIfMatch(Type closedType, Type matchingOpenType)
        {
            if (!closedType.IsGenericType)
            {
                return null;
            }

            Type openType = closedType.GetGenericTypeDefinition();
            return (matchingOpenType == openType) ? closedType.GetGenericArguments() : null;
        }

        public static bool IsCompatibleObject(Type type, object value)
        {
            return (value == null && TypeAllowsNullValue(type)) || type.IsInstanceOfType(value);
        }

        public static bool IsNullableValueType(Type type)
        {
            return Nullable.GetUnderlyingType(type) != null;
        }

        public static bool TypeAllowsNullValue(Type type)
        {
            return !type.IsValueType || IsNullableValueType(type);
        }

        public static bool IsSimpleType(Type type)
        {
            return type.IsPrimitive ||
                   type.Equals(typeof(string)) ||
                   type.Equals(typeof(DateTime)) ||
                   type.Equals(typeof(Decimal)) ||
                   type.Equals(typeof(Guid)) ||
                   type.Equals(typeof(DateTimeOffset)) ||
                   type.Equals(typeof(TimeSpan));
        }

        public static bool IsSimpleUnderlyingType(Type type)
        {
            Type underlyingType = Nullable.GetUnderlyingType(type);
            if (underlyingType != null)
            {
                type = underlyingType;
            }

            return TypeHelper.IsSimpleType(type);
        }

        public static bool CanConvertFromString(Type type)
        {
            return TypeHelper.IsSimpleUnderlyingType(type) ||
                TypeHelper.HasStringConverter(type);
        }

        public static bool HasStringConverter(Type type)
        {
            return TypeDescriptor.GetConverter(type).CanConvertFrom(typeof(string));
        }

        /// <summary>
        /// Fast implementation to get the subset of a given type.
        /// </summary>
        /// <typeparam name="T">type to search for</typeparam>
        /// <returns>subset of objects that can be assigned to T</returns>
        public static ReadOnlyCollection<T> OfType<T>(object[] objects) where T : class
        {
            int max = objects.Length;
            List<T> list = new List<T>(max);
            int idx = 0;
            for (int i = 0; i < max; i++)
            {
                T attr = objects[i] as T;
                if (attr != null)
                {
                    list.Add(attr);
                    idx++;
                }
            }
            list.Capacity = idx;

            return new ReadOnlyCollection<T>(list);
        }
    }
    public static class TypesExtensionsMethodsManager
    {

            public static IEnumerable<MemberInfo> GetMembersByMemberType<TMember>(this Type targetType, bool includeIndexer = false)
            {
                //var type = target.GetType();
                var memberType = typeof(TMember);
                return
                    TypeHelper
                        .GetMembersByMemberType
                                (
                                    targetType
                                    , memberType
                                    , includeIndexer
                                );
            }

        public static IEnumerable<MemberInfo>
                       GetCustomAttributedMembers<TAttribute>
                               (
                                   this Type type
                                   , Func<MemberTypes, MemberInfo, TAttribute, bool> onAttributeProcessFunc = null
                                   //, BindingFlags bindingFlags = BindingFlags.Public
                               )
                            where TAttribute : Attribute

        {
            return
                TypeHelper
                    .GetCustomAttributedMembers<TAttribute>
                        (
                            type
                            , onAttributeProcessFunc
                        );
        }

        public static bool IsNullableType(this Type type)
        {
            return TypeHelper.IsNullableType(type);
        }
        public static Type GetNullableUnderlyingType(this Type type)
        {
            return TypeHelper.GetNullableUnderlyingType(type);
        }
        public static bool IsNumericType(this Type type)
        {
            return TypeHelper.IsNumericType(type);
        }
        public static bool IsNumericOrNullableNumericType(this Type type)
        {
            return TypeHelper.IsNumericOrNullableNumericType(type);
        }


        


    }
}
