using System;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using Microsoft.Phone.Controls;
using Xunit;

namespace Community.Phone.Controls.Toolkit.Tests
{
    /// <summary>
    /// Guards the values that come from the original
    /// <c>Transitions/Storyboards/*.xaml</c> resources: every family has to
    /// animate the same property over the same range as the toolkit.
    /// </summary>
    public class TransitionStoryboardTests
    {
        [Fact]
        public void RollTransition_AnimatesRotationZOverTheOriginalRange()
        {
            RunTransitionTest(
                () => new RollTransition(),
                (host, content) =>
                {
                    PlaneProjection projection = (PlaneProjection)ProjectionService.GetProjection(content);
                    Assert.NotNull(projection);
                    Assert.Equal(0.5, projection.CenterOfRotationX, 3);
                    Assert.Equal(0.5, projection.CenterOfRotationY, 3);
                    Assert.True(
                        projection.RotationZ > 5 && projection.RotationZ < 90,
                        "Roll should run the original 0 -> 45 -> 90 rotation; RotationZ=" + projection.RotationZ);
                });
        }

        [Fact]
        public void RotateTransition_AnimatesRotationZOverTheOriginalRange()
        {
            RunTransitionTest(
                () => new RotateTransition { Mode = RotateTransitionMode.In90Clockwise },
                (host, content) =>
                {
                    PlaneProjection projection = (PlaneProjection)ProjectionService.GetProjection(content);
                    Assert.NotNull(projection);
                    Assert.True(
                        projection.RotationZ > 0 && projection.RotationZ < 90,
                        "Rotate In90Clockwise should run the original 90 -> 0 rotation; RotationZ=" + projection.RotationZ);
                });
        }

        [Fact]
        public void SwivelTransition_AnimatesRotationXOverTheOriginalRange()
        {
            RunTransitionTest(
                () => new SwivelTransition { Mode = SwivelTransitionMode.ForwardIn },
                (host, content) =>
                {
                    PlaneProjection projection = (PlaneProjection)ProjectionService.GetProjection(content);
                    Assert.NotNull(projection);
                    Assert.True(
                        projection.RotationX > -45 && projection.RotationX < 0,
                        "Swivel ForwardIn should run the original -45 -> 0 rotation; RotationX=" + projection.RotationX);
                });
        }

        [Fact]
        public void TurnstileTransition_AnimatesRotationYOverTheOriginalRange()
        {
            RunTransitionTest(
                () => new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn },
                (host, content) =>
                {
                    PlaneProjection projection = (PlaneProjection)ProjectionService.GetProjection(content);
                    Assert.NotNull(projection);
                    Assert.Equal(-0.05, projection.CenterOfRotationX, 3);
                    Assert.True(
                        projection.RotationY > -80 && projection.RotationY < 0,
                        "Turnstile ForwardIn should run the original -80 -> 0 rotation; RotationY=" + projection.RotationY);
                    Assert.True(host.IsRenderingIn3D, "A projected transition has to render through the 3D path.");
                });
        }

        [Fact]
        public void SlideTransition_AnimatesTheRenderTransformOverTheOriginalRange()
        {
            RunTransitionTest(
                () => new SlideTransition { Mode = SlideTransitionMode.SlideLeftFadeIn },
                (host, content) =>
                {
                    Assert.Null(ProjectionService.GetProjection(content));

                    TranslateTransform transform = content.RenderTransform as TranslateTransform;
                    Assert.NotNull(transform);
                    Assert.True(
                        transform.X > 0 && transform.X < 200,
                        "SlideLeftFadeIn should run the original 200 -> 0 offset; X=" + transform.X);

                    Assert.True(
                        content.Opacity > 0 && content.Opacity < 1,
                        "SlideLeftFadeIn should fade the content in; Opacity=" + content.Opacity);
                });
        }

