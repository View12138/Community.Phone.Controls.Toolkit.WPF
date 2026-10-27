// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Describes a menu item that can appear in the application bar.
    /// </summary>
    public interface IApplicationBarMenuItem
    {
        /// <summary>
        /// Gets or sets a value indicating whether the item is enabled.
        /// </summary>
        bool IsEnabled { get; set; }

        /// <summary>
        /// Gets or sets the text of the item.
        /// </summary>
        string Text { get; set; }

        /// <summary>
        /// Occurs when the item is clicked.
        /// </summary>
        event EventHandler Click;
    }
}
