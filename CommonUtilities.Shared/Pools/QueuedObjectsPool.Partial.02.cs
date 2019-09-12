#if NETFRAMEWORK4_X

namespace Microshaoft
{
    using System.Diagnostics;
    public partial class QueuedObjectsPool<T> where T: new()
    {
        //private CommonPerformanceCountersContainer _performanceCountersContainer = null;
        private bool _isAttachPerformanceCounters = false;
        public void AttachPerformanceCountersCategoryInstance
                                (
                                    string performanceCountersCategoryName
                                    , string performanceCountersCategoryInstanceNamePrefix
                                    , MultiPerformanceCountersTypeFlags enablePerformanceCounters = MultiPerformanceCountersTypeFlags.ProcessNonTimeBasedCounters
                                    , PerformanceCounterInstanceLifetime performanceCounterInstanceLifetime = PerformanceCounterInstanceLifetime.Process
                                )
        {
            if (!_isAttachPerformanceCounters)
            {
                _isAttachPerformanceCounters = true;
            }
            else
            {
                CommonPerformanceCountersContainer container = null;
                EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                        .AttachPerformanceCountersCategoryInstance
                            (
                                performanceCountersCategoryName
                                , string.Format
                                            (
                                                "{1}{0}{2}"
                                                , "-"
                                                , "Non-Pooled Objects"
                                                , performanceCountersCategoryInstanceNamePrefix
                                            ) 
                                , out container
                                , PerformanceCounterInstanceLifetime.Process
                                , initializePerformanceCounterInstanceRawValue: 1009
                            );
                EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                        .AttachPerformanceCountersCategoryInstance
                            (
                                performanceCountersCategoryName
                                , string.Format
                                            (
                                                "{1}{0}{2}"
                                                , "-"
                                                , "Pooled Objects"
                                                , performanceCountersCategoryInstanceNamePrefix
                                            )
                                , out container
                                , PerformanceCounterInstanceLifetime.Process
                                , initializePerformanceCounterInstanceRawValue: 1009
                            );

            }
        }
        
    }
}

#endif
