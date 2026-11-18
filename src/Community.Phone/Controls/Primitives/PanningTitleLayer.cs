// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Pans the title of a <see cref="Panorama"/> slower than its items.
    /// </summary>
    public class PanningTitleLayer : PanningLayer
    {
        private double WidthAdjustment
        {
            get { return Owner != null ? Owner.ViewportWidth * 0.625 : 0.0; }
        }

        private double ExtraWidthAdjustment
        {
            get { return Owner != null ? Owner.ViewportWidth * 0.1 : 0.0; }
        }

        /// <inheritdoc/>
        protected override double PanRate
        {
            get
            {
                if (Owner == null || ContentPresenter == null)
                {
                    return 1.0;
                }

                double totalWidth = Math.Max(Owner.ViewportWidth, Owner.ItemsWidth);
                if (totalWidth == 0.0)
                {
                    return 1.0;
                }

                return (Math.Max(Owner.ViewportWidth, ContentPresenter.ActualWidth + WidthAdjustment) - Owner.ViewportWidth / 5 * 4)
                    / totalWidth;
            }
        }

        /// <inheritdoc/>
        public override void Wraparound(int direction)
        {
            double width = ContentPresenter != null ? ContentPresenter.ActualWidth : 0.0;
            if (direction < 0)
            {
                GoTo((int)((ActualOffset + width + WidthAdjustment + ExtraWidthAdjustment) / PanRate), Immediately);
            }
            else
            {
                GoTo((int)((ActualOffset - width - WidthAdjustment - ExtraWidthAdjustment) / PanRate), Immediately);
            }
        }

        /// <inheritdoc/>
        protected override void UpdateWrappingRectangles()
        {
            Thickness margin = ContentPresenter != null ? ContentPresenter.Margin : new Thickness(0.0);
            Thickness leftMargin = new Thickness(
                margin.Left,
                margin.Top,
                margin.Right + WidthAdjustment + ExtraWidthAdjustment,
                margin.Bottom);
            Thickness rightMargin = new Thickness(
                margin.Left + WidthAdjustment + ExtraWidthAdjustment,
                margin.Top,
                margin.Right,
                margin.Bottom);
            UpdateWrappingRectanglesInternal(leftMargin, rightMargin);
        }
    }
}
