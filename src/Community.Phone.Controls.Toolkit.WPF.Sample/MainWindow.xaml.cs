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
    }
}
