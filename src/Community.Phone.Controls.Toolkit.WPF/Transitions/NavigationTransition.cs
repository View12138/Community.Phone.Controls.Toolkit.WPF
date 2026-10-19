// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Has <see cref="TransitionElement"/>s for forward and backward navigation.
    /// </summary>
    public class NavigationTransition : DependencyObject
    {
        /// <summary>
        /// Identifies the <see cref="Backward"/> property.
        /// </summary>
        public static readonly DependencyProperty BackwardProperty =
            DependencyProperty.Register("Backward", typeof(TransitionElement), typeof(NavigationTransition), null);

        /// <summary>
        /// Identifies the <see cref="Forward"/> property.
        /// </summary>
        public static readonly DependencyProperty ForwardProperty =
            DependencyProperty.Register("Forward", typeof(TransitionElement), typeof(NavigationTransition), null);

        /// <summary>
        /// Occurs when the navigation transition begins.
        /// </summary>
        public event RoutedEventHandler BeginTransition;

        /// <summary>
        /// Occurs when the navigation transition ends.
        /// </summary>
        public event RoutedEventHandler EndTransition;

        /// <summary>
        /// Gets or sets the backward transition.
        /// </summary>
        public TransitionElement Backward
        {
            get { return (TransitionElement)GetValue(BackwardProperty); }
            set { SetValue(BackwardProperty, value); }
        }

        /// <summary>
        /// Gets or sets the forward transition.
        /// </summary>
        public TransitionElement Forward
        {
            get { return (TransitionElement)GetValue(ForwardProperty); }
            set { SetValue(ForwardProperty, value); }
        }

        internal void OnBeginTransition()
        {
            RoutedEventHandler handler = BeginTransition;
            if (handler != null)
            {
                handler(this, new RoutedEventArgs());
            }
        }

        internal void OnEndTransition()
        {
            RoutedEventHandler handler = EndTransition;
            if (handler != null)
            {
                handler(this, new RoutedEventArgs());
            }
        }
    }
}
