using System;
using System.Windows;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Pages
{
    public partial class HomePage : PhoneApplicationPage
    {
        public HomePage()
        {
            InitializeComponent();
            Loaded += OnLoaded;
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
        }

        private void OnOpenDetail(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new DetailPage());
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
    }
}
