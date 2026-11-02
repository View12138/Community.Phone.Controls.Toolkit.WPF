// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// The items host used by <see cref="LongListSelector"/>. It deliberately
    /// creates plain <see cref="ContentPresenter"/> containers: the original
    /// control exposes those containers through ItemRealized/ItemUnrealized and
    /// never draws a selection chrome of its own.
    /// </summary>
    internal sealed class LongListSelectorItemsHost : ItemsControl
    {
        internal LongListSelectorItemsHost(LongListSelector owner)
        {
            Owner = owner;
        }

        internal LongListSelector Owner { get; }

        protected override DependencyObject GetContainerForItemOverride()
        {
            return new ContentPresenter();
        }

        protected override bool IsItemItsOwnContainerOverride(object item)
        {
            return item is ContentPresenter;
        }

        protected override void PrepareContainerForItemOverride(DependencyObject element, object item)
        {
            base.PrepareContainerForItemOverride(element, item);

            if (element is not ContentPresenter container)
            {
                return;
            }

            container.HorizontalAlignment = HorizontalAlignment.Stretch;
            container.VerticalAlignment = VerticalAlignment.Top;

            LongListSelectorItem entry = item as LongListSelectorItem;
            if (entry != null)
            {
                // The generator keeps the wrapper as its item (that is what the
                // selection and realization code asks for), but templates bind
                // to the real item/group, exactly like the phone control.
                container.Content = entry.Data;
                container.ContentTemplate = Owner.GetTemplateFor(entry);
                container.ContentTemplateSelector = null;

                // The generator's item is the internal wrapper; without this
                // the ContentPresenter automation peer would announce the
                // wrapper instead of the item/group.
                AutomationProperties.SetName(container, entry.Data?.ToString() ?? string.Empty);
            }

            bool isGridCell =
                entry != null &&
                entry.ItemKind == LongListSelectorItemKind.Item &&
                Owner.LayoutMode == LongListSelectorLayoutMode.Grid &&
                Owner.GridCellSize.Width > 0 &&
                Owner.GridCellSize.Height > 0;

            if (isGridCell)
            {
                container.Width = Owner.GridCellSize.Width;
                container.Height = Owner.GridCellSize.Height;
            }
            else
            {
                container.ClearValue(WidthProperty);
                container.ClearValue(HeightProperty);
            }
        }
    }

    /// <summary>
    /// Lays out the flattened elements in <see cref="LongListSelectorLayoutMode.Grid"/>:
    /// items occupy a fixed cell, while the list header/footer and the group
    /// headers/footers span the full width. The original control does the same
    /// (it places a group header at x = 0 with the viewport width).
    /// </summary>
    internal sealed class LongListSelectorGridPanel : Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            LongListSelector owner = GetOwner();
            if (owner == null)
            {
                return MeasureWithoutOwner(availableSize);
            }

            double width = availableSize.Width;
            if (double.IsInfinity(width) || double.IsNaN(width) || width <= 0)
            {
                width = Math.Max(owner.GridCellSize.Width, 1);
            }

            double cellWidth = owner.GridCellSize.Width > 0 ? owner.GridCellSize.Width : width;
            double cellHeight = owner.GridCellSize.Height;
            int columns = cellWidth > 0 && width > cellWidth
                ? Math.Max(1, (int)Math.Floor(width / cellWidth))
                : 1;

            double y = 0;
            int column = 0;
            double rowHeight = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (IsGridCell(child))
                {
                    child.Measure(new Size(cellWidth, cellHeight > 0 ? cellHeight : double.PositiveInfinity));
                    rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
                    column++;
                    if (column == columns)
                    {
                        y += cellHeight > 0 ? cellHeight : rowHeight;
                        column = 0;
                        rowHeight = 0;
                    }
                }
                else
                {
                    if (column > 0)
                    {
                        y += cellHeight > 0 ? cellHeight : rowHeight;
                        column = 0;
                        rowHeight = 0;
                    }

                    child.Measure(new Size(width, double.PositiveInfinity));
                    y += child.DesiredSize.Height;
                }
            }

            if (column > 0)
            {
                y += cellHeight > 0 ? cellHeight : rowHeight;
            }

            return new Size(width, y);
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            LongListSelector owner = GetOwner();
            double width = finalSize.Width;
            double cellWidth = owner != null && owner.GridCellSize.Width > 0 ? owner.GridCellSize.Width : width;
            double cellHeight = owner?.GridCellSize.Height ?? 0;
            int columns = cellWidth > 0 && width > cellWidth
                ? Math.Max(1, (int)Math.Floor(width / cellWidth))
                : 1;

            double y = 0;
            int column = 0;
            double rowHeight = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (IsGridCell(child))
                {
                    if (column == 0)
                    {
                        rowHeight = child.DesiredSize.Height;
                    }
                    else
                    {
                        rowHeight = Math.Max(rowHeight, child.DesiredSize.Height);
                    }

                    child.Arrange(new Rect(column * cellWidth, y, cellWidth, cellHeight > 0 ? cellHeight : child.DesiredSize.Height));
                    column++;
                    if (column == columns)
                    {
                        y += cellHeight > 0 ? cellHeight : rowHeight;
                        column = 0;
                        rowHeight = 0;
                    }
                }
                else
                {
                    if (column > 0)
                    {
                        y += cellHeight > 0 ? cellHeight : rowHeight;
                        column = 0;
                        rowHeight = 0;
                    }

                    double height = child.DesiredSize.Height;
                    child.Arrange(new Rect(0, y, width, height));
                    y += height;
                }
            }

            return finalSize;
        }

        private Size MeasureWithoutOwner(Size availableSize)
        {
            double width = double.IsInfinity(availableSize.Width) || availableSize.Width <= 0
                ? double.PositiveInfinity
                : availableSize.Width;
            double height = 0;
            foreach (UIElement child in InternalChildren)
            {
                child.Measure(new Size(width, double.PositiveInfinity));
                height += child.DesiredSize.Height;
            }

            return new Size(double.IsInfinity(width) ? 0 : width, height);
        }

        private bool IsGridCell(UIElement child)
        {
            if (child is not ContentPresenter)
            {
                return false;
            }

            object entry = ItemsControl.GetItemsOwner(this)?.ItemContainerGenerator.ItemFromContainer(child);
            return entry is LongListSelectorItem item && item.ItemKind == LongListSelectorItemKind.Item;
        }

        private LongListSelector GetOwner()
        {
            return (ItemsControl.GetItemsOwner(this) as LongListSelectorItemsHost)?.Owner;
        }
    }
}
