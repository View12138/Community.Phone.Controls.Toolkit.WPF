using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Microsoft.Phone
{
    internal class RenderBitmap
    {
        public static BitmapSource Render(FrameworkElement element)
        {
            var dpi = VisualTreeHelper.GetDpi(element);
            RenderTargetBitmap ownerBitmap = new RenderTargetBitmap(
                (int)Math.Ceiling(element.ActualWidth * dpi.DpiScaleX),
                (int)Math.Ceiling(element.ActualHeight * dpi.DpiScaleY),
                96 * dpi.DpiScaleX,
                96 * dpi.DpiScaleY,
                PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                VisualBrush visualBrush = new VisualBrush(element)
                {
                    Stretch = Stretch.Fill,
                    AlignmentX = AlignmentX.Left,
                    AlignmentY = AlignmentY.Top,
                    ViewboxUnits = BrushMappingMode.RelativeToBoundingBox,
                    Viewbox = new Rect(0, 0, 1, 1),
                    ViewportUnits = BrushMappingMode.Absolute,
                    Viewport = new Rect(0, 0, element.ActualWidth, element.ActualHeight)
                };
                drawingContext.DrawRectangle(visualBrush, null, new Rect(0, 0, element.ActualWidth, element.ActualHeight));
            }
            ownerBitmap.Render(drawingVisual);
            ownerBitmap.Freeze();
            return ownerBitmap;
        }
    }
}
