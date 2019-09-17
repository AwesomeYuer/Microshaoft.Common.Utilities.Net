#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System.Collections.Generic;
    using System.Diagnostics;
    public interface IPerformanceCountersValuesClearable
    {
        void ClearPerformanceCountersValues(int level);
        //void ClearPerformanceCountersValues(ref bool enabledCountPerformance, int level);
        IEnumerable<PerformanceCounter> GetPerformanceCountersByLevel(int level);
    }
}
#endif
