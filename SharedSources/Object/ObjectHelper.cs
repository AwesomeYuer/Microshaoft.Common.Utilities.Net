namespace Microshaoft
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Dynamic;
    using System.Reflection;

    public static class ObjectHelper
    {
        public static ExpandoObject ShapeData<TSource>(this TSource @this, string fields)
        {
            if (@this == null)
            {
                throw new ArgumentNullException(nameof(@this));
            }

            var expandoObj = new ExpandoObject();

            if (string.IsNullOrWhiteSpace(fields))
            {
                var propertyInfos =
                    typeof(TSource).GetProperties(BindingFlags.IgnoreCase | BindingFlags.Public |
                                                  BindingFlags.Instance);
                foreach (var propertyInfo in propertyInfos)
                {
                    var propertyValue = propertyInfo.GetValue(@this);
                    ((IDictionary<string, object>)expandoObj).Add(propertyInfo.Name, propertyValue);
                }
            }

            else
            {
                var fieldsAfterSplit = fields.Split(",");
                foreach (var field in fieldsAfterSplit)
                {
                    var propertyName = field.Trim();

                    var propertyInfo = typeof(TSource).GetProperty(propertyName,
                        BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);

                    if (propertyInfo == null)
                    {
                        throw new Exception($"在{typeof(TSource)}上没有找到{propertyName}这个属性");
                    }

                    var propertyValue = propertyInfo.GetValue(@this);
                    ((IDictionary<string, object>)expandoObj).Add(propertyInfo.Name, propertyValue);
                }
            }

            return expandoObj;
        }
        /// <summary>
        /// 类型转换
        /// 支持值类型之间的转换
        /// 支持object to object
        /// </summary>
        /// <typeparam name="T">要转换的结果类型</typeparam>
        /// <param name="this">原对象实例</param>
        /// <returns>返回转换后的结果</returns>
        public static T To<T>(this object @this)
        {
            return (T) @this.To(typeof(T));
        }
        /// <summary>
        /// 类型转换
        /// 支持值类型之间的转换
        /// 支持object to object
        /// </summary>
        /// <typeparam name="T">要转换的结果类型</typeparam>
        /// <param name="this">原对象实例</param>
        /// <param name="t">要转换的结果类型，这个参数主要为了支持匿名类型</param>
        /// <returns>返回转换后的结果</returns>
        public static T To<T>(this object @this, T t)
        {
            return (T)@this.To(typeof(T));
        }
        /// <summary>
        /// 类型转换
        /// 支持值类型之间的转换
        /// 支持object to object
        /// </summary>
        /// <param name="this">原对象实例</param>
        /// <param name="targetType">要转换的结果类型</param>
        /// <returns>返回转换后的结果</returns>
        public static object To(this object @this, Type targetType)
        {
            return @this.InternalTo(targetType, targetType);
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="this"></param>
        /// <param name="targetType"></param>
        /// <param name="rootType">根类型</param>
        /// <param name="deep">记录object引用类型深度，不能超过2（防止循环引用导致的死循环）</param>
        /// <returns></returns>
        static object InternalTo(this object @this, Type targetType, Type rootType, int deep = 0)
        {
            if (@this == null)
            {
                return DefaultInstance(targetType);
            }
            var sourceType = @this.GetType();
            if (sourceType.IsValueType)
            {
                if (targetType.IsValueType)
                {
                    //ValueType to ValueType
                    if (sourceType == targetType)
                    {
                        return @this;
                    }

                    return ChangeType(@this, targetType);

                }
                else if (IsString(targetType))
                {
                    //ValueType to string
                    return @this.ToString();
                }
                else
                {
                    //ValueType to object
                    return null;
                }

            }
            else if (IsString(sourceType))
            {
                if (targetType.IsValueType)
                {
                    //string to ValueType
                    return ChangeType(@this, targetType);
                }

                if (IsString(targetType))
                {
                    //string to string
                    return @this;
                }
                //string to object
                return null;

            }
            else //object to xxx
            {
                if (targetType.IsValueType)
                {
                    //object to valueType
                    return DefaultInstance(targetType);
                }
                if (IsString(targetType))
                {
                    //object to string
                    return @this.ToString();
                }

                //值为2时  支持 a.b   不支持a.b.c
                if (deep >= 2)
                {
                    return null;
                }

                //object to object
                var targetObj = CreateInstance(targetType, @this, sourceType);
                foreach (var sourceProperty in sourceType.GetProperties())
                {
                    var targetProperty = targetType.GetProperty(sourceProperty.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (targetProperty != null && targetProperty.CanWrite)
                    {
                        //if (targetProperty.PropertyType != rootType)
                        //{
                        targetProperty.SetValue(targetObj, sourceProperty.GetValue(@this).InternalTo(targetProperty.PropertyType, rootType, deep + 1));
                        //}
                    }
                    else
                    {
                        var targetField = targetType.GetField(sourceProperty.Name, BindingFlags.Public | BindingFlags.Instance);
                        if (targetField != null)
                        {
                            //if (targetField.FieldType != rootType)
                            //{
                            targetField.SetValue(targetObj, sourceProperty.GetValue(@this).InternalTo(targetField.FieldType, rootType, deep + 1));
                            //}
                        }
                    }
                }

                foreach (var sourceField in sourceType.GetFields())
                {
                    var targetField = targetType.GetField(sourceField.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (targetField != null)
                    {
                        //if (targetField.FieldType != rootType)
                        //{
                        targetField.SetValue(targetObj, sourceField.GetValue(@this).InternalTo(targetField.FieldType, rootType, deep + 1));
                        //}
                    }
                    else
                    {
                        var targetProperty = targetType.GetProperty(sourceField.Name, BindingFlags.Public | BindingFlags.Instance);
                        if (targetProperty != null && targetProperty.CanWrite)
                        {
                            //if (targetProperty.PropertyType != rootType)
                            //{
                            targetProperty.SetValue(targetObj, sourceField.GetValue(@this).InternalTo(targetProperty.PropertyType, rootType, deep + 1));
                            //}
                        }
                    }
                }
                return targetObj;
            }

            throw new Exception("不支持值类型与引用类型间的转换");
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="type"></param>
        /// <param name="source"></param>
        /// <param name="sourceType"></param>
        /// <returns></returns>
        static object CreateInstance(Type type, object source, Type sourceType)
        {
            var constructors = type.GetConstructors();
            object[] args = null;
            foreach (var con in constructors)
            {
                var parameters = con.GetParameters();
                if (parameters.Length == 0)
                {
                    break;
                }
                args = new object[parameters.Length];
                bool flag = true;
                for (int i = 0; i < parameters.Length; i++)
                {
                    var param = parameters[i];

                    var prop = sourceType.GetProperty(param.Name, BindingFlags.Public | BindingFlags.Instance);
                    if (prop != null)
                    {
                        args[i] = prop.GetValue(source).To(param.ParameterType);
                    }
                    else
                    {
                        var field = sourceType.GetField(param.Name, BindingFlags.Public | BindingFlags.Instance);
                        if (field != null)
                        {
                            args[i] = field.GetValue(source).To(param.ParameterType);
                        }
                        else
                        {
                            flag = false;
                            break;
                        }
                    }
                }

                if (flag)
                {
                    break;
                }
            }
            return Activator.CreateInstance(type, args);
        }


        static object ChangeType(object source, Type targetType)
        {
            try
            {
                return Convert.ChangeType(source, targetType);
            }
            catch
            {
                return DefaultInstance(targetType);
            }
        }



        static object DefaultInstance(Type type)
        {
            if (type.IsValueType)
            {
                return Activator.CreateInstance(type);
            }
            return null;
        }

        static bool IsString(Type type)
        {
            return Type.GetTypeCode(type) == TypeCode.String;
        }
    }
}
