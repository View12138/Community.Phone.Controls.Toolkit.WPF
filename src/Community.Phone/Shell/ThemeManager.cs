// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Microsoft.Phone.Shell;

/// <summary>
/// Applies the Windows Phone theme (light/dark) and accent color to a WPF
/// application.
/// </summary>
public static class ThemeManager
{
    /// <summary>
    /// Gets every accent color known to the Windows Phone palette.
    /// </summary>
    public static IReadOnlyList<ApplicationTheme> AvailableApplicationThemes => Array.AsReadOnly(Enum.GetValues<ApplicationTheme>());
    /// <summary>
    /// Gets every accent color known to the Windows Phone palette.
    /// </summary>
    public static IReadOnlyList<AccentColor> AvailableAccentColors => Array.AsReadOnly(Enum.GetValues<AccentColor>());

    /// <summary>
    /// Occurs right after a theme has been applied (the color animation, when
    /// enabled, has just been started).
    /// </summary>
    public static event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    /// <summary>
    /// Gets the theme that is actually in effect (resolves
    /// <see cref="ApplicationTheme.Auto"/> against Windows).
    /// </summary>
    public static ApplicationTheme ActualTheme => requestedTheme == ApplicationTheme.Auto ? GetSystemTheme() : requestedTheme;

    /// <summary>
    /// Gets the accent color that is actually in effect, ignoring
    /// <c>Application.CustomAccentColor</c> (resolves
    /// <see cref="Shell.AccentColor.Auto"/> against Windows).
    /// </summary>
    public static Color ActualAccentColor => accentColor == AccentColor.Auto ? GetSystemAccentColor() : AccentColors.From(accentColor);

    private static TimeSpan ThemeTransitionDuration = TimeSpan.FromMilliseconds(300);
    private static ApplicationTheme requestedTheme;
    private static AccentColor accentColor;
    private static Color? customAccentColor;

    private static ApplicationTheme lastAppliedTheme = (ApplicationTheme)(-2);
    private static Color lastAppliedAccent;

    extension(Application application)
    {
        /// <summary>
        /// Gets or sets the requested theme. Setting it applies the new theme
        /// to the application (turned into a Windows Phone style color
        /// transition), without recreating any window, page or control.
        /// </summary>
        public ApplicationTheme RequestedTheme
        {
            get => requestedTheme;
            set
            {
                if (requestedTheme != value)
                {
                    requestedTheme = value;
                    ApplyTheme(application);
                }
            }
        }

        /// <summary>
        /// Gets or sets the accent color. <see cref="Shell.AccentColor.Auto"/>
        /// follows the Windows accent color.
        /// </summary>
        public AccentColor AccentColor
        {
            get => accentColor;
            set
            {
                if (accentColor != value)
                {
                    accentColor = value;
                    ApplyTheme(application);
                }
            }
        }

        /// <summary>
        /// Gets or sets an arbitrary accent color. When set, it takes
        /// precedence over <see cref="AccentColor"/>.
        /// </summary>
        public Color? CustomAccentColor
        {
            get => customAccentColor;
            set
            {
                if (customAccentColor != value)
                {
                    customAccentColor = value;
                    ApplyTheme(application);
                }
            }
        }

        /// <summary>
        /// Ensures the theme dictionary exists and applies the current theme.
        /// Call this once at startup, before the first window is created.
        /// </summary>
        public void InitializeTheme() => ApplyTheme(application, false);
    }

