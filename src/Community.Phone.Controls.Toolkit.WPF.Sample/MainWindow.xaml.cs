using PhoneToolkitSample8;
using System.Windows;

namespace PhoneToolkitSample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public Microsoft.Phone.Controls.TransitionFrame PhoneFrame
        {
            get { return (Microsoft.Phone.Controls.TransitionFrame)FindName("Frame"); }
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            PhoneFrame.GoBack();
        }

        private void Home_Button_Click(object sender, RoutedEventArgs e)
        {
            if (PhoneFrame.Content.GetType() != typeof(MainPage))
            {
                PhoneFrame.Navigate(new MainPage());
            }
        }

        private void Search_Button_Click(object sender, RoutedEventArgs e)
        {

        }
    }
}
