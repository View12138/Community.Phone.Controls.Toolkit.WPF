// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides event data for cancellable routed property-changing events.
    /// </summary>
    public class RoutedPropertyChangingEventArgs<T> : RoutedEventArgs
    {
        private bool _cancel;

        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="RoutedPropertyChangingEventArgs{T}"/> class.
        /// </summary>
        public RoutedPropertyChangingEventArgs(DependencyProperty property, T oldValue, T newValue, bool isCancelable)
        {
            Property = property;
            OldValue = oldValue;
            NewValue = newValue;
            IsCancelable = isCancelable;
        }

        /// <summary>
        /// Gets the property that is changing.
        /// </summary>
        public DependencyProperty Property { get; private set; }

        /// <summary>
        /// Gets the old value.
        /// </summary>
        public T OldValue { get; private set; }

        /// <summary>
        /// Gets or sets the new value.
        /// </summary>
        public T NewValue { get; set; }

        /// <summary>
        /// Gets whether the change can be cancelled.
        /// </summary>
        public bool IsCancelable { get; private set; }

        /// <summary>
        /// Gets or sets whether the change is cancelled.
        /// </summary>
        public bool Cancel
        {
            get { return _cancel; }
            set
            {
                if (IsCancelable)
                {
                    _cancel = value;
                }
                else if (value)
                {
                    throw new InvalidOperationException("This property change is not cancellable.");
                }
            }
        }

        /// <summary>
        /// Gets or sets whether coercion is active.
        /// </summary>
        public bool InCoercion { get; set; }
    }

    /// <summary>
    /// Represents a handler for cancellable routed property-changing events.
    /// </summary>
    public delegate void RoutedPropertyChangingEventHandler<T>(object sender, RoutedPropertyChangingEventArgs<T> e);
}
