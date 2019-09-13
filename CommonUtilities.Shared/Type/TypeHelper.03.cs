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

        public static IEnumerable<MemberInfo> GetMembersByMemberType<TMember>(this Type target, bool includeIndexer = false)
        {
            //var type = target.GetType();
            var memberType = typeof(TMember);
            return
                GetMembersByMemberType
                            (
                                target
                                , memberType
                                , includeIndexer
                            );
        }

        public static IEnumerable<MemberInfo> GetMembersByMemberType(this Type target, Type memberType, bool includeIndexer = false)
        {
            return
                target
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
                                   this Type target
                                )
        {
            return
                target
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
                                   this Type target
                                   , Func<MemberTypes, MemberInfo, TAttribute, bool> onAttributeProcessFunc = null
                                   //, BindingFlags bindingFlags = BindingFlags.
                               )
                            where TAttribute : Attribute

        {
            return
                target
                    .GetFields()
                    .Where
                        (
                            (x) =>
                            {
                                var rr = false;
                                if 
                                    (
                                        x
                                            .GetCustomAttributes
                                                (typeof(TAttribute), true)
                                            .FirstOrDefault() is TAttribute attribute
                                    )
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
                            target
                                .GetProperties()
                                .Where
                                    (
                                        (x) =>
                                        {
                                            var rr = false;
                                            if 
                                                (
                                                    x
                                                        .GetCustomAttributes
                                                            (typeof(TAttribute), true)
                                                        .FirstOrDefault() is TAttribute attribute
                                                )
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
                                    this Type target
                                    , bool needDefinitionAttributeProcess = false
                                )
                                
        {
            var members = target
                                .GetModelMembers();
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
                                .CreateMemberGetter(target, memberName)
                    ,
                    Setter = DynamicExpressionTreeHelper
                                .CreateMemberSetter(target, memberName)
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
                        if (attribute is MemberAdditionalDefinitionAttribute asAttribute)
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
                                            this Type target
                                            , bool needDefinitionAttributeProcess = false
                                        )
        {
            Dictionary<string, MemberAccessor> dictionary = null;
            var result = GetModelMembersAccessors(target, needDefinitionAttributeProcess);
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
        public static bool IsNullableType(this Type target)
        {
            return
                (
                    (
                        target.IsGenericType
                        &&
                        target.GetGenericTypeDefinition() == typeof(Nullable<>)
                    )
                );
        }
        public static Type GetNullableUnderlyingType(this Type target)
        {
            //Type r = null;
            if (IsNullableType(target))
            {
                target = Nullable.GetUnderlyingType(target); 
            }
            return target;
        }
        public static bool IsNumericType(this Type target)
        {
            var typeCode = Type.GetTypeCode(target);
            return
                (
                    (
                        target.IsPrimitive
                        &&
                        target.IsValueType
                        &&
                        !target.IsEnum
                        &&
                        typeCode != TypeCode.Char
                        &&
                        typeCode != TypeCode.Boolean
                    )
                    ||
                    typeCode == TypeCode.Decimal
                );
        }
        public static bool IsNumericOrNullableNumericType(this Type target)
        {
            return
                (
                    IsNumericType(target)
                    ||
                    (
                        IsNullableType(target)
                        && 
                        IsNumericType
                                (
                                    //type.GetGenericArguments()[0]
                                    Nullable.GetUnderlyingType(target)
                                )
                    )
                );
        }
        private static readonly Type TaskGenericType = typeof(Task<>);

        

        public static Type GetTaskInnerTypeOrNull(this Type target)
        {
            //Contract.Assert(type != null);
            if (target.IsGenericType && !target.IsGenericTypeDefinition)
            {
                Type genericTypeDefinition = target.GetGenericTypeDefinition();

                if (TaskGenericType == genericTypeDefinition)
                {
                    return target.GetGenericArguments()[0];
                }
            }

            return null;
        }

        public static Type[] GetTypeArgumentsIfMatch(this Type target, Type matchingOpenType)
        {
            if (!target.IsGenericType)
            {
                return null;
            }
            Type openType = target.GetGenericTypeDefinition();
            return
                (matchingOpenType == openType)
                ?
                target.GetGenericArguments()
                :
                null;
        }

        public static bool IsCompatibleObject(this Type target, object value)
        {
            return
                (
                    value == null
                    &&
                    TypeAllowsNullValue(target)
                )
                ||
                target.IsInstanceOfType(value);
        }

        public static bool IsNullableValueType(this Type target)
        {
            return Nullable.GetUnderlyingType(target) != null;
        }

        public static bool TypeAllowsNullValue(this Type target)
        {
            return
                !target.IsValueType
                ||
                IsNullableValueType(target);
        }

        public static bool IsSimpleType(this Type target)
        {
            return
                target.IsPrimitive
                ||
                target.Equals(typeof(string))
                ||
                target.Equals(typeof(DateTime))
                ||
                target.Equals(typeof(Decimal))
                ||
                target.Equals(typeof(Guid))
                ||
                target.Equals(typeof(DateTimeOffset))
                ||
                target.Equals(typeof(TimeSpan));
        }

        public static bool IsSimpleUnderlyingType(this Type target)
        {
            Type underlyingType = Nullable.GetUnderlyingType(target);
            if (underlyingType != null)
            {
                target = underlyingType;
            }

            return
                target.IsSimpleType();
        }

        public static bool CanConvertFromString(this Type target)
        {
            return
                target.IsSimpleUnderlyingType()
                ||
                target.HasStringConverter();
        }

        public static bool HasStringConverter(this Type target)
        {
            return
                TypeDescriptor
                    .GetConverter(target)
                    .CanConvertFrom(typeof(string));
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
            return
                new ReadOnlyCollection<T>(list);
        }
    }
}