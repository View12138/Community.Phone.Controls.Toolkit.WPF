// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Microsoft.Phone.Controls.LocalizedResources;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Allows the user to choose recurring days of the week.
    /// </summary>
    public class RecurringDaysPicker : ListPicker
    {
        private const string CommaSpace = ", ";
        private static readonly string[] EnglishShortDayNames = { "Sun", "Mon", "Tue", "Wed", "Thu", "Fri", "Sat" };

        private readonly string[] _dayNames = CultureInfo.CurrentCulture.DateTimeFormat.DayNames;
        private readonly string[] _shortestDayNames;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecurringDaysPicker"/> class.
        /// </summary>
        public RecurringDaysPicker()
        {
            _shortestDayNames = CultureInfo.CurrentCulture.Name.StartsWith("en", StringComparison.OrdinalIgnoreCase)
                ? EnglishShortDayNames
                : CultureInfo.CurrentCulture.DateTimeFormat.ShortestDayNames;

            DayOfWeek first = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            for (int i = 0; i < _dayNames.Length; i++)
            {
                Items.Add(_dayNames[((int)first + i) % _dayNames.Length]);
            }

            SelectionMode = SelectionMode.Multiple;
            SummaryForSelectedItemsDelegate = SummarizeDaysOfWeek;
        }

        /// <summary>
        /// Summarizes a list of days into a compact string.
        /// </summary>
        protected string SummarizeDaysOfWeek(IList selection)
        {
            if (selection == null)
            {
                return ControlResources.RepeatsOnlyOnce;
            }

            List<string> contents = new List<string>();
            foreach (object item in selection)
            {
                contents.Add((string)item);
            }

            return DaysOfWeekToString(contents);
        }

        private string DaysOfWeekToString(List<string> daysList)
        {
            List<string> days = new List<string>();
            foreach (string day in daysList)
            {
                if (!days.Contains(day))
                {
                    days.Add(day);
                }
            }

            if (days.Count == 0)
            {
                return ControlResources.RepeatsOnlyOnce;
            }

            StringBuilder builder = new StringBuilder();
            IEnumerable<string> unhandledDays;
            builder.Append(HandleGroups(days, out unhandledDays));

            if (builder.Length > 0)
            {
                builder.Append(CommaSpace);
            }

            DayOfWeek first = CultureInfo.CurrentCulture.DateTimeFormat.FirstDayOfWeek;
            for (int i = 0; i < _dayNames.Length; i++)
            {
                int index = ((int)first + i) % _dayNames.Length;
                string day = _dayNames[index];
                if (unhandledDays.Contains(day))
                {
                    builder.Append(_shortestDayNames[index]);
                    builder.Append(CommaSpace);
                }
            }

            if (builder.Length >= CommaSpace.Length)
            {
                builder.Length -= CommaSpace.Length;
            }

            return builder.ToString();
        }

        private static string HandleGroups(List<string> days, out IEnumerable<string> unhandledDays)
        {
            if (days.Count == 7)
            {
                unhandledDays = new List<string>();
                return ControlResources.RepeatsEveryDay;
            }

            List<string> weekdays = CultureInfo.CurrentCulture.Weekdays().ToList();
            List<string> weekends = CultureInfo.CurrentCulture.Weekends().ToList();

            if (days.Intersect(weekdays).Count() == weekdays.Count)
            {
                unhandledDays = days.Where(day => !weekdays.Contains(day));
                return ControlResources.RepeatsOnWeekdays;
            }

            if (days.Intersect(weekends).Count() == weekends.Count)
            {
                unhandledDays = days.Where(day => !weekends.Contains(day));
                return ControlResources.RepeatsOnWeekends;
            }

            unhandledDays = days;
            return string.Empty;
        }
    }
}
