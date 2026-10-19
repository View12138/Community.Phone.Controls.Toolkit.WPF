using System;
using System.Globalization;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.LocalizedResources;
using Xunit;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    public class ConverterTests
    {
        [Fact]
        public void OffOnConverterUsesLocalizedStrings()
        {
            OffOnConverter converter = new OffOnConverter();
            CultureInfo oldCulture = ControlResources.Culture;
            try
            {
                ControlResources.Culture = new CultureInfo("en-US");
                Assert.Equal("On", converter.Convert(true, typeof(string), null, CultureInfo.GetCultureInfo("en-US")));
                Assert.Equal("Off", converter.Convert(false, typeof(string), null, CultureInfo.GetCultureInfo("en-US")));
                Assert.Equal("Off", converter.Convert(null, typeof(string), null, CultureInfo.GetCultureInfo("en-US")));
            }
            finally
            {
                ControlResources.Culture = oldCulture;
            }
        }

        [Fact]
        public void TileSizeConvertersReturnExpectedDimensions()
        {
            TileSizeToWidthConverter width = new TileSizeToWidthConverter();
            TileSizeToHeightConverter height = new TileSizeToHeightConverter();

            Assert.Equal(99.0, width.Convert(TileSize.Small, typeof(double), null, CultureInfo.InvariantCulture));
            Assert.Equal(210.0, width.Convert(TileSize.Medium, typeof(double), null, CultureInfo.InvariantCulture));
            Assert.Equal(432.0, width.Convert(TileSize.Large, typeof(double), null, CultureInfo.InvariantCulture));
            Assert.Equal(173.0, width.Convert(TileSize.Default, typeof(double), null, CultureInfo.InvariantCulture));
            Assert.Equal(210.0, height.Convert(TileSize.Large, typeof(double), null, CultureInfo.InvariantCulture));
        }

        [Fact]
        public void RelativeTimeConverterFormatsMinutesAndHours()
        {
            RelativeTimeConverter converter = new RelativeTimeConverter();
            CultureInfo oldCulture = ControlResources.Culture;
            try
            {
                ControlResources.Culture = new CultureInfo("en-US");
                string minutes = (string)converter.Convert(
                    DateTime.Now.AddMinutes(-5),
                    typeof(string),
                    null,
                    CultureInfo.GetCultureInfo("en-US"));
                string hours = (string)converter.Convert(
                    DateTime.Now.AddHours(-3),
                    typeof(string),
                    null,
                    CultureInfo.GetCultureInfo("en-US"));

                Assert.Contains("5", minutes);
                Assert.Contains("minutes ago", minutes);
                Assert.Contains("3", hours);
                Assert.Contains("hours ago", hours);
            }
            finally
            {
                ControlResources.Culture = oldCulture;
            }
        }
    }
}
