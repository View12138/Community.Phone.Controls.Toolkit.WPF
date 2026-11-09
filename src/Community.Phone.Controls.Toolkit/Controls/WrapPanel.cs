// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Positions child elements sequentially from left to right or top to
    /// bottom. When elements extend beyond the panel edge, elements are
    /// positioned in the next row or column.
    /// </summary>
    public partial class WrapPanel : Panel
    {
        private bool _ignorePropertyChange;

        /// <summary>
        /// Identifies the <see cref="ItemHeight"/> property.
        /// </summary>
        public static readonly DependencyProperty ItemHeightProperty =
            DependencyProperty.Register(
                "ItemHeight",
                typeof(double),
                typeof(WrapPanel),
                new PropertyMetadata(double.NaN, OnItemHeightOrWidthPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="ItemWidth"/> property.
        /// </summary>
        public static readonly DependencyProperty ItemWidthProperty =
            DependencyProperty.Register(
                "ItemWidth",
                typeof(double),
                typeof(WrapPanel),
                new PropertyMetadata(double.NaN, OnItemHeightOrWidthPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(Orientation),
                typeof(WrapPanel),
                new PropertyMetadata(Orientation.Horizontal, OnOrientationPropertyChanged));

        /// <summary>
        /// Gets or sets the height of the layout area for each item.
        /// </summary>
        public double ItemHeight
        {
            get { return (double)GetValue(ItemHeightProperty); }
            set { SetValue(ItemHeightProperty, value); }
        }

        /// <summary>
        /// Gets or sets the width of the layout area for each item.
        /// </summary>
        public double ItemWidth
        {
            get { return (double)GetValue(ItemWidthProperty); }
            set { SetValue(ItemWidthProperty, value); }
        }

        /// <summary>
        /// Gets or sets the direction in which child elements are arranged.
        /// </summary>
        public Orientation Orientation
        {
            get { return (Orientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="WrapPanel"/> class.
        /// </summary>
        public WrapPanel()
        {
        }

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WrapPanel source = (WrapPanel)d;
            Orientation value = (Orientation)e.NewValue;

            if (source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            if (value != Orientation.Horizontal && value != Orientation.Vertical)
            {
                source._ignorePropertyChange = true;
                source.SetValue(OrientationProperty, (Orientation)e.OldValue);
                throw new ArgumentException("Invalid orientation value.", "value");
            }

            source.InvalidateMeasure();
        }

        private static void OnItemHeightOrWidthPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            WrapPanel source = (WrapPanel)d;
            double value = (double)e.NewValue;

            if (source._ignorePropertyChange)
            {
                source._ignorePropertyChange = false;
                return;
            }

            if (!double.IsNaN(value) && (value <= 0.0 || double.IsPositiveInfinity(value)))
            {
                source._ignorePropertyChange = true;
                source.SetValue(e.Property, (double)e.OldValue);
                throw new ArgumentException("The length must be NaN or a positive finite number.", "value");
            }

            source.InvalidateMeasure();
        }

        /// <inheritdoc />
        protected override Size MeasureOverride(Size constraint)
        {
            Orientation orientation = Orientation;
            OrientedSize lineSize = new OrientedSize(orientation);
            OrientedSize totalSize = new OrientedSize(orientation);
            OrientedSize maximumSize = new OrientedSize(orientation, constraint.Width, constraint.Height);

            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            bool hasFixedWidth = !double.IsNaN(itemWidth);
            bool hasFixedHeight = !double.IsNaN(itemHeight);
            Size itemSize = new Size(
                hasFixedWidth ? itemWidth : constraint.Width,
                hasFixedHeight ? itemHeight : constraint.Height);

            foreach (UIElement element in Children)
            {
                element.Measure(itemSize);
                OrientedSize elementSize = new OrientedSize(
                    orientation,
                    hasFixedWidth ? itemWidth : element.DesiredSize.Width,
                    hasFixedHeight ? itemHeight : element.DesiredSize.Height);

                if (NumericExtensions.IsGreaterThan(lineSize.Direct + elementSize.Direct, maximumSize.Direct))
                {
                    totalSize.Direct = Math.Max(lineSize.Direct, totalSize.Direct);
                    totalSize.Indirect += lineSize.Indirect;

                    lineSize = elementSize;

                    if (NumericExtensions.IsGreaterThan(elementSize.Direct, maximumSize.Direct))
                    {
                        totalSize.Direct = Math.Max(elementSize.Direct, totalSize.Direct);
                        totalSize.Indirect += elementSize.Indirect;
                        lineSize = new OrientedSize(orientation);
                    }
                }
                else
                {
                    lineSize.Direct += elementSize.Direct;
                    lineSize.Indirect = Math.Max(lineSize.Indirect, elementSize.Indirect);
                }
            }

            totalSize.Direct = Math.Max(lineSize.Direct, totalSize.Direct);
            totalSize.Indirect += lineSize.Indirect;
            return new Size(totalSize.Width, totalSize.Height);
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size finalSize)
        {
            Orientation orientation = Orientation;
            OrientedSize lineSize = new OrientedSize(orientation);
            OrientedSize maximumSize = new OrientedSize(orientation, finalSize.Width, finalSize.Height);

            double itemWidth = ItemWidth;
            double itemHeight = ItemHeight;
            bool hasFixedWidth = !double.IsNaN(itemWidth);
            bool hasFixedHeight = !double.IsNaN(itemHeight);
            double indirectOffset = 0;
            double? directDelta = orientation == Orientation.Horizontal
                ? (hasFixedWidth ? (double?)itemWidth : null)
                : (hasFixedHeight ? (double?)itemHeight : null);

            UIElementCollection children = Children;
            int count = children.Count;
            int lineStart = 0;
            for (int lineEnd = 0; lineEnd < count; lineEnd++)
            {
                UIElement element = children[lineEnd];
                OrientedSize elementSize = new OrientedSize(
                    orientation,
                    hasFixedWidth ? itemWidth : element.DesiredSize.Width,
                    hasFixedHeight ? itemHeight : element.DesiredSize.Height);

                if (NumericExtensions.IsGreaterThan(lineSize.Direct + elementSize.Direct, maximumSize.Direct))
                {
                    ArrangeLine(lineStart, lineEnd, directDelta, indirectOffset, lineSize.Indirect);

                    indirectOffset += lineSize.Indirect;
                    lineSize = elementSize;

                    if (NumericExtensions.IsGreaterThan(elementSize.Direct, maximumSize.Direct))
                    {
                        ArrangeLine(lineEnd, ++lineEnd, directDelta, indirectOffset, elementSize.Indirect);
                        indirectOffset += lineSize.Indirect;
                        lineSize = new OrientedSize(orientation);
                    }

                    lineStart = lineEnd;
                }
                else
                {
                    lineSize.Direct += elementSize.Direct;
                    lineSize.Indirect = Math.Max(lineSize.Indirect, elementSize.Indirect);
                }
            }

            if (lineStart < count)
            {
                ArrangeLine(lineStart, count, directDelta, indirectOffset, lineSize.Indirect);
            }

            return finalSize;
        }

        private void ArrangeLine(int lineStart, int lineEnd, double? directDelta, double indirectOffset, double indirectGrowth)
        {
            double directOffset = 0.0;
            Orientation orientation = Orientation;
            bool isHorizontal = orientation == Orientation.Horizontal;
            UIElementCollection children = Children;

            for (int index = lineStart; index < lineEnd; index++)
            {
                UIElement element = children[index];
                OrientedSize elementSize = new OrientedSize(orientation, element.DesiredSize.Width, element.DesiredSize.Height);
                double directGrowth = directDelta != null ? directDelta.Value : elementSize.Direct;
                Rect bounds = isHorizontal
                    ? new Rect(directOffset, indirectOffset, directGrowth, indirectGrowth)
                    : new Rect(indirectOffset, directOffset, indirectGrowth, directGrowth);

                element.Arrange(bounds);
                directOffset += directGrowth;
            }
        }
    }

    internal struct OrientedSize
    {
        private readonly Orientation _orientation;
        private double _direct;
        private double _indirect;

        public OrientedSize(Orientation orientation)
            : this(orientation, 0.0, 0.0)
        {
        }

        public OrientedSize(Orientation orientation, double width, double height)
        {
            _orientation = orientation;
            _direct = 0.0;
            _indirect = 0.0;
            Width = width;
            Height = height;
        }

        public Orientation Orientation
        {
            get { return _orientation; }
        }

        public double Direct
        {
            get { return _direct; }
            set { _direct = value; }
        }

        public double Indirect
        {
            get { return _indirect; }
            set { _indirect = value; }
        }

        public double Width
        {
            get { return _orientation == Orientation.Horizontal ? Direct : Indirect; }
            set
            {
                if (_orientation == Orientation.Horizontal)
                {
                    Direct = value;
                }
                else
                {
                    Indirect = value;
                }
            }
        }

        public double Height
        {
            get { return _orientation != Orientation.Horizontal ? Direct : Indirect; }
            set
            {
                if (_orientation != Orientation.Horizontal)
                {
                    Direct = value;
                }
                else
                {
                    Indirect = value;
                }
            }
        }
    }

    internal static class NumericExtensions
    {
        public static bool IsNaN(this double value)
        {
            return double.IsNaN(value);
        }

        public static bool IsGreaterThan(double left, double right)
        {
            return left > right && !AreClose(left, right);
        }

        public static bool AreClose(double left, double right)
        {
            if (left == right)
            {
                return true;
            }

            double tolerance = (Math.Abs(left) + Math.Abs(right) + 10.0) * 2.2204460492503131E-16;
            double difference = left - right;
            return -tolerance < difference && tolerance > difference;
        }
    }
}
