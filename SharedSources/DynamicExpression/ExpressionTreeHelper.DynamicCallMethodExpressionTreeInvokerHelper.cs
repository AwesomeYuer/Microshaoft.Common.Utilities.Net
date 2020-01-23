namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    public static partial class DynamicExpressionTreeHelper //DynamicCallMethodExpressionTreeInvokerHelper
    {
        public static Func<object[], object> CreateNewInstanceConstructorInvokerFunc
                                                        (
                                                            Type type
                                                            , Func<ConstructorInfo> getConstructorInfoFunc
                                                        )
        {
            var constructorInfo = getConstructorInfoFunc();
            return
                CreateNewInstanceConstructorInvokerFunc
                                                    <object>
                                                        (
                                                            type
                                                            , constructorInfo
                                                        );
        }
        public static Func<object[], T> CreateNewInstanceConstructorInvokerFunc<T>
                                                        (
                                                            Type type
                                                            , Func<ConstructorInfo> getConstructorInfoFunc
                                                        )
        {
            var constructorInfo = getConstructorInfoFunc();
            return CreateNewInstanceConstructorInvokerFunc<T>
                                                        (
                                                            type
                                                            , constructorInfo
                                                        );
        }
        public static Func<object[], object> CreateNewInstanceConstructorInvokerFunc
                                                        (
                                                            Type type
                                                            , ConstructorInfo constructorInfo
                                                        )
        {
            return CreateNewInstanceConstructorInvokerFunc<object>(type, constructorInfo);
        }
        public static Func<object[], T> CreateNewInstanceConstructorInvokerFunc<T>
                                                        (
                                                            Type type
                                                            , ConstructorInfo constructorInfo
                                                        )
        {
            var parametersInfos = constructorInfo.GetParameters();
            var constructorParametersExpressions = new List<ParameterExpression>();
            int i = 0;
            Array.ForEach
                    (
                        parametersInfos
                        , (x) =>
                        {
                            var parameterExpression = Expression.Parameter
                                                                    (
                                                                        x.ParameterType
                                                                        , "p" + i.ToString()
                                                                    );
                            constructorParametersExpressions.Add(parameterExpression);
                            i++;
                        }
                    );
            var newExpression = Expression.New(constructorInfo, constructorParametersExpressions);
            var inner = Expression.Lambda(newExpression, constructorParametersExpressions);
            var args = Expression.Parameter(typeof(object[]), "args");
            var body = Expression
                            .Invoke
                                (
                                    inner
                                    , constructorParametersExpressions
                                            .Select
                                                (
                                                    (p, ii) =>
                                                    {
                                                        return
                                                            Expression
                                                                .Convert
                                                                    (
                                                                        Expression
                                                                            .ArrayIndex
                                                                                (
                                                                                    args
                                                                                    , Expression
                                                                                        .Constant(ii)
                                                                                )
                                                                        , p.Type
                                                                    );
                                                    }
                                                )
                                            .ToArray()
                                );
            var outer = Expression.Lambda<Func<object[], T>>(body, args);
            var func = outer.Compile();
            return func;
        }
        public static Action<T, object[]> CreateMethodCallInvokerAction<T>
                                                            (
                                                                Type type
                                                                , Func<MethodInfo> getMethodInfoFunc
                                                            )
        {
            var methodInfo = getMethodInfoFunc();
            return CreateMethodCallInvokerAction<T>
                                    (
                                        type
                                        , methodInfo
                                    );
        }
        public static Action<T, object[]> CreateMethodCallInvokerAction<T>
                                                            (
                                                                Type type
                                                                , MethodInfo methodInfo
                                                            )
        {
            ParameterExpression instanceParameterExpression;
            MethodCallExpression methodCallExpression;
            ParameterExpression argumentsParameterExpression
                                    = GetMethodArgumentsParameterExpression
                                            (
                                                type
                                                , methodInfo
                                                , out instanceParameterExpression
                                                , out methodCallExpression
                                            );
            var lambda = Expression
                                .Lambda
                                    <Action<T, object[]>>
                                        (
                                            methodCallExpression
                                            , instanceParameterExpression
                                            , argumentsParameterExpression
                                        );
            var action = lambda.Compile();
            return action;
        }
        public static Func<TTarget, object[], TResult>
                            CreateTargetMethodCallInvokerFunc
                                    <TTarget, TResult>
                                            (
                                                //Type type
                                                Func<MethodInfo> getMethodInfoFunc
                                            )
        {
            _ = typeof(TTarget);
            var methodInfo = getMethodInfoFunc();
            return
                CreateTargetMethodCallInvokerFunc
                        <TTarget, TResult>
                                (
                                    methodInfo
                                );
        }
        public static Func<TTarget, object[], TResult>
                            CreateTargetMethodCallInvokerFunc
                                    <TTarget, TResult>
                                            (
                                                //Type type
                                                MethodInfo methodInfo
                                            )
        {
            var type = typeof(TTarget);
            ParameterExpression argumentsParameterExpression
                                        = GetMethodArgumentsParameterExpression
                                                (
                                                    type
                                                    , methodInfo
                                                    , out ParameterExpression instanceParameterExpression
                                                    , out MethodCallExpression methodCallExpression
                                                );
            var lambda = Expression
                                .Lambda<Func<TTarget, object[], TResult>>
                                        (
                                            methodCallExpression
                                            , instanceParameterExpression
                                            , argumentsParameterExpression
                                        );
            var func = lambda.Compile();
            return func;
        }
        public static Delegate CreateDelegate
                                        (
                                            MethodInfo methodInfo
                                        )
        {
            var parametersTypes = methodInfo
                                        .GetParameters()
                                        .Select
                                            (
                                                x => x.ParameterType
                                            )
                                        .Concat
                                            (
                                                new[] { methodInfo.ReturnType }
                                            )
                                        .ToArray();
            var delegateType = Expression
                                    .GetDelegateType
                                            (
                                               parametersTypes
                                            );
            var r = methodInfo
                        .CreateDelegate(delegateType);
            return r;
        }
        public static Func<object, object[], TResult>
                            CreateTargetMethodCallInvokerFunc<TResult>
                                                (
                                                    MethodInfo methodInfo
                                                )
        {
            return
                CreateTargetMethodCallInvokerFunc<object, TResult>
                    (
                        methodInfo
                    );
        }
        public static Func<object, object[], TResult> CreateTargetMethodCallInvokerFunc<TResult>
                                                    (
                                                        Func<MethodInfo> getMethodInfo
                                                    )
        {
            var methodInfo = getMethodInfo();
            return
                CreateTargetMethodCallInvokerFunc<object, TResult>
                    (
                        methodInfo
                    );
        }

        private static ParameterExpression
                                GetMethodArgumentsParameterExpression
                                        (
                                            Type type
                                            , MethodInfo methodInfo
                                            , out ParameterExpression instanceParameterExpression
                                            , out MethodCallExpression methodCallExpression
                                        )
        {
            var argumentsParameterExpression = Expression.Parameter(typeof(object[]), "args");
            instanceParameterExpression = Expression.Parameter(type);
            UnaryExpression instanceConvertUnaryExpression = null;
            if (!methodInfo.IsStatic)
            {
                instanceConvertUnaryExpression = Expression.Convert(instanceParameterExpression, type);
            }
            var parametersParameterExpressionList = new List<Expression>();
            int i = 0;
            var parametersInfos = methodInfo.GetParameters();
            Array
                .ForEach
                    (
                        parametersInfos
                        , (x) =>
                        {
                            BinaryExpression valueObject
                                                = Expression
                                                        .ArrayIndex
                                                            (
                                                                argumentsParameterExpression
                                                                , Expression.Constant(i)
                                                            );
                            UnaryExpression valueCast
                                                = Expression
                                                        .Convert
                                                            (
                                                                valueObject
                                                                , x.ParameterType
                                                            );
                            parametersParameterExpressionList.Add(valueCast);
                            i++;
                        }
                    );
            methodCallExpression = Expression
                                            .Call
                                                (
                                                    instanceConvertUnaryExpression
                                                    , methodInfo
                                                    , parametersParameterExpressionList
                                                );
            return argumentsParameterExpression;
        }
    }
}