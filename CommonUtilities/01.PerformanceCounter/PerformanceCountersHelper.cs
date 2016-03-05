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
                                Func<bool> onEnabledCountPerformanceProcessFunc = null
                                , bool reThrowException = false
                                , PerformanceCounter[] IncrementCountersBeforeCountPerformance = null
                                , PerformanceCounter[] DecrementCountersBeforeCountPerformance = null
                                , Tuple
                                        <
                                            bool						//before时是否已经启动
                                            , Stopwatch
                                            , PerformanceCounter
                                            , PerformanceCounter		//base计数器
                                        >[] timerCounters = null
                                , Action onTryCountPerformanceProcessAction = null
                                , Func<Exception, Exception, string, bool> onCaughtExceptionCountPerformanceProcessFunc = null
                                , Action<bool, Exception, Exception, string> onFinallyCountPerformanceProcessAction = null
                                , PerformanceCounter[] DecrementCountersAfterCountPerformance = null
                                , PerformanceCounter[] IncrementCountersAfterCountPerformance = null
                            )
        {
            var enabledCountPerformance = true;
            {
                if (onEnabledCountPerformanceProcessFunc != null)
                {
                    enabledCountPerformance = onEnabledCountPerformanceProcessFunc();
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
                                            reThrowException = onCaughtExceptionCountPerformanceProcessFunc
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
                                                                    Stopwatch stopwatch = xx.Item2;
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

        public static void AttachPerformanceCountersToProperties<T>
                                    (
                                        string performanceCountersCategoryName
                                        , string performanceCounterInstanceName
                                        , T target//= default(T)
                                        , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
                                        , long? initializePerformanceCounterInstanceRawValue = null
                                    )
        {
            var type = typeof(T);
            var properties = type
                                .GetProperties()
                                .Where
                                    (
                                        (pi) =>
                                        {
                                            var parameters = pi.GetIndexParameters();
                                            return
                                                (
                                                    (
                                                        pi.PropertyType == typeof(PerformanceCounter)
                                                        ||
                                                        pi.PropertyType == typeof(PerformanceCountersPair)
                                                    )
                                                    && (parameters == null ? 0 : parameters.Length) <= 0
                                                );
                                        }
                                    );
            if (!PerformanceCounterCategory.Exists(performanceCountersCategoryName))
            {
                var ccdc = new CounterCreationDataCollection();
                foreach (PropertyInfo propertyInfo in properties)
                {
                    var propertyName = propertyInfo.Name;
                    var performanceCounterType = PerformanceCounterType.NumberOfItems64;
                    var performanceCounterName = propertyName;
                    var attribute
                            = propertyInfo
                                .GetCustomAttribute<PerformanceCounterDefinitionAttribute>();
                                    
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
                    if (propertyInfo.PropertyType == typeof(PerformanceCountersPair))
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
                            performanceCountersCategoryName
                            , string.Format("{0} Category Help.", performanceCountersCategoryName)
                            , PerformanceCounterCategoryType.MultiInstance
                            , ccdc
                        );
            }
            foreach (PropertyInfo propertyInfo in properties)
            {
                //PerformanceCounter performanceCounter = null;
                var propertyName = propertyInfo.Name;
                var performanceCounterType = PerformanceCounterType.NumberOfItems64;
                var performanceCounterName = propertyName;
                //var performanceCounterInstanceLifetime = defaultPerformanceCounterInstanceLifetime;
                long? performanceCounterInstanceInitializeRawValue = initializePerformanceCounterInstanceRawValue;
                var attribute
                            = propertyInfo
                                .GetCustomAttribute<PerformanceCounterDefinitionAttribute>();
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
                    var counterInstanceLifetime = attribute.CounterInstanceLifetime;
                    if (counterInstanceLifetime != null && counterInstanceLifetime.HasValue)
                    {
                        performanceCounterInstanceLifetime = counterInstanceLifetime.Value;
                    }
                    var counterInstanceInitializeRawValue = attribute.CounterInstanceInitializeRawValue;
                    if (counterInstanceInitializeRawValue != null && counterInstanceInitializeRawValue.HasValue)
                    {
                        performanceCounterInstanceInitializeRawValue = counterInstanceInitializeRawValue.Value;
                    }
                }
                var performanceCounter
                                    = CreatePerformanceCounter
                                            (
                                                performanceCountersCategoryName
                                                , performanceCounterInstanceName
                                                , performanceCounterInstanceLifetime
                                                , performanceCounterName
                                                , performanceCounterInstanceInitializeRawValue
                                            );
                if (propertyInfo.PropertyType == typeof(PerformanceCounter))
                {
                    
                    SetPropertyValueToTarget(target, propertyInfo, performanceCounter);
                }
                else if (propertyInfo.PropertyType == typeof(PerformanceCountersPair))
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
                                                performanceCountersCategoryName
                                                , performanceCounterInstanceName
                                                , performanceCounterInstanceLifetime
                                                , performanceCounterName
                                                , performanceCounterInstanceInitializeRawValue
                                            );


                    var performanceCountersPair = new PerformanceCountersPair()
                    {
                        Counter = performanceCounter
                        , BaseCounter = basePerformanceCounter
                    };
                    SetPropertyValueToTarget(target, propertyInfo, performanceCountersPair);

                }
                                                             




            }
        }

        private static void SetPropertyValueToTarget<TTarget, TProperty>
                                (
                                        TTarget target
                                        , PropertyInfo propertyInfo
                                        , TProperty propertyValue
                                )
        {
            string propertyName = propertyInfo.Name;
            if (propertyInfo.GetGetMethod().IsStatic)
            {
                var setter = DynamicPropertyAccessor
                                    .CreateTargetSetStaticPropertyValueAction<TTarget, TProperty>
                                        (
                                            propertyName
                                        );
                setter(propertyValue);
            }
            else
            {
                if (target != null)
                {
                    var setter = DynamicPropertyAccessor
                                           .CreateTargetSetPropertyValueAction<TTarget, TProperty>
                                               (
                                                   propertyName
                                               );
                    setter(target, propertyValue);
                }
            }
        }

        private static PerformanceCounter CreatePerformanceCounter(string performanceCountersCategoryName, string performanceCounterInstanceName, PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime, string performanceCounterName, long? performanceCounterInstanceInitializeRawValue)
        {
            var performanceCounter = new PerformanceCounter()
            {
                CategoryName = performanceCountersCategoryName
                ,
                CounterName = performanceCounterName
                ,
                InstanceLifetime = performanceCounterInstanceLifetime
                ,
                InstanceName = performanceCounterInstanceName
                ,
                ReadOnly = false
                //                                    ,
                //RawValue = 0
            };
            if (performanceCounterInstanceInitializeRawValue != null)
            {
                performanceCounter.RawValue = performanceCounterInstanceInitializeRawValue.Value;
            }

            return performanceCounter;
        }

        public static CounterCreationData GetCounterCreationData
                                                (
                                                    string counterName
                                                    , PerformanceCounterType performanceCounterType
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