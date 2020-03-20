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

        public static IEnumerable<MemberInfo> GetMembersByMemberType<TMember>(this Type @this, bool includeIndexer = false)
        {
            //var type = target.GetType();
            var memberType = typeof(TMember);
            return
                GetMembersByMemberType
                            (
                                @this
                                , memberType
                                , includeIndexer
                            );
        }

        public static IEnumerable<MemberInfo> GetMembersByMemberType(this Type @this, Type memberType, bool includeIndexer = false)
        {
            return
                @this
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
                                   this Type @this
                                )
        {
            return
                @this
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
                                   this Type @this
                                   , Func<MemberTypes, MemberInfo, TAttribute, bool> onAttributeProcessFunc = null
                                   //, BindingFlags bindingFlags = BindingFlags.
                               )
                            where TAttribute : Attribute

        {
            return
                @this
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
                            @this
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
                                    this Type @this
                                    , bool needDefinitionAttributeProcess = false
                                )
                                
        {
            var members = @this
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
                                .CreateMemberGetter(@this, memberName)
                    ,
                    Setter = DynamicExpressionTreeHelper
                                .CreateMemberSetter(@this, memberName)
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
                                            this Type @this
                                            , bool needDefinitionAttributeProcess = false
                                        )
        {
            Dictionary<string, MemberAccessor> dictionary = null;
            var result = GetModelMembersAccessors(@this, needDefinitionAttributeProcess);
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
        public static bool IsNullableType(this Type @this)
        {
            return
                (
                    (
                        @this.IsGenericType
                        &&
                        @this.GetGenericTypeDefinition() == typeof(Nullable<>)
                    )
                );
        }
        public static Type GetNullableUnderlyingType(this Type @this)
        {
            //Type r = null;
            if (IsNullableType(@this))
            {
                @this = Nullable.GetUnderlyingType(@this); 
            }
            return @this;
        }
        public static bool IsNumericType(this Type @this)
        {
            var typeCode = Type.GetTypeCode(@this);
            return
                (
                    (
                        @this.IsPrimitive
                        &&
                        @this.IsValueType
                        &&
                        !@this.IsEnum
                        &&
                        typeCode != TypeCode.Char
                        &&
                        typeCode != TypeCode.Boolean
                    )
                    ||
                    typeCode == TypeCode.Decimal
                );
        }
        public static bool IsNumericOrNullableNumericType(this Type @this)
        {
            return
                (
                    IsNumericType(@this)
                    ||
                    (
                        IsNullableType(@this)
                        && 
                        IsNumericType
                                (
                                    //type.GetGenericArguments()[0]
                                    Nullable.GetUnderlyingType(@this)
                                )
                    )
                );
        }
        private static readonly Type TaskGenericType = typeof(Task<>);

        

        public static Type GetTaskInnerTypeOrNull(this Type @this)
        {
            //Contract.Assert(type != null);
            if (@this.IsGenericType && !@this.IsGenericTypeDefinition)
            {
                Type genericTypeDefinition = @this.GetGenericTypeDefinition();

                if (TaskGenericType == genericTypeDefinition)
                {
                    return @this.GetGenericArguments()[0];
                }
            }

            return null;
        }

        public static Type[] GetTypeArgumentsIfMatch(this Type @this, Type matchingOpenType)
        {
            if (!@this.IsGenericType)
            {
                return null;
            }
            Type openType = @this.GetGenericTypeDefinition();
            return
                (matchingOpenType == openType)
                ?
                @this.GetGenericArguments()
                :
                null;
        }

        public static bool IsCompatibleObject(this Type @this, object value)
        {
            return
                (
                    value == null
                    &&
                    TypeAllowsNullValue(@this)
                )
                ||
                @this.IsInstanceOfType(value);
        }

        public static bool IsNullableValueType(this Type @this)
        {
            return Nullable.GetUnderlyingType(@this) != null;
        }

        public static bool TypeAllowsNullValue(this Type @this)
        {
            return
                !@this.IsValueType
                ||
                IsNullableValueType(@this);
        }

        public static bool IsSimpleType(this Type @this)
        {
            return
                @this.IsPrimitive
                ||
                @this.Equals(typeof(string))
                ||
                @this.Equals(typeof(DateTime))
                ||
                @this.Equals(typeof(Decimal))
                ||
                @this.Equals(typeof(Guid))
                ||
                @this.Equals(typeof(DateTimeOffset))
                ||
                @this.Equals(typeof(TimeSpan));
        }

        public static bool IsSimpleUnderlyingType(this Type @this)
        {
            Type underlyingType = Nullable.GetUnderlyingType(@this);
            if (underlyingType != null)
            {
                @this = underlyingType;
            }

            return
                @this.IsSimpleType();
        }

        public static bool CanConvertFromString(this Type @this)
        {
            return
                @this.IsSimpleUnderlyingType()
                ||
                @this.HasStringConverter();
        }

        public static bool HasStringConverter(this Type @this)
        {
            return
                TypeDescriptor
                    .GetConverter(@this)
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
            var list = new List<T>(max);
            int idx = 0;
            for (int i = 0; i < max; i++)
            {
                if (objects[i] is T attr)
                {
                    list.Add(attr);
                    idx++;
                }
            }
            list.Capacity = idx;
            return
                new
                    ReadOnlyCollection<T>
                        (list);
        }
    }
}