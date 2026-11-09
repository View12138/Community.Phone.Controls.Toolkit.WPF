// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A Metro-styled context menu. This type derives from the WPF
    /// <see cref="System.Windows.Controls.ContextMenu"/> so that standard
    /// popup, keyboard, and automation behavior is preserved.
    /// </summary>
    public class ContextMenu : MenuBase
    {
        /// <summary>
        /// Identifies the <see cref="IsZoomEnabled"/> property.
        /// </summary>
        public static readonly DependencyProperty IsZoomEnabledProperty =
            DependencyProperty.Register("IsZoomEnabled", typeof(bool), typeof(ContextMenu), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="IsFadeEnabled"/> property.
        /// </summary>
        public static readonly DependencyProperty IsFadeEnabledProperty =
            DependencyProperty.Register("IsFadeEnabled", typeof(bool), typeof(ContextMenu), new PropertyMetadata(true));

        /// <summary>
        /// Identifies the <see cref="RegionOfInterest"/> property.
        /// </summary>
        public static readonly DependencyProperty RegionOfInterestProperty =
            DependencyProperty.Register("RegionOfInterest", typeof(Rect?), typeof(ContextMenu), new PropertyMetadata(null));

        /// <summary>
        /// Gets or sets the owner of the context menu.
        /// </summary>
        public DependencyObject Owner { get; internal set; }

        /// <summary>
        /// Gets or sets whether the zoom animation is enabled.
        /// </summary>
        public bool IsZoomEnabled
        {
            get { return (bool)GetValue(IsZoomEnabledProperty); }
            set { SetValue(IsZoomEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets whether the fade animation is enabled.
        /// </summary>
        public bool IsFadeEnabled
        {
            get { return (bool)GetValue(IsFadeEnabledProperty); }
            set { SetValue(IsFadeEnabledProperty, value); }
        }

        /// <summary>
        /// Gets or sets the region of interest used to position the menu.
        /// </summary>
        public Rect? RegionOfInterest
        {
            get { return (Rect?)GetValue(RegionOfInterestProperty); }
            set { SetValue(RegionOfInterestProperty, value); }
        }
    }

    /// <summary>
    /// Base class for phone menus.
    /// </summary>
    public class MenuBase : System.Windows.Controls.ContextMenu
    {
        /// <inheritdoc />
        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is System.Windows.Controls.MenuItem
                || item is Separator
                || item is System.Windows.Controls.Separator;
        }
    }

    /// <summary>
    /// A menu item for a phone context menu.
    /// </summary>
    public class MenuItem : System.Windows.Controls.MenuItem
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="MenuItem"/> class.
        /// </summary>
        public MenuItem()
        {
        }
    }

    /// <summary>
    /// A separator for a phone context menu.
    /// </summary>
    public class Separator : System.Windows.Controls.Separator
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Separator"/> class.
        /// </summary>
        public Separator()
        {
        }
    }
}
