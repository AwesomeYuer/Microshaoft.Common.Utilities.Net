namespace Microshaoft
{
    using System;
    using System.Globalization;
    public static class DateTimeHelper
    {
        public static void GetAlignSecondsDateTimes<T>
                                (
                                    DateTime time
                                    , params Tuple<long, Func<DateTime, long, DateTime, T, T>>[] processAlignSecondsDateTimesFuncs
                                )
        {
            T r = default(T);
            foreach (var x in processAlignSecondsDateTimesFuncs)
            {
                var alignSeconds = x.Item1;
                var alignTime = DateTimeHelper.GetAlignSecondsDateTime(time, alignSeconds);
                if (x.Item2 != null)
                {
                    r = x.Item2(time, alignSeconds, alignTime, r);
                }
            }
        }
        public static bool IsVaildateTimestamp(DateTime timeStamp, int timeoutSeconds)
        {
            long l = SecondsDiffNow(timeStamp);
            return ((l > 0) && (l < timeoutSeconds));
        }

        public static long MillisecondsDiff(DateTime time1, DateTime time2)
        {
            long t1 = time1.Ticks;
            long t2 = time2.Ticks;
            return (t2 - t1) / 10000;
        }
        public static long SecondsDiff(DateTime time1, DateTime time2)
        {
            return MillisecondsDiff(time1, time2) / 1000;
        }
        public static long MillisecondsDiffNow(DateTime time)
        {
            return MillisecondsDiff(time, DateTime.Now);
        }
        public static long SecondsDiffNow(DateTime time)
        {
            return MillisecondsDiffNow(time) / 1000;
        }
        public static DateTime GetAlignSecondsDateTime(DateTime time, long alignSeconds)
        {
            long ticks = time.Ticks;
            ticks -= ticks % (10000 * 1000 * alignSeconds);
            DateTime dt = new DateTime(ticks);
            return dt;
        }
        public static string GetDateTimeString(DateTime time, string format)
        {
            return time.ToString(format);
        }
        public static DateTime? ParseExactNullableDateTime(string text, string format)
        {
            DateTime time;
            DateTime? r = DateTime.TryParseExact
                                        (
                                            text
                                            , format
                                            , DateTimeFormatInfo.InvariantInfo
                                            , DateTimeStyles.None
                                            , out time
                                        ) ? time as DateTime? : null;
            return r;
        }
    }
}