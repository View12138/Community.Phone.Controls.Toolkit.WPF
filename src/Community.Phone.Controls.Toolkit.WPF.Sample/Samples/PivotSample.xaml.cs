using System.Windows;
using System.Windows.Controls;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Samples
{
    public partial class PivotSample : PhoneApplicationPage
    {
        public PivotSample()
        {
            InitializeComponent();
            UpdateStatus();
        }

        private void OnPivotSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateStatus();
        }

        private void OnLockChanged(object sender, RoutedEventArgs e)
        {
            if (SamplePivot == null)
            {
                return;
            }

            SamplePivot.IsLocked = LockToggle.IsChecked == true;
        }

        private void UpdateStatus()
        {
            if (SelectionStatus == null || SamplePivot == null)
            {
                return;
            }

            SelectionStatus.Text = string.Format(
                "SelectedIndex {0} of {1}",
                SamplePivot.SelectedIndex,
                SamplePivot.Items.Count);
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            SampleNavigator.GoBack(this, TransitionKind.Turnstile);
        }
    }
}
