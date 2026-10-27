// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Creates the container that renders an item for a given list.
    /// </summary>
    internal static class ApplicationBarItemContainerFactory
    {
        /// <summary>
        /// Gets the container for an item.
        /// </summary>
        /// <param name="item">The item.</param>
        /// <param name="itemParentType">The kind of list the item belongs to.</param>
        /// <returns>The container that tracks the item.</returns>
        internal static ApplicationBarItemContainer GetApplicationBarItemContainer(
            IApplicationBarMenuItem item,
            ApplicationBarItemType itemParentType)
        {
            ApplicationBarIconButton iconButton = item as ApplicationBarIconButton;
            if (itemParentType != ApplicationBarItemType.TextItem && iconButton != null)
            {
                return new ApplicationBarIconButtonContainer(iconButton);
            }

            if (item is ApplicationBarMenuItem || iconButton != null)
            {
                return new ApplicationBarMenuItemContainer(item);
            }

            throw new InvalidOperationException("This list does not support that type");
        }
    }
}
