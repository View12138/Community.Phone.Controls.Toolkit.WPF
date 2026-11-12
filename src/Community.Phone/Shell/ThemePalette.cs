// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;

namespace Microsoft.Phone.Shell;

/// <summary>
/// Describes one complete theme color scheme (a "palette").
/// </summary>
/// <remarks>
/// <para>
/// A palette only carries colors (plus the couple of non-color values that are
/// inherently part of a Windows Phone theme: <see cref="IsDark"/> and
/// <see cref="PhonePivotUnselectedItemOpacity"/>). It never owns brushes:
/// <see cref="ThemeManager"/> maps every palette color onto the persistent
/// <see cref="SolidColorBrush"/> instances declared in <c>Themes/Theme.xaml</c>.
/// </para>
/// <para>
/// The 1:1 counterpart of the properties below is the
/// <c>Phone*Color</c> resource set of <c>Themes/Theme.xaml</c>.
/// </para>
/// </remarks>
public record ThemePalette
{
    public required Color PhoneAccentColor { get; init; }
    public required Color PhoneForegroundColor { get; init; }
    public required Color PhoneBackgroundColor { get; init; }
    public required Color PhoneContrastForegroundColor { get; init; }
    public required Color PhoneContrastBackgroundColor { get; init; }
    public required Color PhoneDisabledColor { get; init; }
    public required Color PhoneProgressBarBackgroundColor { get; init; }
    public required Color PhoneTextCaretColor { get; init; }
    public required Color PhoneTextBoxColor { get; init; }
    public required Color PhoneTextBoxForegroundColor { get; init; }
    public required Color PhoneTextBoxEditBackgroundColor { get; init; }
    public required Color PhoneTextBoxReadOnlyColor { get; init; }
    public required Color PhoneSubtleColor { get; init; }
    public required Color PhoneTextBoxSelectionForegroundColor { get; init; }
    public required Color PhoneButtonBasePressedForegroundColor { get; init; }
    public required Color PhoneTextHighContrastColor { get; init; }
    public required Color PhoneTextMidContrastColor { get; init; }
    public required Color PhoneTextLowContrastColor { get; init; }
    public required Color PhoneSemitransparentColor { get; init; }
    public required Color PhoneChromeColor { get; init; }
    public required Color PhoneInactiveColor { get; init; }
    public required Color PhoneInverseInactiveColor { get; init; }
    public required Color PhoneInverseBackgroundColor { get; init; }
    public required Color PhoneBorderColor { get; init; }

    public required Visibility PhoneDarkThemeVisibility { get; set; }
    public required Visibility PhoneLightThemeVisibility { get; set; }
    public required double PhoneDarkThemeOpacity { get; init; }
    public required double PhoneLightThemeOpacity { get; init; }
    public required double PhonePivotUnselectedItemOpacity { get; init; }


    public ThemePalette WithAccent(Color accent) => this with { PhoneAccentColor = accent };

    /// <summary>
    /// Gets the built-in Windows Phone dark theme palette.
    /// </summary>
    public static ThemePalette Dark { get; } = new()
    {
        PhoneAccentColor = Color.FromHex("#FF0050EF"),
        PhoneForegroundColor = Color.FromHex("#FFFFFFFF"),
        PhoneBackgroundColor = Color.FromHex("#FF000000"),
        PhoneContrastForegroundColor = Color.FromHex("#FF000000"),
        PhoneContrastBackgroundColor = Color.FromHex("#FFFFFFFF"),
        PhoneDisabledColor = Color.FromHex("#66FFFFFF"),
        PhoneProgressBarBackgroundColor = Color.FromHex("#19FFFFFF"),
        PhoneTextCaretColor = Color.FromHex("#FF000000"),
        PhoneTextBoxColor = Color.FromHex("#BFFFFFFF"),
        PhoneTextBoxForegroundColor = Color.FromHex("#FF000000"),
        PhoneTextBoxEditBackgroundColor = Color.FromHex("#FFFFFFFF"),
        PhoneTextBoxReadOnlyColor = Color.FromHex("#77000000"),
        PhoneSubtleColor = Color.FromHex("#99FFFFFF"),
        PhoneTextBoxSelectionForegroundColor = Color.FromHex("#FFFFFFFF"),
        PhoneButtonBasePressedForegroundColor = Color.FromHex("#FFFFFFFF"),
        PhoneTextHighContrastColor = Color.FromHex("#FFFFFFFF"),
        PhoneTextMidContrastColor = Color.FromHex("#99FFFFFF"),
        PhoneTextLowContrastColor = Color.FromHex("#73FFFFFF"),
        PhoneSemitransparentColor = Color.FromHex("#AA000000"),
        PhoneChromeColor = Color.FromHex("#FF1F1F1F"),
        PhoneInactiveColor = Color.FromHex("#33FFFFFF"),
        PhoneInverseInactiveColor = Color.FromHex("#FFCCCCCC"),
        PhoneInverseBackgroundColor = Color.FromHex("#FFFFFFFF"),
        PhoneBorderColor = Color.FromHex("#BFFFFFFF"),

        PhoneDarkThemeVisibility = Visibility.Visible,
        PhoneLightThemeVisibility = Visibility.Collapsed,
        PhoneDarkThemeOpacity = 1,
        PhoneLightThemeOpacity = 0,
        PhonePivotUnselectedItemOpacity = 0.4,
    };

