// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Carries the result of an application bar item update. Mirrors the
    /// original interop argument: the item container validates the new value
    /// and reports back whether it could be applied.
    /// </summary>
    internal sealed class BoolEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the update was applied.
        /// </summary>
        public bool Success { get; set; }
    }
}
