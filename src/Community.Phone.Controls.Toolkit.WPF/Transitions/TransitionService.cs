// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides attached properties for navigation transitions.
    /// </summary>
    public static class TransitionService
    {
        /// <summary>
        /// Identifies the NavigationInTransition attached property.
        /// </summary>
        public static readonly DependencyProperty NavigationInTransitionProperty =
            DependencyProperty.RegisterAttached("NavigationInTransition", typeof(NavigationInTransition), typeof(TransitionService), null);

        /// <summary>
        /// Identifies the NavigationOutTransition attached property.
        /// </summary>
        public static readonly DependencyProperty NavigationOutTransitionProperty =
            DependencyProperty.RegisterAttached("NavigationOutTransition", typeof(NavigationOutTransition), typeof(TransitionService), null);

        /// <summary>
        /// Gets the NavigationInTransition attached property.
        /// </summary>
        public static NavigationInTransition GetNavigationInTransition(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (NavigationInTransition)element.GetValue(NavigationInTransitionProperty);
        }

        /// <summary>
        /// Gets the NavigationOutTransition attached property.
        /// </summary>
        public static NavigationOutTransition GetNavigationOutTransition(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (NavigationOutTransition)element.GetValue(NavigationOutTransitionProperty);
        }

        /// <summary>
        /// Sets the NavigationInTransition attached property.
        /// </summary>
        public static void SetNavigationInTransition(UIElement element, NavigationInTransition value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(NavigationInTransitionProperty, value);
        }

        /// <summary>
        /// Sets the NavigationOutTransition attached property.
        /// </summary>
        public static void SetNavigationOutTransition(UIElement element, NavigationOutTransition value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(NavigationOutTransitionProperty, value);
        }
    }
}
