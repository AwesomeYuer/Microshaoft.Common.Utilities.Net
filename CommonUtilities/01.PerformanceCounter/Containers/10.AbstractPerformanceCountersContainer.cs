namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Reflection;
    public class PerformanceCountersPair
    {
        public PerformanceCounter Counter
        {
            get;
            set;
           
        }

        public PerformanceCounter BaseCounter
        {
            get;
            set;
        }
    }



    public abstract class AbstractPerformanceCountersContainer
                                : IPerformanceCountersContainer
                                    , IPerformanceCountersValuesClearable
    {
        private readonly object  _clearPerformanceCountersValueslocker = new object();
        public void ClearPerformanceCountersValues(int level)
        {
            var counters = GetPerformanceCountersByLevel(level);
            foreach (var counter in counters)
            {
                counter
                    .RawValue = 0;
            }
        }
        public void ClearPerformanceCountersValues(ref bool enabledCountPerformance, int level)
        {
            lock (_clearPerformanceCountersValueslocker)
            {
                if (enabledCountPerformance)
                {
                    enabledCountPerformance = false;
                    {
                        ClearPerformanceCountersValues(level);
                    }
                    enabledCountPerformance = true;
                }
            }
        }
        public abstract IEnumerable<PerformanceCounter> GetPerformanceCountersByLevel(int level);
        protected IEnumerable<PerformanceCounter>
                        GetPerformanceCountersByLevel<TPerformanceCountersContainer>
                            (
                                TPerformanceCountersContainer container
                                , int level
                            )
        {
            return
                typeof(TPerformanceCountersContainer)
                    .GetProperties()
                    .Where
                        (
                            (pi) =>
                            {
                                var r = false;
                                var attribute
                                        = pi
                                            .GetCustomAttributes(false)
                                            .FirstOrDefault
                                                (
                                                    (x) =>
                                                    {
                                                        return
                                                            (
                                                                x as PerformanceCounterDefinitionAttribute
                                                                !=
                                                                null
                                                            );
                                                    }
                                                ) as PerformanceCounterDefinitionAttribute;
                                if (attribute != null)
                                {
                                    r = (attribute.Level == level);
                                }
                                return r;
                            }
                        )
                        .Select
                            (
                                (pi) =>
                                {
                                    var func =
                                            DynamicPropertyAccessor
                                                .CreateGetPropertyValueFunc
                                                    <TPerformanceCountersContainer, PerformanceCounter>
                                                        (
                                                            pi.Name
                                                        );
                                    return func(container);
                                }
                            );
        }

        protected void AttachPerformanceCountersToProperties<TContainer>
                            (
                                string categoryName
                                , string instanceName
                                , TContainer container
                                , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
                                , long? initializePerformanceCounterInstanceRawValue = null
                            )
        {
            PerformanceCountersHelper
                .AttachPerformanceCountersToProperties<TContainer>
                    (
                        categoryName
                        , instanceName
                        , container
                        , performanceCounterInstanceLifetime
                        , initializePerformanceCounterInstanceRawValue

                    );
        }
        public abstract void AttachPerformanceCountersToProperties
                                (
                                    string categoryName
                                    , string instanceName
                                    , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
                                    , long? initializePerformanceCounterInstanceRawValue = null
                                );
        public abstract PerformanceCounter this[string key]
        {
            get;
        }
        protected PerformanceCounter GetPerformanceCounterByName
                                            <TPerformanceCountersContainer>
                                                (
                                                    TPerformanceCountersContainer target
                                                    , string performanceCounterName
                                                )
        {
            
            var propertyInfo
                    = typeof(TPerformanceCountersContainer)
                            .GetProperties()
                            .FirstOrDefault
                                (
                                    (pi) =>
                                    {
                                        var rr = false;
                                        var attribute
                                                = pi
                                                    .GetCustomAttributes(false)
                                                    .FirstOrDefault
                                                        (
                                                            (x) =>
                                                            {
                                                                return
                                                                    (
                                                                        x as PerformanceCounterDefinitionAttribute
                                                                        !=
                                                                        null
                                                                    );
                                                            }
                                                        ) as PerformanceCounterDefinitionAttribute;
                                        if (attribute != null)
                                        {
                                            rr = (attribute.CounterName == performanceCounterName);
                                        }
                                        return rr;
                                    }
                                );
            PerformanceCounter r = null;
            Func<TPerformanceCountersContainer, PerformanceCounter> func = null;
            if (propertyInfo != null)
            {
                func = DynamicPropertyAccessor
                            .CreateGetPropertyValueFunc
                                <TPerformanceCountersContainer, PerformanceCounter>
                                    (
                                        propertyInfo.Name
                                    );
                r = func(target);
            }
            return r;
        }

        protected IEnumerable<PropertyInfo> GetPerformanceCountersProperties<TPerformanceCountersContainer, TProperty>()
        {
            return
                typeof(TPerformanceCountersContainer)
                    .GetProperties()
                    .Where
                        (
                            (x) =>
                            {
                                return
                                    (
                                        x.PropertyType
                                        ==
                                        typeof(TProperty)
                                    );
                            }
                        );

        }


        protected IEnumerable<PerformanceCounter>
                        GetPropertiesPerformanceCounters<TPerformanceCountersContainer>
                            (
                                TPerformanceCountersContainer target
                            )
        {
            return
                GetPerformanceCountersProperties<TPerformanceCountersContainer, PerformanceCounter>()
                    .Select
                            (
                                (x) =>
                                {
                                    return
                                            DynamicPropertyAccessor
                                                .CreateGetPropertyValueFunc
                                                    <TPerformanceCountersContainer, PerformanceCounter>
                                                        (
                                                            x.Name
                                                        )(target);
                                }
                            );
        }
        public abstract IEnumerable<PerformanceCounter> PerformanceCounters 
        {
            get;
        }

        public abstract PerformanceCounter[] IncrementOnBeginPerformanceCounters
        {
            get;
            set;
        }
        public abstract PerformanceCounter[] DecrementOnBeginPerformanceCounters
        {
            get;
            set;
        }
        public abstract PerformanceCounter[] IncrementOnEndPerformanceCounters
        {
            get;
            set;
        }
        public abstract PerformanceCounter[] DecrementOnEndPerformanceCounters
        {
            get;
            set;
        }
        public abstract PerformanceCounter[] IncrementOnBeginDecrementOnEndPerformanceCounters
        {
            get;
            set;
        }
        public abstract PerformanceCounter[] TimeBasedOnBeginOnEndPerformanceCounters
        {
            get;
            set;
        }
        public abstract PerformanceCountersPair[] TimeBasedOnBeginOnEndPerformanceCountersPairs
        {
            get;
            set;
        }
        protected void
                        InitializeProcessingTypedPerformanceCounters<TPerformanceCountersContainer>
                            (
                                TPerformanceCountersContainer target
                                , PerformanceCounterProcessingFlagsType
                                            inclusivePerformanceCounterProcessingFlagsType
                                , PerformanceCounterProcessingFlagsType
                                            exclusivePerformanceCounterProcessingFlagsType
                                                    = PerformanceCounterProcessingFlagsType.None
                            )
            where TPerformanceCountersContainer : AbstractPerformanceCountersContainer
        {
            var properties = GetPerformanceCountersProperties<TPerformanceCountersContainer, PerformanceCounter>();
            var propertyName =
                                string
                                    .Format
                                        (
                                            "{0}{1}"
                                            , Enum
                                                .GetName
                                                    (
                                                        typeof(PerformanceCounterProcessingFlagsType)
                                                        , inclusivePerformanceCounterProcessingFlagsType
                                                    )
                                            , "PerformanceCounters"
                                        );
           var setter = DynamicPropertyAccessor
                                .CreateTargetSetPropertyValueAction
                                        <TPerformanceCountersContainer, PerformanceCounter[]>
                                            (
                                                propertyName
                                            );
            var setterValue = properties
                                    .Where
                                        (
                                            (x) =>
                                            {
                                                var r = false;
                                                var attribute = x
                                                                    .GetCustomAttribute
                                                                        <PerformanceCounterDefinitionAttribute>();
                                                if (attribute != null)
                                                {
                                                    r = attribute
                                                            .CounterProcessingType
                                                            .HasFlag(inclusivePerformanceCounterProcessingFlagsType);
                                                    //if
                                                    //      (
                                                    //          r
                                                    //          &&
                                                    //          (
                                                    //              exclusivePerformanceCounterProcessingFlagsType
                                                    //              !=
                                                    //              PerformanceCounterProcessingFlagsType.None
                                                    //          )
                                                    //      )
                                                    //{
                                                    //    r = !attribute
                                                    //                .CounterProcessingType
                                                    //                .HasFlag(exclusivePerformanceCounterProcessingFlagsType);
                                                    //}
                                                }
                                                return r;
                                            }
                                        )
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                return
                                                    DynamicPropertyAccessor
                                                        .CreateGetPropertyValueFunc
                                                            <TPerformanceCountersContainer, PerformanceCounter>
                                                                (
                                                                    x.Name
                                                                )(target);
                                            }
                                        )
                                    .ToArray();
            setter(target,setterValue);
        }
        protected void
                        InitializeProcessingTypedPerformanceCountersPairs<TPerformanceCountersContainer>
                            (
                                TPerformanceCountersContainer target
                                , PerformanceCounterProcessingFlagsType
                                            inclusivePerformanceCounterProcessingFlagsType
                                , PerformanceCounterProcessingFlagsType
                                            exclusivePerformanceCounterProcessingFlagsType
                                                    = PerformanceCounterProcessingFlagsType.None
                            )
            where TPerformanceCountersContainer : AbstractPerformanceCountersContainer
        {
            var properties = GetPerformanceCountersProperties<TPerformanceCountersContainer, PerformanceCountersPair>();
            var propertyName =
                                string
                                    .Format
                                        (
                                            "{0}{1}"
                                            , Enum
                                                .GetName
                                                    (
                                                        typeof(PerformanceCounterProcessingFlagsType)
                                                        , inclusivePerformanceCounterProcessingFlagsType
                                                    )
                                            , "PerformanceCountersPairs"
                                        );
            var setter = DynamicPropertyAccessor
                                 .CreateTargetSetPropertyValueAction
                                         <TPerformanceCountersContainer, PerformanceCountersPair[]>
                                             (
                                                 propertyName
                                             );
            var setterValue = properties
                                    .Where
                                        (
                                            (x) =>
                                            {
                                                var r = false;
                                                var attribute = x
                                                                    .GetCustomAttribute
                                                                        <PerformanceCounterDefinitionAttribute>();
                                                if (attribute != null)
                                                {
                                                    r = attribute
                                                            .CounterProcessingType
                                                            .HasFlag(inclusivePerformanceCounterProcessingFlagsType);
                                                }
                                                return r;
                                            }
                                        )
                                    .Select
                                        (
                                            (x) =>
                                            {
                                                return
                                                    DynamicPropertyAccessor
                                                        .CreateGetPropertyValueFunc
                                                            <TPerformanceCountersContainer, PerformanceCountersPair>
                                                                (
                                                                    x.Name
                                                                )(target);
                                            }
                                        )
                                    .ToArray();
            setter(target, setterValue);
        }


    }
}
