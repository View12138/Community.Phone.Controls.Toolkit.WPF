using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Pages
{
    public partial class MainPage : PhoneApplicationPage
    {
        public MainPage()
        {
            InitializeComponent();
        }

        private void OnSampleSelected(object sender, SelectionChangedEventArgs e)
        {
            ListBoxItem item = SampleList.SelectedItem as ListBoxItem;
            if (item == null)
            {
                return;
            }

            SampleList.SelectedIndex = -1;

            TransitionKind kind = TransitionKind.Turnstile;
            PhoneApplicationPage target;

            switch (item.Tag as string)
            {
                case "controls":
                    target = new ControlsPage();
                    break;
                case "projection":
                    target = new ProjectionPage();
                    break;
                case "transitions":
                    target = new TransitionsPage();
                    break;
                case "feather":
                    target = new FeatherPage();
                    kind = TransitionKind.Feather;
                    break;
                case "menus":
                    target = new MenusPage();
                    break;
                default:
                    return;
            }

            SampleNavigator.Navigate(this, target, kind);
        }
    }
}
