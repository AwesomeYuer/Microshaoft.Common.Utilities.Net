#if NETFRAMEWORK4_X

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
            var type = typeof(TPerformanceCountersContainer);
            return
                type
                    .GetCustomAttributedMembers<PerformanceCounterDefinitionAttribute>
                        (
                            (memberTypes, memberInfo, attribute) =>
                            {
                                var r = false;
                                if (attribute != null)
                                {
                                    r = (attribute.Level == level);
                                }
                                return r;
                            }
                        )
                        .Select
                        (
                            (x) =>
                            {
                                var func = DynamicExpressionTreeHelper
                                                .CreateMemberGetter
                                                    <TPerformanceCountersContainer, PerformanceCounter>
                                                        (
                                                            x.Name
                                                        );
                                return func(container);
                            }
                        );
            //return
            //    typeof(TPerformanceCountersContainer)
            //        .GetProperties()
            //        .Where
            //            (
            //                (pi) =>
            //                {
            //                    var r = false;
            //                    var attribute
            //                            = pi
            //                                .GetCustomAttributes(false)
            //                                .FirstOrDefault
            //                                    (
            //                                        (x) =>
            //                                        {
            //                                            return
            //                                                (
            //                                                    x as PerformanceCounterDefinitionAttribute
            //                                                    !=
            //                                                    null
            //                                                );
            //                                        }
            //                                    ) as PerformanceCounterDefinitionAttribute;
            //                    if (attribute != null)
            //                    {
            //                        r = (attribute.Level == level);
            //                    }
            //                    return r;
            //                }
            //            )
            //            .Select
            //                (
            //                    (pi) =>
            //                    {
            //                        var func =
            //                                DynamicExpressionTreeHelper
            //                                    .CreateGetPropertyValueFunc
            //                                        <TPerformanceCountersContainer, PerformanceCounter>
            //                                            (
            //                                                pi.Name
            //                                            );
            //                        return func(container);
            //                    }
            //                );
        }
        protected void AttachPerformanceCountersToMembers<TContainer>
                            (
                                string categoryName
                                , string instanceName
                                , TContainer container
                                , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Global
                                , long? initializePerformanceCounterInstanceRawValue = null
                            )
        {
            PerformanceCountersHelper
                .AttachPerformanceCountersToMembers<TContainer>
                    (
                        categoryName
                        , instanceName
                        , container
                        , performanceCounterInstanceLifetime
                        , initializePerformanceCounterInstanceRawValue
                    );
        }
        public abstract void AttachPerformanceCountersToMembers
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
            //var propertyInfo
            //        = typeof(TPerformanceCountersContainer)
            //                .GetProperties()
            //                .FirstOrDefault
            //                    (
            //                        (pi) =>
            //                        {
            //                            var rr = false;
            //                            var attribute
            //                                    = pi
            //                                        .GetCustomAttributes(false)
            //                                        .FirstOrDefault
            //                                            (
            //                                                (x) =>
            //                                                {
            //                                                    return
            //                                                        (
            //                                                            x as PerformanceCounterDefinitionAttribute
            //                                                            !=
            //                                                            null
            //                                                        );
            //                                                }
            //                                            ) as PerformanceCounterDefinitionAttribute;
            //                            if (attribute != null)
            //                            {
            //                                rr = (attribute.CounterName == performanceCounterName);
            //                            }
            //                            return rr;
            //                        }
            //                    );

            var type = typeof(TPerformanceCountersContainer);
            //return
            var memberInfo = type
                                .GetCustomAttributedMembers<PerformanceCounterDefinitionAttribute>
                                    (
                                        (memberTypes, x, attribute) =>
                                        {
                                            var rr = false;
                                            if (attribute != null)
                                            {
                                                rr = true;
                                            }
                                            return rr;
                                        }
                                    )
                                .SingleOrDefault();

            PerformanceCounter r = null;
            Func<TPerformanceCountersContainer, PerformanceCounter> func = null;
            if (memberInfo != null)
            {
                func = DynamicExpressionTreeHelper
                            .CreateMemberGetter
                                <TPerformanceCountersContainer, PerformanceCounter>
                                    (
                                        memberInfo.Name
                                    );
                r = func(target);
            }
            return r;
        }
        protected IEnumerable<MemberInfo> GetPerformanceCountersMembers<TPerformanceCountersContainer>()
        {
            return
                TypeHelper
                    .GetMembersByMemberType<TPerformanceCountersContainer, PerformanceCounter>();
        }

        protected IEnumerable<MemberInfo> GetPerformanceCountersPairsMembers<TPerformanceCountersContainer>()
        {
            return
                TypeHelper
                    .GetMembersByMemberType<TPerformanceCountersContainer, PerformanceCountersPair>();
        }

        protected IEnumerable<PerformanceCounter>
                        GetMembersPerformanceCounters<TPerformanceCountersContainer>
                            (
                                TPerformanceCountersContainer target
                            )
        {
            return
                //GetPerformanceCountersPropertiesOrFields
                GetPerformanceCountersMembers
                    <TPerformanceCountersContainer>()
                        .Select
                            (
                                (x) =>
                                {
                                    return
                                            DynamicExpressionTreeHelper
                                                .CreateMemberGetter
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
                        InitializeProcessingTypedPerformanceCounters
                            <TPerformanceCountersContainer>
                                (
                                    TPerformanceCountersContainer target
                                    , PerformanceCounterProcessingFlagsType
                                                inclusivePerformanceCounterProcessingFlagsType
                                    , PerformanceCounterProcessingFlagsType
                                                exclusivePerformanceCounterProcessingFlagsType
                                                        = PerformanceCounterProcessingFlagsType.None
                                )
                            where
                                TPerformanceCountersContainer : AbstractPerformanceCountersContainer
        {
            var members = GetPerformanceCountersMembers<TPerformanceCountersContainer>(); 
            var memberName = string
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
           var setter = DynamicExpressionTreeHelper
                                .CreateMemberSetter
                                        <TPerformanceCountersContainer, PerformanceCounter[]>
                                            (
                                                memberName
                                            );
           var targetValue = members
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
                                                DynamicExpressionTreeHelper
                                                    .CreateMemberGetter
                                                        <TPerformanceCountersContainer, PerformanceCounter>
                                                            (
                                                                x.Name
                                                            )(target);
                                        }
                                    )
                                .ToArray();
            setter(target, targetValue);
        }
        protected void
                        InitializeProcessingTypedPerformanceCountersPairs
                            <TPerformanceCountersContainer>
                                (
                                    TPerformanceCountersContainer target
                                    , PerformanceCounterProcessingFlagsType
                                                inclusivePerformanceCounterProcessingFlagsType
                                    , PerformanceCounterProcessingFlagsType
                                                exclusivePerformanceCounterProcessingFlagsType
                                                        = PerformanceCounterProcessingFlagsType.None
                                )
                            where
                                TPerformanceCountersContainer : AbstractPerformanceCountersContainer
        {
            //var properties = GetPerformanceCountersPropertiesOrFields<TPerformanceCountersContainer, PerformanceCountersPair>();
            var members = GetPerformanceCountersPairsMembers<TPerformanceCountersContainer>();
            var memberName = string
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
            var setter = DynamicExpressionTreeHelper
                                    .CreateMemberSetter
                                         <TPerformanceCountersContainer, PerformanceCountersPair[]>
                                             (
                                                 memberName
                                             );
            var targetValue = members
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
                                                    DynamicExpressionTreeHelper
                                                        .CreateMemberGetter
                                                            <TPerformanceCountersContainer, PerformanceCountersPair>
                                                                (
                                                                    x.Name
                                                                )(target);
                                            }
                                        )
                                    .ToArray();
            setter(target, targetValue);
        }
    }
}

#endif
