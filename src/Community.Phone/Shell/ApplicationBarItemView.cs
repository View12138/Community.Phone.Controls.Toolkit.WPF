// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// The shared behaviour of one application bar item: the enabled state, the
    /// click and the keyboard activation. The visuals come from the control
    /// template in Themes/Controls/ApplicationBar.xaml; a derived view only
    /// pushes its data into the dependency properties the template binds to.
    /// </summary>
    internal abstract class ApplicationBarItemView : Control
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarItemView"/> class.
        /// </summary>
        /// <param name="container">The container that tracks the item.</param>
        protected ApplicationBarItemView(ApplicationBarItemContainer container)
        {
            Container = container;

            KeyDown += OnKeyDown;
            MouseLeftButtonUp += OnMouseLeftButtonUp;

            Refresh();
        }

        /// <summary>
        /// Occurs after the item was activated by the user.
        /// </summary>
        internal event EventHandler Activated;

        /// <summary>
        /// Gets the container that tracks the item.
        /// </summary>
        internal ApplicationBarItemContainer Container { get; }

        /// <summary>
        /// Applies the current container state to the view.
        /// </summary>
        internal virtual void Refresh()
        {
            IsEnabled = Container.IsEnabled;
            AutomationProperties.SetName(this, Container.Text ?? string.Empty);
            UpdateContent();
        }

        /// <summary>
        /// Pushes the container state into the properties the template binds to.
        /// </summary>
        protected abstract void UpdateContent();

        /// <summary>
        /// Called after the item was activated by the user.
        /// </summary>
        protected virtual void OnActivated()
        {
        }

        /// <summary>
        /// Activates the item, as a mouse click or the keyboard would.
        /// </summary>
        internal void Activate()
        {
            if (!Container.IsEnabled)
            {
                return;
            }

            Container.ClickEvent();
            OnActivated();

            EventHandler handler = Activated;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            Activate();
            e.Handled = true;
        }

        private void OnKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                Activate();
                e.Handled = true;
            }
        }
    }
}
