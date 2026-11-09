// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Tracks an item that is rendered as a text row in the application bar
    /// menu. Like the original, an icon button added to the menu item list is
    /// rendered as a text row.
    /// </summary>
    internal class ApplicationBarMenuItemContainer : ApplicationBarItemContainer
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarMenuItemContainer"/> class.
        /// </summary>
        /// <param name="item">The menu item.</param>
        internal ApplicationBarMenuItemContainer(IApplicationBarMenuItem item)
            : base(item, ApplicationBarItemType.TextItem)
        {
        }

        /// <inheritdoc />
        internal override void ClickEvent()
        {
            ApplicationBarIconButton iconButton = Item as ApplicationBarIconButton;
            if (iconButton != null)
            {
                iconButton.ClickEvent();
                return;
            }

            ApplicationBarMenuItem menuItem = Item as ApplicationBarMenuItem;
            if (menuItem != null)
            {
                menuItem.ClickEvent();
                return;
            }

            throw new InvalidOperationException("ApplicationBar menu item type not supported");
        }

        /// <inheritdoc />
        protected override void Subscribe()
        {
            ApplicationBarIconButton iconButton = Item as ApplicationBarIconButton;
            if (iconButton != null)
            {
                iconButton.Changed += HandleItemChanged;
                return;
            }

            ApplicationBarMenuItem menuItem = Item as ApplicationBarMenuItem;
            if (menuItem == null)
            {
                throw new InvalidOperationException("ApplicationBar menu item type not supported");
            }

            menuItem.Changed += HandleItemChanged;
        }

        /// <inheritdoc />
        protected override void Unsubscribe()
        {
            ApplicationBarIconButton iconButton = Item as ApplicationBarIconButton;
            if (iconButton != null)
            {
                iconButton.Changed -= HandleItemChanged;
                return;
            }

            ApplicationBarMenuItem menuItem = Item as ApplicationBarMenuItem;
            if (menuItem == null)
            {
                throw new InvalidOperationException("ApplicationBar menu item type not supported");
            }

            menuItem.Changed -= HandleItemChanged;
        }
    }
}
