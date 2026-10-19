// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides turnstile feather transitions.
    /// </summary>
    public class TurnstileFeatherTransition : TransitionElement
    {
        /// <summary>
        /// Identifies the <see cref="Mode"/> property.
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(TurnstileFeatherTransitionMode), typeof(TurnstileFeatherTransition), null);

        /// <summary>
        /// Identifies the <see cref="BeginTime"/> property.
        /// </summary>
        public static readonly DependencyProperty BeginTimeProperty =
            DependencyProperty.Register("BeginTime", typeof(TimeSpan?), typeof(TurnstileFeatherTransition), new PropertyMetadata(TimeSpan.Zero));

        /// <summary>
        /// Gets or sets the transition mode.
        /// </summary>
        public TurnstileFeatherTransitionMode Mode
        {
            get { return (TurnstileFeatherTransitionMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        /// <summary>
        /// Gets or sets the time at which the transition should begin.
        /// </summary>
        public TimeSpan? BeginTime
        {
            get { return (TimeSpan?)GetValue(BeginTimeProperty); }
            set { SetValue(BeginTimeProperty, value); }
        }

        /// <inheritdoc />
        public override ITransition GetTransition(UIElement element)
        {
            return Transitions.TurnstileFeather(element, Mode, BeginTime);
        }
    }
}
