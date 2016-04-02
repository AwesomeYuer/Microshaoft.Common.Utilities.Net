namespace Microshaoft
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    public static partial class DynamicExpressionTreeHelper
    {
        private static Assembly GetAssemblyByTypeName(string typeName)
        {
            return
                AppDomain
                    .CurrentDomain
                    .GetAssemblies()
                    .First
                        (
                            (a) =>
                            {
                                return
                                    a
                                        .GetTypes()
                                            .Any
                                                (
                                                    (t) =>
                                                    {
                                                        return
                                                            (
                                                                t.FullName
                                                                ==
                                                                typeName
                                                            );
                                                    }
                                                );
                            }
                        );
        }
        public static Func<object, object> CreateGetPropertyValueFunc
                        (
                            string typeName
                            , string propertyName
                            , bool isTypeFromAssembly = false
                        )
        {
            Type type;
            if (isTypeFromAssembly)
            {
                var assembly = GetAssemblyByTypeName(typeName);
                type = assembly.GetType(typeName);
            }
            else
            {
                type = Type.GetType(typeName);
            }
            return CreateGetPropertyValueFunc(type, propertyName);
        }
        public static Func<object, object> CreateGetPropertyValueFunc
                        (
                            Type type
                            , string propertyName
                        )
        {
            var target = Expression.Parameter(typeof(object), "P");
            var castTarget = Expression.Convert(target, type);
            var getPropertyValue = Expression.Property(castTarget, propertyName);
            var castPropertyValue = Expression.Convert(getPropertyValue, typeof(object));
            var lambda = Expression.Lambda<Func<object, object>>(castPropertyValue, target);
            return lambda.Compile();
        }
        public static Func<object, TProperty> CreateGetPropertyValueFunc<TProperty>
                        (
                            string typeName
                            , string propertyName
                            , bool isTypeFromAssembly = false
                        )
        {
            Type type;
            if (isTypeFromAssembly)
            {
                var assembly = GetAssemblyByTypeName(typeName);
                type = assembly.GetType(typeName);
            }
            else
            {
                type = Type.GetType(typeName);
            }
            return CreateGetPropertyValueFunc<TProperty>(type, propertyName);
        }
        public static Func<object, TProperty> CreateGetPropertyValueFunc<TProperty>
                        (
                            Type type
                            , string propertyName
                        )
        {
            var target = Expression.Parameter(typeof(object), "p");
            var castTarget = Expression.Convert(target, type);
            var getPropertyValue = Expression.Property(castTarget, propertyName);
            var lambda = Expression.Lambda<Func<object, TProperty>>(getPropertyValue, target);
            return lambda.Compile();
        }
        public static Func<TTarget, TProperty> CreateGetPropertyValueFunc<TTarget, TProperty>
                (
                    string propertyName
                )
        {
            var target = Expression.Parameter(typeof(TTarget), "p");
            var getPropertyValue = Expression.Property(target, propertyName);
            var lambda = Expression.Lambda<Func<TTarget, TProperty>>(getPropertyValue, target);
            return lambda.Compile();
        }
        public static Func<TProperty> CreateGetStaticPropertyValueFunc<TProperty>
                        (
                            string typeName
                            , string propertyName
                            , bool isTypeFromAssembly = false
                        )
        {
            Type type;
            if (isTypeFromAssembly)
            {
                var assembly = GetAssemblyByTypeName(typeName);
                type = assembly.GetType(typeName);
            }
            else
            {
                type = Type.GetType(typeName);
            }
            return CreateGetStaticPropertyValueFunc<TProperty>(type, propertyName);
        }
        public static Func<TProperty> CreateGetStaticPropertyValueFunc<TProperty>
                        (
                            Type type
                            , string propertyName
                        )
        {
            Func<TProperty> func = null;
            var property = type.GetProperty(propertyName, typeof(TProperty));
            if (property == null)
            {
                property = type
                                .GetProperties()
                                .FirstOrDefault
                                    (
                                        (x) =>
                                        {
                                            return
                                                    (
                                                        string
                                                            .Compare
                                                                (
                                                                    x.Name.Trim()
                                                                    , propertyName.Trim()
                                                                    , true
                                                                )
                                                        == 0
                                                    );
                                        }
                                    );
            }
            if (property != null)
            {
                var getPropertyValue = Expression.Property(null, property);
                var lambda = Expression.Lambda<Func<TProperty>>(getPropertyValue, null);
                func = lambda.Compile();
            }
            return func;
        }
        public static Func<object> CreateGetStaticPropertyValueFunc
                        (
                            Type type
                            , string propertyName
                        )
        {
            Func<object> func = null;
            var property = type.GetProperty(propertyName);
            if (property == null)
            {
                property =
                            type
                                .GetProperties()
                                .FirstOrDefault
                                    (
                                        (x) =>
                                        {
                                            return
                                                (
                                                    string
                                                        .Compare
                                                            (
                                                                x.Name.Trim()
                                                                , propertyName.Trim()
                                                                , true
                                                            )
                                                    == 0
                                                );
                                        }
                                    );
            }
            if (property != null)
            {
                var getPropertyValue = Expression.Property(null, property);
                var castPropertyValue = Expression.Convert(getPropertyValue, typeof(object));
                var lambda = Expression.Lambda<Func<object>>(castPropertyValue, null);
                func = lambda.Compile();
            }
            return func;
        }
        public static Func<object> CreateGetStaticPropertyValueFunc
                        (
                            string typeName
                            , string propertyName
                            , bool isTypeFromAssembly = false
                        )
        {
            Type type;
            if (isTypeFromAssembly)
            {
                var assembly = GetAssemblyByTypeName(typeName);
                type = assembly.GetType(typeName);
            }
            else
            {
                type = Type.GetType(typeName);
            }
            return CreateGetStaticPropertyValueFunc(type, propertyName);
        }
        public static Action<object, object> CreateSetPropertyValueAction
                        (
                            Type type
                            , string propertyName
                        )
        {
            Action<object, object> action = null;
            var property = type.GetProperty(propertyName);
            if (property == null)
            {
                property =
                            type
                                .GetProperties()
                                .FirstOrDefault
                                    (
                                        (x) =>
                                        {
                                            return
                                                (
                                                    string
                                                        .Compare
                                                            (
                                                                x.Name.Trim()
                                                                , propertyName.Trim()
                                                                , true
                                                            )
                                                    == 0
                                                );
                                        }
                                    );
            }
            if (property.CanWrite)
            {
                if (property != null)
                {
                    var target = Expression.Parameter(typeof(object), "p");
                    var propertyValue = Expression.Parameter(typeof(object), "p");
                    var castTarget = Expression.Convert(target, type);
                    var castPropertyValue = Expression.Convert(propertyValue, property.PropertyType);
                    var getSetMethod = property.GetSetMethod();
                    if (getSetMethod == null)
                    {
                        getSetMethod = property.GetSetMethod(true);
                    }
                    var call = Expression.Call(castTarget, getSetMethod, castPropertyValue);
                    var lambda = Expression.Lambda<Action<object, object>>(call, target, propertyValue);
                    action = lambda.Compile();
                }
            }
            return action;
        }
        public static Action<object, object> CreateSetPropertyValueAction
                        (
                            string typeName
                            , string propertyName
                            , bool isTypeFromAssembly = false
                        )
        {
            Type type;
            if (isTypeFromAssembly)
            {
                var assembly = GetAssemblyByTypeName(typeName);
                type = assembly.GetType(typeName);
            }
            else
            {
                type = Type.GetType(typeName);
            }
            return CreateSetPropertyValueAction(type, propertyName);
        }
        public static Action<TTarget, TProperty> CreateTargetSetPropertyValueAction<TTarget, TProperty>
                        (
                            //Type type
                            //, 
                            string propertyName
                        )
        {
            Action<TTarget, TProperty> action = null;
            var type = typeof(TTarget);
            var property = type.GetProperty(propertyName);
            if (property == null)
            {
                property = type
                                .GetProperties()
                                .FirstOrDefault
                                    (
                                        (x) =>
                                        {
                                            return
                                                (
                                                    string
                                                        .Compare
                                                            (
                                                                x.Name.Trim()
                                                                , propertyName.Trim()
                                                                , true
                                                            )
                                                    == 0
                                                );
                                        }
                                    );
            }
            if (property != null)
            {
                var target = Expression.Parameter(typeof(TTarget), "p");
                var propertyValue = Expression.Parameter(typeof(TProperty), "p");
                var getSetMethod = property.GetSetMethod();
                if (getSetMethod == null)
                {
                    getSetMethod = property.GetSetMethod(true);
                }
                var call = Expression.Call(target, getSetMethod, propertyValue);
                var lambda = Expression.Lambda<Action<TTarget, TProperty>>(call, target, propertyValue);
                action = lambda.Compile();
            }
            return action;
        }
        public static Action<TTarget, object> CreateTargetSetPropertyValueAction<TTarget>
                (
                    //Type type
                    string propertyName
                )
        {
            Action<TTarget, object> action = null;
            var type = typeof(TTarget);
            var property = type.GetProperty(propertyName);
            if (property == null)
            {
                property = type
                                .GetProperties()
                                .FirstOrDefault
                                    (
                                        (x) =>
                                        {
                                            return
                                                (
                                                    string
                                                        .Compare
                                                            (
                                                                x.Name.Trim()
                                                                , propertyName.Trim()
                                                                , true
                                                            )
                                                    == 0
                                                );
                                        }
                                    );
            }
            if (property != null)
            {

                var target = Expression.Parameter(typeof(TTarget), "p");
                var propertyValue = Expression.Parameter(typeof(object), "p");
                var castPropertyValue = Expression.Convert(propertyValue, property.PropertyType);
                var getSetMethod = property.GetSetMethod();
                if (getSetMethod == null)
                {
                    getSetMethod = property.GetSetMethod(true);
                }
                var call = Expression.Call(target, getSetMethod, castPropertyValue);
                var lambda = Expression.Lambda<Action<TTarget, object>>(call, target, propertyValue);
                action = lambda.Compile();
            }
            return action;
        }
        public static Action<object, TProperty> CreateSetPropertyValueAction<TProperty>
                (
                    Type type
                    , string propertyName
                )
        {
            Action<object, TProperty> action = null;
            var property = type.GetProperty(propertyName);
            if (property == null)
            {
                property = type
                                .GetProperties()
                                .FirstOrDefault
                                    (
                                        (x) =>
                                        {
                                            return
                                                (
                                                    string
                                                        .Compare
                                                            (
                                                                x.Name.Trim()
                                                                , propertyName.Trim()
                                                                , true
                                                            )
                                                    == 0
                                                );
                                        }
                                    );
            }
            if (property != null)
            {
                var target = Expression.Parameter(typeof(object), "p");
                var propertyValue = Expression.Parameter(typeof(TProperty), "p");
                var castTarget = Expression.Convert(target, type);
                var getSetMethod = property.GetSetMethod();
                if (getSetMethod == null)
                {
                    getSetMethod = property.GetSetMethod(true);
                }
                var call = Expression.Call(castTarget, getSetMethod, propertyValue);
                var lambda = Expression.Lambda<Action<object, TProperty>>(call, target, propertyValue);
                action = lambda.Compile();
            }
            return action;
        }

        public static Action<object, TProperty> CreateSetPropertyValueAction<TProperty>
                        (
                            string typeName
                            , string propertyName
                            , bool isTypeFromAssembly = false
                        )
        {
            Type type;
            if (isTypeFromAssembly)
            {
                var assembly = GetAssemblyByTypeName(typeName);
                type = assembly.GetType(typeName);
            }
            else
            {
                type = Type.GetType(typeName);
            }
            return CreateSetPropertyValueAction<TProperty>(type, propertyName);
        }


        public static Action<TProperty> CreateTargetSetStaticPropertyValueAction<TTarget, TProperty>
                        (
                            string propertyName
                        )
        {
            Action<TProperty> action = null;
            var type = typeof(TTarget);
            var property = type.GetProperty(propertyName);
            if (property == null)
            {
                property = type
                                .GetProperties()
                                .FirstOrDefault
                                    (
                                        (x) =>
                                        {
                                            return
                                                (
                                                    string
                                                        .Compare
                                                            (
                                                                x.Name.Trim()
                                                                , propertyName.Trim()
                                                                , true
                                                            )
                                                    == 0
                                                );
                                        }
                                    );
            }
            if (property != null)
            {
                var propertyValue = Expression.Parameter(typeof(TProperty), "p");
                var castPropertyValue = Expression.Convert(propertyValue, property.PropertyType);
                var getSetMethod = property.GetSetMethod();
                if (getSetMethod == null)
                {
                    getSetMethod = property.GetSetMethod(true);
                }
                var call = Expression.Call(null, getSetMethod, castPropertyValue);
                var lambda = Expression.Lambda<Action<TProperty>>(call, propertyValue);
                action = lambda.Compile();
            }
            return action;
        }





        public static Action<object> CreateSetStaticPropertyValueAction
                        (
                            Type type
                            , string propertyName
                        )
        {
            Action<object> action = null;
            var property = type.GetProperty(propertyName);
            if (property == null)
            {
                property = type
                                .GetProperties()
                                .FirstOrDefault
                                    (
                                        (x) =>
                                        {
                                            return
                                                (
                                                    string
                                                        .Compare
                                                            (
                                                                x.Name.Trim()
                                                                , propertyName.Trim()
                                                                , true
                                                            )
                                                    == 0
                                                );
                                        }
                                    );
            }
            if (property != null)
            {
                var propertyValue = Expression.Parameter(typeof(object), "p");
                var castPropertyValue = Expression.Convert(propertyValue, property.PropertyType);
                var getSetMethod = property.GetSetMethod();
                if (getSetMethod == null)
                {
                    getSetMethod = property.GetSetMethod(true);
                }
                var call = Expression.Call(null, getSetMethod, castPropertyValue);
                var lambda = Expression.Lambda<Action<object>>(call, propertyValue);
                action = lambda.Compile();
            }
            return action;
        }
        public static Action<object> CreateSetStaticPropertyValueAction
                        (
                            string typeName
                            , string propertyName
                            , bool isTypeFromAssembly = false
                        )
        {
            Type type;
            if (isTypeFromAssembly)
            {
                var assembly = GetAssemblyByTypeName(typeName);
                type = assembly.GetType(typeName);
            }
            else
            {
                type = Type.GetType(typeName);
            }
            return CreateSetStaticPropertyValueAction(type, propertyName);
        }
        public static Action<TProperty> CreateSetStaticPropertyValueAction<TProperty>
                        (
                            Type type
                            , string propertyName
                        )
        {
            Action<TProperty> action = null;
            var property = type.GetProperty(propertyName);
            if (property == null)
            {
                property = type
                                .GetProperties()
                                .FirstOrDefault
                                    (
                                        (x) =>
                                        {
                                            return
                                                (
                                                    string
                                                        .Compare
                                                            (
                                                                x.Name.Trim()
                                                                , propertyName.Trim()
                                                                , true
                                                            )
                                                    == 0
                                                );
                                        }
                                    );
            }
            if (property != null)
            {
                var propertyValue = Expression.Parameter(typeof(TProperty), "p");
                var getSetMethod = property.GetSetMethod();
                if (getSetMethod == null)
                {
                    getSetMethod = property.GetSetMethod(true);
                }
                var call = Expression.Call(null, getSetMethod, propertyValue);
                var lambda = Expression.Lambda<Action<TProperty>>(call, propertyValue);
                action = lambda.Compile();
            }
            return action;
        }
        public static Action<TProperty> CreateSetStaticPropertyValueAction<TProperty>
                        (
                            string typeName
                            , string propertyName
                            , bool isTypeFromAssembly = false
                        )
        {
            Type type;
            if (isTypeFromAssembly)
            {
                var assembly = GetAssemblyByTypeName(typeName);
                type = assembly.GetType(typeName);
            }
            else
            {
                type = Type.GetType(typeName);
            }
            return CreateSetStaticPropertyValueAction<TProperty>(type, propertyName);
        }
    }
}