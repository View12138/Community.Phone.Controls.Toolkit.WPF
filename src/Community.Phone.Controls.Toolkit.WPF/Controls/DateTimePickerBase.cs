// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Base class for controls that allow the user to choose a date or time.
    /// </summary>
    [TemplatePart(Name = "DateTimeButton", Type = typeof(ButtonBase))]
    public class DateTimePickerBase : Control
    {
        private ButtonBase _dateButtonPart;

        /// <summary>
        /// Identifies the <see cref="Value"/> property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(DateTime?), typeof(DateTimePickerBase), new PropertyMetadata(null, OnValueChanged));

        /// <summary>
        /// Identifies the <see cref="ValueString"/> property.
        /// </summary>
        public static readonly DependencyProperty ValueStringProperty =
            DependencyProperty.Register("ValueString", typeof(string), typeof(DateTimePickerBase), null);

        /// <summary>
        /// Identifies the <see cref="ValueStringFormat"/> property.
        /// </summary>
        public static readonly DependencyProperty ValueStringFormatProperty =
            DependencyProperty.Register("ValueStringFormat", typeof(string), typeof(DateTimePickerBase), new PropertyMetadata(null, OnValueStringFormatChanged));

        /// <summary>
        /// Identifies the <see cref="Header"/> property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(DateTimePickerBase), null);

        /// <summary>
        /// Identifies the <see cref="HeaderTemplate"/> property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(DateTimePickerBase), null);

        /// <summary>
        /// Identifies the <see cref="PickerPageUri"/> property.
        /// </summary>
        public static readonly DependencyProperty PickerPageUriProperty =
            DependencyProperty.Register("PickerPageUri", typeof(Uri), typeof(DateTimePickerBase), null);

        /// <summary>
        /// Occurs when <see cref="Value"/> changes.
        /// </summary>
        public event EventHandler<DateTimeValueChangedEventArgs> ValueChanged;

        /// <summary>
        /// Gets or sets the selected date/time.
        /// </summary>
        public DateTime? Value
        {
            get { return (DateTime?)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Gets the formatted value string.
        /// </summary>
        public string ValueString
        {
            get { return (string)GetValue(ValueStringProperty); }
            private set { SetValue(ValueStringProperty, value); }
        }

        /// <summary>
        /// Gets or sets the format string used for <see cref="ValueString"/>.
        /// </summary>
        public string ValueStringFormat
        {
            get { return (string)GetValue(ValueStringFormatProperty); }
            set { SetValue(ValueStringFormatProperty, value); }
        }

        /// <summary>
        /// Gets or sets the header.
        /// </summary>
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Gets or sets the header template.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the picker page URI. Retained for compatibility.
        /// </summary>
        public Uri PickerPageUri
        {
            get { return (Uri)GetValue(PickerPageUriProperty); }
            set { SetValue(PickerPageUriProperty, value); }
        }

        /// <summary>
        /// Gets the fallback value string format.
        /// </summary>
        protected virtual string ValueStringFormatFallback
        {
            get { return "{0}"; }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            if (_dateButtonPart != null)
            {
                _dateButtonPart.Click -= OnDateButtonClick;
            }

            base.OnApplyTemplate();

            _dateButtonPart = GetTemplateChild("DateTimeButton") as ButtonBase;
            if (_dateButtonPart != null)
            {
                _dateButtonPart.Click += OnDateButtonClick;
            }

            UpdateValueString();
        }

        /// <summary>
        /// Opens the picker UI.
        /// </summary>
        protected virtual void OpenPicker()
        {
        }

        private void OnDateButtonClick(object sender, RoutedEventArgs e)
        {
            OpenPicker();
        }

        private void UpdateValueString()
        {
            ValueString = string.Format(CultureInfo.CurrentCulture, ValueStringFormat ?? ValueStringFormatFallback, Value);
        }

        private static void OnValueChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateTimePickerBase)d).OnValueChanged((DateTime?)e.OldValue, (DateTime?)e.NewValue);
        }

        private void OnValueChanged(DateTime? oldValue, DateTime? newValue)
        {
            UpdateValueString();
            OnValueChanged(new DateTimeValueChangedEventArgs(oldValue, newValue));
        }

        /// <summary>
        /// Raises the <see cref="ValueChanged"/> event.
        /// </summary>
        protected virtual void OnValueChanged(DateTimeValueChangedEventArgs e)
        {
            EventHandler<DateTimeValueChangedEventArgs> handler = ValueChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private static void OnValueStringFormatChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((DateTimePickerBase)d).UpdateValueString();
        }

        internal static bool DateShouldFlowRTL()
        {
            string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            return lang == "ar" || lang == "fa";
        }

        internal static bool IsRTLLanguage()
        {
            string lang = CultureInfo.CurrentCulture.TwoLetterISOLanguageName;
            return lang == "ar" || lang == "he" || lang == "fa";
        }
    }
}
