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
    }
}