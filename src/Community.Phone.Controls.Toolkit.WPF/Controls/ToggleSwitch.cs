// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Phone.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a switch that can be toggled between two states.
    /// </summary>
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplatePart(Name = "Switch", Type = typeof(ToggleSwitchButton))]
    public class ToggleSwitch : ContentControl
    {
        /// <summary>
        /// Identifies the <see cref="Header"/> property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty =
            DependencyProperty.Register("Header", typeof(object), typeof(ToggleSwitch), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="HeaderTemplate"/> property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty =
            DependencyProperty.Register("HeaderTemplate", typeof(DataTemplate), typeof(ToggleSwitch), new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="SwitchForeground"/> property.
        /// </summary>
        public static readonly DependencyProperty SwitchForegroundProperty =
            DependencyProperty.Register("SwitchForeground", typeof(Brush), typeof(ToggleSwitch), null);

        /// <summary>
        /// Identifies the <see cref="IsChecked"/> property.
        /// </summary>
        public static readonly DependencyProperty IsCheckedProperty =
            DependencyProperty.Register("IsChecked", typeof(bool?), typeof(ToggleSwitch), new PropertyMetadata(false, OnIsCheckedChanged));

        private ToggleSwitchButton _toggleButton;
        private bool _wasContentSet;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleSwitch"/> class.
        /// </summary>
        public ToggleSwitch()
        {
        }

        static ToggleSwitch()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToggleSwitch),
                new FrameworkPropertyMetadata(typeof(ToggleSwitch)));
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
        /// Gets or sets the template used to display the header.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the switch foreground.
        /// </summary>
        public Brush SwitchForeground
        {
            get { return (Brush)GetValue(SwitchForegroundProperty); }
            set { SetValue(SwitchForegroundProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the switch is checked.
        /// </summary>
        [TypeConverter(typeof(NullableBoolConverter))]
        public bool? IsChecked
        {
            get { return (bool?)GetValue(IsCheckedProperty); }
            set
            {
                if (IsChecked != value)
                {
                    SetValue(IsCheckedProperty, value);
                }
            }
        }

        /// <summary>
        /// Occurs when the switch is checked.
        /// </summary>
        public event EventHandler<RoutedEventArgs> Checked;

        /// <summary>
        /// Occurs when the switch is unchecked.
        /// </summary>
        public event EventHandler<RoutedEventArgs> Unchecked;

        /// <summary>
        /// Occurs when the switch is indeterminate.
        /// </summary>
        public event EventHandler<RoutedEventArgs> Indeterminate;

        /// <summary>
        /// Occurs when the switch is clicked.
        /// </summary>
        public event EventHandler<RoutedEventArgs> Click;

        private static void OnIsCheckedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ToggleSwitch toggleSwitch = (ToggleSwitch)d;
            if (toggleSwitch._toggleButton != null)
            {
                toggleSwitch._toggleButton.IsChecked = (bool?)e.NewValue;
            }
        }

        private void SetDefaultContent()
        {
            Binding binding = new Binding("IsChecked") { Source = this, Converter = new OffOnConverter() };
            SetBinding(ContentProperty, binding);
        }

        /// <inheritdoc />
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            _wasContentSet = true;
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (!_wasContentSet && GetBindingExpression(ContentProperty) == null)
            {
                SetDefaultContent();
            }

            if (_toggleButton != null)
            {
                _toggleButton.Checked -= OnChecked;
                _toggleButton.Unchecked -= OnUnchecked;
                _toggleButton.Indeterminate -= OnIndeterminate;
                _toggleButton.Click -= OnClick;
            }

            _toggleButton = GetTemplateChild("Switch") as ToggleSwitchButton;
            if (_toggleButton != null)
            {
                _toggleButton.Checked += OnChecked;
                _toggleButton.Unchecked += OnUnchecked;
                _toggleButton.Indeterminate += OnIndeterminate;
                _toggleButton.Click += OnClick;
                _toggleButton.IsChecked = IsChecked;
            }

            IsEnabledChanged -= OnIsEnabledChanged;
            IsEnabledChanged += OnIsEnabledChanged;
            ChangeVisualState(false);
        }

        private void OnIsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            ChangeVisualState(true);
        }

        private void ChangeVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", useTransitions);
        }

        private void OnChecked(object sender, RoutedEventArgs e)
        {
            IsChecked = true;
            EventHandler<RoutedEventArgs> handler = Checked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnUnchecked(object sender, RoutedEventArgs e)
        {
            IsChecked = false;
            EventHandler<RoutedEventArgs> handler = Unchecked;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnIndeterminate(object sender, RoutedEventArgs e)
        {
            IsChecked = null;
            EventHandler<RoutedEventArgs> handler = Indeterminate;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        private void OnClick(object sender, RoutedEventArgs e)
        {
            EventHandler<RoutedEventArgs> handler = Click;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return string.Format(CultureInfo.InvariantCulture, "{{ToggleSwitch IsChecked={0}, Content={1}}}", IsChecked, Content);
        }
    }
}
