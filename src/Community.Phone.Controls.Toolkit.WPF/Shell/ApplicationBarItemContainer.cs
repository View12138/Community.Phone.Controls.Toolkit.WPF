// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// The kind of element an application bar item is rendered as.
    /// </summary>
    internal enum ApplicationBarItemType
    {
        /// <summary>
        /// An icon button in the application bar.
        /// </summary>
        ImageButton,

        /// <summary>
        /// A text item in the application bar menu.
        /// </summary>
        TextItem,
    }

    /// <summary>
    /// Validates and tracks the state of one application bar item. In the
    /// original this is the bridge to the native app bar; in the WPF port it
    /// is the bridge to the rendered presenter.
    /// </summary>
    internal class ApplicationBarItemContainer
    {
        /// <summary>
        /// The maximum length of an item text.
        /// </summary>
        internal static readonly int MaxTextLength = 255;

        private readonly IApplicationBarMenuItem _item;
        private readonly ApplicationBarItemType _type;

        private string _text;
        private bool _isEnabled;
        private Uri _iconUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarItemContainer"/> class.
        /// </summary>
        /// <param name="item">The item being tracked.</param>
        /// <param name="type">The rendered type of the item.</param>
        internal ApplicationBarItemContainer(IApplicationBarMenuItem item, ApplicationBarItemType type)
        {
            _item = item;
            _type = type;
        }

        /// <summary>
        /// Occurs when the validated state of the item changed.
        /// </summary>
        internal event EventHandler Changed;

        /// <summary>
        /// Gets the item being tracked.
        /// </summary>
        internal IApplicationBarMenuItem Item
        {
            get { return _item; }
        }

        /// <summary>
        /// Gets the rendered type of the item.
        /// </summary>
        internal ApplicationBarItemType Type
        {
            get { return _type; }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the item is enabled.
        /// </summary>
        internal bool IsEnabled
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
                    OnChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the text of the item. The original validates the text
        /// in the container (and stores the trimmed value), so invalid text
        /// throws as soon as the item belongs to an application bar.
        /// </summary>
        internal string Text
        {
            get
            {
                return _text;
            }

            set
            {
                if (string.IsNullOrEmpty(value))
                {
                    throw new InvalidOperationException("Text cannot be empty.");
                }

                if (value.Contains("\n") || value.Contains("\r"))
                {
                    throw new InvalidOperationException("Text cannot contain multiple lines");
                }

                string trimmed = value.Trim();
                if (trimmed.Length == 0)
                {
                    throw new InvalidOperationException("Text cannot be only white space.");
                }

                if (!NullSafeCompare(_text, trimmed))
                {
                    if (trimmed.Length > MaxTextLength)
                    {
                        throw new ArgumentOutOfRangeException("value", "Text cannot be longer than maximum allowed length.");
                    }

                    _text = trimmed;
                    OnChanged();
                }
            }
        }

        /// <summary>
        /// Gets or sets the icon of the item. An icon cannot be cleared while
        /// the item belongs to an application bar.
        /// </summary>
        internal Uri IconUri
        {
            get
            {
                return _iconUri;
            }

            set
            {
                if (value == null)
                {
                    throw new ArgumentNullException("value", "Cannot clear the icon while in a list");
                }

                if (!NullSafeCompare(value, _iconUri))
                {
                    _iconUri = value;
                    OnChanged();
                }
            }
        }

        /// <summary>
        /// Compares two values, treating two nulls as equal.
        /// </summary>
        /// <param name="a">The first value.</param>
        /// <param name="b">The second value.</param>
        /// <returns><c>true</c> when both values are equal.</returns>
        internal static bool NullSafeCompare(object a, object b)
        {
            if (a == null && b == null)
            {
                return true;
            }

            if (a != null && b != null)
            {
                return a.Equals(b);
            }

            return false;
        }

        /// <summary>
        /// Raises an event handler on the UI thread, like the original.
        /// </summary>
        /// <param name="handler">The handler to raise.</param>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The event arguments.</param>
        internal static void FireEventHandler(EventHandler handler, object sender, EventArgs args)
        {
            if (handler == null)
            {
                return;
            }

            Dispatcher dispatcher = Application.Current != null
                ? Application.Current.Dispatcher
                : Dispatcher.CurrentDispatcher;

            if (dispatcher != null && !dispatcher.CheckAccess())
            {
                dispatcher.BeginInvoke((Action)(() => handler(sender, args)));
                return;
            }

            handler(sender, args);
        }

        /// <summary>
        /// Starts tracking the item.
        /// </summary>
        internal virtual void Attach()
        {
            Subscribe();
            HandleItemChanged(null, new BoolEventArgs());
        }

        /// <summary>
        /// Stops tracking the item.
        /// </summary>
        internal virtual void Detach()
        {
            Unsubscribe();
        }

        /// <summary>
        /// Raises the click of the tracked item.
        /// </summary>
        internal virtual void ClickEvent()
        {
        }

        /// <summary>
        /// Applies the item properties to the container.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The validation result.</param>
        protected virtual void UpdateProperties(object sender, BoolEventArgs args)
        {
            IsEnabled = _item.IsEnabled;
            Text = _item.Text;
            if (args != null)
            {
                args.Success = true;
            }
        }

        /// <summary>
        /// Handles a change of the tracked item.
        /// </summary>
        /// <param name="sender">The event sender.</param>
        /// <param name="args">The validation result.</param>
        protected void HandleItemChanged(object sender, BoolEventArgs args)
        {
            UpdateProperties(sender, args);
        }

        /// <summary>
        /// Subscribes to the concrete item's change notification.
        /// </summary>
        protected virtual void Subscribe()
        {
        }

        /// <summary>
        /// Unsubscribes from the concrete item's change notification.
        /// </summary>
        protected virtual void Unsubscribe()
        {
        }

        private void OnChanged()
        {
            EventHandler handler = Changed;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }
}
