// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Renders <see cref="SystemTray.ProgressIndicator"/> at the very top of the
    /// frame: a 4 DIP bar flush with the top edge (no margin), plus the optional
    /// indicator text. In Windows Phone this bar is drawn by the system tray; the
    /// WPF port hosts this presenter in the
    /// <see cref="Microsoft.Phone.Controls.PhoneApplicationFrame"/> template.
    /// This type is additive (it has no Windows Phone counterpart) and is
    /// documented as such.
    /// </summary>
    public class ProgressIndicatorPresenter : Control
    {
        /// <summary>
        /// The template part that draws the bar.
        /// </summary>
        private const string ProgressBarPartName = "PART_ProgressBar";

        /// <summary>
        /// The template part that shows <see cref="ProgressIndicator.Text"/>.
        /// </summary>
        private const string TextPartName = "PART_Text";

        private ProgressBar _progressBar;
        private TextBlock _text;

        static ProgressIndicatorPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ProgressIndicatorPresenter),
                new FrameworkPropertyMetadata(typeof(ProgressIndicatorPresenter)));
        }

        /// <summary>
        /// Initializes a new instance of the ProgressIndicatorPresenter class.
        /// </summary>
        public ProgressIndicatorPresenter()
        {
            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        /// <summary>
        /// Builds the visual tree for the presenter when a new template is applied.
        /// </summary>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _progressBar = GetTemplateChild(ProgressBarPartName) as ProgressBar;
            _text = GetTemplateChild(TextPartName) as TextBlock;
            UpdateIndicator();
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            SystemTrayState.Current.PropertyChanged += OnStateChanged;
            UpdateIndicator();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            SystemTrayState.Current.PropertyChanged -= OnStateChanged;
        }

        private void OnStateChanged(object sender, PropertyChangedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.PropertyName) || e.PropertyName == "ProgressIndicator")
            {
                UpdateIndicator();
            }
        }

        private void UpdateIndicator()
        {
            ProgressIndicator indicator = SystemTray.ProgressIndicator;
            bool isVisible = indicator != null && indicator.IsVisible;

            Visibility = isVisible ? Visibility.Visible : Visibility.Collapsed;
            if (!isVisible)
            {
                return;
            }

            if (_progressBar != null)
            {
                _progressBar.IsIndeterminate = indicator.IsIndeterminate;
                _progressBar.Value = indicator.Value;
            }

            if (_text != null)
            {
                string text = indicator.Text ?? string.Empty;
                _text.Text = text;
                _text.Visibility = text.Length == 0 ? Visibility.Collapsed : Visibility.Visible;
            }
        }
    }
}
