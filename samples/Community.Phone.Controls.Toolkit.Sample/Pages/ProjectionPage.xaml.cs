using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample.Pages
{
    public partial class ProjectionPage : PhoneApplicationPage
    {
        private readonly DispatcherTimer _statusTimer;

        public ProjectionPage()
        {
            InitializeComponent();

            _statusTimer = new DispatcherTimer
            {
                Interval = TimeSpan.FromMilliseconds(100)
            };
            _statusTimer.Tick += OnStatusTick;

            ProjectedList.ItemsSource = CreateItems();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;
        }

        private static IEnumerable<string> CreateItems()
        {
            List<string> items = new List<string>();
            for (int i = 1; i <= 200; i++)
            {
                items.Add("Item " + i.ToString(CultureInfo.InvariantCulture));
            }

            return items;
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

            DoubleAnimation hostlessAnimation = new DoubleAnimation
            {
                From = -22,
                To = 22,
                Duration = TimeSpan.FromSeconds(3.5),
                AutoReverse = true,
                RepeatBehavior = RepeatBehavior.Forever,
                BeginTime = TimeSpan.FromSeconds(0.4),
                EasingFunction = new SineEase { EasingMode = EasingMode.EaseInOut }
            };

            HostlessProjection.BeginAnimation(PlaneProjection.RotationYProperty, hostlessAnimation);

            UpdateStatus();
            _statusTimer.Start();
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            _statusTimer.Stop();
        }

        private void OnStatusTick(object sender, EventArgs e)
        {
            UpdateStatus();
        }

        private void UpdateStatus()
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

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            if (Frame != null)
            {
                Frame.GoBack();
            }
        }
    }
}
