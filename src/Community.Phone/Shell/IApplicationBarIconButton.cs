// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Describes an icon button that can appear in the application bar.
    /// </summary>
    public interface IApplicationBarIconButton : IApplicationBarMenuItem
    {
        /// <summary>
        /// Gets or sets the URI of the icon shown by the button.
        /// </summary>
        Uri IconUri { get; set; }
    }
}
