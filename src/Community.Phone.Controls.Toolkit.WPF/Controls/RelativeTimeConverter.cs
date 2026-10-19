// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;
using System.Windows.Data;
using Microsoft.Phone.Controls.LocalizedResources;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> into an elapsed-time string relative
    /// to the present.
    /// </summary>
    public class RelativeTimeConverter : IValueConverter
    {
        private const double Minute = 60.0;
        private const double Hour = 60.0 * Minute;
        private const double Day = 24 * Hour;
        private const double Week = 7 * Day;
        private const double Month = 30.5 * Day;
        private const double Year = 365 * Day;
        private const string DefaultCulture = "en-US";

        private string[] _pluralHourStrings;
        private string[] _pluralMinuteStrings;
        private string[] _pluralSecondStrings;

        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
            {
                throw new ArgumentException("The value must be a DateTime.");
            }

            DateTime given = ((DateTime)value).ToLocalTime();
            DateTime current = DateTime.Now;
            TimeSpan difference = current - given;

            SetLocalizationCulture(culture ?? CultureInfo.CurrentCulture);

            string result;
            if (DateTimeFormatHelper.IsFutureDateTime(current, given))
            {
                result = GetPluralTimeUnits(2, _pluralSecondStrings);
            }
            else if (difference.TotalSeconds > Year)
            {
                result = ControlResources.OverAYearAgo;
            }
            else if (difference.TotalSeconds > 1.5 * Month)
            {
                int months = (int)((difference.TotalSeconds + Month / 2) / Month);
                result = GetPluralMonth(months);
            }
            else if (difference.TotalSeconds >= 3.5 * Week)
            {
                result = ControlResources.AboutAMonthAgo;
            }
            else if (difference.TotalSeconds >= Week)
            {
                int weeks = (int)(difference.TotalSeconds / Week);
                result = weeks > 1
                    ? string.Format(CultureInfo.CurrentUICulture, ControlResources.XWeeksAgo_2To4, weeks.ToString(ControlResources.Culture))
                    : ControlResources.AboutAWeekAgo;
            }
            else if (difference.TotalSeconds >= 5 * Day)
            {
                result = GetLastDayOfWeek(given.DayOfWeek);
            }
            else if (difference.TotalSeconds >= Day)
            {
                result = GetOnDayOfWeek(given.DayOfWeek);
            }
            else if (difference.TotalSeconds >= 2 * Hour)
            {
                int hours = (int)(difference.TotalSeconds / Hour);
                result = GetPluralTimeUnits(hours, _pluralHourStrings);
            }
            else if (difference.TotalSeconds >= Hour)
            {
                result = ControlResources.AboutAnHourAgo;
            }
            else if (difference.TotalSeconds >= 2 * Minute)
            {
                int minutes = (int)(difference.TotalSeconds / Minute);
                result = GetPluralTimeUnits(minutes, _pluralMinuteStrings);
            }
            else if (difference.TotalSeconds >= Minute)
            {
                result = ControlResources.AboutAMinuteAgo;
            }
            else
            {
                int seconds = (int)difference.TotalSeconds > 1 ? (int)difference.TotalSeconds : 2;
                result = GetPluralTimeUnits(seconds, _pluralSecondStrings);
            }

            return result;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter is one-way.");
        }

        private void SetLocalizationCulture(CultureInfo culture)
        {
            if (!culture.Name.Equals(DefaultCulture, StringComparison.Ordinal))
            {
                ControlResources.Culture = culture;
            }

            _pluralHourStrings = new[]
            {
                ControlResources.XHoursAgo_2To4,
                ControlResources.XHoursAgo_EndsIn1Not11,
                ControlResources.XHoursAgo_EndsIn2To4Not12To14,
                ControlResources.XHoursAgo_Other
            };

            _pluralMinuteStrings = new[]
            {
                ControlResources.XMinutesAgo_2To4,
                ControlResources.XMinutesAgo_EndsIn1Not11,
                ControlResources.XMinutesAgo_EndsIn2To4Not12To14,
                ControlResources.XMinutesAgo_Other
            };

            _pluralSecondStrings = new[]
            {
                ControlResources.XSecondsAgo_2To4,
                ControlResources.XSecondsAgo_EndsIn1Not11,
                ControlResources.XSecondsAgo_EndsIn2To4Not12To14,
                ControlResources.XSecondsAgo_Other
            };
        }

        private static string GetPluralMonth(int month)
        {
            if (month >= 2 && month <= 4)
            {
                return string.Format(CultureInfo.CurrentUICulture, ControlResources.XMonthsAgo_2To4, month.ToString(ControlResources.Culture));
            }

            if (month >= 5 && month <= 12)
            {
                return string.Format(CultureInfo.CurrentUICulture, ControlResources.XMonthsAgo_5To12, month.ToString(ControlResources.Culture));
            }

            throw new ArgumentException("The number of months must be between 2 and 12.");
        }

        private static string GetPluralTimeUnits(int units, string[] resources)
        {
            int modTen = units % 10;
            int modHundred = units % 100;

            if (units <= 1)
            {
                throw new ArgumentException("The number of time units must be greater than one.");
            }

            if (units >= 2 && units <= 4)
            {
                return string.Format(CultureInfo.CurrentUICulture, resources[0], units.ToString(ControlResources.Culture));
            }

            if (modTen == 1 && modHundred != 11)
            {
                return string.Format(CultureInfo.CurrentUICulture, resources[1], units.ToString(ControlResources.Culture));
            }

            if (modTen >= 2 && modTen <= 4 && !(modHundred >= 12 && modHundred <= 14))
            {
                return string.Format(CultureInfo.CurrentUICulture, resources[2], units.ToString(ControlResources.Culture));
            }

            return string.Format(CultureInfo.CurrentUICulture, resources[3], units.ToString(ControlResources.Culture));
        }

        private static string GetLastDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday: return ControlResources.last_Monday;
                case DayOfWeek.Tuesday: return ControlResources.last_Tuesday;
                case DayOfWeek.Wednesday: return ControlResources.last_Wednesday;
                case DayOfWeek.Thursday: return ControlResources.last_Thursday;
                case DayOfWeek.Friday: return ControlResources.last_Friday;
                case DayOfWeek.Saturday: return ControlResources.last_Saturday;
                default: return ControlResources.last_Sunday;
            }
        }

        private static string GetOnDayOfWeek(DayOfWeek dayOfWeek)
        {
            switch (dayOfWeek)
            {
                case DayOfWeek.Monday: return ControlResources.on_Monday;
                case DayOfWeek.Tuesday: return ControlResources.on_Tuesday;
                case DayOfWeek.Wednesday: return ControlResources.on_Wednesday;
                case DayOfWeek.Thursday: return ControlResources.on_Thursday;
                case DayOfWeek.Friday: return ControlResources.on_Friday;
                case DayOfWeek.Saturday: return ControlResources.on_Saturday;
                default: return ControlResources.on_Sunday;
            }
        }
    }
}