    static ThemeManager()
    {
        requestedTheme = ApplicationTheme.Auto;
        accentColor = AccentColor.Auto;
        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            if (requestedTheme == ApplicationTheme.Auto || accentColor == AccentColor.Auto)
            {
                if (e.Category == UserPreferenceCategory.General || e.Category == UserPreferenceCategory.VisualStyle || e.Category == UserPreferenceCategory.Color)
                {
                    ApplyTheme(Application.Current);
                }
            }
        };
    }


    internal static void ApplyTheme(Application application, bool useAnimation = true)
    {
        if (!application.Dispatcher.CheckAccess())
        {
            application.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => ApplyTheme(application, useAnimation)));
            return;
        }
        ThemePalette palette = (ActualTheme == ApplicationTheme.Light ? ThemePalette.Light : ThemePalette.Dark).WithAccent(customAccentColor ?? ActualAccentColor);
        ApplyTheme(application.Resources, palette, useAnimation);
        if (ActualTheme != lastAppliedTheme || palette.PhoneAccentColor != lastAppliedAccent)
        {
            lastAppliedTheme = ActualTheme;
            lastAppliedAccent = palette.PhoneAccentColor;
            ThemeChanged?.Invoke(application, new ThemeChangedEventArgs(ActualTheme, palette.PhoneAccentColor));
        }
    }

    internal static void ApplyTheme(FrameworkElement element, bool useAnimation = true)
    {
        if (!element.Dispatcher.CheckAccess())
        {
            element.Dispatcher.BeginInvoke(DispatcherPriority.Normal, new Action(() => ApplyTheme(element, useAnimation)));
            return;
        }
        ThemePalette palette = (ActualTheme == ApplicationTheme.Light ? ThemePalette.Light : ThemePalette.Dark).WithAccent(customAccentColor ?? ActualAccentColor);
        ApplyTheme(element.Resources, palette, useAnimation);
    }

    internal static void ApplyTheme(ResourceDictionary dictionary, ThemePalette palette, bool useAnimation = true)
    {
        var themeDictionary = ResolveDictionary(dictionary, "/Community.Phone;component/Themes/Theme.xaml")
            ?? throw new Exception("Theme dictionary not found.");

        foreach (var key in ThemePalette.ResourceKeys)
        {
            var oldValue = themeDictionary[key];
            var newValue = palette.GetValue(key);
            if (newValue != null)
            {
                if (oldValue is SolidColorBrush oldBrush && newValue is Color newColor)
                {
                    if (oldBrush.IsFrozen)
                    {
                        oldBrush = new SolidColorBrush(oldBrush.Color);
                        var animation = new ColorAnimation(oldBrush.Color, oldBrush.Color, TimeSpan.Zero, FillBehavior.HoldEnd);
                        oldBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation);
                        themeDictionary[key] = oldBrush;
                    }

                    if (oldBrush.Color != newColor && !oldBrush.IsFrozen)
                    {
                        var duration = useAnimation ? ThemeTransitionDuration : TimeSpan.Zero;
                        var animation = new ColorAnimation(oldBrush.Color, newColor, duration, FillBehavior.HoldEnd);
                        oldBrush.BeginAnimation(SolidColorBrush.ColorProperty, animation, HandoffBehavior.SnapshotAndReplace);
                    }

                }
                else if (oldValue != newValue)
                {
                    themeDictionary[key] = newValue;
                }
            }
        }
    }

    public static ResourceDictionary? ResolveDictionary(ResourceDictionary root, string source, HashSet<ResourceDictionary>? visited = null)
    {
        visited ??= new HashSet<ResourceDictionary>();
        if (!visited.Add(root))
        {
            return null;
        }

        if (root.Source?.OriginalString.Contains(source, StringComparison.OrdinalIgnoreCase) ?? false)
        {
            return root;
        }

        foreach (ResourceDictionary mergedDictionary in root.MergedDictionaries)
        {
            var result = ResolveDictionary(mergedDictionary, source, visited);
            if (result != null)
            {
                return result;
            }
        }
        return null;
    }

    internal static ApplicationTheme GetSystemTheme()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Themes\Personalize");
            if (int.TryParse(key?.GetValue("AppsUseLightTheme")?.ToString(), out int value))
            {
                return value == 0 ? ApplicationTheme.Dark : ApplicationTheme.Light;
            }
        }
        catch (System.IO.IOException) { }
        catch (System.Security.SecurityException) { }
        catch (UnauthorizedAccessException) { }
        catch (PlatformNotSupportedException) { }
        return ApplicationTheme.Dark;
    }

    public static Color GetSystemAccentColor()
    {
        try
        {
            using var key = Registry.CurrentUser.OpenSubKey(@"SOFTWARE\Microsoft\Windows\DWM");
            if (int.TryParse(key?.GetValue("ColorizationColor")?.ToString(), out int value))
            {
                return Color.FromHexValue(value);
            }
        }
        catch (System.IO.IOException) { }
        catch (System.Security.SecurityException) { }
        catch (UnauthorizedAccessException) { }
        catch (PlatformNotSupportedException) { }
        return AccentColors.Cobalt;
    }
}

public sealed class ThemeChangedEventArgs : EventArgs
{
    public ApplicationTheme Theme { get; }
    public Color AccentColor { get; }

    internal ThemeChangedEventArgs(ApplicationTheme theme, Color accentColor)
    {
        Theme = theme;
        AccentColor = accentColor;
    }
}
