// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Represents a text menu item in the application bar menu.
    /// </summary>
    public class ApplicationBarMenuItem : IApplicationBarMenuItem
    {
        private string _text;
        private bool _isEnabled;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarMenuItem"/> class.
        /// </summary>
        public ApplicationBarMenuItem()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarMenuItem"/> class.
        /// </summary>
        /// <param name="text">The text of the menu item.</param>
        public ApplicationBarMenuItem(string text)
        {
            Text = text;
            IsEnabled = true;
        }

        /// <summary>
        /// Occurs when the menu item is clicked.
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Occurs when a property value changed, so the item container can
        /// validate and apply it.
        /// </summary>
        internal event EventHandler<BoolEventArgs> Changed;

        /// <summary>
        /// Gets or sets a value indicating whether the menu item is enabled.
        /// </summary>
        public bool IsEnabled
        {
            get
            {
                return _isEnabled;
            }

            set
            {
                if (value != _isEnabled)
                {
                    _isEnabled = value;
                    UpdateOrRevert();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text of the menu item.
        /// </summary>
        public string Text
        {
            get
            {
                return _text;
            }

            set
            {
                if (!ApplicationBarItemContainer.NullSafeCompare(_text, value))
                {
                    _text = value;
                    UpdateOrRevert();
                }
            }
        }

        /// <summary>
        /// Raises the <see cref="Click"/> event.
        /// </summary>
        internal void ClickEvent()
        {
            ApplicationBarItemContainer.FireEventHandler(Click, this, EventArgs.Empty);
        }

        private void UpdateOrRevert()
        {
            EventHandler<BoolEventArgs> changed = Changed;
            if (changed != null)
            {
                changed(this, new BoolEventArgs());
            }
        }
    }
}
