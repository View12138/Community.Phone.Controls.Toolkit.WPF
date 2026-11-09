// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System.Windows;
using System.Windows.Media;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Provides an attached property that clips a <see cref="FrameworkElement"/>
    /// to its bounds.
    /// </summary>
    public class ClipToBounds : DependencyObject
    {
        /// <summary>
        /// Identifies the IsEnabled attached property.
        /// </summary>
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(ClipToBounds), new PropertyMetadata(false, OnIsEnabledPropertyChanged));

        /// <summary>
        /// Gets the IsEnabled attached property.
        /// </summary>
        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        /// <summary>
        /// Sets the IsEnabled attached property.
        /// </summary>
        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        private static void OnIsEnabledPropertyChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement target = obj as FrameworkElement;
            if (target == null)
            {
                return;
            }

            if ((bool)e.NewValue)
            {
                target.SizeChanged += OnSizeChanged;
                UpdateClip(target);
            }
            else
            {
                target.SizeChanged -= OnSizeChanged;
                target.Clip = null;
            }
        }

        private static void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateClip(sender as FrameworkElement);
        }

        private static void UpdateClip(FrameworkElement element)
        {
            if (element != null)
            {
                element.Clip = new RectangleGeometry { Rect = new Rect(0, 0, element.ActualWidth, element.ActualHeight) };
            }
        }
    }
}
