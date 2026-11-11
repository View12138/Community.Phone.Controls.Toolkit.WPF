using System.Reflection;
using System.Windows.Media;

namespace Microsoft.Phone.Shell;

public enum AccentColor
{
    Auto = -1,
    Amber = 0,
    Brown,
    Cobalt,
    Crimson,
    Cyan,
    Emerald,
    Green,
    Indigo,
    Lime,
    Magenta,
    Mauve,
    Olive,
    Orange,
    Pink,
    Red,
    Steel,
    Taupe,
    Teal,
    Violet,
    Yellow,
}

public static class AccentColors
{
    public static Color From(AccentColor accentColor) => accentColor switch
    {
        AccentColor.Auto => ThemeManager.GetSystemAccentColor(),
        AccentColor.Amber => Amber,
        AccentColor.Brown => Brown,
        AccentColor.Cobalt => Cobalt,
        AccentColor.Crimson => Crimson,
        AccentColor.Cyan => Cyan,
        AccentColor.Emerald => Emerald,
        AccentColor.Green => Green,
        AccentColor.Indigo => Indigo,
        AccentColor.Lime => Lime,
        AccentColor.Magenta => Magenta,
        AccentColor.Mauve => Mauve,
        AccentColor.Olive => Olive,
        AccentColor.Orange => Orange,
        AccentColor.Pink => Pink,
        AccentColor.Red => Red,
        AccentColor.Steel => Steel,
        AccentColor.Taupe => Taupe,
        AccentColor.Teal => Teal,
        AccentColor.Violet => Violet,
        AccentColor.Yellow => Yellow,
        _ => Cobalt,
    };

    public readonly static Color Amber = (Color)ColorConverter.ConvertFromString("#FFF0A30A");
    public readonly static Color Brown = (Color)ColorConverter.ConvertFromString("#FF825A2C");
    public readonly static Color Cobalt = (Color)ColorConverter.ConvertFromString("#FF0050EF");
    public readonly static Color Crimson = (Color)ColorConverter.ConvertFromString("#FFA20025");
    public readonly static Color Cyan = (Color)ColorConverter.ConvertFromString("#FF1BA1E2");
    public readonly static Color Emerald = (Color)ColorConverter.ConvertFromString("#FF008A00");
    public readonly static Color Green = (Color)ColorConverter.ConvertFromString("#FF60A917");
    public readonly static Color Indigo = (Color)ColorConverter.ConvertFromString("#FF6A00FF");
    public readonly static Color Lime = (Color)ColorConverter.ConvertFromString("#FFA4C400");
    public readonly static Color Magenta = (Color)ColorConverter.ConvertFromString("#FFD80073");
    public readonly static Color Mauve = (Color)ColorConverter.ConvertFromString("#FF76608A");
    public readonly static Color Olive = (Color)ColorConverter.ConvertFromString("#FF6D8764");
    public readonly static Color Orange = (Color)ColorConverter.ConvertFromString("#FFFA6800");
    public readonly static Color Pink = (Color)ColorConverter.ConvertFromString("#FFF472D0");
    public readonly static Color Red = (Color)ColorConverter.ConvertFromString("#FFE51400");
    public readonly static Color Steel = (Color)ColorConverter.ConvertFromString("#FF647687");
    public readonly static Color Taupe = (Color)ColorConverter.ConvertFromString("#FF87794E");
    public readonly static Color Teal = (Color)ColorConverter.ConvertFromString("#FF00ABA9");
    public readonly static Color Violet = (Color)ColorConverter.ConvertFromString("#FFAA00FF");
    public readonly static Color Yellow = (Color)ColorConverter.ConvertFromString("#FFE3C800");
}