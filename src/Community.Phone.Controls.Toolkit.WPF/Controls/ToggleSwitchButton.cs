// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Represents a switch that can be toggled between two states.
    /// </summary>
    [TemplateVisualState(Name = "Normal", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Disabled", GroupName = "CommonStates")]
    [TemplateVisualState(Name = "Checked", GroupName = "CheckStates")]
    [TemplateVisualState(Name = "Dragging", GroupName = "CheckStates")]
    [TemplateVisualState(Name = "Unchecked", GroupName = "CheckStates")]
    [TemplatePart(Name = "SwitchRoot", Type = typeof(Grid))]
    [TemplatePart(Name = "SwitchBackground", Type = typeof(UIElement))]
    [TemplatePart(Name = "SwitchTrack", Type = typeof(Grid))]
    [TemplatePart(Name = "SwitchThumb", Type = typeof(FrameworkElement))]
    public class ToggleSwitchButton : ToggleButton
    {
        /// <summary>
        /// Identifies the <see cref="SwitchForeground"/> property.
        /// </summary>
        public static readonly DependencyProperty SwitchForegroundProperty =
            DependencyProperty.Register("SwitchForeground", typeof(Brush), typeof(ToggleSwitchButton), new PropertyMetadata(null));

        private TranslateTransform _backgroundTranslation;
        private TranslateTransform _thumbTranslation;
        private Grid _root;
        private Grid _track;
        private FrameworkElement _thumb;
        private const double UncheckedTranslation = 0;
        private double _checkedTranslation;
        private double _dragTranslation;
        private bool _wasDragged;
        private bool _isDragging;

        /// <summary>
        /// Initializes a new instance of the <see cref="ToggleSwitchButton"/> class.
        /// </summary>
        public ToggleSwitchButton()
        {
        }

        static ToggleSwitchButton()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ToggleSwitchButton),
                new FrameworkPropertyMetadata(typeof(ToggleSwitchButton)));
        }

        /// <summary>
        /// Gets or sets the switch foreground.
        /// </summary>
        public Brush SwitchForeground
        {
            get { return (Brush)GetValue(SwitchForegroundProperty); }
            set { SetValue(SwitchForegroundProperty, value); }
        }

        private double Translation
        {
            get { return _backgroundTranslation == null ? _thumbTranslation.X : _backgroundTranslation.X; }
            set
            {
                if (_backgroundTranslation != null)
                {
                    _backgroundTranslation.X = value;
                }

                if (_thumbTranslation != null)
                {
                    _thumbTranslation.X = value;
                }
            }
        }

        /// <inheritdoc />
        protected override void OnToggle()
        {
            IsChecked = !(IsChecked ?? false);
            ChangeVisualState(true);
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            if (_track != null)
            {
                _track.SizeChanged -= OnSizeChanged;
            }

            if (_thumb != null)
            {
                _thumb.SizeChanged -= OnSizeChanged;
            }

            if (_root != null)
            {
                _root.ManipulationStarted -= OnManipulationStarted;
                _root.ManipulationDelta -= OnManipulationDelta;
                _root.ManipulationCompleted -= OnManipulationCompleted;
            }

            base.OnApplyTemplate();

            _root = GetTemplateChild("SwitchRoot") as Grid;
            UIElement background = GetTemplateChild("SwitchBackground") as UIElement;
            _backgroundTranslation = background == null ? null : background.RenderTransform as TranslateTransform;
            _track = GetTemplateChild("SwitchTrack") as Grid;
            _thumb = GetTemplateChild("SwitchThumb") as FrameworkElement;
            _thumbTranslation = _thumb == null ? null : _thumb.RenderTransform as TranslateTransform;

            if (_root != null && _track != null && _thumb != null && (_backgroundTranslation != null || _thumbTranslation != null))
            {
                _root.IsManipulationEnabled = true;
                _root.ManipulationStarted += OnManipulationStarted;
                _root.ManipulationDelta += OnManipulationDelta;
                _root.ManipulationCompleted += OnManipulationCompleted;

                _track.SizeChanged += OnSizeChanged;
                _thumb.SizeChanged += OnSizeChanged;
            }

            ChangeVisualState(false);
        }

        private void ChangeVisualState(bool useTransitions)
        {
            VisualStateManager.GoToState(this, IsEnabled ? "Normal" : "Disabled", useTransitions);

            if (_isDragging)
            {
                VisualStateManager.GoToState(this, "Dragging", useTransitions);
            }
            else if (IsChecked ?? false)
            {
                VisualStateManager.GoToState(this, "Checked", useTransitions);
            }
            else
            {
                VisualStateManager.GoToState(this, "Unchecked", useTransitions);
            }
        }

        private void OnManipulationStarted(object sender, System.Windows.Input.ManipulationStartedEventArgs e)
        {
            e.Handled = true;
            _isDragging = true;
            _dragTranslation = Translation;
            ChangeVisualState(true);
            Translation = _dragTranslation;
        }

        private void OnManipulationDelta(object sender, System.Windows.Input.ManipulationDeltaEventArgs e)
        {
            e.Handled = true;
            double horizontalChange = e.DeltaManipulation.Translation.X;
            Orientation direction = Math.Abs(horizontalChange) >= Math.Abs(e.DeltaManipulation.Translation.Y)
                ? Orientation.Horizontal
                : Orientation.Vertical;

            if (direction == Orientation.Horizontal && horizontalChange != 0)
            {
                _wasDragged = true;
                _dragTranslation += horizontalChange;
                Translation = Math.Max(UncheckedTranslation, Math.Min(_checkedTranslation, _dragTranslation));
            }
        }

        private void OnManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            e.Handled = true;
            _isDragging = false;

            bool click = false;
            if (_wasDragged)
            {
                double edge = (IsChecked ?? false) ? _checkedTranslation : UncheckedTranslation;
                click = Translation != edge;
            }
            else
            {
                click = true;
            }

            if (click)
            {
                OnClick();
            }

            _wasDragged = false;
        }

        /// <inheritdoc />
        protected override void OnMouseLeave(System.Windows.Input.MouseEventArgs e)
        {
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (_track != null)
            {
                _track.Clip = new RectangleGeometry { Rect = new Rect(0, 0, _track.ActualWidth, _track.ActualHeight) };
            }

            if (_track != null && _thumb != null)
            {
                _checkedTranslation = _track.ActualWidth - _thumb.ActualWidth - _thumb.Margin.Left - _thumb.Margin.Right;
            }
        }
    }
}
