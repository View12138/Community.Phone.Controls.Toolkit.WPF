// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides swivel transitions.
    /// </summary>
    public class SwivelTransition : TransitionElement
    {
        /// <summary>
        /// Identifies the <see cref="Mode"/> property.
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(SwivelTransitionMode), typeof(SwivelTransition), null);

        /// <summary>
        /// Gets or sets the transition mode.
        /// </summary>
        public SwivelTransitionMode Mode
        {
            get { return (SwivelTransitionMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        /// <inheritdoc />
        public override ITransition GetTransition(UIElement element)
        {
            return Transitions.Swivel(element, Mode);
        }
    }
}