    /// <summary>
    /// Gets the built-in Windows Phone light theme palette.
    /// </summary>
    public static ThemePalette Light { get; } = new()
    {
        PhoneAccentColor = Color.FromHex("#FF0050EF"),
        PhoneForegroundColor = Color.FromHex("#DE000000"),
        PhoneBackgroundColor = Color.FromHex("#FFFFFFFF"),
        PhoneContrastForegroundColor = Color.FromHex("#FFFFFFFF"),
        PhoneContrastBackgroundColor = Color.FromHex("#DE000000"),
        PhoneDisabledColor = Color.FromHex("#4D000000"),
        PhoneProgressBarBackgroundColor = Color.FromHex("#19000000"),
        PhoneTextCaretColor = Color.FromHex("#DE000000"),
        PhoneTextBoxColor = Color.FromHex("#26000000"),
        PhoneTextBoxForegroundColor = Color.FromHex("#DE000000"),
        PhoneTextBoxEditBackgroundColor = Color.FromHex("#00000000"),
        PhoneTextBoxReadOnlyColor = Color.FromHex("#2E000000"),
        PhoneSubtleColor = Color.FromHex("#66000000"),
        PhoneTextBoxSelectionForegroundColor = Color.FromHex("#FFFFFFFF"),
        PhoneButtonBasePressedForegroundColor = Color.FromHex("#FFFFFFFF"),
        PhoneTextHighContrastColor = Color.FromHex("#DE000000"),
        PhoneTextMidContrastColor = Color.FromHex("#73000000"),
        PhoneTextLowContrastColor = Color.FromHex("#40000000"),
        PhoneSemitransparentColor = Color.FromHex("#AAFFFFFF"),
        PhoneChromeColor = Color.FromHex("#FFDDDDDD"),
        PhoneInactiveColor = Color.FromHex("#33000000"),
        PhoneInverseInactiveColor = Color.FromHex("#FFE5E5E5"),
        PhoneInverseBackgroundColor = Color.FromHex("#FFDDDDDD"),
        PhoneBorderColor = Color.FromHex("#99000000"),

        PhoneDarkThemeVisibility = Visibility.Collapsed,
        PhoneLightThemeVisibility = Visibility.Visible,
        PhoneDarkThemeOpacity = 0,
        PhoneLightThemeOpacity = 1,
        PhonePivotUnselectedItemOpacity = 0.23144,
    };


