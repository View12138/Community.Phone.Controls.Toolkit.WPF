// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Represents an icon button in the application bar.
    /// </summary>
    public class ApplicationBarIconButton : IApplicationBarIconButton
    {
        private string _text;
        private bool _isEnabled;
        private Uri _iconUri;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarIconButton"/> class.
        /// </summary>
        public ApplicationBarIconButton()
            : this(null)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarIconButton"/> class.
        /// </summary>
        /// <param name="iconUri">The URI of the icon shown by the button.</param>
        public ApplicationBarIconButton(Uri iconUri)
        {
            IconUri = iconUri;
            IsEnabled = true;
        }

        /// <summary>
        /// Occurs when the button is clicked.
        /// </summary>
        public event EventHandler Click;

        /// <summary>
        /// Occurs when a property value changed, so the item container can
        /// validate and apply it.
        /// </summary>
        internal event EventHandler<BoolEventArgs> Changed;

        /// <summary>
        /// Gets or sets the URI of the icon shown by the button. The icon is
        /// required while the button belongs to an application bar.
        /// </summary>
        public Uri IconUri
        {
            get
            {
                return _iconUri;
            }

            set
            {
                if (!ApplicationBarItemContainer.NullSafeCompare(_iconUri, value))
                {
                    _iconUri = value;
                    UpdateOrRevert();
                }
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the button is enabled.
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
        /// Gets or sets the text shown under the icon.
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
