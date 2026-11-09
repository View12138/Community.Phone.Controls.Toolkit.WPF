// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides the <see cref="PivotItem"/> for the pivot item lifecycle events.
    /// </summary>
    public class PivotItemEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PivotItemEventArgs"/> class.
        /// </summary>
        public PivotItemEventArgs()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PivotItemEventArgs"/> class.
        /// </summary>
        /// <param name="item">The pivot item the event applies to.</param>
        public PivotItemEventArgs(PivotItem item)
            : this()
        {
            Item = item;
        }

        /// <summary>
        /// Gets the pivot item the event applies to.
        /// </summary>
        public PivotItem Item { get; private set; }
    }
}
