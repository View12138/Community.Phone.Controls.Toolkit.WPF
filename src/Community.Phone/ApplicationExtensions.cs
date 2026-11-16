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
    private static PhoneApplicationFrame? _loadedFrame;

    extension(Application application)
    {
        /// <summary>
        /// Gets or sets the root visual of the application.
        /// </summary>
        /// <remarks>
        /// The Windows Phone platform always sets the root visual to the
        /// application frame before the first page is created, which is what
        /// popup positioning, context menus and transitions rely on. WPF has
        /// no such notion, so when nothing assigned the property the first
        /// <see cref="PhoneApplicationFrame"/> that loads is reported as the
        /// root visual. Applications that want a different root can still
        /// assign it explicitly.
        /// </remarks>
        public UIElement? RootVisual
        {
            get => _rootVisual ?? _loadedFrame;
            set => _rootVisual = value;
        }

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
    }

    internal static void RegisterRootFrame(PhoneApplicationFrame frame)
    {
        _loadedFrame = frame;
    }

    internal static void UnregisterRootFrame(PhoneApplicationFrame frame)
    {
        if (ReferenceEquals(_loadedFrame, frame))
        {
            _loadedFrame = null;
        }
    }
}