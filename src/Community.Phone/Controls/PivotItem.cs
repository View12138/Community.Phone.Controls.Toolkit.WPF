// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls.Primitives;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A single page of content inside a <see cref="Pivot"/>.
    /// </summary>
    /// <remarks>
    /// The original control is animated by the phone's compositor: the pivot
    /// moves the container and only uses the position states as markers. The
    /// port keeps the same contract — the states carry no storyboards in the
    /// default theme and <see cref="Pivot"/> animates the container itself —
    /// so a custom template that does add storyboards still receives the state
    /// transitions it expects.
    /// </remarks>
    [TemplateVisualState(Name = PivotStateLeft, GroupName = PivotPositionsGroupName)]
    [TemplateVisualState(Name = PivotStateCenter, GroupName = PivotPositionsGroupName)]
    [TemplateVisualState(Name = PivotStateRight, GroupName = PivotPositionsGroupName)]
    public class PivotItem : ContentControl
    {
        private const string PivotPositionsGroupName = "Position States";
        private const string PivotStateCenter = "Center";
        private const string PivotStateLeft = "Left";
        private const string PivotStateRight = "Right";

        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header",
            typeof(object),
            typeof(PivotItem),
            new PropertyMetadata(null));

        private AnimationDirection? _firstAnimation;
        private AnimationDirection _direction;

        /// <summary>
        /// Initializes a new instance of the <see cref="PivotItem"/> class.
        /// </summary>
        public PivotItem()
        {
            DefaultStyleKey = typeof(PivotItem);
        }

        /// <summary>
        /// Gets or sets the object that is shown in the header strip.
        /// </summary>
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <inheritdoc/>
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            // A direction requested before the template existed is deferred so
            // the state transition runs on a live visual tree.
            AnimationDirection? firstAnimation = _firstAnimation;
            MoveTo(AnimationDirection.Center);
            if (firstAnimation.HasValue)
            {
                MoveTo(firstAnimation.Value);
            }

            _firstAnimation = AnimationDirection.Center;
        }

        /// <summary>
        /// Moves the item to the given position, animating the transition when
        /// the direction is not <see cref="AnimationDirection.Center"/>.
        /// </summary>
        /// <param name="direction">The direction the item is moving in.</param>
        internal void MoveTo(AnimationDirection direction)
        {
            bool useTransitions = direction != AnimationDirection.Center;
            _direction = direction;

            if (!_firstAnimation.HasValue && useTransitions)
            {
                _firstAnimation = direction;
            }
            else
            {
                UpdateVisualStates(useTransitions);
            }
        }

        private void UpdateVisualStates(bool useTransitions)
        {
            VisualStateManager.GoToState(this, StateNameFromAnimationDirection(_direction), useTransitions);
        }

        private static string StateNameFromAnimationDirection(AnimationDirection direction)
        {
            switch (direction)
            {
                case AnimationDirection.Center:
                    return PivotStateCenter;

                case AnimationDirection.Left:
                    return PivotStateLeft;

                case AnimationDirection.Right:
                    return PivotStateRight;

                default:
                    throw new InvalidOperationException("Unknown animation direction.");
            }
        }
    }
}
