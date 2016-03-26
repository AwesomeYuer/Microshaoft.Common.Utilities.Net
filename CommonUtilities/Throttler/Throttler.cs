namespace Microshaoft
{
    using System;
    using System.Collections.Concurrent;
    using System.Linq;
    using System.Threading;

    public class Throttler2
    {
        private ConcurrentDictionary<DateTime, ThrottlerEntry> _data
                        = new ConcurrentDictionary<DateTime, ThrottlerEntry>();

        private class ThrottlerEntry
        {
            public int Current = 0;
            public bool Flag = false;
        }

        private long _samplingPeriodInSeconds = 5;
        private int _clearExpiredIntervalInSeconds = 60;
        private int _clearExpiredInSeconds = 60;
        private int _throttle = 0;

        public Throttler2
                    (
                        int throttle
                        , int samplingPeriodInSeconds
                        , int clearExpiredIntervalInSeconds = 60
                        , int clearExpiredInSeconds = 60
                    )
        {
            _throttle = throttle;
            _samplingPeriodInSeconds = samplingPeriodInSeconds;
            _clearExpiredIntervalInSeconds = clearExpiredIntervalInSeconds;
            _clearExpiredInSeconds = clearExpiredInSeconds;
            new EasyTimer
                    (
                        _clearExpiredIntervalInSeconds
                        , 1
                        , (x) =>
                            {

                            }
                        , false
                        , true
                        , (x, y, z, w) =>
                            {
                                var result = _data
                                                .Where
                                                    (
                                                        (xx) =>
                                                        {
                                                            return
                                                                (
                                                                    DateTimeHelper
                                                                        .SecondsDiff(xx.Key, DateTime.Now)
                                                                    >
                                                                    _clearExpiredInSeconds
                                                                );
                                                        }
                                                    );
                                foreach (var xx in result)
                                {
                                    ThrottlerEntry throttlerEntry = null;
                                    _data.TryRemove
                                            (
                                                xx.Key
                                                , out throttlerEntry
                                            );
                                }
                                return false;
                            }
                    ).Start();

        }

        public bool IncrementBy(int increment, out int current , out long sleepInMilliseconds)
        {
            var r = true;
            sleepInMilliseconds = 0;
            var dateTime = DateTimeHelper
                                .GetAlignSecondsDateTime
                                    (
                                        DateTime.Now
                                        , _samplingPeriodInSeconds
                                    );
            var throttlerEntry = _data.GetOrAdd
                                    (
                                       dateTime
                                       , new ThrottlerEntry()
                                    );
            if (!throttlerEntry.Flag)
            {
                if
                    (
                        (current = Interlocked.Add(ref throttlerEntry.Current, increment))
                        >=
                        _throttle
                    )
                {
                    throttlerEntry.Flag = true;
                    r = true;
                }
            }
            else
            {
                current = throttlerEntry.Current;
                var dateTimeNext = DateTimeHelper
                                        .GetAlignSecondsDateTime
                                            (
                                                DateTime.Now
                                                , _samplingPeriodInSeconds
                                            );
                if (dateTime == dateTimeNext)
                {
                    sleepInMilliseconds =
                        (
                            _samplingPeriodInSeconds * 1000
                            -
                            DateTimeHelper.MillisecondsDiff(dateTime, DateTime.Now)
                        );
                    if (sleepInMilliseconds < 0)
                    {
                        sleepInMilliseconds = 0;
                    }
                }
            
            }
            return r;
        }
    }
}
