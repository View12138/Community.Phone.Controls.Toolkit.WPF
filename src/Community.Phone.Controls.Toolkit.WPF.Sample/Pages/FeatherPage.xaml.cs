using System.Windows;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Pages
{
    public partial class FeatherPage : PhoneApplicationPage
    {
        public FeatherPage()
        {
            InitializeComponent();
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
