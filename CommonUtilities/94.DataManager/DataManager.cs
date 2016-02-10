namespace Microsoft.Boc.Communication.Configurations
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using System.Threading.Tasks;

    public static class DataManager
    {
        //public static Throttler2 ConcurrentSendMessagesPerPerodControlThrottler2;

        public static Throttler ConcurrentSendMessagesPerPerodControlThrottler;

        public static MessagesPerformanceCountersContainer ServerSendedToClientMessagesPerformanceCountersContainer;
        private static EasyReaderWriterLockSlim _easyReaderWriterLockSlim = new EasyReaderWriterLockSlim();
        private static volatile float _recentLastValueOfServerSendedToClientMessagesPerformanceCountersRateOfCountsPerSecond = 0;

        public static float RecentLastValueOfServerSendedToClientMessagesPerformanceCountersRateOfCountsPerSecond
        {
            get 
            {
                return _recentLastValueOfServerSendedToClientMessagesPerformanceCountersRateOfCountsPerSecond;
            }
            set
            {
                _easyReaderWriterLockSlim
                    .SafeWrite
                        (
                            (x) =>
                            {
                                _recentLastValueOfServerSendedToClientMessagesPerformanceCountersRateOfCountsPerSecond = value;
                            }
                        );
            }
        }

    }
}
