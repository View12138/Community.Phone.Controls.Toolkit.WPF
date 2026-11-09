// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;
using System.Windows.Data;

namespace Microsoft.Phone.Controls
{
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
