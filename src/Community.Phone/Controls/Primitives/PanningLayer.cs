// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Pans one of the layers of a <see cref="Panorama"/> at its own rate.
    /// </summary>
    [TemplatePart(Name = LocalTransformName, Type = typeof(TranslateTransform))]
    [TemplatePart(Name = ContentPresenterName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = LeftWraparoundName, Type = typeof(Border))]
    [TemplatePart(Name = PanningStackPanelName, Type = typeof(StackPanel))]
    [TemplatePart(Name = RightWraparoundName, Type = typeof(Border))]
    [TemplatePart(Name = PanningTransformName, Type = typeof(TranslateTransform))]
    public class PanningLayer : ContentControl
    {
        private const string LocalTransformName = "LocalTransform";
        private const string PanningTransformName = "PanningTransform";
        private const string LeftWraparoundName = "LeftWraparound";
        private const string RightWraparoundName = "RightWraparound";
        private const string PanningStackPanelName = "PanningStackPanel";
        private const string ContentPresenterName = "ContentPresenter";

        protected static readonly Duration Immediately = new Duration(TimeSpan.Zero);

        private readonly IEasingFunction _easingFunction = new ExponentialEase
        {
            Exponent = -8.00636756765025,
            EasingMode = EasingMode.EaseIn
        };

        private ContentPresenter _contentPresenter;
        private TransformAnimator _animator;
        private bool _isStatic;

        protected TranslateTransform LocalTransform { get; set; }

        protected TranslateTransform PanningTransform { get; set; }

        internal Border LeftWraparound { get; set; }

        internal Border RightWraparound { get; set; }

        private StackPanel PanningStackPanel { get; set; }

        protected internal ContentPresenter ContentPresenter
        {
            get { return _contentPresenter; }
            set
            {
                if (_contentPresenter != null)
                {
                    _contentPresenter.SizeChanged -= OnContentSizeChanged;
                }
                _contentPresenter = value;
                if (_contentPresenter != null)
                {
                    _contentPresenter.SizeChanged += OnContentSizeChanged;
                }
            }
        }

        /// <summary>
        /// Gets the rate at which this layer moves relative to the items layer.
        /// </summary>
        protected virtual double PanRate
        {
            get { return 1.0; }
        }

        internal bool IsStatic
        {
            get { return _isStatic; }
            set
            {
                if (value != _isStatic)
                {
                    _isStatic = value;
                    if (_isStatic)
                    {
                        ActualOffset = 0.0;
                    }
                    else
                    {
                        UpdateWrappingRectangles();
                    }
                }
            }
        }

        protected internal virtual bool ShouldShowWrappers
        {
            get { return !IsStatic && Owner != null && Owner.ShouldAllowWraparound; }
        }

        internal Panorama Owner { get; set; }

        internal double ActualOffset
        {
            get { return PanningTransform != null ? PanningTransform.X : 0.0; }
            private set
            {
                if (PanningTransform != null)
                {
                    PanningTransform.X = value;
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PanningLayer"/> class.
        /// </summary>
        public PanningLayer()
        {
            DefaultStyleKey = typeof(PanningLayer);
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            LocalTransform = GetTemplateChild(LocalTransformName) as TranslateTransform;
            PanningTransform = GetTemplateChild(PanningTransformName) as TranslateTransform;
            LeftWraparound = GetTemplateChild(LeftWraparoundName) as Border;
            RightWraparound = GetTemplateChild(RightWraparoundName) as Border;
            PanningStackPanel = GetTemplateChild(PanningStackPanelName) as StackPanel;
            ContentPresenter = GetTemplateChild(ContentPresenterName) as ContentPresenter;
            _animator = PanningTransform != null ? new TransformAnimator(PanningTransform) : null;
            UpdateWrappingRectangles();
        }

        /// <summary>
        /// Animates the layer to an offset expressed in items-layer pixels.
        /// </summary>
        /// <param name="targetOffset">The target offset.</param>
        /// <param name="duration">The animation duration.</param>
        public void GoTo(int targetOffset, Duration duration)
        {
            if (_animator != null && !IsStatic)
            {
                _animator.GoTo((int)(targetOffset * PanRate), duration, _easingFunction, null);
            }
        }

        /// <summary>
        /// Shifts the layer by one content width so wrapping can continue.
        /// </summary>
        /// <param name="direction">Negative for forward, positive for backward wrapping.</param>
        public virtual void Wraparound(int direction)
        {
            double width = ContentPresenter != null ? ContentPresenter.ActualWidth : 0.0;
            if (direction < 0)
            {
                GoTo((int)((ActualOffset + width) / PanRate), Immediately);
            }
            else
            {
                GoTo((int)((ActualOffset - width) / PanRate), Immediately);
            }
        }

        internal void Refresh()
        {
            UpdateWrappingRectangles();
        }

        protected virtual void UpdateWrappingRectangles()
        {
            Thickness margin = ContentPresenter != null ? ContentPresenter.Margin : new Thickness(0.0);
            UpdateWrappingRectanglesInternal(margin, margin);
        }

        internal void UpdateWrappingRectanglesInternal(Thickness leftWraparoundMargin, Thickness rightWraparoundMargin)
        {
            UpdateWrappingBorder(LeftWraparound, leftWraparoundMargin);
            UpdateWrappingBorder(RightWraparound, rightWraparoundMargin);
            if (LeftWraparound != null && LocalTransform != null)
            {
                double offset = LeftWraparound.Visibility == Visibility.Visible
                    ? 0.0 - LeftWraparound.Width - LeftWraparound.Margin.Left
                    : 0.0;
                LocalTransform.X = IsStatic ? 0.0 : offset - LeftWraparound.Margin.Right;
            }
        }

        private void UpdateWrappingBorder(Border wraparoundBorder, Thickness margin)
        {
            if (wraparoundBorder == null)
            {
                return;
            }

            wraparoundBorder.Visibility = ShouldShowWrappers ? Visibility.Visible : Visibility.Collapsed;
            if (!ShouldShowWrappers)
            {
                wraparoundBorder.Background = null;
                return;
            }

            wraparoundBorder.Height = ContentPresenter != null ? ContentPresenter.ActualHeight : 0.0;
            wraparoundBorder.Width = ContentPresenter != null ? ContentPresenter.ActualWidth : 0.0;
            wraparoundBorder.Margin = margin;
            if (ContentPresenter != null && !(wraparoundBorder.Background is VisualBrush))
            {
                wraparoundBorder.Background = new VisualBrush
                {
                    Visual = ContentPresenter,
                    Stretch = Stretch.None,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top
                };
            }
        }

        private void OnContentSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateWrappingRectangles();
        }
    }
}
