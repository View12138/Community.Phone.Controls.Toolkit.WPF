// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Renders an <see cref="ApplicationBarIconButton"/>: the 48x48 icon above
    /// the lower-case label, inside a 96 pixel wide slot.
    /// </summary>
    internal class ApplicationBarIconButtonView : ApplicationBarItemView
    {
        /// <summary>
        /// The width of one icon button slot.
        /// </summary>
        internal const double SlotWidth = 96;

        /// <summary>
        /// The size of the circular button that hosts the icon.
        /// </summary>
        internal const double IconSize = 48;

        /// <summary>
        /// The size of the glyph inside the circular button.
        /// </summary>
        internal const double GlyphSize = 32;

        /// <summary>
        /// The thickness of the circle around the icon.
        /// </summary>
        internal const double CircleThickness = 2;

        /// <summary>
        /// The font size of the label under the icon.
        /// </summary>
        internal const double LabelFontSize = 12;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarIconButtonView"/> class.
        /// </summary>
        /// <param name="container">The container that tracks the icon button.</param>
        internal ApplicationBarIconButtonView(ApplicationBarItemContainer container)
            : base(container)
        {
            Width = SlotWidth;
            HorizontalAlignment = HorizontalAlignment.Right;
        }

        /// <inheritdoc />
        protected override void UpdateContent()
        {
            // Leave room above the circle so the label sits below it instead of
            // overlapping its lower edge (4 + 48 + 20 = 72).
            Grid grid = new Grid();
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(IconSize + 4) });
            grid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

            // Windows Phone draws a circular outline around every application
            // bar icon; the icon itself is only the glyph inside it.
            Border circle = new Border
            {
                Width = IconSize,
                Height = IconSize,
                Margin = new Thickness(0, 4, 0, 0),
                CornerRadius = new CornerRadius(IconSize / 2),
                BorderThickness = new Thickness(CircleThickness),
                BorderBrush = ForegroundBrush,
                Background = Brushes.Transparent,
                IsHitTestVisible = false,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };
            grid.Children.Add(circle);

            Image icon = new Image
            {
                Width = GlyphSize,
                Height = GlyphSize,
                Margin = new Thickness(0, 4, 0, 0),
                Stretch = Stretch.Uniform,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                Source = ApplicationBarIconLoader.Load(Container.IconUri),
            };

            RenderOptions.SetBitmapScalingMode(icon, BitmapScalingMode.HighQuality);
            grid.Children.Add(icon);

            TextBlock label = new TextBlock
            {
                Text = Container.Text,
                FontSize = LabelFontSize,
                FontFamily = ApplicationBarPresenter.GetDefaultFontFamily(),
                Foreground = ForegroundBrush,
                TextAlignment = TextAlignment.Center,
                TextTrimming = TextTrimming.CharacterEllipsis,
                TextWrapping = TextWrapping.NoWrap,
                Margin = new Thickness(2, 1, 2, 2),
                VerticalAlignment = VerticalAlignment.Top,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };

            Grid.SetRow(label, 1);
            grid.Children.Add(label);

            Child = grid;
        }
    }

    /// <summary>
    /// Loads the icon of an application bar item. A missing or unreadable icon
    /// is not an error: the button is rendered without an image, so a page that
    /// references a resource the app does not ship still works. Results are
    /// cached per URI because the bar is rebuilt whenever it changes.
    /// </summary>
    internal static class ApplicationBarIconLoader
    {
        private static readonly Dictionary<Uri, ImageSource> Cache = new Dictionary<Uri, ImageSource>();

        private static readonly object CacheLock = new object();

        /// <summary>
        /// Loads an icon.
        /// </summary>
        /// <param name="iconUri">The icon URI, absolute or application relative.</param>
        /// <returns>The loaded image, or <c>null</c>.</returns>
        internal static ImageSource Load(Uri iconUri)
        {
            if (iconUri == null)
            {
                return null;
            }

            ImageSource cached;
            lock (CacheLock)
            {
                if (Cache.TryGetValue(iconUri, out cached))
                {
                    return cached;
                }
            }

            ImageSource image = LoadCore(iconUri);
            lock (CacheLock)
            {
                Cache[iconUri] = image;
            }

            return image;
        }

        private static ImageSource LoadCore(Uri iconUri)
        {
            try
            {
                Uri source = iconUri.IsAbsoluteUri
                    ? iconUri
                    : new Uri("pack://application:,,,/" + iconUri.OriginalString.TrimStart('/'), UriKind.Absolute);

                if (source.IsFile && !File.Exists(source.LocalPath))
                {
                    return null;
                }

                BitmapImage image = new BitmapImage();
                image.BeginInit();
                image.UriSource = source;
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.EndInit();
                image.Freeze();
                return image;
            }
            catch (Exception)
            {
                return null;
            }
        }
    }
}
