// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Provides the newly selected index for the header control.
    /// </summary>
    internal class SelectedIndexChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SelectedIndexChangedEventArgs"/> class.
        /// </summary>
        /// <param name="index">The index that was selected.</param>
        public SelectedIndexChangedEventArgs(int index)
        {
            SelectedIndex = index;
        }

        /// <summary>
        /// Gets the index that was selected.
        /// </summary>
        public int SelectedIndex { get; private set; }
    }
}
