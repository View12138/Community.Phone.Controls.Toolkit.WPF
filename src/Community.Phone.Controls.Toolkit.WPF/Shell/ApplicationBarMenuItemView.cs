// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Renders an application bar menu item: a 48 pixel high text row.
    /// </summary>
    internal class ApplicationBarMenuItemView : ApplicationBarItemView
    {
        /// <summary>
        /// The height of one menu row.
        /// </summary>
        internal const double RowHeight = 48;

        /// <summary>
        /// The font size of the menu text.
        /// </summary>
        internal const double FontSize = 20;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarMenuItemView"/> class.
        /// </summary>
        /// <param name="container">The container that tracks the menu item.</param>
        internal ApplicationBarMenuItemView(ApplicationBarItemContainer container)
            : base(container)
        {
            Height = RowHeight;
            HorizontalAlignment = HorizontalAlignment.Stretch;
        }

        /// <inheritdoc />
        protected override void UpdateContent()
        {
            Child = new TextBlock
            {
                Text = Container.Text,
                FontSize = FontSize,
                FontFamily = ApplicationBarPresenter.GetDefaultFontFamily(),
                Foreground = ForegroundBrush,
                Margin = new Thickness(16, 0, 16, 0),
                VerticalAlignment = VerticalAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.NoWrap,
            };
        }
    }
}
