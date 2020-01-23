#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Diagnostics;

    public interface IPerformanceCountersContainer : IPerformanceCountersValuesClearable
    {
        PerformanceCounter this[string key]
        {
            get;
        }
        IEnumerable<PerformanceCounter> PerformanceCounters
        {
            get;
        }
    }
    public interface ICommonPerformanceCountersContainer
    {
        PerformanceCounter PrcocessPerformanceCounter
        {
            get;
        }
        PerformanceCounter ProcessingPerformanceCounter
        {
            get;
        }
        PerformanceCounter ProcessedPerformanceCounter
        {
            get;
        }
        PerformanceCounter ProcessedRateOfCountsPerSecondPerformanceCounter
        {
            get;
        }
        PerformanceCounter ProcessedAverageTimerPerformanceCounter
        {
            get;
        }
        PerformanceCounter ProcessedAverageBasePerformanceCounter
        {
            get;
        }
        PerformanceCounter CaughtExceptionsPerformanceCounter
        {
            get;
        }
    }
}

#endif
