// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Represents a single page inside a <see cref="Panorama"/>.
    /// </summary>
    public class PanoramaItem : ContentControl
    {
        /// <summary>
        /// Identifies the <see cref="Header"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderProperty = DependencyProperty.Register(
            "Header", typeof(object), typeof(PanoramaItem), null);

        /// <summary>
        /// Identifies the <see cref="HeaderTemplate"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty HeaderTemplateProperty = DependencyProperty.Register(
            "HeaderTemplate", typeof(DataTemplate), typeof(PanoramaItem), null);

        /// <summary>
        /// Identifies the <see cref="Orientation"/> dependency property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register(
            "Orientation", typeof(Orientation), typeof(PanoramaItem), new PropertyMetadata(Orientation.Vertical, OnOrientationChanged));

        internal int StartPosition { get; set; }

        internal int ItemWidth { get; set; }

        /// <summary>
        /// Gets or sets the content displayed as the item's header.
        /// </summary>
        public object Header
        {
            get { return GetValue(HeaderProperty); }
            set { SetValue(HeaderProperty, value); }
        }

        /// <summary>
        /// Gets or sets the template used to display <see cref="Header"/>.
        /// </summary>
        public DataTemplate HeaderTemplate
        {
            get { return (DataTemplate)GetValue(HeaderTemplateProperty); }
            set { SetValue(HeaderTemplateProperty, value); }
        }

        /// <summary>
        /// Gets or sets the direction in which the item's content is laid out.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PanoramaItem"/> class.
        /// </summary>
        public PanoramaItem()
        {
            DefaultStyleKey = typeof(PanoramaItem);
            EnsureHasTranslateTransform();
        }

        internal void EnsureHasTranslateTransform()
        {
            if (!(RenderTransform is TranslateTransform))
            {
                RenderTransform = new TranslateTransform();
            }
        }

        private static void OnOrientationChanged(DependencyObject obj, DependencyPropertyChangedEventArgs args)
        {
            PanoramaItem panoramaItem = (PanoramaItem)obj;
            panoramaItem.InvalidateMeasure();
            if (VisualTreeHelper.GetParent(panoramaItem) is FrameworkElement frameworkElement)
            {
                frameworkElement.InvalidateMeasure();
            }
        }
    }
}
