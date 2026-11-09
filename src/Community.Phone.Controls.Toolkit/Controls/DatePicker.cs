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
    /// Allows the user to choose a date.
    /// </summary>
    public class DatePicker : DateTimePickerBase
    {
        private string _fallbackValueStringFormat;

        /// <summary>
        /// Initializes a new instance of the <see cref="DatePicker"/> class.
        /// </summary>
        public DatePicker()
        {
            Value = DateTime.Now.Date;
        }

        static DatePicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(DatePicker),
                new FrameworkPropertyMetadata(typeof(DatePicker)));
        }

        /// <inheritdoc />
        protected override string ValueStringFormatFallback
        {
            get
            {
                if (_fallbackValueStringFormat == null)
                {
                    string pattern = CultureInfo.CurrentCulture.DateTimeFormat.ShortDatePattern;
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
                Title = "Choose date",
                Width = 320,
                Height = 180,
                ResizeMode = ResizeMode.NoResize,
                WindowStartupLocation = owner == null ? WindowStartupLocation.CenterScreen : WindowStartupLocation.CenterOwner,
                Owner = owner
            };

            StackPanel panel = new StackPanel { Margin = new Thickness(12) };
            System.Windows.Controls.DatePicker picker = new System.Windows.Controls.DatePicker
            {
                SelectedDate = Value.HasValue ? Value.Value.Date : DateTime.Now.Date
            };
            panel.Children.Add(picker);

            StackPanel buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Right,
                Margin = new Thickness(0, 12, 0, 0)
            };
            Button cancel = new Button { Content = "cancel", Width = 90, Margin = new Thickness(0, 0, 8, 0) };
            cancel.Click += delegate { window.DialogResult = false; };
            Button done = new Button { Content = "done", Width = 90 };
            done.Click += delegate { window.DialogResult = true; };
            buttons.Children.Add(cancel);
            buttons.Children.Add(done);
            panel.Children.Add(buttons);
            window.Content = panel;

            if (window.ShowDialog() == true && picker.SelectedDate.HasValue)
            {
                Value = picker.SelectedDate.Value.Date;
            }
        }
    }
}
