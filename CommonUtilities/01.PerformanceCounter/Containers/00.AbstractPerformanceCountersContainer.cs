namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    public abstract class AbstractPerformanceCountersContainer
                                : IPerformanceCountersContainer, IPerformanceCountersValuesClearable
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

        protected IEnumerable<PerformanceCounter>
                        GetPropertiesPerformanceCounters<TPerformanceCountersContainer>
                            (
                                TPerformanceCountersContainer target
                            )
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
                                        typeof(PerformanceCounter)
                                    );
                            }
                        ).Select
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
    }
}
