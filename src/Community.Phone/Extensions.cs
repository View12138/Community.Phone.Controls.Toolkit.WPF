using Microsoft.Phone.Controls;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Xml;

namespace Microsoft.Phone;

internal static class Extensions
{
    extension(Color)
    {
        public static Color FromHex(string hex)
        {
            hex = hex.TrimStart('#');
            if (hex.Length != 6 && hex.Length != 8)
                throw new ArgumentException("Hex string must be 6 or 8 characters long.", nameof(hex));

            if (hex.Length == 6)
            { hex = "FF" + hex; }
            byte a = Convert.ToByte(hex[0..2], 16);
            byte r = Convert.ToByte(hex[2..4], 16);
            byte g = Convert.ToByte(hex[4..6], 16);
            byte b = Convert.ToByte(hex[6..8], 16);
            return Color.FromArgb(a, r, g, b);
        }

        public static Color FromHexValue(int hex)
        {
            byte a = (byte)((hex >> 24) & 0xFF);
            byte r = (byte)((hex >> 16) & 0xFF);
            byte g = (byte)((hex >> 8) & 0xFF);
            byte b = (byte)(hex & 0xFF);
            return Color.FromArgb(a, r, g, b);
        }
    }

    extension(DesignerProperties)
    {
        public static bool IsInDesignTool { get => false; }
    }

    extension(XamlReader)
    {
        public static object Load(string xaml)
        {
            using StringReader stringReader = new StringReader(xaml);
            using XmlReader xmlReader = XmlReader.Create(stringReader);
            return XamlReader.Load(xmlReader);
        }
    }
}
