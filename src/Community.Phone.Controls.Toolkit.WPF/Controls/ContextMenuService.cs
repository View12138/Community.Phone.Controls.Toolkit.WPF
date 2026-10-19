// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides an attached property that associates a
    /// <see cref="ContextMenu"/> with an element.
    /// </summary>
    public static class ContextMenuService
    {
        /// <summary>
        /// Identifies the ContextMenu attached property.
        /// </summary>
        public static readonly DependencyProperty ContextMenuProperty =
            DependencyProperty.RegisterAttached(
                "ContextMenu",
                typeof(ContextMenu),
                typeof(ContextMenuService),
                new PropertyMetadata(null, OnContextMenuChanged));

        /// <summary>
        /// Gets the context menu for an element.
        /// </summary>
        public static ContextMenu GetContextMenu(DependencyObject element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            return (ContextMenu)element.GetValue(ContextMenuProperty);
        }

        /// <summary>
        /// Sets the context menu for an element.
        /// </summary>
        public static void SetContextMenu(DependencyObject element, ContextMenu value)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            element.SetValue(ContextMenuProperty, value);
        }

        private static void OnContextMenuChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element == null)
            {
                return;
            }

            element.PreviewMouseRightButtonDown -= OnPreviewMouseRightButtonDown;
            if (e.NewValue is ContextMenu)
            {
                element.PreviewMouseRightButtonDown += OnPreviewMouseRightButtonDown;
            }
        }

        private static void OnPreviewMouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            ContextMenu menu = GetContextMenu(element);
            if (menu == null)
            {
                return;
            }

            menu.Owner = element;
            menu.PlacementTarget = element;
            menu.IsOpen = true;
            e.Handled = true;
        }
    }
}
