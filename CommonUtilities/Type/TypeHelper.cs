namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Reflection;

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

    public static class TypeHelper
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
        public static IEnumerable<MemberInfo> GetMembersByMemberType(Type targetType, Type memberType)
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
                                    Type type
                                    , bool needDefinitionAttributeProcess = false
                                )
                                
        {
            var members = TypeHelper
                                .GetModelMembers
                                    (
                                        type
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
                                .CreateMemberGetter(memberType, memberName)
                    ,
                    Setter = DynamicExpressionTreeHelper
                                .CreateMemberSetter(type, memberName)
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
    }
    public static class TypesExtensionsMethodsManager
    {

            public static IEnumerable<MemberInfo> GetMembersByMemberType<TMember>(this Type target)
            {
                var type = target.GetType();
                return
                    type
                        .GetMembers()
                        .Where
                            (
                                (x) =>
                                {
                                    Type memberType = null;
                                    var r = false;
                                    if (x is PropertyInfo)
                                    {
                                        var propertyInfo = x as PropertyInfo;
                                        memberType = propertyInfo.PropertyType;
                                    }
                                    else if (x is FieldInfo)
                                    {
                                        var fieldInfo = x as FieldInfo;
                                        memberType = fieldInfo.FieldType;
                                    }
                                    if (type != null)
                                    {
                                        r =
                                            (
                                                memberType
                                                ==
                                                typeof(TMember)
                                            );
                                    }
                                    return r;
                                }
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
