using Microsoft.Phone.Controls;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    public partial class NavigationCachePage : PhoneApplicationPage
    {
        public NavigationCachePage()
        {
            InstanceCount++;
            InitializeComponent();
        }

        public static int InstanceCount { get; set; }
    }
}
