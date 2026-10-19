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
    /// Converts <see cref="Nullable{Boolean}"/> values to "Off" and "On"
    /// strings.
    /// </summary>
    public class OffOnConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == null)
            {
                throw new ArgumentNullException("targetType");
            }

            if (targetType != typeof(object) && targetType != typeof(string))
            {
                throw new ArgumentException("Unexpected target type.", "targetType");
            }

            if (value is bool? || value == null)
            {
                return (bool?)value == true ? ControlResources.On : ControlResources.Off;
            }

            throw new ArgumentException("Unexpected value type.", "value");
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException("This converter is one-way.");
        }
    }
}
