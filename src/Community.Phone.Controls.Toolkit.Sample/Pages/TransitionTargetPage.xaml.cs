using System.Windows;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Pages
{
    public partial class TransitionTargetPage : PhoneApplicationPage
    {
        private readonly TransitionKind _kind;

        public TransitionTargetPage()
            : this(TransitionKind.Turnstile)
        {
        }

        internal TransitionTargetPage(TransitionKind kind)
        {
            _kind = kind;
            InitializeComponent();

            if (TransitionStatus != null)
            {
                TransitionStatus.Text = "The " + kind + " transition brought you here.";
            }
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            SampleNavigator.GoBack(this, _kind);
        }
    }
}
