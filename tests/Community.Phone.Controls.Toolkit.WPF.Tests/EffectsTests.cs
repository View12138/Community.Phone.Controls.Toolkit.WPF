using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Xunit;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    public class EffectsTests
    {
        [Fact]
        public void TiltEffect_WrapsAChildButtonAndAnimatesARealProjection()
        {
            StaTest.Run(() =>
            {
                StackPanel panel = new StackPanel { Width = 320, Height = 220, Background = Brushes.Transparent };
                Button button = new Button { Content = "Tilt", Width = 200, Height = 60 };
                panel.Children.Add(button);
                TiltEffect.SetIsTiltEnabled(panel, true);

                Window window = new Window { Width = 400, Height = 300, Content = panel };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    button.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = Mouse.PreviewMouseDownEvent
                    });
                    WpfTestHelper.PumpDispatcher();

                    ProjectionHost host = ProjectionService.GetProjectionHost(button);
                    Assert.NotNull(host);
                    PlaneProjection projection = ProjectionService.GetProjection(button) as PlaneProjection;
                    Assert.NotNull(projection);
                    Assert.False(projection.IsIdentity);

                    button.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = Mouse.PreviewMouseUpEvent
                    });
                    PumpFor(700);

                    Assert.Null(ProjectionService.GetProjection(button));
                }
                finally
                {
                    TiltEffect.SetIsTiltEnabled(panel, false);
                    window.Close();
                }
            });
        }

        [Fact]
        public void TiltEffect_DoesNotWrapWhenSuppressed()
        {
            StaTest.Run(() =>
            {
                StackPanel panel = new StackPanel { Width = 320, Height = 220 };
                Button button = new Button { Content = "No tilt", Width = 200, Height = 60 };
                panel.Children.Add(button);
                TiltEffect.SetIsTiltEnabled(panel, true);
                TiltEffect.SetSuppressTilt(button, true);

                Window window = new Window { Width = 400, Height = 300, Content = panel };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    button.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = Mouse.PreviewMouseDownEvent
                    });
                    WpfTestHelper.PumpDispatcher();

                    Assert.Null(ProjectionService.GetProjection(button));
                }
                finally
                {
                    TiltEffect.SetIsTiltEnabled(panel, false);
                    window.Close();
                }
            });
        }

        [Fact]
        public void TurnstileFeatherEffect_FeathersElementsInOrderAndCleansUp()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationPage page = new PhoneApplicationPage();
                StackPanel panel = new StackPanel { Margin = new Thickness(20) };
                TextBlock first = new TextBlock { Text = "First", FontSize = 24, Height = 50 };
                TextBlock second = new TextBlock { Text = "Second", FontSize = 24, Height = 50 };
                panel.Children.Add(first);
                panel.Children.Add(second);
                page.Content = panel;

                Window window = new Window { Width = 480, Height = 800, Content = page };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    page.UpdateLayout();

                    TurnstileFeatherEffect.SetFeatheringIndex(first, 0);
                    TurnstileFeatherEffect.SetFeatheringIndex(second, 1);

                    ITransition transition = new TurnstileFeatherTransition
                    {
                        Mode = TurnstileFeatherTransitionMode.ForwardIn
                    }.GetTransition(page);

                    Assert.NotNull(transition);
                    transition.Begin();
                    PumpFor(120);

                    Assert.NotNull(ProjectionService.GetProjection(first));
                    Assert.NotNull(ProjectionService.GetProjection(second));

                    PumpFor(1200);

                    Assert.Equal(ClockState.Stopped, transition.GetCurrentState());
                    Assert.Null(ProjectionService.GetProjection(first));
                    Assert.Null(ProjectionService.GetProjection(second));
                    Assert.Equal(1.0, first.Opacity, 2);
                    Assert.Equal(1.0, second.Opacity, 2);
                }
                finally
                {
                    TurnstileFeatherEffect.SetFeatheringIndex(first, -1);
                    TurnstileFeatherEffect.SetFeatheringIndex(second, -1);
                    window.Close();
                }
            });
        }

        [Fact]
        public void TurnstileFeatherEffect_RejectsPageAsATarget()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationPage page = new PhoneApplicationPage();
                Assert.Throws<InvalidOperationException>(() =>
                    TurnstileFeatherEffect.SetFeatheringIndex(page, 0));
            });
        }

        private static void PumpFor(int milliseconds)
        {
            DateTime end = DateTime.UtcNow.AddMilliseconds(milliseconds);
            while (DateTime.UtcNow < end)
            {
                WpfTestHelper.PumpDispatcher();
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
