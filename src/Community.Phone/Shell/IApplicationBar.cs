// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Windows.Media;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Describes the application bar displayed at the bottom of a phone page.
    /// </summary>
    public interface IApplicationBar
    {
        /// <summary>
        /// Gets or sets a value indicating whether the application bar is visible.
        /// </summary>
        bool IsVisible { get; set; }

        /// <summary>
        /// Gets or sets the opacity of the application bar.
        /// </summary>
        double Opacity { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether the menu is available.
        /// </summary>
        bool IsMenuEnabled { get; set; }

        /// <summary>
        /// Gets or sets the background color of the application bar.
        /// </summary>
        Color BackgroundColor { get; set; }

        /// <summary>
        /// Gets or sets the foreground color of the application bar.
        /// </summary>
        Color ForegroundColor { get; set; }

        /// <summary>
        /// Gets or sets the display mode of the application bar.
        /// </summary>
        ApplicationBarMode Mode { get; set; }

        /// <summary>
        /// Gets the height of the application bar in the default mode.
        /// </summary>
        double DefaultSize { get; }

        /// <summary>
        /// Gets the height of the application bar in the minimized mode.
        /// </summary>
        double MiniSize { get; }

        /// <summary>
        /// Gets the icon buttons shown by the application bar.
        /// </summary>
        IList Buttons { get; }

        /// <summary>
        /// Gets the menu items shown by the application bar.
        /// </summary>
        IList MenuItems { get; }

        /// <summary>
        /// Occurs when the application bar menu opens or closes.
        /// </summary>
        event EventHandler<ApplicationBarStateChangedEventArgs> StateChanged;
    }
}
