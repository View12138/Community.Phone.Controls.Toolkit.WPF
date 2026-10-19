// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Extends <see cref="CultureInfo"/> with weekday and weekend helpers.
    /// </summary>
    public static class CultureInfoExtensions
    {
        private static readonly string[] CulturesWithThursdayFridayWeekends = { "ar-SA" };
        private static readonly string[] CulturesWithFridaySaturdayWeekends = { "he-IL", "ar-EG" };

        /// <summary>
        /// Returns the days that are weekdays in the given culture.
        /// </summary>
        public static ReadOnlyCollection<string> Weekdays(this CultureInfo culture)
        {
            DayOfWeek[] daysOfWeek;
            if (CulturesWithThursdayFridayWeekends.Contains(culture.Name))
            {
                daysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Saturday, DayOfWeek.Sunday };
            }
            else if (CulturesWithFridaySaturdayWeekends.Contains(culture.Name))
            {
                daysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Sunday };
            }
            else
            {
                daysOfWeek = new[] { DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday };
            }

            return new ReadOnlyCollection<string>(daysOfWeek.Select(culture.DateTimeFormat.GetDayName).ToList());
        }

        /// <summary>
        /// Returns the days that are weekends in the given culture.
        /// </summary>
        public static ReadOnlyCollection<string> Weekends(this CultureInfo culture)
        {
            DayOfWeek[] daysOfWeek;
            if (CulturesWithThursdayFridayWeekends.Contains(culture.Name))
            {
                daysOfWeek = new[] { DayOfWeek.Thursday, DayOfWeek.Friday };
            }
            else if (CulturesWithFridaySaturdayWeekends.Contains(culture.Name))
            {
                daysOfWeek = new[] { DayOfWeek.Friday, DayOfWeek.Saturday };
            }
            else
            {
                daysOfWeek = new[] { DayOfWeek.Saturday, DayOfWeek.Sunday };
            }

            return new ReadOnlyCollection<string>(daysOfWeek.Select(culture.DateTimeFormat.GetDayName).ToList());
        }
    }
}
