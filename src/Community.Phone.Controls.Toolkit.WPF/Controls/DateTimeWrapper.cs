// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Wraps a <see cref="DateTime"/> and exposes formatted component strings.
    /// </summary>
    public class DateTimeWrapper
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeWrapper"/> class.
        /// </summary>
        public DateTimeWrapper(DateTime dateTime)
        {
            DateTime = dateTime;
        }

        /// <summary>
        /// Gets the wrapped date/time.
        /// </summary>
        public DateTime DateTime { get; private set; }

        /// <summary>Gets the four-digit year.</summary>
        public string YearNumber { get { return DateTime.ToString("yyyy", CultureInfo.CurrentCulture); } }

        /// <summary>Gets the two-digit month.</summary>
        public string MonthNumber { get { return DateTime.ToString("MM", CultureInfo.CurrentCulture); } }

        /// <summary>Gets the month name.</summary>
        public string MonthName { get { return DateTime.ToString("MMMM", CultureInfo.CurrentCulture); } }

        /// <summary>Gets the two-digit day.</summary>
        public string DayNumber { get { return DateTime.ToString("dd", CultureInfo.CurrentCulture); } }

        /// <summary>Gets the day name.</summary>
        public string DayName { get { return DateTime.ToString("dddd", CultureInfo.CurrentCulture); } }

        /// <summary>Gets the hour.</summary>
        public string HourNumber
        {
            get { return DateTime.ToString(CurrentCultureUsesTwentyFourHourClock() ? "%H" : "%h", CultureInfo.CurrentCulture); }
        }

        /// <summary>Gets the two-digit minute.</summary>
        public string MinuteNumber { get { return DateTime.ToString("mm", CultureInfo.CurrentCulture); } }

        /// <summary>Gets the AM/PM designator.</summary>
        public string AmPmString { get { return DateTime.ToString("tt", CultureInfo.CurrentCulture); } }

        /// <summary>
        /// Returns whether the current culture uses a 24-hour clock.
        /// </summary>
        public static bool CurrentCultureUsesTwentyFourHourClock()
        {
            return !CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.Contains("t");
        }
    }
}
