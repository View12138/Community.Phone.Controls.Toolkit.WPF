// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Allows the user to choose a time.
    /// </summary>
    public class TimePicker : DateTimePickerBase
    {
        private string _fallbackValueStringFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="TimePicker"/> class.
        /// </summary>
        public TimePicker()
        {
            Value = DateTime.Now;
        }

        static TimePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TimePicker),
                new FrameworkPropertyMetadata(typeof(TimePicker)));
        }

        /// <inheritdoc />
        protected override string ValueStringFormatFallback
        {
            get
            {
                if (_fallbackValueStringFormat == null)
                {
                    string pattern = CultureInfo.CurrentCulture.DateTimeFormat.LongTimePattern.Replace(":ss", string.Empty);
                    string language = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
                    pattern = (language == "ar" || language == "fa") ? "\u200F" + pattern : "\u200E" + pattern;
                    _fallbackValueStringFormat = "{0:" + pattern + "}";
                }

                return _fallbackValueStringFormat;
            }
        }

        /// <inheritdoc />
        protected override void OpenPicker()
        {
            Window owner = Application.Current == null ? null : Application.Current.MainWindow;
            Window window = new Window
            {
                Title = "Choose time",
                Width = 300,
                Height = 190,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
                Owner = owner
            };

            DateTime current = Value ?? DateTime.Now;
            StackPanel panel = new StackPanel { Margin = new Thickness(12) };
            StackPanel fields = new StackPanel { Orientation = Orientation.Horizontal };
            ComboBox hour = new ComboBox { Width = 70, Margin = new Thickness(0, 0, 8, 0) };
            ComboBox minute = new ComboBox { Width = 70 };
            for (int i = 0; i < 24; i++)
            {
                hour.Items.Add(i.ToString("00", CultureInfo.InvariantCulture));
            }

            for (int i = 0; i < 60; i++)
            {
                minute.Items.Add(i.ToString("00", CultureInfo.InvariantCulture));
            }

            hour.SelectedIndex = current.Hour;
            minute.SelectedIndex = current.Minute;
            fields.Children.Add(hour);
            fields.Children.Add(new TextBlock { Text = ":", VerticalAlignment = VerticalAlignment.Center, Margin = new Thickness(0, 0, 8, 0) });
            fields.Children.Add(minute);
            panel.Children.Add(fields);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 16, 0, 0)
            };
            Button cancel = new Button { Content = "cancel", Width = 90, Margin = new Thickness(0, 0, 8, 0) };
            cancel.Click += delegate { window.DialogResult = false; };
            Button done = new Button { Content = "done", Width = 90 };
            done.Click += delegate { window.DialogResult = true; };
            buttons.Children.Add(cancel);
            buttons.Children.Add(done);
            panel.Children.Add(buttons);
            window.Content = panel;

            if (window.ShowDialog() == true)
            {
                DateTime baseDate = Value ?? DateTime.Today;
                Value = new DateTime(baseDate.Year, baseDate.Month, baseDate.Day, hour.SelectedIndex, minute.SelectedIndex, 0);
            }
        }
    }
}