        [Fact]
        public void Transition_ReportsStateAndTimeBeforeItBegins()
        {
            StaTest.Run(() =>
            {
                Border content = new Border { Background = Brushes.Red, Width = 200, Height = 100 };
                ProjectionHost host = new ProjectionHost { Width = 200, Height = 100, Content = content };
                Window window = new Window { Width = 300, Height = 200, Content = host };
                ITransition transition = null;

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    transition = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn }.GetTransition(content);

                    // WPF's parameterless Storyboard.GetCurrentState() throws
                    // for a storyboard that has no clock yet, which used to
                    // break TransitionFrame.EnsureStoppedTransition.
                    Assert.Equal(ClockState.Stopped, transition.GetCurrentState());
                    Assert.Equal(TimeSpan.Zero, transition.GetCurrentTime());

                    // None of the controls may fail before the storyboard runs.
                    transition.Pause();
                    transition.Resume();
                    transition.Seek(TimeSpan.Zero);
                    transition.SeekAlignedToLastTick(TimeSpan.Zero);
                    transition.SkipToFill();
                    transition.Stop();
                    Assert.Equal(ClockState.Stopped, transition.GetCurrentState());
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Transition_RaisesCompletedOncePerRun()
        {
            StaTest.Run(() =>
            {
                Border content = new Border { Background = Brushes.Red, Width = 200, Height = 100 };
                ProjectionHost host = new ProjectionHost { Width = 200, Height = 100, Content = content };
                Window window = new Window { Width = 300, Height = 200, Content = host };
                ITransition transition = null;
                int completed = 0;

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    transition = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardOut }.GetTransition(content);
                    transition.Completed += delegate { completed++; };

                    transition.Begin();
                    Assert.Equal(ClockState.Active, transition.GetCurrentState());

                    PumpFor(700);
                    Assert.Equal(1, completed);
                    Assert.Equal(ClockState.Stopped, transition.GetCurrentState());

                    // Stopping a finished transition must not report it again:
                    // the frame completes the navigation from this event.
                    transition.Stop();
                    transition.Stop();
                    Assert.Equal(1, completed);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ProjectedTransition_ClearsTheProjectionWhenItStops()
        {
            StaTest.Run(() =>
            {
                Border content = new Border { Background = Brushes.Red, Width = 200, Height = 100 };
                ProjectionHost host = new ProjectionHost { Width = 200, Height = 100, Content = content };
                Window window = new Window { Width = 300, Height = 200, Content = host };
                ITransition transition = null;

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    transition = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn }.GetTransition(content);
                    transition.Begin();
                    PumpFor(80);

                    Assert.NotNull(ProjectionService.GetProjection(content));

                    transition.Stop();
                    Assert.Null(ProjectionService.GetProjection(content));
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Theory]
        [InlineData("Turnstile")]
        [InlineData("Slide")]
        [InlineData("Rotate")]
        [InlineData("Swivel")]
        [InlineData("Roll")]
        public void TransitionFrame_NavigatesWithEveryFamily(string family)
        {
            StaTest.Run(() =>
            {
                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage first = new TestPage();
                TestPage second = new TestPage();

                NavigationOutTransition outTransition = new NavigationOutTransition { Forward = CreateOutTransition(family) };
                NavigationInTransition inTransition = new NavigationInTransition { Forward = CreateInTransition(family) };
                TransitionService.SetNavigationOutTransition(first, outTransition);
                TransitionService.SetNavigationInTransition(second, inTransition);

                int outEnded = 0;
                int inBegun = 0;
                int inEnded = 0;
                outTransition.EndTransition += delegate { outEnded++; };
                inTransition.BeginTransition += delegate { inBegun++; };
                inTransition.EndTransition += delegate { inEnded++; };

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    frame.ApplyTemplate();
                    window.UpdateLayout();

                    frame.Navigate(first);
                    Assert.True(WpfTestHelper.PumpUntil(() => ReferenceEquals(frame.Content, first) && first.IsHitTestVisible));

                    frame.Navigate(second);
                    Assert.True(WpfTestHelper.PumpUntil(() => inEnded == 1), "the entry transition did not complete");

                    Assert.Same(second, frame.Content);
                    Assert.Equal(1, outEnded);
                    Assert.Equal(1, inBegun);
                    Assert.Equal(1, inEnded);
                    Assert.True(second.IsHitTestVisible);
                    Assert.Null(ProjectionService.GetProjection(second));
                }
                finally
                {
                    window.Close();
                }
            });
        }

        private static TransitionElement CreateInTransition(string family)
        {
            switch (family)
            {
                case "Slide":
                    return new SlideTransition { Mode = SlideTransitionMode.SlideLeftFadeIn };
                case "Rotate":
                    return new RotateTransition { Mode = RotateTransitionMode.In90Clockwise };
                case "Swivel":
                    return new SwivelTransition { Mode = SwivelTransitionMode.ForwardIn };
                case "Roll":
                    return new RollTransition();
                default:
                    return new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn };
            }
        }

        private static TransitionElement CreateOutTransition(string family)
        {
            switch (family)
            {
                case "Slide":
                    return new SlideTransition { Mode = SlideTransitionMode.SlideRightFadeOut };
                case "Rotate":
                    return new RotateTransition { Mode = RotateTransitionMode.Out90Clockwise };
                case "Swivel":
                    return new SwivelTransition { Mode = SwivelTransitionMode.ForwardOut };
                case "Roll":
                    return new RollTransition();
                default:
                    return new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardOut };
            }
        }

        private sealed class TestPage : PhoneApplicationPage
        {
            public TestPage()
            {
                Content = new Grid();
            }
        }

        private static void RunTransitionTest(Func<TransitionElement> createElement, Action<ProjectionHost, Border> verify)
        {
            StaTest.Run(() =>
            {
                Border content = new Border { Background = Brushes.Red, Width = 200, Height = 100 };
                ProjectionHost host = new ProjectionHost { Width = 200, Height = 100, Content = content };
                Window window = new Window { Width = 300, Height = 200, Content = host };
                ITransition transition = null;

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    transition = createElement().GetTransition(content);
                    Assert.NotNull(transition);

                    transition.Begin();
                    PumpFor(110);
                    verify(host, content);
                }
                finally
                {
                    if (transition != null)
                    {
                        transition.Stop();
                    }

                    window.Close();
                }
            });
        }

        private static void PumpFor(int milliseconds)
        {
            DateTime end = DateTime.UtcNow.AddMilliseconds(milliseconds);
            while (DateTime.UtcNow < end)
            {
                WpfTestHelper.PumpDispatcher();
                Thread.Sleep(10);
            }
        }
    }
}
