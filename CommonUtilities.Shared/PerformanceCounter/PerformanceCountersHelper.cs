#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    public static class PerformanceCountersHelper
    {
        public static void TryCountPerformance
                            (
                                Func<bool> onGetEnableCountProcessFunc = null
                                , bool reThrowException = false
                                , PerformanceCounter[]
                                            IncrementCountersBeforeCountPerformance = null
                                , PerformanceCounter[]
                                            DecrementCountersBeforeCountPerformance = null
                                , WriteableTuple
                                        <
                                            bool						//before时是否已经启动
                                            , Stopwatch
                                            , PerformanceCounter
                                            , PerformanceCounter		//base计数器
                                        >[] timerCounters = null
                                , Action onTryCountPerformanceProcessAction = null
                                , Func<Exception, Exception, string, bool>
                                            onCaughtExceptionCountPerformanceProcessFunc = null
                                , Action<bool, Exception, Exception, string>
                                            onFinallyCountPerformanceProcessAction = null
                                , PerformanceCounter[]
                                            DecrementCountersAfterCountPerformance = null
                                , PerformanceCounter[]
                                            IncrementCountersAfterCountPerformance = null
                            )
        {
            var enabledCountPerformance = true;
            {
                if (onGetEnableCountProcessFunc != null)
                {
                    enabledCountPerformance = onGetEnableCountProcessFunc();
                }
            }
            if 
                (
                    enabledCountPerformance
                    &&
                    onTryCountPerformanceProcessAction != null
                )
            {
                if (enabledCountPerformance)
                {
#region before
                    if (IncrementCountersBeforeCountPerformance != null)
                    {
                        Array
                            .ForEach
                                (
                                    IncrementCountersBeforeCountPerformance
                                    , (x) =>
                                    {
                                        var l = x.Increment();
                                    }
                                );
                    }
                    if (DecrementCountersBeforeCountPerformance != null)
                    {
                        Array
                            .ForEach
                                (
                                    DecrementCountersBeforeCountPerformance
                                    , (x) =>
                                    {
                                        var l = x.Decrement();
                                        if (l < 0)
                                        {
                                            x.RawValue = 0;
                                        }
                                    }
                                );
                    }

                    if (timerCounters != null)
                    {
                        Array
                            .ForEach
                                (
                                    timerCounters
                                    , (x) =>
                                    {
                                        if
                                            (
                                                x.Item1
                                                && x.Item2 != null
                                            )
                                        {
                                            x.Item2.Restart();
                                        }
                                    }
                                );
                    }
#endregion
                }
                var needTry = true;
                TryCatchFinallyProcessHelper
                            .TryProcessCatchFinally
                                (
                                    needTry
                                    , () =>
                                    {
                                        onTryCountPerformanceProcessAction();
                                    }
                                    , reThrowException
                                    , (x, y, z) =>
                                    {
                                        if (onCaughtExceptionCountPerformanceProcessFunc != null)
                                        {
                                            reThrowException
                                                    = onCaughtExceptionCountPerformanceProcessFunc
                                                                (
                                                                    x
                                                                    , y
                                                                    , z
                                                                );
                                        }
                                        return reThrowException;
                                    }
                                    , (x, y, z, w) =>
                                    {
                                        if (enabledCountPerformance)
                                        {
#region after

                                            if (timerCounters != null)
                                            {
                                                Array
                                                    .ForEach
                                                        (
                                                            timerCounters
                                                            , (xx) =>
                                                            {
                                                                if (xx.Item2 != null)
                                                                {
                                                                    var stopwatch = xx.Item2;
                                                                    stopwatch.Stop();
                                                                    long elapsedTicks = stopwatch.ElapsedTicks;
                                                                    var counter = xx.Item3;
                                                                    counter.IncrementBy(elapsedTicks);
                                                                    //池化
                                                                    //stopwatch = null;
                                                                    counter = xx.Item4;  //base
                                                                    counter.Increment();
                                                                }
                                                            }
                                                        );
                                            }
                                            if (IncrementCountersAfterCountPerformance != null)
                                            {
                                                Array
                                                    .ForEach
                                                        (
                                                            IncrementCountersAfterCountPerformance
                                                            , (xx) =>
                                                            {
                                                                var l = xx.Increment();
                                                            }
                                                        );
                                            }
                                            if (DecrementCountersAfterCountPerformance != null)
                                            {
                                                Array
                                                    .ForEach
                                                        (
                                                            DecrementCountersAfterCountPerformance
                                                            , (xx) =>
                                                            {
                                                                var l = xx.Decrement();
                                                                if (l < 0)
                                                                {
                                                                    xx.RawValue = 0;
                                                                }
                                                            }
                                                        );
                                            }
#endregion
                                        }
                                        if (onFinallyCountPerformanceProcessAction != null)
                                        {
                                            onFinallyCountPerformanceProcessAction(x, y, z, w);
                                        }
                                    }
                                );
            }
        }
        public static void AttachPerformanceCountersToMembers<T>
                                    (
                                        string categoryName
                                        , string instanceName
                                        , T target//= default(T)
                                        , PerformanceCounterInstanceLifetime
                                                    instanceLifetime
                                                            = PerformanceCounterInstanceLifetime.Global
                                        , long? instanceInitializeRawValue = null
                                    )
        {
            var type = typeof(T);
            var members = type
                            .GetMembersByMemberType<PerformanceCounter>()
                            .Concat
                                (
                                    type
                                        .GetMembersByMemberType<PerformanceCountersPair>()
                                );

                                //.GetProperties()
                                //.Where
                                //    (
                                //        (pi) =>
                                //        {
                                //            var parameters = pi.GetIndexParameters();
                                //            return
                                //                (
                                //                    (
                                //                        pi.PropertyType == typeof(PerformanceCounter)
                                //                        ||
                                //                        pi.PropertyType == typeof(PerformanceCountersPair)
                                //                    )
                                //                    && (parameters == null ? 0 : parameters.Length) <= 0
                                //                );
                                //        }
                                //    );
            if (!PerformanceCounterCategory.Exists(categoryName))
            {
                var ccdc = new CounterCreationDataCollection();
                foreach (var memberInfo in members)
                {
                    var memberName = memberInfo.Name;
                    var performanceCounterType = PerformanceCounterType
                                                                .NumberOfItems64;
                    var performanceCounterName = memberName;
                    var attribute
                            = memberInfo
                                .GetCustomAttribute
                                        <PerformanceCounterDefinitionAttribute>
                                            ();
                    if (attribute != null)
                    {
                        var counterName = attribute.CounterName;
                        if (!string.IsNullOrEmpty(counterName))
                        {
                            performanceCounterName = counterName;
                        }
                        var counterType = attribute.CounterType;
                        //if (counterType != null)
                        {
                            performanceCounterType = counterType;
                        }
                    }
                    var ccd = GetCounterCreationData
                                    (
                                        performanceCounterName
                                        , performanceCounterType
                                    );
                    ccdc.Add(ccd);
                    Type memberType = null;
                    if (memberInfo is FieldInfo)
                    {
                        var fieldInfo = memberInfo as FieldInfo;
                        memberType = fieldInfo.FieldType;
                    }
                    else if (memberInfo is PropertyInfo)
                    {
                        var propertyInfo = memberInfo as PropertyInfo;
                        memberType = propertyInfo.PropertyType;
                    }
                    

                    if (memberType == typeof(PerformanceCountersPair))
                    {
                        performanceCounterName = string.Format("{0}.(Base)", performanceCounterName);
                        var counterName = attribute.BaseCounterName;
                        if (!string.IsNullOrEmpty(counterName))
                        {
                            performanceCounterName = counterName;
                        }
                        var counterType = attribute.BaseCounterType;
                        //if (counterType != null)
                        {
                            performanceCounterType = counterType;
                        }
                        ccd = GetCounterCreationData
                        (
                            performanceCounterName
                            , performanceCounterType
                        );
                        ccdc.Add(ccd);
                    }
                }
                PerformanceCounterCategory
                            .Create
                                (
                                    categoryName
                                    , string
                                            .Format
                                                (
                                                    "{0} Category Help."
                                                    , categoryName
                                                )
                                    , PerformanceCounterCategoryType
                                                            .MultiInstance
                                    , ccdc
                                );
            }
            foreach (var memberInfo in members)
            {
                //PerformanceCounter performanceCounter = null;
                var memberName = memberInfo.Name;
                var performanceCounterType = PerformanceCounterType
                                                            .NumberOfItems64;
                var performanceCounterName = memberName;
                //var performanceCounterInstanceLifetime = defaultPerformanceCounterInstanceLifetime;
                long? performanceCounterInstanceInitializeRawValue
                                            = instanceInitializeRawValue;
                var attribute
                            = memberInfo
                                .GetCustomAttribute
                                        <PerformanceCounterDefinitionAttribute>
                                            ();
                if (attribute != null)
                {
                    var counterName = attribute.CounterName;
                    if (!string.IsNullOrEmpty(counterName))
                    {
                        performanceCounterName = counterName;
                    }
                    var counterType = attribute.CounterType;
                    //if (counterType != null)
                    {
                        performanceCounterType = counterType;
                    }
                    var counterInstanceLifetime
                                    = attribute.CounterInstanceLifetime;
                    if 
                        (
                            counterInstanceLifetime != null
                            &&
                            counterInstanceLifetime.HasValue
                        )
                    {
                        instanceLifetime = counterInstanceLifetime.Value;
                    }
                    var counterInstanceInitializeRawValue
                                    = attribute.CounterInstanceInitializeRawValue;
                    if 
                        (
                            counterInstanceInitializeRawValue != null
                            &&
                            counterInstanceInitializeRawValue.HasValue
                        )
                    {
                        performanceCounterInstanceInitializeRawValue
                                    = counterInstanceInitializeRawValue.Value;
                    }
                }
                var performanceCounter
                                    = CreatePerformanceCounter
                                            (
                                                categoryName
                                                , performanceCounterName
                                                , instanceName
                                                , instanceLifetime
                                                , performanceCounterInstanceInitializeRawValue
                                            );
                Type memberType = null;
                if (memberInfo is FieldInfo)
                {
                    var fieldInfo = memberInfo as FieldInfo;
                    memberType = fieldInfo.FieldType;
                }
                else if (memberInfo is PropertyInfo)
                {
                    var propertyInfo = memberInfo as PropertyInfo;
                    memberType = propertyInfo.PropertyType;
                }
                if (memberType == typeof(PerformanceCounter))
                {
                    SetMemberValueToTarget(target, memberInfo, performanceCounter);
                }
                else if (memberType == typeof(PerformanceCountersPair))
                {
                    performanceCounterName = string.Format("{0}.(Base)", performanceCounterName);
                    var baseCounterName = attribute.BaseCounterName;
                    if (!string.IsNullOrEmpty(baseCounterName))
                    {
                        performanceCounterName = baseCounterName;
                    }
                    var baseCounterType = attribute.BaseCounterType;
                    {
                        performanceCounterType = baseCounterType;
                    }
                    var basePerformanceCounter
                                    = CreatePerformanceCounter
                                            (
                                                categoryName
                                                , performanceCounterName
                                                , instanceName
                                                , instanceLifetime
                                                , performanceCounterInstanceInitializeRawValue
                                            );
                    var performanceCountersPair = new PerformanceCountersPair()
                    {
                        Counter = performanceCounter
                        , BaseCounter = basePerformanceCounter
                    };
                    SetMemberValueToTarget
                            (
                                target
                                , memberInfo
                                , performanceCountersPair
                            );
                }
            }
        }
        private static void SetMemberValueToTarget<TTarget, TMember>
                                (
                                    TTarget target
                                    , MemberInfo memberInfo
                                    , TMember memberValue
                                )
        {
            string memberName = memberInfo.Name;
            
            //not support static member
            //
            //if (memberInfo.GetGetMethod().IsStatic)
            //{
            //    var setter = DynamicExpressionTreeHelper
            //                        .CreateTargetSetStaticPropertyValueAction
            //                                <TTarget, TProperty>
            //                                    (
            //                                        memberName
            //                                    );
            //    setter(propertyValue);
            //}
            //else
            //{

                if (target != null)
                {
                    //var setter = DynamicExpressionTreeHelper
                    //                       .CreateTargetSetPropertyValueAction
                    //                            <TTarget, TProperty>
                    //                               (
                    //                                   propertyName
                    //                               );

                    var setter = DynamicExpressionTreeHelper
                                            .CreateMemberSetter<TTarget, TMember>
                                                    (
                                                        memberName
                                                    );
                    setter(target, memberValue);
                }
            //}
        }
        private static PerformanceCounter CreatePerformanceCounter
                                        (
                                            string categoryName
                                            , string counterName
                                            , string instanceName
                                            , PerformanceCounterInstanceLifetime instanceLifetime
                                            , long? instanceInitializeRawValue
                                        )
        {
            var performanceCounter = new PerformanceCounter()
            {
                CategoryName = categoryName
                ,
                CounterName = counterName
                ,
                InstanceLifetime = instanceLifetime
                ,
                InstanceName = instanceName
                ,
                ReadOnly = false
                //                                    ,
                //RawValue = 0
            };
            if (instanceInitializeRawValue != null)
            {
                performanceCounter.RawValue = instanceInitializeRawValue.Value;
            }
            return performanceCounter;
        }
        public static CounterCreationData GetCounterCreationData
                                                (
                                                    string counterName
                                                    , PerformanceCounterType
                                                                performanceCounterType
                                                )
        {
            return
                    new CounterCreationData()
                    {
                        CounterName = counterName
                        , CounterHelp = string.Format("{0} Help", counterName)
                        , CounterType = performanceCounterType
                    };
        }
    }
}
#endif
