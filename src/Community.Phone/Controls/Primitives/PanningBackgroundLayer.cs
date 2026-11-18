// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;

namespace Microsoft.Phone.Controls.Primitives
{
    /// <summary>
    /// Pans the background of a <see cref="Panorama"/> slower than its items.
    /// </summary>
    public class PanningBackgroundLayer : PanningLayer
    {
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

                return (Math.Max(Owner.ViewportWidth, ContentPresenter.ActualWidth) - Owner.ViewportWidth / 5 * 4)
                    / totalWidth;
            }
        }

        /// <inheritdoc/>
        protected internal override bool ShouldShowWrappers
        {
            get { return !IsStatic; }
        }

        /// <inheritdoc/>
        protected override void UpdateWrappingRectangles()
        {
            Thickness margin = ContentPresenter != null ? ContentPresenter.Margin : new Thickness(0.0);
            Thickness leftMargin = new Thickness(margin.Left + 1.0, margin.Top, margin.Right - 1.0, margin.Bottom);
            Thickness rightMargin = new Thickness(margin.Left - 1.0, margin.Top, margin.Right, margin.Bottom);
            UpdateWrappingRectanglesInternal(leftMargin, rightMargin);
        }
    }
}