    public object? GetValue(string? name)
    {
        return name switch
        {
            "PhoneAccentColor" or "PhoneTextBoxEditBorderBrush" or "PhoneRadioCheckBoxPressedBrush" or "PhoneAccentBrush" => PhoneAccentColor,
            "PhoneForegroundColor" or "PhoneForegroundBrush" or "PhoneRadioCheckBoxBorderBrush" or "PhoneRadioCheckBoxCheckBrush" => PhoneForegroundColor,
            "PhoneBackgroundColor" or "PhoneBackgroundBrush" => PhoneBackgroundColor,
            "PhoneContrastForegroundColor" or "PhoneContrastForegroundBrush" => PhoneContrastForegroundColor,
            "PhoneContrastBackgroundColor" or "PhoneContrastBackgroundBrush" => PhoneContrastBackgroundColor,
            "PhoneDisabledColor" or "PhoneDisabledBrush" => PhoneDisabledColor,
            "PhoneProgressBarBackgroundColor" or "PhoneProgressBarBackgroundBrush" => PhoneProgressBarBackgroundColor,
            "PhoneTextCaretColor" or "PhoneTextCaretBrush" => PhoneTextCaretColor,
            "PhoneTextBoxColor" or "PhoneTextBoxBrush" => PhoneTextBoxColor,
            "PhoneTextBoxForegroundColor" or "PhoneTextBoxForegroundBrush" => PhoneTextBoxForegroundColor,
            "PhoneTextBoxEditBackgroundColor" or "PhoneTextBoxEditBackgroundBrush" => PhoneTextBoxEditBackgroundColor,
            "PhoneTextBoxReadOnlyColor" or "PhoneTextBoxReadOnlyBrush" => PhoneTextBoxReadOnlyColor,
            "PhoneSubtleColor" or "PhoneSubtleBrush" => PhoneSubtleColor,
            "PhoneTextBoxSelectionForegroundColor" or "PhoneTextBoxSelectionForegroundBrush" => PhoneTextBoxSelectionForegroundColor,
            "PhoneButtonBasePressedForegroundColor" or "PhoneButtonBasePressedForegroundBrush" => PhoneButtonBasePressedForegroundColor,
            "PhoneTextHighContrastColor" or "PhoneTextHighContrastBrush" => PhoneTextHighContrastColor,
            "PhoneTextMidContrastColor" or "PhoneTextMidContrastBrush" => PhoneTextMidContrastColor,
            "PhoneTextLowContrastColor" or "PhoneTextLowContrastBrush" => PhoneTextLowContrastColor,
            "PhoneSemitransparentColor" or "PhoneSemitransparentBrush" => PhoneSemitransparentColor,
            "PhoneChromeColor" or "PhoneChromeBrush" => PhoneChromeColor,
            "PhoneInactiveColor" or "PhoneInactiveBrush" => PhoneInactiveColor,
            "PhoneInverseInactiveColor" or "PhoneInverseInactiveBrush" => PhoneInverseInactiveColor,
            "PhoneInverseBackgroundColor" or "PhoneInverseBackgroundBrush" => PhoneInverseBackgroundColor,
            "PhoneBorderColor" or "PhoneBorderBrush" => PhoneBorderColor,

            "PhoneDarkThemeVisibility" => PhoneDarkThemeVisibility,
            "PhoneLightThemeVisibility" => PhoneLightThemeVisibility,
            "PhoneDarkThemeOpacity" => PhoneDarkThemeOpacity,
            "PhoneLightThemeOpacity" => PhoneLightThemeOpacity,
            "PhonePivotUnselectedItemOpacity" => PhonePivotUnselectedItemOpacity,

            _ => null,
        };
    }

    public readonly static string[] ResourceKeys = [
        "PhoneAccentColor",
        "PhoneForegroundColor",
        "PhoneBackgroundColor",
        "PhoneContrastForegroundColor",
        "PhoneContrastBackgroundColor",
        "PhoneDisabledColor",
        "PhoneProgressBarBackgroundColor",
        "PhoneTextCaretColor",
        "PhoneTextBoxColor",
        "PhoneTextBoxForegroundColor",
        "PhoneTextBoxEditBackgroundColor",
        "PhoneTextBoxReadOnlyColor",
        "PhoneSubtleColor",
        "PhoneTextBoxSelectionForegroundColor",
        "PhoneButtonBasePressedForegroundColor",
        "PhoneTextHighContrastColor",
        "PhoneTextMidContrastColor",
        "PhoneTextLowContrastColor",
        "PhoneSemitransparentColor",
        "PhoneChromeColor",
        "PhoneInactiveColor",
        "PhoneInverseInactiveColor",
        "PhoneInverseBackgroundColor",
        "PhoneBorderColor",

        "PhoneDarkThemeVisibility",
        "PhoneLightThemeVisibility",
        "PhoneDarkThemeOpacity",
        "PhoneLightThemeOpacity",
        "PhonePivotUnselectedItemOpacity",

        "PhoneForegroundBrush",
        "PhoneBackgroundBrush",
        "PhoneContrastForegroundBrush",
        "PhoneContrastBackgroundBrush",
        "PhoneTextCaretBrush",
        "PhoneTextBoxBrush",
        "PhoneTextBoxForegroundBrush",
        "PhoneTextBoxEditBackgroundBrush",
        "PhoneTextBoxEditBorderBrush",
        "PhoneTextBoxReadOnlyBrush",
        "PhoneTextBoxSelectionForegroundBrush",
        "PhoneSubtleBrush",
        "PhoneButtonBasePressedForegroundBrush",
        "PhoneRadioCheckBoxBrush",
        "PhoneRadioCheckBoxBorderBrush",
        "PhoneRadioCheckBoxCheckBrush",
        "PhoneRadioCheckBoxPressedBrush",
        "PhoneDisabledBrush",
        "PhoneSemitransparentBrush",
        "PhoneChromeBrush",
        "PhoneAccentBrush",
        "PhoneProgressBarBackgroundBrush",
        "PhoneTextHighContrastBrush",
        "PhoneTextMidContrastBrush",
        "PhoneTextLowContrastBrush",
        "PhoneInactiveBrush",
        "PhoneInverseInactiveBrush",
        "PhoneInverseBackgroundBrush",
        "PhoneBorderBrush",
        ];
}
