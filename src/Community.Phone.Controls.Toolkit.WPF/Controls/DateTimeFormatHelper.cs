// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace Microsoft.Phone.Controls
{
    internal static class DateTimeFormatHelper
    {
        private const double Hour = 60.0;
        private const double Day = 24 * Hour;
        private const string SingleMeridiemDesignator = "t";
        private const string DoubleMeridiemDesignator = "tt";

        private static DateTimeFormatInfo _superShortTimeFormat;
        private static DateTimeFormatInfo _monthAndDayFormat;
        private static DateTimeFormatInfo _shortTimeFormat;
        private static readonly object SuperShortTimeLock = new object();
        private static readonly object MonthAndDayLock = new object();
        private static readonly object ShortTimeLock = new object();
        private static readonly Regex MonthAndDayRegex = new Regex("(d{1,2}[^A-Za-z]M{1,3})|(M{1,3}[^A-Za-z]d{1,2})");
        private static readonly Regex SecondsRegex = new Regex("([^A-Za-z]s{1,2})");

        public static int GetRelativeDayOfWeek(DateTime value)
        {
            return ((int)value.DayOfWeek - (int)CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek + 7) % 7;
        }

        public static bool IsFutureDateTime(DateTime relative, DateTime given)
        {
            return relative < given;
        }

        public static bool IsAnOlderYear(DateTime relative, DateTime given)
        {
            return relative.Year > given.Year;
        }

        public static bool IsAtLeastOneWeekOld(DateTime relative, DateTime given)
        {
            return (int)(relative - given).TotalMinutes >= 7 * Day;
        }

        public static bool IsAnOlderWeek(DateTime relative, DateTime given)
        {
            return IsAtLeastOneWeekOld(relative, given)
                || GetRelativeDayOfWeek(given) > GetRelativeDayOfWeek(relative);
        }

        public static bool IsPastDayOfWeek(DateTime relative, DateTime given)
        {
            return GetRelativeDayOfWeek(relative) > GetRelativeDayOfWeek(given);
        }

        public static bool IsPastDayOfWeekWithWindow(DateTime relative, DateTime given)
        {
            return IsPastDayOfWeek(relative, given) && (int)(relative - given).TotalMinutes > 3 * Hour;
        }

        public static bool IsCurrentCultureJapanese()
        {
            return CultureInfo.CurrentCulture.Name.StartsWith("ja", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCurrentCultureKorean()
        {
            return CultureInfo.CurrentCulture.Name.StartsWith("ko", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCurrentCultureTurkish()
        {
            return CultureInfo.CurrentCulture.Name.StartsWith("tr", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCurrentCultureHungarian()
        {
            return CultureInfo.CurrentCulture.Name.StartsWith("hu", StringComparison.OrdinalIgnoreCase);
        }

        public static bool IsCurrentUICultureFrench()
        {
            return CultureInfo.CurrentUICulture.Name.Equals("fr-FR", StringComparison.Ordinal);
        }

        public static string GetAbbreviatedDay(DateTime value)
        {
            string day = value.ToString("ddd", CultureInfo.CurrentCulture);
            return IsCurrentCultureJapanese() || IsCurrentCultureKorean() ? "(" + day + ")" : day;
        }

        public static string GetSuperShortTime(DateTime value)
        {
            if (_superShortTimeFormat == null)
            {
                lock (SuperShortTimeLock)
                {
                    if (_superShortTimeFormat == null)
                    {
                        DateTimeFormatInfo info = (DateTimeFormatInfo)CultureInfo.CurrentCulture.DateTimeFormat.Clone();
                        StringBuilder pattern = new StringBuilder(info.LongTimePattern);
                        string seconds = SecondsRegex.Match(pattern.ToString()).Value;
                        pattern.Replace(" ", string.Empty);
                        pattern.Replace(seconds, string.Empty);

                        if (!IsCurrentCultureJapanese() && !IsCurrentCultureKorean() && !IsCurrentCultureHungarian())
                        {
                            pattern.Replace(DoubleMeridiemDesignator, SingleMeridiemDesignator);
                        }

                        info.ShortTimePattern = pattern.ToString();
                        _superShortTimeFormat = info;
                    }
                }
            }

            return value.ToString("t", _superShortTimeFormat).ToLowerInvariant();
        }

        public static string GetMonthAndDay(DateTime value)
        {
            if (_monthAndDayFormat == null)
            {
                lock (MonthAndDayLock)
                {
                    if (_monthAndDayFormat == null)
                    {
                        DateTimeFormatInfo info = (DateTimeFormatInfo)CultureInfo.CurrentCulture.DateTimeFormat.Clone();
                        StringBuilder pattern = new StringBuilder(MonthAndDayRegex.Match(info.ShortDatePattern).Value);
                        if (pattern.ToString().Contains("."))
                        {
                            pattern.Append(".");
                        }

                        info.ShortDatePattern = pattern.ToString();
                        _monthAndDayFormat = info;
                    }
                }
            }

            return value.ToString("d", _monthAndDayFormat);
        }

        public static string GetShortDate(DateTime value)
        {
            return value.ToString("d", CultureInfo.CurrentCulture);
        }

        public static string GetShortTime(DateTime value)
        {
            if (_shortTimeFormat == null)
            {
                lock (ShortTimeLock)
                {
                    if (_shortTimeFormat == null)
                    {
                        DateTimeFormatInfo info = (DateTimeFormatInfo)CultureInfo.CurrentCulture.DateTimeFormat.Clone();
                        StringBuilder pattern = new StringBuilder(info.LongTimePattern);
                        string seconds = SecondsRegex.Match(pattern.ToString()).Value;
                        pattern.Replace(seconds, string.Empty);
                        info.ShortTimePattern = pattern.ToString();
                        _shortTimeFormat = info;
                    }
                }
            }

            return value.ToString("t", _shortTimeFormat);
        }
    }
}
