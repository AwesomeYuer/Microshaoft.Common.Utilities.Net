namespace Microshaoft
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml.Serialization;

    public static class TimeSpanHelper
    {
        public const int TimeSpanTicksPerSecond = 10000000;
        private static readonly double _timestampToTicks = TimeSpan.TicksPerSecond / (double)Stopwatch.Frequency;

        public static long PerformanceCounterTicks(this TimeSpan @this)
        {
            return @this.Ticks * Stopwatch.Frequency / TimeSpanTicksPerSecond;
        }

        public static double ToWellFormatSeconds(this TimeSpan @this)
        {
            return Math.Round(@this.TotalMilliseconds / 1000, 7, MidpointRounding.ToEven);
        }
        public static TimeSpan GetElapsedTimeToNow(this long @this)
        {
            var endTimestamp = Stopwatch.GetTimestamp();
            return
                GetElapsedTime(@this, endTimestamp);
        }
        public static TimeSpan GetElapsedTime(this long @this, long endTimestamp)
        {
            var timestampDelta = endTimestamp - @this;
            var ticks = (long) (_timestampToTicks * timestampDelta);
            return new TimeSpan(ticks);
        }
    }


    public static class DateTimeHelper
    {
        //public static readonly DateTime TimeEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc).Add(TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now));
        public static readonly DateTime TimeUtcEpoch = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
        public const string TimeShortFormat = @"yyyy-MM-dd HH:mm:ss";
        public const string TimeLongFormat = @"yyyy-MM-dd HH:mm:ss.fffffff";
        public const string TimeUIDisplayFormat = @"MM/dd/yyyy hh:mm:ss tt";

        /// <summary>
        /// The "O" or "o" standard format specifier represents a custom date and time format string 
        /// using a pattern that preserves time zone information and emits a result string that complies with ISO 8601. 
        /// The "O" or "o" standard format specifier corresponds to 
        /// the "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffK" custom format string for DateTime values 
        /// and to the "yyyy'-'MM'-'dd'T'HH':'mm':'ss'.'fffffffzzz" custom format string for DateTimeOffset values.
        /// </summary>
        public const string TimeIso8601Format = @"o";

        public static string ToFormatString(this DateTime? @this)
        {
            return @this.HasValue ? @this.Value.ToFormatString() : string.Empty;
        }

        public static string ToFormatShortString(this DateTime? @this)
        {
            return @this.HasValue ? @this.Value.ToFormatShortString() : string.Empty;
        }

        public static string ToFormatString(this DateTime @this)
        {
            return @this.ToString(TimeLongFormat, CultureInfo.InvariantCulture);
        }

        public static string ToFormatShortString(this DateTime @this)
        {
            return @this.ToString(TimeShortFormat, CultureInfo.InvariantCulture);
        }

        public static string ToIso8601FormatString(this DateTimeOffset? @this)
        {
            return @this.HasValue ? @this.Value.ToIso8601FormatString() : string.Empty;
        }

        public static string ToIso8601FormatString(this DateTimeOffset @this)
        {
            return @this.ToString(TimeIso8601Format, CultureInfo.InvariantCulture);
        }

        public static long TotalSecondsSinceEpoch(this DateTime @this)
        {
            return @this < TimeUtcEpoch ? 0 : (long)(@this - TimeUtcEpoch).TotalSeconds;
        }

        public static long TotalMillisecondsSinceEpoch(this DateTime @this)
        {
            return @this < TimeUtcEpoch ? 0 : (long)(@this - TimeUtcEpoch).TotalMilliseconds;
        }

        public static long TotalTicksSinceEpoch(this DateTime @this)
        {
            return @this < TimeUtcEpoch ? 0 : (@this - TimeUtcEpoch).Ticks;
        }

        public static long TotalSecondsSinceEpoch(this DateTimeOffset @this)
        {
            if (@this < TimeUtcEpoch)
            {
                return 0;
            }
            return (long)(@this - TimeUtcEpoch).TotalSeconds;
        }

        public static long TotalMillisecondsSinceEpoch(this DateTimeOffset @this)
        {
            if (@this < TimeUtcEpoch)
            {
                return 0;
            }
            return (long)(@this - TimeUtcEpoch).TotalMilliseconds;
        }

        public static long TotalTicksSinceEpoch(this DateTimeOffset @this)
        {
            if (@this < TimeUtcEpoch)
            {
                return 0;
            }

            return (@this - TimeUtcEpoch).Ticks;
        }

        public static DateTime Truncate(this DateTime @this, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
            {
                return @this;
            }

            return @this.AddTicks(-(@this.Ticks % timeSpan.Ticks));
        }

        public static DateTimeOffset Truncate(this DateTimeOffset @this, TimeSpan timeSpan)
        {
            if (timeSpan == TimeSpan.Zero)
            {
                return @this;
            }

            return @this.AddTicks(-(@this.Ticks % timeSpan.Ticks));
        }

        public static DateTime TruncateToWholeMilliseconds(this DateTime @this)
        {
            return @this.Truncate(TimeSpan.FromMilliseconds(1));
        }

        public static DateTimeOffset TruncateToWholeMilliseconds(this DateTimeOffset @this)
        {
            return @this.Truncate(TimeSpan.FromMilliseconds(1));
        }

        public static DateTime TruncateToWholeSeconds(this DateTime @this)
        {
            return @this.Truncate(TimeSpan.FromSeconds(1));
        }

        public static DateTimeOffset TruncateToWholeSeconds(this DateTimeOffset @this)
        {
            return @this.Truncate(TimeSpan.FromSeconds(1));
        }

        public static bool Within(this DayOfWeek dayOfWeek, List<DayOfWeek> daysOfWeek)
        {
            return daysOfWeek.Contains(dayOfWeek);
        }

        public static bool IsFirstDayOfWeek(this DayOfWeek dayOfWeek, DayOfWeek firstDayOfWeek)
        {
            return dayOfWeek == firstDayOfWeek;
        }

        public static bool IsLastDayOfWeek(this DayOfWeek dayOfWeek, DayOfWeek firstDayOfWeek)
        {
            return dayOfWeek == LastDayOfWeek(firstDayOfWeek);
        }

        public static DayOfWeek LastDayOfWeek(DayOfWeek firstDayOfWeek)
        {
            return (DayOfWeek)(((int)firstDayOfWeek + 6) % 7);
        }

        #region DateTimeOffset Extensions

        public static DateTimeOffset ConvertTime(this DateTimeOffset @this, TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTime(@this, timeZone);
        }

        public static bool IsWeekendDay(this DateTimeOffset @this)
        {
            return @this.DayOfWeek == DayOfWeek.Saturday || @this.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsWeekday(this DateTimeOffset @this)
        {
            return !IsWeekendDay(@this);
        }

        public static bool IsFirstDayOfWeek(this DateTimeOffset @this, DayOfWeek firstDayOfWeek)
        {
            return @this.DayOfWeek == firstDayOfWeek;
        }

        public static bool IsLastDayOfWeek(this DateTimeOffset @this, DayOfWeek firstDayOfWeek)
        {
            return @this.DayOfWeek == LastDayOfWeek(firstDayOfWeek);
        }

        public static DateTimeOffset GetFirstDayOfWeek(this DateTimeOffset @this, DayOfWeek firstDayOfWeek)
        {
            int delta = firstDayOfWeek - @this.DayOfWeek;
            if (delta > 0)
            {
                delta -= 7;
            }

            return @this.AddDays(delta);
        }

        public static DateTimeOffset GetLastDayOfWeek(this DateTimeOffset @this, DayOfWeek firstDayOfWeek)
        {
            return @this.GetFirstDayOfWeek(firstDayOfWeek).AddDays(7);
        }

        public static DateTimeOffset GetFirstDayOfMonth(this DateTimeOffset @this)
        {
            return @this.AddDays((-1 * @this.Day) + 1);
        }

        public static DateTimeOffset GetSecondDayOfMonth(this DateTimeOffset @this)
        {
            return @this.GetFirstDayOfMonth().AddDays(1);
        }

        public static DateTimeOffset GetThirdDayOfMonth(this DateTimeOffset @this)
        {
            return @this.GetFirstDayOfMonth().AddDays(2);
        }

        public static DateTimeOffset GetFourthDayOfMonth(this DateTimeOffset @this)
        {
            return @this.GetFirstDayOfMonth().AddDays(3);
        }

        public static DateTimeOffset GetLastDayOfMonth(this DateTimeOffset @this)
        {
            return @this.GetFirstDayOfNextMonth().AddDays(-1);
        }

        public static DateTimeOffset GetFirstDayOfNextMonth(this DateTimeOffset @this)
        {
            return @this.GetFirstDayOfNextSeveralMonths(1);
        }

        public static DateTimeOffset GetFirstDayOfNextSeveralMonths(this DateTimeOffset @this, int nextSeveralMonths)
        {
            return @this.AddMonths(nextSeveralMonths).GetFirstDayOfMonth();
        }

        public static DateTimeOffset GetNextWeekday(this DateTimeOffset @this)
        {
            var nextDay = @this.AddDays(1);
            while (!nextDay.IsWeekday())
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        public static DateTimeOffset GetNextWeekendDay(this DateTimeOffset @this)
        {
            var nextDay = @this.AddDays(1);
            while (!nextDay.IsWeekendDay())
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        public static DateTimeOffset GetFirstWeekdayOfMonth(this DateTimeOffset @this)
        {
            var nextDay = GetFirstDayOfMonth(@this);
            while (!nextDay.IsWeekday())
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        public static DateTimeOffset GetSecondWeekdayOfMonth(this DateTimeOffset @this)
        {
            return @this.GetFirstWeekdayOfMonth().GetNextWeekday();
        }

        public static DateTimeOffset GetThirdWeekdayOfMonth(this DateTimeOffset @this)
        {
            return @this.GetSecondWeekdayOfMonth().GetNextWeekday();
        }

        public static DateTimeOffset GetFourthWeekdayOfMonth(this DateTimeOffset @this)
        {
            return @this.GetThirdWeekdayOfMonth().GetNextWeekday();
        }

        public static DateTimeOffset GetLastWeekdayOfMonth(this DateTimeOffset @this)
        {
            var previousDay = GetLastDayOfMonth(@this);
            while (!previousDay.IsWeekday())
            {
                previousDay = previousDay.AddDays(-1);
            }

            return previousDay;
        }

        public static DateTimeOffset GetFirstWeekendDayOfMonth(this DateTimeOffset @this)
        {
            var nextDay = GetFirstDayOfMonth(@this);
            while (!nextDay.IsWeekendDay())
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        public static DateTimeOffset GetSecondWeekendDayOfMonth(this DateTimeOffset @this)
        {
            return @this.GetFirstWeekendDayOfMonth().GetNextWeekendDay();
        }

        public static DateTimeOffset GetThirdWeekendDayOfMonth(this DateTimeOffset @this)
        {
            return @this.GetSecondWeekendDayOfMonth().GetNextWeekendDay();
        }

        public static DateTimeOffset GetFourthWeekendDayOfMonth(this DateTimeOffset @this)
        {
            return @this.GetThirdWeekendDayOfMonth().GetNextWeekendDay();
        }

        public static DateTimeOffset GetLastWeekendDayOfMonth(this DateTimeOffset @this)
        {
            var previousDay = GetLastDayOfMonth(@this);
            while (!previousDay.IsWeekendDay())
            {
                previousDay = previousDay.AddDays(-1);
            }

            return previousDay;
        }

        public static DateTimeOffset GetFirstDayOfWeekOfMonth(this DateTimeOffset @this, DayOfWeek dayOfWeek)
        {
            var nextDay = GetFirstDayOfMonth(@this);
            while (nextDay.DayOfWeek != dayOfWeek)
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        public static DateTimeOffset GetSecondDayOfWeekOfMonth(this DateTimeOffset @this, DayOfWeek dayOfWeek)
        {
            var nextDay = GetFirstDayOfWeekOfMonth(@this, dayOfWeek);
            nextDay = nextDay.AddDays(7);

            return nextDay;
        }

        public static DateTimeOffset GetThirdDayOfWeekOfMonth(this DateTimeOffset @this, DayOfWeek dayOfWeek)
        {
            var nextDay = GetFirstDayOfWeekOfMonth(@this, dayOfWeek);
            nextDay = nextDay.AddDays(7 * 2);

            return nextDay;
        }

        public static DateTimeOffset GetFourthDayOfWeekOfMonth(this DateTimeOffset @this, DayOfWeek dayOfWeek)
        {
            var nextDay = GetFirstDayOfWeekOfMonth(@this, dayOfWeek);
            nextDay = nextDay.AddDays(7 * 3);

            return nextDay;
        }

        public static DateTimeOffset GetLastDayOfWeekOfMonth(this DateTimeOffset @this, DayOfWeek dayOfWeek)
        {
            var previousDay = GetLastDayOfMonth(@this);
            while (previousDay.DayOfWeek != dayOfWeek)
            {
                previousDay = previousDay.AddDays(-1);
            }

            return previousDay;
        }

        public static bool IsFirstKindDayOfMonth(this DateTimeOffset @this, DayOfKind dayOfKind)
        {
            switch (dayOfKind)
            {
                case DayOfKind.Sunday:
                case DayOfKind.Monday:
                case DayOfKind.Tuesday:
                case DayOfKind.Wednesday:
                case DayOfKind.Thursday:
                case DayOfKind.Friday:
                case DayOfKind.Saturday:
                    return @this.Day == @this.GetFirstDayOfWeekOfMonth((DayOfWeek)((int)dayOfKind)).Day;
                case DayOfKind.Day:
                    return @this.Day == @this.GetFirstDayOfMonth().Day;
                case DayOfKind.Weekday:
                    return @this.Day == @this.GetFirstWeekdayOfMonth().Day;
                case DayOfKind.WeekendDay:
                    return @this.Day == @this.GetFirstWeekendDayOfMonth().Day;
                default:
                    break;
            }

            return false;
        }

        public static bool IsSecondKindDayOfMonth(this DateTimeOffset @this, DayOfKind dayOfKind)
        {
            switch (dayOfKind)
            {
                case DayOfKind.Sunday:
                case DayOfKind.Monday:
                case DayOfKind.Tuesday:
                case DayOfKind.Wednesday:
                case DayOfKind.Thursday:
                case DayOfKind.Friday:
                case DayOfKind.Saturday:
                    return @this.Day == @this.GetSecondDayOfWeekOfMonth((DayOfWeek)((int)dayOfKind)).Day;
                case DayOfKind.Day:
                    return @this.Day == @this.GetSecondDayOfMonth().Day;
                case DayOfKind.Weekday:
                    return @this.Day == @this.GetSecondWeekdayOfMonth().Day;
                case DayOfKind.WeekendDay:
                    return @this.Day == @this.GetSecondWeekendDayOfMonth().Day;
                default:
                    break;
            }

            return false;
        }

        public static bool IsThirdKindDayOfMonth(this DateTimeOffset @this, DayOfKind dayOfKind)
        {
            switch (dayOfKind)
            {
                case DayOfKind.Sunday:
                case DayOfKind.Monday:
                case DayOfKind.Tuesday:
                case DayOfKind.Wednesday:
                case DayOfKind.Thursday:
                case DayOfKind.Friday:
                case DayOfKind.Saturday:
                    return @this.Day == @this.GetThirdDayOfWeekOfMonth((DayOfWeek)((int)dayOfKind)).Day;
                case DayOfKind.Day:
                    return @this.Day == @this.GetThirdDayOfMonth().Day;
                case DayOfKind.Weekday:
                    return @this.Day == @this.GetThirdWeekdayOfMonth().Day;
                case DayOfKind.WeekendDay:
                    return @this.Day == @this.GetThirdWeekendDayOfMonth().Day;
                default:
                    break;
            }

            return false;
        }

        public static bool IsFourthKindDayOfMonth(this DateTimeOffset @this, DayOfKind dayOfKind)
        {
            switch (dayOfKind)
            {
                case DayOfKind.Sunday:
                case DayOfKind.Monday:
                case DayOfKind.Tuesday:
                case DayOfKind.Wednesday:
                case DayOfKind.Thursday:
                case DayOfKind.Friday:
                case DayOfKind.Saturday:
                    return @this.Day == @this.GetFourthDayOfWeekOfMonth((DayOfWeek)((int)dayOfKind)).Day;
                case DayOfKind.Day:
                    return @this.Day == @this.GetFourthDayOfMonth().Day;
                case DayOfKind.Weekday:
                    return @this.Day == @this.GetFourthWeekdayOfMonth().Day;
                case DayOfKind.WeekendDay:
                    return @this.Day == @this.GetFourthWeekendDayOfMonth().Day;
                default:
                    break;
            }

            return false;
        }

        public static bool IsLastKindDayOfMonth(this DateTimeOffset @this, DayOfKind dayOfKind)
        {
            switch (dayOfKind)
            {
                case DayOfKind.Sunday:
                case DayOfKind.Monday:
                case DayOfKind.Tuesday:
                case DayOfKind.Wednesday:
                case DayOfKind.Thursday:
                case DayOfKind.Friday:
                case DayOfKind.Saturday:
                    return @this.Day == @this.GetLastDayOfWeekOfMonth((DayOfWeek)((int)dayOfKind)).Day;
                case DayOfKind.Day:
                    return @this.Day == @this.GetLastDayOfMonth().Day;
                case DayOfKind.Weekday:
                    return @this.Day == @this.GetLastWeekdayOfMonth().Day;
                case DayOfKind.WeekendDay:
                    return @this.Day == @this.GetLastWeekendDayOfMonth().Day;
                default:
                    break;
            }

            return false;
        }

        public static bool IsKindDayOfMonth(this DateTimeOffset @this, DayOfMonth dayOfMonth, DayOfKind dayOfKind)
        {
            switch (dayOfMonth)
            {
                case DayOfMonth.First:
                    return IsFirstKindDayOfMonth(@this, dayOfKind);
                case DayOfMonth.Second:
                    return IsSecondKindDayOfMonth(@this, dayOfKind);
                case DayOfMonth.Third:
                    return IsThirdKindDayOfMonth(@this, dayOfKind);
                case DayOfMonth.Fourth:
                    return IsFourthKindDayOfMonth(@this, dayOfKind);
                case DayOfMonth.Fifth:
                case DayOfMonth.Sixth:
                case DayOfMonth.Seventh:
                case DayOfMonth.Eighth:
                case DayOfMonth.Ninth:
                case DayOfMonth.Tenth:
                case DayOfMonth.Eleventh:
                case DayOfMonth.Twelfth:
                case DayOfMonth.Thirteenth:
                case DayOfMonth.Fourteenth:
                case DayOfMonth.Fifteenth:
                case DayOfMonth.Sixteenth:
                case DayOfMonth.Seventeenth:
                case DayOfMonth.Eighteenth:
                case DayOfMonth.Nineteenth:
                case DayOfMonth.Twentieth:
                case DayOfMonth.TwentyFirst:
                case DayOfMonth.TwentySecond:
                case DayOfMonth.TwentyThird:
                case DayOfMonth.TwentyFourth:
                case DayOfMonth.TwentyFifth:
                case DayOfMonth.TwentySixth:
                case DayOfMonth.TwentySeventh:
                case DayOfMonth.TwentyEighth:
                case DayOfMonth.TwentyNineth:
                case DayOfMonth.Thirtieth:
                case DayOfMonth.ThirtyFirst:
                    {
                        if (dayOfKind != DayOfKind.Day)
                            throw new InvalidProgramException("Does not support given kind of day.");

                        int day = (int)dayOfMonth;
                        return @this.Day == day;
                    }
                case DayOfMonth.Last:
                    return IsLastKindDayOfMonth(@this, dayOfKind);
                default:
                    break;
            }

            throw new InvalidProgramException("Cannot parse the given month and day.");
        }

        public static bool IsMonthOfYear(this DateTimeOffset @this, MonthOfYear monthOfYear)
        {
            switch (monthOfYear)
            {
                case MonthOfYear.January:
                case MonthOfYear.February:
                case MonthOfYear.March:
                case MonthOfYear.April:
                case MonthOfYear.May:
                case MonthOfYear.June:
                case MonthOfYear.July:
                case MonthOfYear.August:
                case MonthOfYear.September:
                case MonthOfYear.October:
                case MonthOfYear.November:
                case MonthOfYear.December:
                    return @this.Month == (int)monthOfYear;
                default:
                    break;
            }

            throw new InvalidProgramException("Does not support given month of year.");
        }

        #endregion

        #region DateTime Extensions

        public static DateTime ConvertTime(this DateTime @this, TimeZoneInfo timeZone)
        {
            return TimeZoneInfo.ConvertTime(@this, timeZone);
        }

        public static bool IsWeekendDay(this DateTime @this)
        {
            return @this.DayOfWeek == DayOfWeek.Saturday || @this.DayOfWeek == DayOfWeek.Sunday;
        }

        public static bool IsWeekday(this DateTime @this)
        {
            return !IsWeekendDay(@this);
        }

        public static bool IsFirstDayOfWeek(this DateTime @this, DayOfWeek firstDayOfWeek)
        {
            return @this.DayOfWeek == firstDayOfWeek;
        }

        public static bool IsLastDayOfWeek(this DateTime @this, DayOfWeek firstDayOfWeek)
        {
            return @this.DayOfWeek == LastDayOfWeek(firstDayOfWeek);
        }

        public static DateTime GetFirstDayOfWeek(this DateTime @this, DayOfWeek firstDayOfWeek)
        {
            int delta = firstDayOfWeek - @this.DayOfWeek;
            if (delta > 0) delta -= 7;
            return @this.AddDays(delta);
        }

        public static DateTime GetLastDayOfWeek(this DateTime @this, DayOfWeek firstDayOfWeek)
        {
            return @this.GetFirstDayOfWeek(firstDayOfWeek).AddDays(7);
        }

        public static DateTime GetFirstDayOfMonth(this DateTime @this)
        {
            return @this.AddDays((-1 * @this.Day) + 1);
        }

        public static DateTime GetSecondDayOfMonth(this DateTime @this)
        {
            return @this.GetFirstDayOfMonth().AddDays(1);
        }

        public static DateTime GetThirdDayOfMonth(this DateTime @this)
        {
            return @this.GetFirstDayOfMonth().AddDays(2);
        }

        public static DateTime GetFourthDayOfMonth(this DateTime @this)
        {
            return @this.GetFirstDayOfMonth().AddDays(3);
        }

        public static DateTime GetLastDayOfMonth(this DateTime @this)
        {
            return @this.GetFirstDayOfNextMonth().AddDays(-1);
        }

        public static DateTime GetFirstDayOfNextMonth(this DateTime @this)
        {
            return @this.GetFirstDayOfNextSeveralMonths(1);
        }

        public static DateTime GetFirstDayOfNextSeveralMonths(this DateTime @this, int nextSeveralMonths)
        {
            return @this.AddMonths(nextSeveralMonths).GetFirstDayOfMonth();
        }

        public static DateTime GetNextWeekday(this DateTime @this)
        {
            var nextDay = @this.AddDays(1);
            while (!nextDay.IsWeekday())
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        public static DateTime GetNextWeekendDay(this DateTime @this)
        {
            var nextDay = @this.AddDays(1);
            while (!nextDay.IsWeekendDay())
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        public static DateTime GetFirstWeekdayOfMonth(this DateTime @this)
        {
            var nextDay = GetFirstDayOfMonth(@this);
            while (!nextDay.IsWeekday())
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        public static DateTime GetSecondWeekdayOfMonth(this DateTime @this)
        {
            return @this.GetFirstWeekdayOfMonth().GetNextWeekday();
        }

        public static DateTime GetThirdWeekdayOfMonth(this DateTime @this)
        {
            return @this.GetSecondWeekdayOfMonth().GetNextWeekday();
        }

        public static DateTime GetFourthWeekdayOfMonth(this DateTime @this)
        {
            return @this.GetThirdWeekdayOfMonth().GetNextWeekday();
        }

        public static DateTime GetLastWeekdayOfMonth(this DateTime @this)
        {
            var previousDay = GetLastDayOfMonth(@this);
            while (!previousDay.IsWeekday())
            {
                previousDay = previousDay.AddDays(-1);
            }

            return previousDay;
        }

        public static DateTime GetFirstWeekendDayOfMonth(this DateTime @this)
        {
            var nextDay = GetFirstDayOfMonth(@this);
            while (!nextDay.IsWeekendDay())
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        public static DateTime GetSecondWeekendDayOfMonth(this DateTime @this)
        {
            return @this.GetFirstWeekendDayOfMonth().GetNextWeekendDay();
        }

        public static DateTime GetThirdWeekendDayOfMonth(this DateTime @this)
        {
            return @this.GetSecondWeekendDayOfMonth().GetNextWeekendDay();
        }

        public static DateTime GetFourthWeekendDayOfMonth(this DateTime @this)
        {
            return @this.GetThirdWeekendDayOfMonth().GetNextWeekendDay();
        }

        public static DateTime GetLastWeekendDayOfMonth(this DateTime @this)
        {
            var previousDay = GetLastDayOfMonth(@this);
            while (!previousDay.IsWeekendDay())
            {
                previousDay = previousDay.AddDays(-1);
            }

            return previousDay;
        }

        public static DateTime GetFirstDayOfWeekOfMonth(this DateTime @this, DayOfWeek dayOfWeek)
        {
            var nextDay = GetFirstDayOfMonth(@this);
            while (nextDay.DayOfWeek != dayOfWeek)
            {
                nextDay = nextDay.AddDays(1);
            }

            return nextDay;
        }

        public static DateTime GetSecondDayOfWeekOfMonth(this DateTime @this, DayOfWeek dayOfWeek)
        {
            var nextDay = GetFirstDayOfWeekOfMonth(@this, dayOfWeek);
            nextDay = nextDay.AddDays(7);

            return nextDay;
        }

        public static DateTime GetThirdDayOfWeekOfMonth(this DateTime @this, DayOfWeek dayOfWeek)
        {
            var nextDay = GetFirstDayOfWeekOfMonth(@this, dayOfWeek);
            nextDay = nextDay.AddDays(7 * 2);

            return nextDay;
        }

        public static DateTime GetFourthDayOfWeekOfMonth(this DateTime @this, DayOfWeek dayOfWeek)
        {
            var nextDay = GetFirstDayOfWeekOfMonth(@this, dayOfWeek);
            nextDay = nextDay.AddDays(7 * 3);

            return nextDay;
        }

        public static DateTime GetLastDayOfWeekOfMonth(this DateTime @this, DayOfWeek dayOfWeek)
        {
            var previousDay = GetLastDayOfMonth(@this);
            while (previousDay.DayOfWeek != dayOfWeek)
            {
                previousDay = previousDay.AddDays(-1);
            }

            return previousDay;
        }

        public static bool IsFirstKindDayOfMonth(this DateTime @this, DayOfKind dayOfKind)
        {
            switch (dayOfKind)
            {
                case DayOfKind.Sunday:
                case DayOfKind.Monday:
                case DayOfKind.Tuesday:
                case DayOfKind.Wednesday:
                case DayOfKind.Thursday:
                case DayOfKind.Friday:
                case DayOfKind.Saturday:
                    return @this.Day == @this.GetFirstDayOfWeekOfMonth((DayOfWeek)((int)dayOfKind)).Day;
                case DayOfKind.Day:
                    return @this.Day == @this.GetFirstDayOfMonth().Day;
                case DayOfKind.Weekday:
                    return @this.Day == @this.GetFirstWeekdayOfMonth().Day;
                case DayOfKind.WeekendDay:
                    return @this.Day == @this.GetFirstWeekendDayOfMonth().Day;
                default:
                    break;
            }

            return false;
        }

        public static bool IsSecondKindDayOfMonth(this DateTime @this, DayOfKind dayOfKind)
        {
            switch (dayOfKind)
            {
                case DayOfKind.Sunday:
                case DayOfKind.Monday:
                case DayOfKind.Tuesday:
                case DayOfKind.Wednesday:
                case DayOfKind.Thursday:
                case DayOfKind.Friday:
                case DayOfKind.Saturday:
                    return @this.Day == @this.GetSecondDayOfWeekOfMonth((DayOfWeek)((int)dayOfKind)).Day;
                case DayOfKind.Day:
                    return @this.Day == @this.GetSecondDayOfMonth().Day;
                case DayOfKind.Weekday:
                    return @this.Day == @this.GetSecondWeekdayOfMonth().Day;
                case DayOfKind.WeekendDay:
                    return @this.Day == @this.GetSecondWeekendDayOfMonth().Day;
                default:
                    break;
            }

            return false;
        }

        public static bool IsThirdKindDayOfMonth(this DateTime @this, DayOfKind dayOfKind)
        {
            switch (dayOfKind)
            {
                case DayOfKind.Sunday:
                case DayOfKind.Monday:
                case DayOfKind.Tuesday:
                case DayOfKind.Wednesday:
                case DayOfKind.Thursday:
                case DayOfKind.Friday:
                case DayOfKind.Saturday:
                    return @this.Day == @this.GetThirdDayOfWeekOfMonth((DayOfWeek)((int)dayOfKind)).Day;
                case DayOfKind.Day:
                    return @this.Day == @this.GetThirdDayOfMonth().Day;
                case DayOfKind.Weekday:
                    return @this.Day == @this.GetThirdWeekdayOfMonth().Day;
                case DayOfKind.WeekendDay:
                    return @this.Day == @this.GetThirdWeekendDayOfMonth().Day;
                default:
                    break;
            }

            return false;
        }

        public static bool IsFourthKindDayOfMonth(this DateTime @this, DayOfKind dayOfKind)
        {
            switch (dayOfKind)
            {
                case DayOfKind.Sunday:
                case DayOfKind.Monday:
                case DayOfKind.Tuesday:
                case DayOfKind.Wednesday:
                case DayOfKind.Thursday:
                case DayOfKind.Friday:
                case DayOfKind.Saturday:
                    return @this.Day == @this.GetFourthDayOfWeekOfMonth((DayOfWeek)((int)dayOfKind)).Day;
                case DayOfKind.Day:
                    return @this.Day == @this.GetFourthDayOfMonth().Day;
                case DayOfKind.Weekday:
                    return @this.Day == @this.GetFourthWeekdayOfMonth().Day;
                case DayOfKind.WeekendDay:
                    return @this.Day == @this.GetFourthWeekendDayOfMonth().Day;
                default:
                    break;
            }

            return false;
        }

        public static bool IsLastKindDayOfMonth(this DateTime @this, DayOfKind dayOfKind)
        {
            switch (dayOfKind)
            {
                case DayOfKind.Sunday:
                case DayOfKind.Monday:
                case DayOfKind.Tuesday:
                case DayOfKind.Wednesday:
                case DayOfKind.Thursday:
                case DayOfKind.Friday:
                case DayOfKind.Saturday:
                    return @this.Day == @this.GetLastDayOfWeekOfMonth((DayOfWeek)((int)dayOfKind)).Day;
                case DayOfKind.Day:
                    return @this.Day == @this.GetLastDayOfMonth().Day;
                case DayOfKind.Weekday:
                    return @this.Day == @this.GetLastWeekdayOfMonth().Day;
                case DayOfKind.WeekendDay:
                    return @this.Day == @this.GetLastWeekendDayOfMonth().Day;
                default:
                    break;
            }

            return false;
        }

        public static bool IsKindDayOfMonth(this DateTime @this, DayOfMonth dayOfMonth, DayOfKind dayOfKind)
        {
            switch (dayOfMonth)
            {
                case DayOfMonth.First:
                    return IsFirstKindDayOfMonth(@this, dayOfKind);
                case DayOfMonth.Second:
                    return IsSecondKindDayOfMonth(@this, dayOfKind);
                case DayOfMonth.Third:
                    return IsThirdKindDayOfMonth(@this, dayOfKind);
                case DayOfMonth.Fourth:
                    return IsFourthKindDayOfMonth(@this, dayOfKind);
                case DayOfMonth.Fifth:
                case DayOfMonth.Sixth:
                case DayOfMonth.Seventh:
                case DayOfMonth.Eighth:
                case DayOfMonth.Ninth:
                case DayOfMonth.Tenth:
                case DayOfMonth.Eleventh:
                case DayOfMonth.Twelfth:
                case DayOfMonth.Thirteenth:
                case DayOfMonth.Fourteenth:
                case DayOfMonth.Fifteenth:
                case DayOfMonth.Sixteenth:
                case DayOfMonth.Seventeenth:
                case DayOfMonth.Eighteenth:
                case DayOfMonth.Nineteenth:
                case DayOfMonth.Twentieth:
                case DayOfMonth.TwentyFirst:
                case DayOfMonth.TwentySecond:
                case DayOfMonth.TwentyThird:
                case DayOfMonth.TwentyFourth:
                case DayOfMonth.TwentyFifth:
                case DayOfMonth.TwentySixth:
                case DayOfMonth.TwentySeventh:
                case DayOfMonth.TwentyEighth:
                case DayOfMonth.TwentyNineth:
                case DayOfMonth.Thirtieth:
                case DayOfMonth.ThirtyFirst:
                    {
                        if (dayOfKind != DayOfKind.Day)
                            throw new InvalidProgramException("Does not support given kind of day.");

                        int day = (int)dayOfMonth;
                        return @this.Day == day;
                    }
                case DayOfMonth.Last:
                    return IsLastKindDayOfMonth(@this, dayOfKind);
                default:
                    break;
            }

            throw new InvalidProgramException("Cannot parse the given month and day.");
        }

        public static bool IsMonthOfYear(this DateTime @this, MonthOfYear monthOfYear)
        {
            switch (monthOfYear)
            {
                case MonthOfYear.January:
                case MonthOfYear.February:
                case MonthOfYear.March:
                case MonthOfYear.April:
                case MonthOfYear.May:
                case MonthOfYear.June:
                case MonthOfYear.July:
                case MonthOfYear.August:
                case MonthOfYear.September:
                case MonthOfYear.October:
                case MonthOfYear.November:
                case MonthOfYear.December:
                    return @this.Month == (int)monthOfYear;
                default:
                    break;
            }

            throw new InvalidProgramException("Does not support given month of year.");
        }

        #endregion
    
        public static void GetAlignSecondsDateTimes<T>
                                (
                                    this DateTime @this
                                    , params Tuple<long, Func<DateTime, long, DateTime, T, T>>[] processAlignSecondsDateTimesFuncs
                                )
        {
            T r = default;
            foreach (var x in processAlignSecondsDateTimesFuncs)
            {
                var alignSeconds = x.Item1;
                var alignTime = @this.GetAlignSecondsDateTime(alignSeconds);
                if (x.Item2 != null)
                {
                    r = x.Item2(@this, alignSeconds, alignTime, r);
                }
            }
        }
        public static bool IsVaildateTimestamp(DateTime timeStamp, int timeoutSeconds)
        {
            long l = SecondsDiffNow(timeStamp);
            var r = ((l > 0) && (l < timeoutSeconds));
            return r;
        }

        public static long MillisecondsDiff(this DateTime @this, DateTime diffTime)
        {
            //long t1 = time1.Ticks;
            //long t2 = time2.Ticks;
            //var r = (t2 - t1) / 10000;
            //return r;
            return (long) (diffTime - @this).TotalMilliseconds;
            
        }
        public static long SecondsDiff(this DateTime @this, DateTime diffTime)
        {
            return @this.MillisecondsDiff(diffTime) / 1000;
        }
        public static long MillisecondsDiffNow(this DateTime @this)
        {
            return @this.MillisecondsDiff(DateTime.Now);
        }
        public static long SecondsDiffNow(this DateTime @this)
        {
            return @this.MillisecondsDiffNow() / 1000;
        }
        public static DateTime GetAlignSecondsDateTime(this DateTime @this, long alignSeconds)
        {
            var ticks = @this.Ticks;
            ticks -= ticks % (10000 * 1000 * alignSeconds);
            var r = new DateTime(ticks);
            return r;
        }

        public static string GetformattedDateTimeString(this DateTime @this, string format)
        {
            return @this.ToString(format);
        }
        public static DateTime? ParseExactNullableDateTime(string text, string format)
        {
            DateTime time;
            DateTime? r = DateTime
                                .TryParseExact
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
    public enum DayOfKind
    {
        [Description("Sunday")]
        [XmlEnum(Name = "Sunday")]
        Sunday = 0,

        [Description("Monday")]
        [XmlEnum(Name = "Monday")]
        Monday = 1,

        [Description("Tuesday")]
        [XmlEnum(Name = "Tuesday")]
        Tuesday = 2,

        [Description("Wednesday")]
        [XmlEnum(Name = "Wednesday")]
        Wednesday = 3,

        [Description("Thursday")]
        [XmlEnum(Name = "Thursday")]
        Thursday = 4,

        [Description("Friday")]
        [XmlEnum(Name = "Friday")]
        Friday = 5,

        [Description("Saturday")]
        [XmlEnum(Name = "Saturday")]
        Saturday = 6,

        [Description("Day")]
        [XmlEnum(Name = "Day")]
        Day = 7,

        [Description("Weekday")]
        [XmlEnum(Name = "Weekday")]
        Weekday = 8,

        [Description("WeekendDay")]
        [XmlEnum(Name = "WeekendDay")]
        WeekendDay = 9,
    }

    public enum DayOfMonth
    {
        [Description("First")]
        [XmlEnum(Name = "First")]
        First = 1,

        [Description("Second")]
        [XmlEnum(Name = "Second")]
        Second = 2,

        [Description("Third")]
        [XmlEnum(Name = "Third")]
        Third = 3,

        [Description("Fourth")]
        [XmlEnum(Name = "Fourth")]
        Fourth = 4,

        [Description("Fifth")]
        [XmlEnum(Name = "Fifth")]
        Fifth = 5,

        [Description("Sixth")]
        [XmlEnum(Name = "Sixth")]
        Sixth = 6,

        [Description("Seventh")]
        [XmlEnum(Name = "Seventh")]
        Seventh = 7,

        [Description("Eighth")]
        [XmlEnum(Name = "Eighth")]
        Eighth = 8,

        [Description("Ninth")]
        [XmlEnum(Name = "Ninth")]
        Ninth = 9,

        [Description("Tenth")]
        [XmlEnum(Name = "Tenth")]
        Tenth = 10,

        [Description("Eleventh")]
        [XmlEnum(Name = "Eleventh")]
        Eleventh = 11,

        [Description("Twelfth")]
        [XmlEnum(Name = "Twelfth")]
        Twelfth = 12,

        [Description("Thirteenth")]
        [XmlEnum(Name = "Thirteenth")]
        Thirteenth = 13,

        [Description("Fourteenth")]
        [XmlEnum(Name = "Fourteenth")]
        Fourteenth = 14,

        [Description("Fifteenth")]
        [XmlEnum(Name = "Fifteenth")]
        Fifteenth = 15,

        [Description("Sixteenth")]
        [XmlEnum(Name = "Sixteenth")]
        Sixteenth = 16,

        [Description("Seventeenth")]
        [XmlEnum(Name = "Seventeenth")]
        Seventeenth = 17,

        [Description("Eighteenth")]
        [XmlEnum(Name = "Eighteenth")]
        Eighteenth = 18,

        [Description("Nineteenth")]
        [XmlEnum(Name = "Nineteenth")]
        Nineteenth = 19,

        [Description("Twentieth")]
        [XmlEnum(Name = "Twentieth")]
        Twentieth = 20,

        [Description("TwentyFirst")]
        [XmlEnum(Name = "TwentyFirst")]
        TwentyFirst = 21,

        [Description("TwentySecond")]
        [XmlEnum(Name = "TwentySecond")]
        TwentySecond = 22,

        [Description("TwentyThird")]
        [XmlEnum(Name = "TwentyThird")]
        TwentyThird = 23,

        [Description("TwentyFourth")]
        [XmlEnum(Name = "TwentyFourth")]
        TwentyFourth = 24,

        [Description("TwentyFifth")]
        [XmlEnum(Name = "TwentyFifth")]
        TwentyFifth = 25,

        [Description("TwentySixth")]
        [XmlEnum(Name = "TwentySixth")]
        TwentySixth = 26,

        [Description("TwentySeventh")]
        [XmlEnum(Name = "TwentySeventh")]
        TwentySeventh = 27,

        [Description("TwentyEighth")]
        [XmlEnum(Name = "TwentyEighth")]
        TwentyEighth = 28,

        [Description("TwentyNineth")]
        [XmlEnum(Name = "TwentyNineth")]
        TwentyNineth = 29,

        [Description("Thirtieth")]
        [XmlEnum(Name = "Thirtieth")]
        Thirtieth = 30,

        [Description("ThirtyFirst")]
        [XmlEnum(Name = "ThirtyFirst")]
        ThirtyFirst = 31,

        [Description("Last")]
        [XmlEnum(Name = "Last")]
        Last = 9999,
    }

    public enum MonthOfYear
    {
        [Description("January")]
        [XmlEnum(Name = "January")]
        January = 1,

        [Description("February")]
        [XmlEnum(Name = "February")]
        February = 2,

        [Description("March")]
        [XmlEnum(Name = "March")]
        March = 3,

        [Description("April")]
        [XmlEnum(Name = "April")]
        April = 4,

        [Description("May")]
        [XmlEnum(Name = "May")]
        May = 5,

        [Description("June")]
        [XmlEnum(Name = "June")]
        June = 6,

        [Description("July")]
        [XmlEnum(Name = "July")]
        July = 7,

        [Description("August")]
        [XmlEnum(Name = "August")]
        August = 8,

        [Description("September")]
        [XmlEnum(Name = "September")]
        September = 9,

        [Description("October")]
        [XmlEnum(Name = "October")]
        October = 10,

        [Description("November")]
        [XmlEnum(Name = "November")]
        November = 11,

        [Description("December")]
        [XmlEnum(Name = "December")]
        December = 12,
    }
}