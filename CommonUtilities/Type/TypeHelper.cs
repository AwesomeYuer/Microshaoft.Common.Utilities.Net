namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Data.SqlTypes;
    using System.Linq;
    using System.Reflection;

    public class PropertyAccessor//<TContext>
    {
        public Func<object, object> Getter;
        public Action<object, object> Setter;
        public PropertyInfo Property;
        public PropertyAdditionalDefinitionAttribute DefinitionAttribute;
        public string PropertyName;
        public string PropertyKey;
        public Type PropertyValueType;
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
        public static IEnumerable<PropertyAccessor>
                        GetTypePropertiesAccessors
                                (
                                    Type type
                                    , bool needDefinitionAttributeProcess = false
                                )
                                
        {
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                if 
                    (
                        _typesWhiteList
                            .Any
                                (
                                    (x) =>
                                    {
                                        var r = false;
                                        var propertyType = property.PropertyType;
                                        if (x == propertyType)
                                        {
                                            r = true;
                                        }
                                        if (!r)
                                        {
                                            if
                                                (
                                                    propertyType
                                                        .IsGenericType
                                                    &&
                                                    propertyType
                                                        .GetGenericTypeDefinition()
                                                        .Equals
                                                            (
                                                                typeof(Nullable<>)
                                                            )
                                                )
                                            {
                                                if
                                                    (
                                                        x
                                                        ==
                                                        GetNullableTypeUnderlyingType
                                                                (propertyType)
                                                    )
                                                {
                                                    r = true;
                                                }
                                            }
                                        }
                                        return r;
                                    }
                                )
                    )
                {
                    var accessor = new PropertyAccessor()
                    {
                        Getter = DynamicPropertyAccessor
                                    .CreateGetPropertyValueFunc(type, property.Name)
                        , Setter = DynamicPropertyAccessor
                                    .CreateSetPropertyValueAction(type, property.Name)
                        , Property = property
                        , PropertyName = property.Name
                        , PropertyKey = property.Name
                        , PropertyValueType = GetNullableTypeUnderlyingType(property.PropertyType)
                    };
                    if (needDefinitionAttributeProcess)
                    {
                        var attribute = property.GetCustomAttributes
                                                (
                                                    typeof(PropertyAdditionalDefinitionAttribute)
                                                    , false
                                                )
                                                .FirstOrDefault(); //as DataTableColumnIDAttribute;
                        if (attribute != null)
                        {
                            var asAttribute =
                                            attribute as PropertyAdditionalDefinitionAttribute;
                            if (asAttribute != null)
                            {
                                if (asAttribute.DataTableColumnDataType != null)
                                {
                                    accessor
                                        .PropertyValueType = asAttribute
                                                                    .DataTableColumnDataType; 
                                }
                                accessor
                                        .DefinitionAttribute = asAttribute;
                                if
                                    (
                                        !string
                                            .IsNullOrEmpty
                                                (
                                                    asAttribute
                                                        .DataTableColumnName
                                                )
                                    )
                                {
                                    accessor
                                        .PropertyKey
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
            }
            //);
            //return dictionary;
        }
        public static Dictionary<string, PropertyAccessor>
                                GetTypeKeyedPropertiesAccessors
                                        (
                                            Type type
                                            , bool needDefinitionAttributeProcess = false
                                        )
        {
            Dictionary<string, PropertyAccessor> dictionary = null;
            var result = GetTypePropertiesAccessors(type, needDefinitionAttributeProcess);
            foreach (var x in result)
            {
                if (dictionary == null)
                {
                    dictionary = new Dictionary<string, PropertyAccessor>();
                }
                dictionary
                    .Add
                    (
                        x.PropertyKey
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
        public static Type GetNullableTypeUnderlyingType(Type type)
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
        public static bool IsNullableType(this Type type)
        {
            return TypeHelper.IsNullableType(type);
        }
        public static Type GetNullableTypeUnderlyingType(this Type type)
        {
            return TypeHelper.GetNullableTypeUnderlyingType(type);
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
