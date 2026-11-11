// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using Microsoft.Win32;

namespace Microsoft.Phone.Shell;

public static class ThemeManager
{
    private static readonly IReadOnlyList<AccentColor> accentColors = Array.AsReadOnly(Enum.GetValues<AccentColor>());
    public static IReadOnlyList<AccentColor> AvailableAccentColors => accentColors;

    public static event EventHandler<ThemeChangedEventArgs>? ThemeChanged;

    public static ApplicationTheme ActualTheme => requestedTheme == ApplicationTheme.Auto ? GetSystemTheme() : requestedTheme;
    public static Color ActualAccentColor => accentColor == AccentColor.Auto ? GetSystemAccentColor() : AccentColors.From(accentColor);

    private static ApplicationTheme requestedTheme;
    private static AccentColor accentColor;
    private static Color? customAccentColor;

    extension(Application application)
    {
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

        public void InitializeTheme() => ApplyTheme(application);
    }


    static ThemeManager()
    {
        requestedTheme = ApplicationTheme.Auto;
        accentColor = AccentColor.Auto;
        SystemEvents.UserPreferenceChanged += (s, e) =>
        {
            if (requestedTheme == ApplicationTheme.Auto)
            {
                if (e.Category == UserPreferenceCategory.General ||
                    e.Category == UserPreferenceCategory.VisualStyle ||
                    e.Category == UserPreferenceCategory.Color)
                {
                    ApplyTheme(Application.Current);
                }
            }
        };
    }

    internal static void ApplyTheme(Application application)
    {
        if (!application.Dispatcher.CheckAccess())
        {
            application.Dispatcher.BeginInvoke(DispatcherPriority.Normal, () => ApplyTheme(application));
            return;
        }

        application.Resources ??= new ResourceDictionary();
        if (!ReplaceThemeDictionaries(application.Resources, CreateColorDictionary()))
        {
            application.Resources.MergedDictionaries.Insert(0, CreateColorDictionary());
        }
        ThemeChanged?.Invoke(application, new ThemeChangedEventArgs(ActualTheme, ActualAccentColor));
    }

    private static ResourceDictionary CreateColorDictionary()
    {
        string themeName = ActualTheme == ApplicationTheme.Light ? "Light" : "Dark";
        string source = $"pack://application:,,,/Community.Phone;component/Themes/{themeName}Theme.xaml";
        ResourceDictionary dictionary = new ResourceDictionary { Source = new Uri(source, UriKind.Absolute) };
        var accentColorValue = customAccentColor ?? ActualAccentColor;
        var brush = new SolidColorBrush(accentColorValue);
        dictionary["PhoneAccentColor"] = accentColorValue;
        dictionary["PhoneAccentBrush"] = brush;
        dictionary["PhoneTextBoxEditBorderBrush"] = brush;
        dictionary["PhoneRadioCheckBoxPressedBrush"] = brush;
        return dictionary;
    }

    private static bool ReplaceThemeDictionaries(ResourceDictionary root, ResourceDictionary themeDictionary, HashSet<ResourceDictionary>? visited = null)
    {
        bool changed = false;
        visited ??= new HashSet<ResourceDictionary>();
        if (!visited.Add(root))
        {
            return changed;
        }

        var mergedDictionaries = root.MergedDictionaries;
        for (int i = mergedDictionaries.Count - 1; i >= 0; i--)
        {
            ResourceDictionary dictionary = mergedDictionaries[i];
            if (dictionary.Source.OriginalString.Contains("/Community.Phone;component/Themes/DarkTheme.xaml", StringComparison.OrdinalIgnoreCase))
            {
                mergedDictionaries.RemoveAt(i);
                mergedDictionaries.Insert(i, themeDictionary);
                changed = true;
            }
            else if (dictionary.Source.OriginalString.Contains("/Community.Phone;component/Themes/LightTheme.xaml", StringComparison.OrdinalIgnoreCase))
            {
                mergedDictionaries.RemoveAt(i);
                mergedDictionaries.Insert(i, themeDictionary);
                changed = true;
            }
            else
            {
                changed |= ReplaceThemeDictionaries(dictionary, themeDictionary, visited);
            }
        }
        return changed;
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
                return Color.FromArgb((byte)((value >> 24) & 0xFF), (byte)((value >> 16) & 0xFF), (byte)((value >> 8) & 0xFF), (byte)(value & 0xFF));
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