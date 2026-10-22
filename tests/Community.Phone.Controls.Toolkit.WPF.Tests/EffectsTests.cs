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
        public void TiltEffect_RotatesAboutTheVisibleContentNotTheLayoutSlot()
        {
            StaTest.Run(() =>
            {
                StackPanel panel = new StackPanel { Width = 441 };
                TiltEffect.SetIsTiltEnabled(panel, true);
                CheckBox checkBox = new CheckBox { Content = "Enable full screen", Margin = new Thickness(0, 12, 0, 0) };
                panel.Children.Add(checkBox);

                Window window = new Window { Width = 480, Height = 200, Content = panel };
                window.Resources.MergedDictionaries.Add(WpfTestHelper.Theme);
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    panel.UpdateLayout();

                    // The CheckBox template paints a left-aligned strip, so its
                    // first visual child is narrower than the layout slot.
                    FrameworkElement content = VisualTreeHelper.GetChild(checkBox, 0) as FrameworkElement;
                    Assert.NotNull(content);
                    Rect contentBounds = content.TransformToAncestor(checkBox).TransformBounds(
                        new Rect(0, 0, content.ActualWidth, content.ActualHeight));
                    Assert.True(
                        checkBox.ActualWidth > contentBounds.Width * 1.5,
                        "The test needs a control whose content does not fill its slot.");

                    // Press in the check box glyph with a deterministic point;
                    // the public entry point reads the real cursor position, so
                    // the internal overload is invoked directly.
                    Point press = checkBox.TranslatePoint(new Point(30, 30), panel);
                    typeof(TiltEffect).GetMethod(
                        "TryStartTiltEffect",
                        System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Static,
                        null,
                        new[] { typeof(FrameworkElement), typeof(FrameworkElement), typeof(Point), typeof(IInputElement) },
                        null).Invoke(null, new object[] { panel, checkBox, press, panel });
                    WpfTestHelper.PumpDispatcher();

                    PlaneProjection projection = ProjectionService.GetProjection(checkBox) as PlaneProjection;
                    Assert.NotNull(projection);
                    Assert.Equal(contentBounds.Left + (contentBounds.Width / 2), projection.CenterOfRotationX * checkBox.ActualWidth, 3);
                    Assert.Equal(contentBounds.Top + (contentBounds.Height / 2), projection.CenterOfRotationY * checkBox.ActualHeight, 3);
                }
                finally
                {
                    TiltEffect.SetIsTiltEnabled(panel, false);
                    window.Close();
                }
            });
        }

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
                    Assert.True(host.IsRenderingIn3D);
                    PlaneProjection projection = ProjectionService.GetProjection(button) as PlaneProjection;
                    Assert.NotNull(projection);
                    Assert.False(projection.IsIdentity);

                    double tiltMagnitude = Math.Abs(projection.RotationX) + Math.Abs(projection.RotationY);

                    button.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = Mouse.PreviewMouseUpEvent
                    });

                    // The release animation waits for TiltReturnAnimationDelay
                    // before it starts, so the tilt is still applied.
                    PumpFor(120);
                    Assert.Same(projection, ProjectionService.GetProjection(button));
                    Assert.True(tiltMagnitude > 0);

                    PumpFor(2000);

                    // The return animation has run and the render path is back
                    // to 2D.
                    Assert.Null(ProjectionService.GetProjection(button));
                    Assert.False(host.IsRenderingIn3D);
                }
                finally
                {
                    TiltEffect.SetIsTiltEnabled(panel, false);
                    window.Close();
                }
            });
        }

        [Fact]
        public void TiltEffect_DoesNotTiltThePageThatHostsTheElement()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationPage page = new PhoneApplicationPage();
                StackPanel panel = new StackPanel { Margin = new Thickness(12) };
                Button button = new Button { Content = "Accent button", Width = 200, Height = 60 };
                panel.Children.Add(button);
                page.Content = panel;
                TiltEffect.SetIsTiltEnabled(button, true);

                TransitionFrame frame = new TransitionFrame { Width = 480, Height = 800 };
                Window window = new Window { Width = 520, Height = 860, Content = frame };
                try
                {
                    window.Show();
                    frame.Content = page;
                    PumpFor(400);

                    ProjectionHost pageHost = ProjectionService.GetProjectionHost(page);
                    Assert.NotNull(pageHost);
                    Assert.Null(pageHost.Projection);

                    button.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = Mouse.PreviewMouseDownEvent
                    });
                    PumpFor(80);

                    PlaneProjection projection = ProjectionService.GetProjection(button) as PlaneProjection;
                    Assert.NotNull(projection);
                    Assert.False(projection.IsIdentity);

                    ProjectionHost buttonHost = ProjectionService.GetProjectionHost(button);
                    Assert.NotNull(buttonHost);
                    Assert.NotSame(pageHost, buttonHost);
                    Assert.Same(button, buttonHost.Content);
                    Assert.Same(projection, buttonHost.Projection);
                    Assert.Null(pageHost.Projection);

                    button.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                    {
                        RoutedEvent = Mouse.PreviewMouseUpEvent
                    });
                    PumpFor(1200);

                    Assert.Null(ProjectionService.GetProjection(button));
                    Assert.Null(buttonHost.Projection);
                    Assert.Null(pageHost.Projection);
                }
                finally
                {
                    TiltEffect.SetIsTiltEnabled(button, false);
                    window.Close();
                }
            });
        }

        [Fact]
        public void TurnstileFeatherEffect_AnimatesFeatheredElements()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationPage page = new PhoneApplicationPage();
                StackPanel panel = new StackPanel { Width = 320, Height = 220 };
                Border first = new Border { Width = 100, Height = 40, Background = Brushes.Blue };
                Border second = new Border { Width = 100, Height = 40, Background = Brushes.Green };
                panel.Children.Add(first);
                panel.Children.Add(second);
                page.Content = panel;

                Window window = new Window { Width = 400, Height = 300, Content = page };
                ITransition transition = null;
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    TurnstileFeatherEffect.SetFeatheringIndex(first, 0);
                    TurnstileFeatherEffect.SetFeatheringIndex(second, 1);

                    transition = new TurnstileFeatherTransition
                    {
                        Mode = TurnstileFeatherTransitionMode.ForwardIn
                    }.GetTransition(page);
                    transition.Begin();
                    PumpFor(120);

                    PlaneProjection firstProjection = ProjectionService.GetProjection(first) as PlaneProjection;
                    Assert.NotNull(firstProjection);
                    Assert.False(firstProjection.IsIdentity);
                    Assert.True(first.Opacity > 0 && first.Opacity <= 1);
                }
                finally
                {
                    if (transition != null)
                    {
                        transition.Stop();
                    }

                    TurnstileFeatherEffect.SetFeatheringIndex(first, -1);
                    TurnstileFeatherEffect.SetFeatheringIndex(second, -1);
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
                    PumpFor(50);

                    // The effect wraps elements when they subscribe, so a
                    // transition never has to modify the visual tree while it
                    // is animating.
                    ProjectionHost firstHost = ProjectionService.GetProjectionHost(first);
                    ProjectionHost secondHost = ProjectionService.GetProjectionHost(second);
                    Assert.NotNull(firstHost);
                    Assert.NotNull(secondHost);
                    Assert.Same(first, firstHost.Content);
                    Assert.Same(second, secondHost.Content);
                    Assert.Null(firstHost.Projection);

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
