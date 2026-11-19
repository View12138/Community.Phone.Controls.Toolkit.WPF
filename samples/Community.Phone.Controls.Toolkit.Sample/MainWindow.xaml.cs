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

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if(e.Key == System.Windows.Input.Key.F1)
            {
                PhoneFrame.GoBack();
            }
            if (e.Key == System.Windows.Input.Key.F2)
            {
                if (PhoneFrame.Content.GetType() != typeof(MainPage))
                {
                    PhoneFrame.Navigate(new MainPage());
                }
            }
            if (e.Key == System.Windows.Input.Key.F1)
            {

            }
        }
    }
}
