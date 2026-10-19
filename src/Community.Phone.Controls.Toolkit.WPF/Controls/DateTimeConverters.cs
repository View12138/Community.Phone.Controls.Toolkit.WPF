// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Converts a <see cref="DateTime"/> into a string for daily feeds.
    /// </summary>
    public class DailyDateTimeConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
            {
                throw new ArgumentException("The value must be a DateTime.");
            }

            DateTime given = (DateTime)value;
            DateTime current = DateTime.Now;

            if (DateTimeFormatHelper.IsFutureDateTime(current, given))
            {
                throw new NotSupportedException("Future dates and times are not supported.");
            }

            if (DateTimeFormatHelper.IsAtLeastOneWeekOld(current, given))
            {
                return DateTimeFormatHelper.GetShortDate(given);
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                "{0} {1}",
                DateTimeFormatHelper.GetAbbreviatedDay(given),
                DateTimeFormatHelper.GetSuperShortTime(given));
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter is one-way.");
        }
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> into a string for hourly feeds.
    /// </summary>
    public class HourlyDateTimeConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
            {
                throw new ArgumentException("The value must be a DateTime.");
            }

            DateTime given = (DateTime)value;
            DateTime current = DateTime.Now;

            if (DateTimeFormatHelper.IsFutureDateTime(current, given))
            {
                throw new NotSupportedException("Future dates and times are not supported.");
            }

            StringBuilder result = new StringBuilder();
            if (DateTimeFormatHelper.IsAnOlderYear(current, given))
            {
                result.AppendFormat(CultureInfo.CurrentCulture, "{0}, {1}", DateTimeFormatHelper.GetShortDate(given), DateTimeFormatHelper.GetSuperShortTime(given));
            }
            else if (DateTimeFormatHelper.IsAnOlderWeek(current, given))
            {
                result.AppendFormat(CultureInfo.CurrentCulture, "{0}, {1}", DateTimeFormatHelper.GetMonthAndDay(given), DateTimeFormatHelper.GetSuperShortTime(given));
            }
            else if (DateTimeFormatHelper.IsPastDayOfWeekWithWindow(current, given))
            {
                result.AppendFormat(CultureInfo.CurrentCulture, "{0}, {1}", DateTimeFormatHelper.GetAbbreviatedDay(given), DateTimeFormatHelper.GetSuperShortTime(given));
            }
            else
            {
                result.Append(DateTimeFormatHelper.GetSuperShortTime(given));
            }

            if (DateTimeFormatHelper.IsCurrentUICultureFrench())
            {
                result.Replace(",", string.Empty);
            }

            return result.ToString();
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter is one-way.");
        }
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> into a full date and time string.
    /// </summary>
    public class FullViewDateTimeConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
            {
                throw new ArgumentException("The value must be a DateTime.");
            }

            DateTime given = (DateTime)value;
            StringBuilder result = new StringBuilder();

            if (DateTimeFormatHelper.IsCurrentCultureJapanese() || DateTimeFormatHelper.IsCurrentCultureKorean())
            {
                result.AppendFormat(
                    CultureInfo.CurrentCulture,
                    "{0} {1} {2}",
                    DateTimeFormatHelper.GetMonthAndDay(given),
                    DateTimeFormatHelper.GetAbbreviatedDay(given),
                    DateTimeFormatHelper.GetShortTime(given));
            }
            else if (DateTimeFormatHelper.IsCurrentCultureTurkish())
            {
                result.AppendFormat(
                    CultureInfo.CurrentCulture,
                    "{0}, {1} {2}",
                    DateTimeFormatHelper.GetMonthAndDay(given),
                    DateTimeFormatHelper.GetAbbreviatedDay(given),
                    DateTimeFormatHelper.GetShortTime(given));
            }
            else
            {
                result.AppendFormat(
                    CultureInfo.CurrentCulture,
                    "{0} {1}, {2}",
                    DateTimeFormatHelper.GetAbbreviatedDay(given),
                    DateTimeFormatHelper.GetMonthAndDay(given),
                    DateTimeFormatHelper.GetShortTime(given));
            }

            if (DateTimeFormatHelper.IsCurrentUICultureFrench())
            {
                result.Replace(",", string.Empty);
            }

            return result.ToString();
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter is one-way.");
        }
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> into a compact list-view string.
    /// </summary>
    public class ListViewDateTimeConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
            {
                throw new ArgumentException("The value must be a DateTime.");
            }

            DateTime given = (DateTime)value;
            DateTime current = DateTime.Now;

            if (DateTimeFormatHelper.IsFutureDateTime(current, given))
            {
                throw new NotSupportedException("Future dates and times are not supported.");
            }

            if (DateTimeFormatHelper.IsAnOlderWeek(current, given))
            {
                return DateTimeFormatHelper.GetMonthAndDay(given);
            }

            if (DateTimeFormatHelper.IsPastDayOfWeek(current, given))
            {
                return DateTimeFormatHelper.GetAbbreviatedDay(given);
            }

            return DateTimeFormatHelper.GetSuperShortTime(given);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter is one-way.");
        }
    }

    /// <summary>
    /// Converts a <see cref="DateTime"/> into a compact thread string.
    /// </summary>
    public class ThreadDateTimeConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is DateTime))
            {
                throw new ArgumentException("The value must be a DateTime.");
            }

            DateTime given = (DateTime)value;
            DateTime current = DateTime.Now;

            if (DateTimeFormatHelper.IsFutureDateTime(current, given))
            {
                throw new NotSupportedException("Future dates and times are not supported.");
            }

            if (DateTimeFormatHelper.IsAnOlderWeek(current, given))
            {
                return DateTimeFormatHelper.GetMonthAndDay(given);
            }

            if (DateTimeFormatHelper.IsPastDayOfWeekWithWindow(current, given))
            {
                return DateTimeFormatHelper.GetAbbreviatedDay(given);
            }

            return DateTimeFormatHelper.GetSuperShortTime(given);
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter is one-way.");
        }
    }
}
