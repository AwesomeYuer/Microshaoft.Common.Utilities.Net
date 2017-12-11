#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    [Flags]
    public enum MultiPerformanceCountersTypeFlags : ushort
    {
        None = 0,
        ProcessCounter = 1,
        ProcessingCounter = 2,
        ProcessedCounter = 4,
        ProcessedAverageTimerCounter = 8,
        ProcessedRateOfCountsPerSecondCounter = 1024,

        ProcessNonTimeBasedCounters = ProcessCounter | ProcessingCounter | ProcessedCounter,
        ProcessTimeBasedCounters = ProcessedAverageTimerCounter | ProcessedRateOfCountsPerSecondCounter,
        ProcessAllCounters = ProcessNonTimeBasedCounters | ProcessTimeBasedCounters
    };

    [FlagsAttribute]
    public enum PerformanceCounterProcessingFlagsType : ushort
    {
        None = 0,
        Increment = 1,
        Decrement = 2,

        //OnBegin  = 4,
        //OnEnd  = 8,

        IncrementOnBegin = 4,
        DecrementOnBegin = 8,

        IncrementOnEnd = 16,
        DecrementOnEnd = 32,

        //TimeBased = 16,

        TimeBasedOnBegin = 64,

        TimeBasedOnEnd = 128,

        TimeBasedOnBeginOnEnd = TimeBasedOnBegin | TimeBasedOnEnd,

        IncrementOnBeginDecrementOnEnd = IncrementOnBegin | DecrementOnEnd,

        NonTimeBased = Increment
                            | Decrement
                            | IncrementOnBegin
                            | DecrementOnBegin
                            | IncrementOnEnd
                            | DecrementOnEnd,

        All = Increment 
                | Decrement
                | IncrementOnBegin
                | DecrementOnBegin
                | IncrementOnEnd
                | DecrementOnEnd
                | TimeBasedOnBeginOnEnd
    }
    [
        AttributeUsage
            (
                AttributeTargets.Property
                , AllowMultiple = false
                , Inherited = false
            )
    ]
    public class PerformanceCounterDefinitionAttribute : Attribute
    {
        public PerformanceCounterType CounterType;
        public string CounterName;
        public PerformanceCounterType BaseCounterType;
        public string BaseCounterName;

        public uint Level = 0;
        public PerformanceCounterInstanceLifetime? CounterInstanceLifetime;
        public long? CounterInstanceInitializeRawValue;

        public PerformanceCounterProcessingFlagsType CounterProcessingType;
    }
}
#endif
