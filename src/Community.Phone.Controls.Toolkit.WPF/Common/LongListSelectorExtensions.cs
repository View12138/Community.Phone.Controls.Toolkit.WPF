// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides helper methods to work with LongListSelector.
    /// </summary>
    internal static class LongListSelectorExtensions
    {
        /// <summary>
        /// Gets the items that are currently in the viewport of a 
        /// <see cref="T:LongListSelector" /> and adds them into a list of weak references.
        /// </summary>
        /// <param name="list">
        /// The LongListSelector instance to search on.
        /// </param>
        /// <param name="items">
        /// The list of weak references where the items in 
        /// the viewport will be added.
        /// </param>
        public static void GetItemsInViewPort(this LongListSelector list, IList<WeakReference> items)
        {
            if (list == null)
            {
                return;
            }

            list.UpdateLayout();

            var itemsInList = new List<KeyValuePair<double, FrameworkElement>>();

            foreach (ContentPresenter container in list.GetRealizedContainers())
            {
                GeneralTransform itemTransform;
                try
                {
                    itemTransform = container.TransformToVisual(list);
                }
                catch (ArgumentException)
                {
                    // Ignore containers that are being recycled out of the tree.
                    continue;
                }

                Rect boundingBox = new Rect(
                    itemTransform.Transform(new Point()),
                    itemTransform.Transform(new Point(container.ActualWidth, container.ActualHeight)));

                if (boundingBox.Bottom <= 0 || boundingBox.Top >= list.ActualHeight)
                {
                    continue;
                }

                // Headers are projected through their content: applying a
                // projection to the header container itself blanks it (the
                // original control does the same on Windows Phone).
                FrameworkElement target = container;
                bool isHeader =
                    list.ItemsHost.ItemContainerGenerator.ItemFromContainer(container) is LongListSelectorItem entry &&
                    entry.ItemKind != LongListSelectorItemKind.Item;
                if (isHeader &&
                    VisualTreeHelper.GetChildrenCount(container) == 1 &&
                    VisualTreeHelper.GetChild(container, 0) is FrameworkElement content)
                {
                    target = content;
                }

                itemsInList.Add(new KeyValuePair<double, FrameworkElement>(boundingBox.Top, target));
            }

            foreach (var pair in itemsInList.OrderBy(selector => selector.Key))
            {
                items.Add(new WeakReference(pair.Value));
            }
        }
    }
}
