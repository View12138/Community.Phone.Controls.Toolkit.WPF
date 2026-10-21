// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Provides a WPF representation of the Windows Phone system tray.
    /// The API is compatible; the visual chrome is rendered by
    /// <see cref="PhoneApplicationFrame"/> when a presentation host is
    /// available.
    /// </summary>
    public static class SystemTray
    {
        /// <summary>
        /// Identifies the IsVisible attached property.
        /// </summary>
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.RegisterAttached(
                "IsVisible",
                typeof(bool),
                typeof(SystemTray),
                new PropertyMetadata(false, OnIsVisibleChanged));

        /// <summary>
        /// Gets or sets the opacity of the system tray.
        /// </summary>
        public static double Opacity
        {
            get { return SystemTrayState.Current.Opacity; }
            set { SystemTrayState.Current.Opacity = value; }
        }

        /// <summary>
        /// Gets or sets the system tray background color.
        /// </summary>
        public static Color BackgroundColor
        {
            get { return SystemTrayState.Current.BackgroundColor; }
            set { SystemTrayState.Current.BackgroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the system tray foreground color.
        /// </summary>
        public static Color ForegroundColor
        {
            get { return SystemTrayState.Current.ForegroundColor; }
            set { SystemTrayState.Current.ForegroundColor = value; }
        }

        /// <summary>
        /// Gets or sets the progress indicator displayed by the system tray.
        /// </summary>
        public static ProgressIndicator ProgressIndicator
        {
            get { return SystemTrayState.Current.ProgressIndicator; }
            set { SystemTrayState.Current.ProgressIndicator = value; }
        }

        /// <summary>
        /// Gets the IsVisible attached property.
        /// </summary>
        public static bool GetIsVisible(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (bool)element.GetValue(IsVisibleProperty);
        }

        /// <summary>
        /// Sets the IsVisible attached property.
        /// </summary>
        public static void SetIsVisible(DependencyObject element, bool value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(IsVisibleProperty, value);
        }

        private static void OnIsVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is PhoneApplicationPage || d is PhoneApplicationFrame || d is Window)
            {
                SystemTrayState.Current.IsVisible = (bool)e.NewValue;
            }
        }
    }

    /// <summary>
    /// The progress indicator shown by the system tray.
    /// </summary>
    public class ProgressIndicator : DependencyObject
    {
        /// <summary>
        /// Identifies the IsVisible property.
        /// </summary>
        public static readonly DependencyProperty IsVisibleProperty =
            DependencyProperty.Register("IsVisible", typeof(bool), typeof(ProgressIndicator), new PropertyMetadata(false, OnChanged));

        /// <summary>
        /// Identifies the IsIndeterminate property.
        /// </summary>
        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(ProgressIndicator), new PropertyMetadata(true, OnChanged));

        /// <summary>
        /// Identifies the Value property.
        /// </summary>
        public static readonly DependencyProperty ValueProperty =
            DependencyProperty.Register("Value", typeof(double), typeof(ProgressIndicator), new PropertyMetadata(0.0, OnChanged));

        /// <summary>
        /// Identifies the Text property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register("Text", typeof(string), typeof(ProgressIndicator), new PropertyMetadata(string.Empty, OnChanged));

        /// <summary>
        /// Gets or sets whether the indicator is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return (bool)GetValue(IsVisibleProperty); }
            set { SetValue(IsVisibleProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the indicator is indeterminate.
        /// </summary>
        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the indicator value.
        /// </summary>
        public double Value
        {
            get { return (double)GetValue(ValueProperty); }
            set { SetValue(ValueProperty, value); }
        }

        /// <summary>
        /// Gets or sets the indicator text.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        private static void OnChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            SystemTrayState.Current.RaiseProgressChanged();
        }
    }

    /// <summary>
    /// Shared state used by the system tray presenter.
    /// </summary>
    public sealed class SystemTrayState : INotifyPropertyChanged
    {
        [ThreadStatic]
        private static SystemTrayState _instance;

        private bool _isVisible;
        private double _opacity = 1.0;
        private Color _backgroundColor = Colors.Black;
        private Color _foregroundColor = Colors.White;
        private ProgressIndicator _progressIndicator = new ProgressIndicator();

        private SystemTrayState()
        {
        }

        /// <summary>
        /// Gets the shared state instance.
        /// </summary>
        public static SystemTrayState Current
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new SystemTrayState();
                }

                return _instance;
            }
        }

        /// <summary>
        /// Occurs when a state value changes.
        /// </summary>
        public event PropertyChangedEventHandler PropertyChanged;

        /// <summary>
        /// Gets or sets whether the tray is visible.
        /// </summary>
        public bool IsVisible
        {
            get { return _isVisible; }
            set { SetField(ref _isVisible, value, "IsVisible"); }
        }

        /// <summary>
        /// Gets or sets the tray opacity.
        /// </summary>
        public double Opacity
        {
            get { return _opacity; }
            set { SetField(ref _opacity, value, "Opacity"); }
        }

        /// <summary>
        /// Gets or sets the background color.
        /// </summary>
        public Color BackgroundColor
        {
            get { return _backgroundColor; }
            set { SetField(ref _backgroundColor, value, "BackgroundColor"); }
        }

        /// <summary>
        /// Gets or sets the foreground color.
        /// </summary>
        public Color ForegroundColor
        {
            get { return _foregroundColor; }
            set { SetField(ref _foregroundColor, value, "ForegroundColor"); }
        }

        /// <summary>
        /// Gets or sets the progress indicator.
        /// </summary>
        public ProgressIndicator ProgressIndicator
        {
            get { return _progressIndicator; }
            set { SetField(ref _progressIndicator, value, "ProgressIndicator"); }
        }

        internal void RaiseProgressChanged()
        {
            RaisePropertyChanged("ProgressIndicator");
        }

        private void SetField<T>(ref T field, T value, string propertyName)
        {
            if (!Equals(field, value))
            {
                field = value;
                RaisePropertyChanged(propertyName);
            }
        }

        private void RaisePropertyChanged(string propertyName)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null)
            {
                handler(this, new PropertyChangedEventArgs(propertyName));
            }
        }
    }
}

