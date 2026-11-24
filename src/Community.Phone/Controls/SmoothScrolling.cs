// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;
using System.Windows.Threading;

namespace Microsoft.Phone.Controls;

/// <summary>
/// 平滑滚动
/// </summary>
public static class SmoothScrolling
{
    public static int GetLineHeight(DependencyObject obj) => (int)obj.GetValue(LineHeightProperty);
    public static void SetLineHeight(DependencyObject obj, int value) => obj.SetValue(LineHeightProperty, value);
    public static readonly DependencyProperty LineHeightProperty =
        DependencyProperty.RegisterAttached("LineHeight", typeof(int), typeof(SmoothScrolling), new PropertyMetadata(32));

    public static int GetMouseWheelDelta(DependencyObject obj) => (int)obj.GetValue(MouseWheelDeltaProperty);
    public static void SetMouseWheelDelta(DependencyObject obj, int value) => obj.SetValue(MouseWheelDeltaProperty, value);
    public static readonly DependencyProperty MouseWheelDeltaProperty =
        DependencyProperty.RegisterAttached("MouseWheelDelta", typeof(int), typeof(SmoothScrolling), new PropertyMetadata(48));

    public static bool GetIsEnabled(DependencyObject element) => (bool)element.GetValue(IsEnabledProperty);
    public static void SetIsEnabled(DependencyObject element, bool value) => element.SetValue(IsEnabledProperty, value);
    public static readonly DependencyProperty IsEnabledProperty =
        DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(SmoothScrolling), new PropertyMetadata(false, OnIsEnabledChanged));


    private static void OnIsEnabledChanged(DependencyObject element, DependencyPropertyChangedEventArgs e)
    {
        if (element is not ScrollViewer viewer)
        {
            return;
        }

        if ((bool)e.NewValue)
        {
            viewer.PreviewMouseWheel += OnPreviewMouseWheel;
        }
        else
        {
            viewer.PreviewMouseWheel -= OnPreviewMouseWheel;
        }
    }

    private static AnimationClock animationClock;

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer viewer)
        { return; }
        e.Handled = true;
        bool isHorizontal = (Keyboard.Modifiers & ModifierKeys.Shift) != 0;
        double lineHeight = GetLineHeight(viewer);
        var mouseWheelDelta = GetMouseWheelDelta(viewer);
        var lines = Math.Max(1, SystemParameters.WheelScrollLines);
        double step = e.Delta / -120.0 * (viewer.CanContentScroll ? lines * lineHeight : mouseWheelDelta);

        double limit = Math.Max(0, isHorizontal ? viewer.ScrollableWidth : viewer.ScrollableHeight);
        if (limit <= 0)
        { return; }

        double currentOffset = isHorizontal ? viewer.HorizontalOffset : viewer.VerticalOffset;
        var targetOffset = Math.Max(0, Math.Min(currentOffset + step, limit));
        var animation = new DoubleAnimation(currentOffset, targetOffset, TimeSpan.FromMilliseconds(150))
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        animationClock?.Controller.Stop();
        animationClock = animation.CreateClock();
        animationClock.Completed += (s, e) => animationClock = null;
        animationClock.CurrentTimeInvalidated += (s, e) =>
        {
            var value = animationClock.GetCurrentValue(currentOffset, targetOffset) as double?;
            if (value.HasValue)
            {
                if (isHorizontal)
                { viewer.ScrollToHorizontalOffset(value.Value); }
                else
                { viewer.ScrollToVerticalOffset(value.Value); }
            }
        };
        animationClock.Controller.Begin();
    }

}
