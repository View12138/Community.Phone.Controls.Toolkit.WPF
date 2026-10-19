// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides data for the <see cref="PhoneApplicationFrame.OrientationChanged"/>
    /// event.
    /// </summary>
    public class OrientationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Initializes a new instance of the
        /// <see cref="OrientationChangedEventArgs"/> class.
        /// </summary>
        /// <param name="orientation">The new orientation.</param>
        public OrientationChangedEventArgs(PageOrientation orientation)
        {
            Orientation = orientation;
        }

        /// <summary>
        /// Gets the new orientation.
        /// </summary>
        public PageOrientation Orientation { get; private set; }
    }
}
