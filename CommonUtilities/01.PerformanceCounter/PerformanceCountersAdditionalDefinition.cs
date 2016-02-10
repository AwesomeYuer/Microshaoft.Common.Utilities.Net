namespace Microshaoft
{
    using System;
    using System.Diagnostics;
    [FlagsAttribute]
    public enum MultiPerformanceCountersTypeFlags : ushort
    {
        None = 0,
        ProcessCounter = 1,
        ProcessingCounter = 2,
        ProcessedCounter = 4,
        ProcessedAverageTimerCounter = 8,
        ProcessedRateOfCountsPerSecondCounter = 16,

        ProcessNonTimeBasedCounters = ProcessCounter | ProcessingCounter | ProcessedCounter,
        ProcessTimeBasedCounters = ProcessedAverageTimerCounter | ProcessedRateOfCountsPerSecondCounter,
        ProcessAllCounters = ProcessNonTimeBasedCounters | ProcessTimeBasedCounters
    };
    [AttributeUsage(AttributeTargets.Property, AllowMultiple = false, Inherited = false)]
    public class PerformanceCounterDefinitionAttribute : Attribute
    {
        public PerformanceCounterType CounterType;
        public string CounterName;
        public uint Level = 0;
        public PerformanceCounterInstanceLifetime? CounterInstanceLifetime;
        public long? CounterInstanceInitializeRawValue;
    }
}