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
/// 把滚轮滚动变成连续动画：一次滚轮事件在固定时长内缓动到目标偏移，而不是瞬间跳转。
/// 目标步长与 WPF 默认一致（像素滚动 48 DIP，项滚动 <see cref="SystemParameters.WheelScrollLines"/> 项）。
/// </summary>
public static class SmoothScrolling
{
    private static readonly TimeSpan Duration = TimeSpan.FromMilliseconds(150);

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

    private static void OnPreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if (sender is not ScrollViewer viewer)
        { return; }
        if (viewer.ScrollableHeight <= 0)
        { return; }

        double pixels = 48;
        double step = e.Delta / -120.0 * (viewer.CanContentScroll ? SystemParameters.WheelScrollLines * pixels : pixels);

        double limit = Math.Max(0, viewer.ScrollableHeight);
        if (limit <= 0)
        { return; }

        double currentOffset = viewer.VerticalOffset;
        var targetOffset = Math.Min(currentOffset + step, limit);
        var animation = new DoubleAnimation(currentOffset, targetOffset, Duration)
        { EasingFunction = new CubicEase { EasingMode = EasingMode.EaseOut } };
        var animationClock = animation.CreateClock();
        animationClock.CurrentTimeInvalidated += (s, e) =>
        {
            var value = animationClock.GetCurrentValue(currentOffset, targetOffset) as double?;
            if (value.HasValue)
            { viewer.ScrollToVerticalOffset(value.Value); }
        };
        animationClock.Controller.Begin();
        e.Handled = true;
    }

}
