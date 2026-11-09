using System;
using System.Globalization;
using System.Windows;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Pages
{
    public partial class WPFControlsSample : PhoneApplicationPage
    {
        public WPFControlsSample()
        {
            InitializeComponent();
        }

        private void OnRatingValueChanged(object sender, EventArgs e)
        {
            if (RatingStatus != null)
            {
                RatingStatus.Text = string.Format(CultureInfo.CurrentCulture, "Rating: {0:F1}", DemoRating.Value);
            }
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            if (Frame != null)
            {
                Frame.GoBack();
            }
        }
    }
}
