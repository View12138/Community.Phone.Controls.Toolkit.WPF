using System;
using System.Collections;
using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace Microsoft.Phone.Controls;

public class JumpListItemForegroundConverter : IValueConverter
{
    public Brush Enabled { get; set; }

    public Brush Disabled { get; set; }

    public JumpListItemForegroundConverter()
    {
        Enabled = new SolidColorBrush(Colors.White);
        Disabled = (SolidColorBrush)Application.Current.Resources["PhoneDisabledBrush"];
    }

    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is IList { Count: not 0 })
        {
            return Enabled;
        }
        return Disabled;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
