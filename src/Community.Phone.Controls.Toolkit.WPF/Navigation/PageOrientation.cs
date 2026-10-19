// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// The orientation of a phone page. Flag values match the Windows Phone
    /// platform enumeration.
    /// </summary>
    [Flags]
    public enum PageOrientation
    {
        /// <summary>
        /// No orientation.
        /// </summary>
        None = 0,

        /// <summary>
        /// Landscape orientation.
        /// </summary>
        Landscape = 1,

        /// <summary>
        /// Portrait orientation.
        /// </summary>
        Portrait = 2,

        /// <summary>
        /// Landscape, left side up.
        /// </summary>
        LandscapeLeft = 4,

        /// <summary>
        /// Landscape, right side up.
        /// </summary>
        LandscapeRight = 8,

        /// <summary>
        /// Portrait, upright.
        /// </summary>
        PortraitUp = 16,

        /// <summary>
        /// Portrait, upside down.
        /// </summary>
        PortraitDown = 32,

        /// <summary>
        /// Either portrait or landscape.
        /// </summary>
        PortraitOrLandscape = PortraitUp | PortraitDown | LandscapeLeft | LandscapeRight
    }
}
