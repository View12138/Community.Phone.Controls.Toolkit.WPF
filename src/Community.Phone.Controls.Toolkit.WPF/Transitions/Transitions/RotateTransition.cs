// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides rotate transitions.
    /// </summary>
    public class RotateTransition : TransitionElement
    {
        /// <summary>
        /// Identifies the <see cref="Mode"/> property.
        /// </summary>
        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register("Mode", typeof(RotateTransitionMode), typeof(RotateTransition), null);

        /// <summary>
        /// Gets or sets the transition mode.
        /// </summary>
        public RotateTransitionMode Mode
        {
            get { return (RotateTransitionMode)GetValue(ModeProperty); }
            set { SetValue(ModeProperty, value); }
        }

        /// <inheritdoc />
        public override ITransition GetTransition(UIElement element)
        {
            return Transitions.Rotate(element, Mode);
        }
    }
}
