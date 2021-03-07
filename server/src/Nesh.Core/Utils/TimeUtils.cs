using System;

namespace Nesh.Core.Utils
{
    public static class TimeUtils
    {
        public const int MILLISECOND = 1;
        public const int SECOND      = 1000 * MILLISECOND;
        public const int MINITE      = 60 * SECOND;
        public const int HOUR        = 60 * MINITE;
        public const int DAY         = 24 * HOUR;

        public const int DAY_HOURS   = 24;
        public const int WEEK_DAYS   = 7;

        private static DateTime _DegineDataTime = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);

        public static long DeginTicks = _DegineDataTime.Ticks;

        public static long Offset = 0;

        public static long NowMilliseconds { get { return (DateTime.Now.ToUniversalTime().Ticks - DeginTicks) / TimeSpan.TicksPerMillisecond + Offset; } }

        public static DateTime Now { get { return DateTime.Now.ToUniversalTime(); } }

        public static DateTime Today { get { return ToShortDate(Now); } }

        public static DateTime Local2UTC(DateTime local_time)
        {
            return TimeZoneInfo.ConvertTimeToUtc(local_time, TimeZoneInfo.Local);
        }

        public static DateTime UTC2Local(DateTime utc_time)
        {
            return TimeZoneInfo.ConvertTimeFromUtc(utc_time, TimeZoneInfo.Local);
        }

        public static bool IsLegal(DateTime time)
        {
            return (time != DateTime.MinValue) && (time != DateTime.MaxValue);
        }

        public static DateTime TimestampToDataTime(long ts)
        {
            return _DegineDataTime.AddMilliseconds(ts);
        }

        public static long DataTimeToTimestampMilliseconds(DateTime time)
        {
            long t = (time.ToUniversalTime().Ticks - DeginTicks) / TimeSpan.TicksPerMillisecond + Offset;
            return t;
        }

        public static bool IsSameSecond(DateTime dt1, DateTime dt2)
        {
            return dt1.Minute == dt2.Minute && dt1.Second == dt2.Second && dt1.Hour == dt2.Hour && dt1.Day == dt2.Day && dt1.Year == dt2.Year && dt1.Month == dt2.Month;
        }

        public static bool IsSameMinute(DateTime dt1, DateTime dt2)
        {
            return dt1.Minute == dt2.Minute && dt1.Hour == dt2.Hour && dt1.Day == dt2.Day && dt1.Year == dt2.Year && dt1.Month == dt2.Month;
        }

        public static bool IsSameHours(DateTime dt1, DateTime dt2)
        {
            return dt1.Hour == dt2.Hour && dt1.Day == dt2.Day && dt1.Year == dt2.Year && dt1.Month == dt2.Month;
        }

        public static bool IsSameDay(DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.Day == dt2.Day;
        }

        public static bool IsSameWeek(DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month && dt1.DayOfWeek == dt2.DayOfWeek;
        }

        public static bool IsSameMonth(DateTime dt1, DateTime dt2)
        {
            return dt1.Year == dt2.Year && dt1.Month == dt2.Month;
        }

        public static bool IsSameDay(long ms1, long ms2)
        {
            DateTime dt1 = TimestampToDataTime(ms1);
            DateTime dt2 = TimestampToDataTime(ms2);

            return IsSameDay(dt1, dt2);
        }

        public static bool IsSameWeek(long ms1, long ms2)
        {
            DateTime dt1 = TimestampToDataTime(ms1);
            DateTime dt2 = TimestampToDataTime(ms2);

            return IsSameWeek(dt1, dt2);
        }

        public static bool IsSameMonth(long ms1, long ms2)
        {
            DateTime dt1 = TimestampToDataTime(ms1);
            DateTime dt2 = TimestampToDataTime(ms2);

            return IsSameMonth(dt1, dt2);
        }

        public static DateTime GetLocalTime(long _time)
        {
            DateTime dt = TimestampToDataTime(_time);
            DateTime local_time = TimeZoneInfo.ConvertTimeFromUtc(dt, TimeZoneInfo.Local);
            return local_time;
        }

        public static void Collabrate(long ts)
        {
            Offset += ts - NowMilliseconds;
        }

        public static int DiffDays(DateTime dateStart, DateTime dateEnd)
        {
            if (IsSameDay(dateStart, dateEnd))
            {
                return 0;
            }

            TimeSpan sp = dateEnd.Subtract(dateStart);

            return (int)sp.TotalDays;
        }

        public static int DiffMinutes(DateTime dateStart, DateTime dateEnd)
        {
            TimeSpan sp = dateEnd.Subtract(dateStart);

            return (int)sp.TotalMinutes;
        }

        public static int DiffSeconds(DateTime dateStart, DateTime dateEnd)
        {
            TimeSpan sp = dateEnd.Subtract(dateStart);

            return (int)sp.TotalSeconds;
        }

        public static int DiffHours(DateTime dateStart, DateTime dateEnd)
        {
            TimeSpan sp = dateEnd.Subtract(dateStart);

            return (int)sp.TotalHours;
        }

        public static DateTime ToShortDate(DateTime time)
        {
            return Convert.ToDateTime(time.ToShortDateString());
        }
    }
}
