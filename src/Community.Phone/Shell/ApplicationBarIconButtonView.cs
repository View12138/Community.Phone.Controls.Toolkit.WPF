// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Renders an <see cref="ApplicationBarIconButton"/>: the circled icon above
    /// the lower-case label. The visuals are the control template in
    /// Themes/Controls/ApplicationBar.xaml; this class only supplies the text
    /// and the icon. The label starts at the default bar's bottom edge, so a
    /// collapsed bar clips it and only the icon shows.
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
        internal const double IconSize = 36;

        /// <summary>
        /// The height of the area the circle is centered in: the default bar
        /// height, so the label starts at the bar's bottom edge.
        /// </summary>
        internal const double IconAreaHeight = ApplicationBarPresenter.DefaultBarHeight;

        /// <summary>
        /// The height of the label row under the icon.
        /// </summary>
        internal const double LabelHeight = 18;

        /// <summary>
        /// The font size of the label under the icon.
        /// </summary>
        internal const double LabelFontSize = 12;

        /// <summary>
        /// Identifies the <see cref="IconSource"/> property.
        /// </summary>
        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register(
                "IconSource",
                typeof(ImageSource),
                typeof(ApplicationBarIconButtonView),
                new PropertyMetadata(null));

        /// <summary>
        /// Identifies the <see cref="Text"/> property.
        /// </summary>
        public static readonly DependencyProperty TextProperty =
            DependencyProperty.Register(
                "Text",
                typeof(string),
                typeof(ApplicationBarIconButtonView),
                new PropertyMetadata(null));

        static ApplicationBarIconButtonView()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ApplicationBarIconButtonView),
                new FrameworkPropertyMetadata(typeof(ApplicationBarIconButtonView)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarIconButtonView"/> class.
        /// </summary>
        /// <param name="container">The container that tracks the icon button.</param>
        internal ApplicationBarIconButtonView(ApplicationBarItemContainer container)
            : base(container)
        {
        }

        /// <summary>
        /// Gets or sets the image shown inside the circle.
        /// </summary>
        public ImageSource IconSource
        {
            get { return (ImageSource)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the label under the icon.
        /// </summary>
        public string Text
        {
            get { return (string)GetValue(TextProperty); }
            set { SetValue(TextProperty, value); }
        }

        /// <inheritdoc />
        protected override void UpdateContent()
        {
            Text = Container.Text;
            IconSource = ApplicationBarIconLoader.Load(Container.IconUri);
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
            ImageSource image = LoadFrom(iconUri, false);
            if (image == null)
            {
                // The toolkit pages reference their icons the Windows Phone way
                // ("/Toolkit.Content/..."), which expects the app to ship them.
                // The port keeps those icons in the toolkit assembly, so a
                // missing app resource falls back to the packaged copy.
                image = LoadFrom(iconUri, true);
            }

            return image;
        }

        private static ImageSource LoadFrom(Uri iconUri, bool toolkitContent)
        {
            try
            {
                Uri source = toolkitContent ? ResolveToolkitContent(iconUri) : Resolve(iconUri);
                if (source == null)
                {
                    return null;
                }

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

        private static Uri Resolve(Uri iconUri)
        {
            return iconUri.IsAbsoluteUri
                ? iconUri
                : new Uri("pack://application:,,,/" + iconUri.OriginalString.TrimStart('/'), UriKind.Absolute);
        }

        private static Uri ResolveToolkitContent(Uri iconUri)
        {
            string fileName = Path.GetFileName(iconUri.OriginalString);
            if (string.IsNullOrEmpty(fileName))
            {
                return null;
            }

            return new Uri(
                "pack://application:,,,/Community.Phone.Controls.Toolkit;component/Content/" + fileName,
                UriKind.Absolute);
        }
    }
}
