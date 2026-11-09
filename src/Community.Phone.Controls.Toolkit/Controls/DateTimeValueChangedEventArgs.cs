// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides data for <see cref="DateTimePickerBase.ValueChanged"/>.
    /// </summary>
    public class DateTimeValueChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="DateTimeValueChangedEventArgs"/> class.
        /// </summary>
        public DateTimeValueChangedEventArgs(DateTime? oldDateTime, DateTime? newDateTime)
        {
            OldDateTime = oldDateTime;
            NewDateTime = newDateTime;
        }

        /// <summary>
        /// Gets the old date/time.
        /// </summary>
        public DateTime? OldDateTime { get; private set; }

        /// <summary>
        /// Gets the new date/time.
        /// </summary>
        public DateTime? NewDateTime { get; private set; }
    }
}
