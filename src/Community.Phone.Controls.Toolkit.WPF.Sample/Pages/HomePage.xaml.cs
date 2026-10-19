using System;
using System.Globalization;
using System.Windows;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Pages
{
    public partial class HomePage : PhoneApplicationPage
    {
        private readonly DispatcherTimer _projectionStatusTimer;

        public HomePage()
        {
            InitializeComponent();

            _projectionStatusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _projectionStatusTimer.Tick += OnProjectionStatusTick;

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = -35,
                To = 35,
                Duration = TimeSpan.FromSeconds(2.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            DemoProjection.BeginAnimation(PlaneProjection.RotationYProperty, animation);
            UpdateProjectionStatus();
            _projectionStatusTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _projectionStatusTimer.Stop();
        }

        private void OnProjectionStatusTick(object sender, EventArgs e)
        {
            UpdateProjectionStatus();
        }

        private void UpdateProjectionStatus()
        {
            if (ProjectionStatus != null)
            {
                ProjectionStatus.Text = string.Format(
                    CultureInfo.CurrentCulture,
                    "RotationX {0:F0}°, RotationY {1:F0}°",
                    DemoProjection.RotationX,
                    DemoProjection.RotationY);
            }
        }

        private void OnOpenDetail(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DetailPage());
        }

        private void OnOpenFeatherPage(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new FeatherPage());
        }

        private void OnAnimateProjection(object sender, RoutedEventArgs e)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = DemoProjection.RotationX,
                To = DemoProjection.RotationX == 0 ? 35 : 0,
                Duration = TimeSpan.FromSeconds(0.6),
                EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
            };

            DemoProjection.BeginAnimation(PlaneProjection.RotationXProperty, animation);
        }

        private void OnRatingValueChanged(object sender, EventArgs e)
        {
            if (RatingStatus != null)
            {
                RatingStatus.Text = string.Format(CultureInfo.CurrentCulture, "Rating: {0:F1}", DemoRating.Value);
            }
        }

        private void OnContextMenuCopy(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("ContextMenu item clicked.", "ContextMenu");
        }

        private void OnShowMessage(object sender, RoutedEventArgs e)
        {
            CustomMessageBox messageBox = new CustomMessageBox
            {
                Title = "CustomMessageBox",
                Caption = "Metro dialog",
                Message = "This dialog is part of the WPF toolkit port.",
                LeftButtonContent = "cancel",
                RightButtonContent = "ok"
            };

            messageBox.Dismissed += (s, args) =>
            {
                if (args.Result == CustomMessageBoxResult.RightButton)
                {
                    MessageBox.Show("The right button was selected.", "CustomMessageBox");
                }
            };

            messageBox.Show();
        }
    }
}
