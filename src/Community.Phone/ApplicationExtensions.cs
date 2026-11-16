using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Phone.Controls;

public static class ApplicationExtensions
{
    private static UIElement? _rootVisual;
    private static WpfHost? _wpfHost;

    extension(Application application)
    {
        public UIElement? RootVisual { get => _rootVisual; set => _rootVisual = value; }

        /// <summary>
        /// Gets or sets the requested theme. Setting it applies the new theme
        /// to the application (turned into a Windows Phone style color
        /// transition), without recreating any window, page or control.
        /// </summary>
        public ApplicationTheme RequestedTheme
        {
            get => ThemeManager.requestedTheme;
            set
            {
                if (ThemeManager.requestedTheme != value)
                {
                    ThemeManager.requestedTheme = value;
                    ThemeManager.ApplyTheme(application);
                }
            }
        }

        /// <summary>
        /// Gets or sets the accent color. <see cref="Shell.AccentColor.Auto"/>
        /// follows the Windows accent color.
        /// </summary>
        public AccentColor AccentColor
        {
            get => ThemeManager.accentColor;
            set
            {
                if (ThemeManager.accentColor != value)
                {
                    ThemeManager.accentColor = value;
                    ThemeManager.ApplyTheme(application);
                }
            }
        }

        /// <summary>
        /// Gets or sets an arbitrary accent color. When set, it takes
        /// precedence over <see cref="AccentColor"/>.
        /// </summary>
        public Color? CustomAccentColor
        {
            get => ThemeManager.customAccentColor;
            set
            {
                if (ThemeManager.customAccentColor != value)
                {
                    ThemeManager.customAccentColor = value;
                    ThemeManager.ApplyTheme(application);
                }
            }
        }

        /// <summary>
        /// Ensures the theme dictionary exists and applies the current theme.
        /// Call this once at startup, before the first window is created.
        /// </summary>
        public void InitializeTheme() => ThemeManager.ApplyTheme(application, false);

        public WpfHost? Host { get => _wpfHost; set => _wpfHost = value; }
    }
}

public sealed class WpfHost
{
    public Content? Content { get; set; }
}
//
// 摘要:
//     提供有关 Windows Phone 应用程序的内容区域的尺寸信息。
public sealed class Content
{
    //
    // 摘要:
    //     初始化 System.Windows.Interop.Content 类的新实例。
    public Content() { }

    //
    // 摘要:
    //     获取 Windows Phone 应用程序内容区域的高度。
    //
    // 返回结果:
    //     Windows Phone 应用程序内容区域的高度（以像素为单位）。
    public double ActualHeight { get; }
    //
    // 摘要:
    //     获取 Windows Phone 应用程序内容区域的宽度。
    //
    // 返回结果:
    //     Windows Phone 应用程序内容区域的宽度（以像素为单位）。
    public double ActualWidth { get; }
    //
    // 摘要:
    //     获取值，应用程序的内容区域可依据该值缩放其内容。
    //
    // 返回结果:
    //     当前应用程序的内容区域的缩放比例。
    public int ScaleFactor { get; }

    //
    // 摘要:
    //     在 Windows Phone 应用程序的 System.Windows.Interop.Content.ActualHeight 或 System.Windows.Interop.Content.ActualWidth
    //     更改时发生。
    public event EventHandler Resized;
    //
    // 摘要:
    //     缩放比例更改时发生。
    public event EventHandler ScaleFactorChanged;
}