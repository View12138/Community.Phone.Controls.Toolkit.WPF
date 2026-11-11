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
        public static Size GetClientSize(Window window)
        {
            var content = window.Content as FrameworkElement;
            return new Size(content.ActualWidth, content.ActualHeight);
        }

        public static Rect GetVisibleWindowBounds(Window window)
        {
            var resizeRect = new Thickness(0);
            if (window.ResizeMode != ResizeMode.NoResize)
            {
                resizeRect = SystemParameters.WindowResizeBorderThickness;
            }
            return new Rect(window.Left, window.Top, window.ActualWidth - resizeRect.Left - resizeRect.Right, window.ActualHeight - resizeRect.Top - resizeRect.Bottom);
        }

    }
}

