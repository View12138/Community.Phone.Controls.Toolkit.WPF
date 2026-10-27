// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics.CodeAnalysis;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Provides data for the <see cref="IApplicationBar.StateChanged"/> event.
    /// </summary>
    [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", Justification = "Args is correct")]
    public class ApplicationBarStateChangedEventArgs : EventArgs
    {
        private readonly bool _isMenuVisible;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarStateChangedEventArgs"/> class.
        /// </summary>
        /// <param name="isMenuVisible">Whether the menu is visible.</param>
        public ApplicationBarStateChangedEventArgs(bool isMenuVisible)
        {
            _isMenuVisible = isMenuVisible;
        }

        /// <summary>
        /// Gets a value indicating whether the application bar menu is visible.
        /// </summary>
        public bool IsMenuVisible
        {
            get { return _isMenuVisible; }
        }
    }
}
