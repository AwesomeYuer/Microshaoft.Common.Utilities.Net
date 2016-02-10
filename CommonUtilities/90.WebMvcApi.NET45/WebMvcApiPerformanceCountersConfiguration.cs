namespace Microsoft.Boc.Communication.Configurations
{
    using System.Diagnostics;
    public static class WebMvcApiPerformanceCountersConfiguration
    {
        public static void AttachPerformanceCounters()
        {
            CommonPerformanceCountersContainer commonPerformanceCountersContainer = null; 
            EasyPerformanceCountersHelper<CommonPerformanceCountersContainer>
                .AttachPerformanceCountersCategoryInstance
                (
                    WebMvcApiPerformanceCountersConfiguration
                        .PerformanceCountersCategoryName
                    , WebMvcApiPerformanceCountersConfiguration
                        .PerformanceCountersCategoryInstanceName
                    , out commonPerformanceCountersContainer
                );
        }
        public static string PerformanceCountersCategoryName
        {
            get
            {
                return ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .WebApiCountPerformanceActionFilterPerformanceCountersCategoryName;
            }
        }
        public static string PerformanceCountersCategoryInstanceName
        {
            get
            {
                return
                    string
                        .Format
                            (
                                "{2}{0}{3}{1}{4}"
                                , ": "
                                , " @ "
                                , Process
                                    .GetCurrentProcess()
                                    .ProcessName
                                , ""
                                , ConfigurationAppSettingsManager
                                    .RunTimeAppSettings
                                    .WebApiCountPerformanceActionFilterPerformanceCountersCategoryInstanceName
                            );
            }
        }
        public static MultiPerformanceCountersTypeFlags EnableCounters
        {
            get
            {
                var enableCounters =
                                MultiPerformanceCountersTypeFlags.ProcessCounter
                                | MultiPerformanceCountersTypeFlags.ProcessedAverageTimerCounter
                                | MultiPerformanceCountersTypeFlags.ProcessedCounter
                                | MultiPerformanceCountersTypeFlags.ProcessedRateOfCountsPerSecondCounter
                                | MultiPerformanceCountersTypeFlags.ProcessingCounter;
                if 
                    (
                        !ConfigurationAppSettingsManager
                            .RunTimeAppSettings
                            .EnableCountPerformance
                    )
                {
                    enableCounters = MultiPerformanceCountersTypeFlags.None;
                }
                return enableCounters;
            }
        }
    }
}