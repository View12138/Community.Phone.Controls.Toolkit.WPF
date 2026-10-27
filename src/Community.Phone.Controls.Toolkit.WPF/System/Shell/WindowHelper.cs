// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;

namespace Microsoft.Phone.Shell
{
    public class WindowHelper
    {

        [StructLayout(LayoutKind.Sequential)]
        private struct RECT { public int Left, Top, Right, Bottom; }

        [DllImport("user32.dll")]
        private static extern bool GetClientRect(IntPtr hWnd, out RECT lpRect);

        private const int DWMWA_EXTENDED_FRAME_BOUNDS = 9;
        [DllImport("dwmapi.dll")]
        private static extern int DwmGetWindowAttribute(IntPtr hwnd, int dwAttribute, out RECT pvAttribute, int cbAttribute);

        public static Size GetClientSize(Window window)
        {
            var content = window.Content as FrameworkElement;
            return new Size(content.ActualWidth, content.ActualHeight);

            var hwnd = new WindowInteropHelper(window).Handle;

            if (hwnd == IntPtr.Zero || !GetClientRect(hwnd, out var rect))
            {
                return Size.Empty;
            }

            var source = HwndSource.FromHwnd(hwnd);
            var dpi = source?.CompositionTarget?.TransformFromDevice;

            if (dpi == null)
            {
                return new Size(rect.Right - rect.Left, rect.Bottom - rect.Top);
            }

            var size = dpi.Value.Transform(new Vector(rect.Right - rect.Left, rect.Bottom - rect.Top));
            return new Size(size.X, size.Y);
        }

        public static Rect GetVisibleWindowBounds(Window window)
        {
            var resizeRect = new Thickness(0);
            if (window.ResizeMode != ResizeMode.NoResize)
            {
                resizeRect = SystemParameters.WindowResizeBorderThickness;
            }
            return new Rect(window.Left, window.Top, window.ActualWidth - resizeRect.Left - resizeRect.Right, window.ActualHeight - resizeRect.Top - resizeRect.Bottom);

            IntPtr hwnd = new WindowInteropHelper(window).Handle;

            if (hwnd == IntPtr.Zero)
            {
                return Rect.Empty;
            }

            int hr = DwmGetWindowAttribute(hwnd, DWMWA_EXTENDED_FRAME_BOUNDS, out RECT rect, Marshal.SizeOf(typeof(RECT)));

            if (hr != 0)
                return Rect.Empty;

            var source = HwndSource.FromHwnd(hwnd);
            var transform = source?.CompositionTarget?.TransformFromDevice;

            DpiScale dpi = VisualTreeHelper.GetDpi(window);
            if (transform == null)
            {
                return Rect.Empty;
            }

            Point topLeft = transform.Value.Transform(new Point(rect.Left, rect.Top));
            Point bottomRight = transform.Value.Transform(new Point(rect.Right, rect.Bottom));
            return new Rect(topLeft, bottomRight);
        }

    }
}

