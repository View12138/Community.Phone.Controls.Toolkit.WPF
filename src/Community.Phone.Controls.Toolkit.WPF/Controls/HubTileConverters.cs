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
    /// Returns the width corresponding to a <see cref="TileSize"/>.
    /// </summary>
    public class TileSizeToWidthConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double baseWidth;
            switch ((TileSize)value)
            {
                case TileSize.Small: baseWidth = 99; break;
                case TileSize.Medium: baseWidth = 210; break;
                case TileSize.Large: baseWidth = 432; break;
                default: baseWidth = 173; break;
            }

            double multiplier;
            if (parameter == null || !double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out multiplier))
            {
                multiplier = 1;
            }

            return baseWidth * multiplier;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }

    /// <summary>
    /// Returns the height corresponding to a <see cref="TileSize"/>.
    /// </summary>
    public class TileSizeToHeightConverter : IValueConverter
    {
        /// <inheritdoc />
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double baseHeight;
            switch ((TileSize)value)
            {
                case TileSize.Small: baseHeight = 99; break;
                case TileSize.Medium: baseHeight = 210; break;
                case TileSize.Large: baseHeight = 210; break;
                default: baseHeight = 173; break;
            }

            double multiplier;
            if (parameter == null || !double.TryParse(parameter.ToString(), NumberStyles.Any, CultureInfo.InvariantCulture, out multiplier))
            {
                multiplier = 1;
            }

            return baseHeight * multiplier;
        }

        /// <inheritdoc />
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
