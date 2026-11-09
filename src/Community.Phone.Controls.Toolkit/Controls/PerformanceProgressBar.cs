// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A progress bar optimized for indeterminate states.
    /// </summary>
    [TemplateVisualState(GroupName = "VisibilityStates", Name = "Normal")]
    [TemplateVisualState(GroupName = "VisibilityStates", Name = "Hidden")]
    [TemplatePart(Name = "EmbeddedProgressBar", Type = typeof(ProgressBar))]
    public class PerformanceProgressBar : Control
    {
        private ProgressBar _progressBar;

        /// <summary>
        /// Identifies the <see cref="ActualIsIndeterminate"/> property.
        /// </summary>
        public static readonly DependencyProperty ActualIsIndeterminateProperty =
            DependencyProperty.Register("ActualIsIndeterminate", typeof(bool), typeof(PerformanceProgressBar), new PropertyMetadata(false));

        /// <summary>
        /// Identifies the <see cref="IsIndeterminate"/> property.
        /// </summary>
        public static readonly DependencyProperty IsIndeterminateProperty =
            DependencyProperty.Register("IsIndeterminate", typeof(bool), typeof(PerformanceProgressBar), new PropertyMetadata(false, OnIsIndeterminatePropertyChanged));

        /// <summary>
        /// Initializes a new instance of the <see cref="PerformanceProgressBar"/> class.
        /// </summary>
        public PerformanceProgressBar()
        {
        }

        static PerformanceProgressBar()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PerformanceProgressBar),
                new FrameworkPropertyMetadata(typeof(PerformanceProgressBar)));
        }

        /// <summary>
        /// Gets or sets the actual indeterminate value.
        /// </summary>
        public bool ActualIsIndeterminate
        {
            get { return (bool)GetValue(ActualIsIndeterminateProperty); }
            set { SetValue(ActualIsIndeterminateProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the progress bar is indeterminate.
        /// </summary>
        public bool IsIndeterminate
        {
            get { return (bool)GetValue(IsIndeterminateProperty); }
            set { SetValue(IsIndeterminateProperty, value); }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _progressBar = GetTemplateChild("EmbeddedProgressBar") as ProgressBar;
            UpdateVisualStates(false);
        }

        private static void OnIsIndeterminatePropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((PerformanceProgressBar)d).OnIsIndeterminateChanged((bool)e.NewValue);
        }

        private void OnIsIndeterminateChanged(bool isIndeterminate)
        {
            ActualIsIndeterminate = isIndeterminate;
            UpdateVisualStates(true);
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsIndeterminate ? "Normal" : "Hidden", useTransitions);
            if (_progressBar != null)
            {
                _progressBar.IsIndeterminate = ActualIsIndeterminate;
            }
        }
    }

    /// <summary>
    /// A content control that exposes a hook for size-relative animations.
    /// </summary>
    /// <remarks>
    /// The original Silverlight control rewrote animation values that used a
    /// magic number to reflect the control's current size. This WPF port keeps
    /// the public type and layout behavior; standard WPF animations should use
    /// relative sizing instead of the magic-number convention.
    /// </remarks>
    public class RelativeAnimatingContentControl : ContentControl
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RelativeAnimatingContentControl"/> class.
        /// </summary>
        public RelativeAnimatingContentControl()
        {
        }

        static RelativeAnimatingContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(RelativeAnimatingContentControl),
                new FrameworkPropertyMetadata(typeof(RelativeAnimatingContentControl)));
        }
    }
}
