// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Lays out <see cref="PanoramaItem"/> pages in a horizontal strip and
    /// supplies the snap positions used by <see cref="Panorama"/>.
    /// </summary>
    public class PanoramaPanel : Panel
    {
        internal class ItemStop
        {
            public int Index { get; private set; }

            public int Position { get; private set; }

            public PanoramaItem Item { get; private set; }

            public ItemStop(PanoramaItem item, int index, int position)
            {
                Item = item;
                Index = index;
                Position = position;
            }
        }

        private Panorama _owner;
        private readonly List<PanoramaItem> _visibleChildren = new List<PanoramaItem>();
        private readonly List<ItemStop> _itemStops = new List<ItemStop>();
        private PanoramaItem _selectedItem;

        internal IList<PanoramaItem> VisibleChildren
        {
            get { return _visibleChildren; }
        }

        private Panorama Owner
        {
            get { return _owner; }
            set
            {
                if (_owner != value)
                {
                    if (_owner != null)
                    {
                        _owner.Panel = null;
                    }
                    _owner = value;
                    if (_owner != null)
                    {
                        _owner.Panel = this;
                    }
                }
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="PanoramaPanel"/> class.
        /// </summary>
        public PanoramaPanel()
        {
            SizeChanged += OnSizeChanged;
            Unloaded += OnUnloaded;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            Owner = null;
        }

        private void OnSizeChanged(object sender, SizeChangedEventArgs e)
        {
            if (Owner != null)
            {
                Owner.ItemsWidth = (int)e.NewSize.Width;
            }
        }

        /// <inheritdoc/>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (_owner == null)
            {
                FindOwner();
            }

            int defaultItemIndex = GetDefaultItemIndex();
            int itemWidth = Owner != null ? Owner.AdjustedViewportWidth : 0;
            int height = Owner != null ? (int)Math.Min(availableSize.Height, Owner.ViewportHeight) : 0;
            Size result = new Size(0.0, height);
            Size verticalSize = new Size(itemWidth, height);
            Size horizontalSize = new Size(double.PositiveInfinity, height);
            int count = Children.Count;
            _visibleChildren.Clear();

            for (int i = 0; i < count; i++)
            {
                int index = (i + defaultItemIndex) % count;
                PanoramaItem panoramaItem = Children[index] as PanoramaItem;
                if (panoramaItem == null)
                {
                    continue;
                }

                if (panoramaItem.Visibility == Visibility.Visible)
                {
                    panoramaItem.Measure(panoramaItem.Orientation == Orientation.Vertical ? verticalSize : horizontalSize);
                    _visibleChildren.Add(panoramaItem);
                    result.Width += panoramaItem.Orientation == Orientation.Vertical
                        ? itemWidth
                        : Math.Max(itemWidth, panoramaItem.DesiredSize.Width);
                }
            }

            return result;
        }

        /// <inheritdoc/>
        protected override Size ArrangeOverride(Size finalSize)
        {
            _itemStops.Clear();
            double position = 0.0;
            Rect finalRect = new Rect(0.0, 0.0, 0.0, finalSize.Height);

            for (int i = 0; i < _visibleChildren.Count; i++)
            {
                PanoramaItem panoramaItem = _visibleChildren[i];
                int startPosition = (panoramaItem.StartPosition = (int)position);
                finalRect.X = startPosition;
                _itemStops.Add(new ItemStop(panoramaItem, i, panoramaItem.StartPosition));

                if (panoramaItem.Orientation == Orientation.Vertical)
                {
                    finalRect.Width = Owner != null ? Owner.AdjustedViewportWidth : 0;
                }
                else
                {
                    finalRect.Width = Math.Max(Owner != null ? Owner.AdjustedViewportWidth : 0, panoramaItem.DesiredSize.Width);
                    if (Owner != null && finalRect.Width > Owner.AdjustedViewportWidth)
                    {
                        _itemStops.Add(new ItemStop(
                            panoramaItem,
                            i,
                            panoramaItem.StartPosition + (int)finalRect.Width - Owner.AdjustedViewportWidth));
                    }
                }

                panoramaItem.ItemWidth = (int)finalRect.Width;
                panoramaItem.Arrange(finalRect);
                position += finalRect.Width;
            }

            if (Owner != null)
            {
                Owner.RequestAdjustSelection();
            }

            return finalSize;
        }

        private int GetDefaultItemIndex()
        {
            PanoramaItem panoramaItem = Owner != null ? Owner.GetDefaultItemContainer() : null;
            int index = panoramaItem != null ? Children.IndexOf(panoramaItem) : 0;
            return index < 0 ? 0 : index;
        }

        private void GetItemsInView(
            int offset,
            int viewportWidth,
            out int leftIndex,
            out int leftInView,
            out int centerIndex,
            out int rightIndex,
            out int rightInView)
        {
            leftIndex = leftInView = centerIndex = rightIndex = rightInView = -1;
            int count = VisibleChildren.Count;
            if (count == 0)
            {
                return;
            }

            for (int i = 0; i < count; i++)
            {
                PanoramaItem panoramaItem = _visibleChildren[i];
                int start = panoramaItem.StartPosition + offset;
                int end = start + panoramaItem.ItemWidth - 1;

                if (start <= 0 && end >= 0)
                {
                    leftIndex = i;
                    leftInView = Math.Min(viewportWidth, panoramaItem.ItemWidth + start);
                }
                if (start < viewportWidth && end >= viewportWidth)
                {
                    rightIndex = i;
                    rightInView = Math.Min(viewportWidth, viewportWidth - start);
                }
                if (start > 0 && end < viewportWidth)
                {
                    centerIndex = i;
                }
                if (i == 0 && leftInView == -1)
                {
                    leftInView = start;
                }
                if (i == count - 1 && rightInView == -1)
                {
                    rightInView = viewportWidth - end - 1;
                }
            }
        }

        internal void GetStops(int offset, int totalWidth, out ItemStop previous, out ItemStop current, out ItemStop next)
        {
            int currentIndex = -1;
            int nextIndex = -1;
            int previousIndex = -1;
            previous = current = next = null;
            if (VisibleChildren.Count == 0 || totalWidth == 0)
            {
                return;
            }

            int position = -offset % totalWidth;
            int index = 0;
            foreach (ItemStop itemStop in _itemStops)
            {
                if (itemStop.Position < position)
                {
                    previousIndex = index;
                }
                else if (itemStop.Position > position)
                {
                    nextIndex = index;
                    break;
                }
                else
                {
                    currentIndex = index;
                }
                index++;
            }

            if (previousIndex == -1)
            {
                previousIndex = _itemStops.Count - 1;
            }
            if (nextIndex == -1)
            {
                nextIndex = 0;
            }

            previous = _itemStops[previousIndex];
            current = currentIndex != -1 ? _itemStops[currentIndex] : null;
            next = _itemStops[nextIndex];
        }

        internal int CoerceTargetOffset(int targetOffset)
        {
            int result = targetOffset;
            int count = _itemStops.Count;
            if (count > 0)
            {
                result = Math.Max(Math.Min(targetOffset, -_itemStops[0].Position), -_itemStops[count - 1].Position);
            }
            return result;
        }

        internal void GetSnapOffset(
            int offset,
            int viewportWidth,
            int direction,
            out int snapTo,
            out PanoramaItem newSelection,
            out bool wraparound)
        {
            int threshold = viewportWidth / 3;
            int snapDirection = direction;
            wraparound = false;
            snapTo = offset;
            newSelection = _selectedItem;
            if (VisibleChildren.Count == 0)
            {
                return;
            }

            foreach (ItemStop itemStop in _itemStops)
            {
                if (itemStop.Position == -offset)
                {
                    newSelection = itemStop.Item;
                    return;
                }
            }

            GetItemsInView(offset, viewportWidth, out int leftIndex, out int leftInView, out int centerIndex, out int rightIndex, out int rightInView);
            if (leftIndex == rightIndex && leftIndex != -1)
            {
                newSelection = _selectedItem = _visibleChildren[leftIndex];
                return;
            }

            bool wrappedLeft = false;
            if (leftIndex == -1)
            {
                wrappedLeft = true;
                leftIndex = _visibleChildren.Count - 1;
            }
            bool wrappedRight = false;
            if (rightIndex == -1)
            {
                wrappedRight = true;
                rightIndex = 0;
            }

            int bestIndex;
            if (direction < 0)
            {
                if (rightInView > threshold)
                {
                    bestIndex = GetBestIndex(centerIndex, rightIndex, leftIndex);
                    snapDirection = -1;
                }
                else
                {
                    bestIndex = GetBestIndex(centerIndex, leftIndex, rightIndex);
                    snapDirection = 1;
                }
            }
            else if (direction > 0)
            {
                if (leftInView > threshold)
                {
                    bestIndex = GetBestIndex(centerIndex, leftIndex, rightIndex);
                    snapDirection = 1;
                }
                else
                {
                    bestIndex = GetBestIndex(centerIndex, rightIndex, leftIndex);
                    snapDirection = -1;
                }
            }
            else if (centerIndex != -1)
            {
                bestIndex = centerIndex;
                snapDirection = -1;
            }
            else if (leftInView > rightInView)
            {
                bestIndex = leftIndex;
                snapDirection = -1;
            }
            else
            {
                bestIndex = rightIndex;
                snapDirection = 1;
            }

            _selectedItem = _visibleChildren[bestIndex];
            snapTo = snapDirection < 0
                ? GetLeftAlignedOffset(_selectedItem)
                : GetRightAlignedOffset(_selectedItem, viewportWidth);
            newSelection = _selectedItem;

            if ((bestIndex == leftIndex && wrappedLeft) || (bestIndex == rightIndex && wrappedRight))
            {
                wraparound = true;
            }
        }

        private static int GetBestIndex(int first, int second, int third)
        {
            if (first >= 0)
            {
                return first;
            }
            if (second >= 0)
            {
                return second;
            }
            if (third >= 0)
            {
                return third;
            }
            throw new System.InvalidOperationException("No best index.");
        }

        private static int GetLeftAlignedOffset(PanoramaItem movingTo)
        {
            return -movingTo.StartPosition;
        }

        private static int GetRightAlignedOffset(PanoramaItem movingTo, int viewportWidth)
        {
            if (movingTo.Orientation != Orientation.Vertical)
            {
                return -movingTo.ItemWidth + viewportWidth - movingTo.StartPosition - Panorama.Spacing;
            }
            return -movingTo.StartPosition;
        }

        private void FindOwner()
        {
            DependencyObject current = this;
            while (current != null)
            {
                current = VisualTreeHelper.GetParent(current);
                if (current is Panorama panorama)
                {
                    Owner = panorama;
                    return;
                }
            }
        }

        internal void NotifyDefaultItemChanged()
        {
            InvalidateMeasure();
            InvalidateArrange();
            UpdateLayout();
        }
    }
}
