using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Pages
{
    public partial class TransitionsPage : PhoneApplicationPage
    {
        private static readonly TransitionKind[] Kinds =
        {
            TransitionKind.Turnstile,
            TransitionKind.Slide,
            TransitionKind.Rotate,
            TransitionKind.Swivel,
            TransitionKind.Roll,
            TransitionKind.Feather
        };

        public TransitionsPage()
        {
            InitializeComponent();
            FamilyPicker.SelectedIndex = 0;
        }

        private TransitionKind SelectedKind
        {
            get
            {
                int index = FamilyPicker.SelectedIndex;
                return index >= 0 && index < Kinds.Length ? Kinds[index] : TransitionKind.Turnstile;
            }
        }

        private void OnFamilyChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FamilyStatus != null)
            {
                FamilyStatus.Text = "The target page will use the " + SelectedKind + " family in both directions.";
            }
        }

        private void OnOpenTarget(object sender, RoutedEventArgs e)
        {
            SampleNavigator.Navigate(this, new TransitionTargetPage(SelectedKind), SelectedKind);
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            SampleNavigator.GoBack(this, SelectedKind);
        }
    }
}
