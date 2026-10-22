using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Xunit;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    public class NavigationTests
    {
        [Fact]
        public void Frame_NavigatesAndRaisesPageLifecycle()
        {
            StaTest.Run(() =>
            {
                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                int firstTo = 0;
                int firstFrom = 0;
                int secondTo = 0;

                TestPage first = new TestPage();
                first.NavigatedTo += delegate { firstTo++; };
                first.NavigatedFrom += delegate { firstFrom++; };
                TestPage second = new TestPage();
                second.NavigatedTo += delegate { secondTo++; };

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    frame.Navigate(first);
                    PumpFor(300);
                    Assert.Same(first, frame.Content);
                    Assert.Equal(1, firstTo);

                    frame.Navigate(second);
                    PumpFor(300);
                    Assert.Same(second, frame.Content);
                    Assert.Equal(1, firstFrom);
                    Assert.Equal(1, secondTo);

                    Assert.True(frame.CanGoBack);
                    frame.GoBack();
                    PumpFor(300);
                    Assert.Same(first, frame.Content);
                    Assert.Equal(2, firstTo);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Frame_NavigationContextCarriesParameters()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationFrame frame = new PhoneApplicationFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage page = new TestPage();
                NavigationContext context = null;
                page.NavigatedTo += delegate { context = page.NavigationContext; };

                Dictionary<string, string> parameters = new Dictionary<string, string>
                {
                    { "id", "42" },
                    { "mode", "edit" }
                };

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(frame.Navigate(page, parameters));
                    PumpFor(250);

                    Assert.NotNull(context);
                    Assert.Equal("42", context.QueryString["id"]);
                    Assert.Equal("edit", context.QueryString["mode"]);
                    Assert.Same(parameters, context.ExtraData);
                    Assert.Null(context.Uri);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Frame_StateSurvivesBackNavigation()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationFrame frame = new PhoneApplicationFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage first = new TestPage();
                TestPage second = new TestPage();

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    frame.Navigate(first);
                    PumpFor(200);
                    first.State["value"] = 7;

                    frame.Navigate(second);
                    PumpFor(200);
                    Assert.Same(second, frame.Content);

                    Assert.True(frame.CanGoBack);
                    frame.GoBack();
                    PumpFor(200);

                    Assert.Same(first, frame.Content);
                    Assert.Equal(7, first.State["value"]);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Frame_BackKeyPressCanCancelBackNavigation()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationFrame frame = new PhoneApplicationFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage first = new TestPage();
                TestPage second = new TestPage();

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    frame.Navigate(first);
                    PumpFor(200);
                    frame.Navigate(second);
                    PumpFor(200);

                    second.BackKeyPress += delegate(object sender, System.ComponentModel.CancelEventArgs e)
                    {
                        e.Cancel = true;
                    };

                    frame.GoBack();
                    PumpFor(200);
                    Assert.Same(second, frame.Content);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Frame_RapidNavigationKeepsTheLatestPage()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationFrame frame = new PhoneApplicationFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage first = new TestPage();
                TestPage second = new TestPage();

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    frame.Navigate(first);
                    frame.Navigate(second);
                    PumpFor(400);

                    Assert.Same(second, frame.Content);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void TransitionFrame_PlaysExitAndEntryTransitionsAndRestoresInteractivity()
        {
            StaTest.Run(() =>
            {
                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage first = new TestPage();
                TestPage second = new TestPage();

                NavigationOutTransition outTransition = new NavigationOutTransition
                {
                    Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardOut }
                };
                NavigationInTransition inTransition = new NavigationInTransition
                {
                    Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn }
                };
                TransitionService.SetNavigationOutTransition(first, outTransition);
                TransitionService.SetNavigationInTransition(second, inTransition);

                int outBegun = 0;
                int outEnded = 0;
                int inBegun = 0;
                int inEnded = 0;
                outTransition.BeginTransition += delegate { outBegun++; };
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
                    PumpFor(300);
                    frame.Navigate(second);
                    PumpFor(100);

                    Assert.Equal(1, outBegun);
                    Assert.Same(second, frame.Content);

                    PumpFor(1000);
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

        [Fact]
        public void TurnstileTransition_AnimatesAStandalonePage()
        {
            StaTest.Run(() =>
            {
                Border content = new Border { Background = Brushes.Blue };
                ProjectionHost host = new ProjectionHost
                {
                    Width = 480,
                    Height = 800,
                    Content = content
                };
                Window window = new Window { Width = 480, Height = 800, Content = host };
                ITransition transition = null;
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    transition = new TurnstileTransition
                    {
                        Mode = TurnstileTransitionMode.ForwardOut
                    }.GetTransition(content);
                    transition.Begin();
                    PumpFor(80);

                    PlaneProjection projection = (PlaneProjection)ProjectionService.GetProjection(content);
                    Assert.NotNull(projection);
                    Assert.NotEqual(0, projection.RotationY, 2);
                    Assert.True(host.IsRenderingIn3D);
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

        [Fact]
        public void TransitionFrame_HostsInitialContentInTheOutgoingPresenter()
        {
            StaTest.Run(() =>
            {
                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage first = new TestPage();
                TestPage second = new TestPage();
                NavigationOutTransition outTransition = new NavigationOutTransition
                {
                    Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardOut }
                };
                NavigationInTransition inTransition = new NavigationInTransition
                {
                    Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn }
                };
                TransitionService.SetNavigationOutTransition(first, outTransition);
                TransitionService.SetNavigationInTransition(second, inTransition);
                int outBegun = 0;
                outTransition.BeginTransition += delegate { outBegun++; };

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    frame.ApplyTemplate();
                    window.UpdateLayout();

                    frame.Navigate(first);
                    PumpFor(650);
                    frame.Navigate(second);
                    PumpFor(80);
                    Assert.Equal(1, outBegun);

                    ProjectionHost outgoing = (ProjectionHost)frame.Template.FindName("SecondContentPresenter", frame);
                    ProjectionHost incoming = (ProjectionHost)frame.Template.FindName("FirstContentPresenter", frame);
                    Assert.Same(first, outgoing.Content);
                    Assert.Same(second, incoming.Content);
                    Assert.NotNull(ProjectionService.GetProjection(first));
                    Assert.Same(outgoing, ProjectionService.GetProjectionHost(first));
                    Assert.NotNull(outgoing.Projection);
                    Assert.Same(ProjectionService.GetProjection(first), outgoing.Projection);
                    PlaneProjection outgoingProjection = (PlaneProjection)outgoing.Projection;
                    Assert.True(
                        outgoing.IsRenderingIn3D,
                        string.Format("RotationY={0}, state={1}, child={2}", outgoingProjection.RotationY, outgoingProjection.IsIdentity, outgoing.Content));

                    PumpFor(800);
                    Assert.Null(ProjectionService.GetProjection(first));
                    Assert.True(second.IsHitTestVisible);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void TransitionFrame_RapidNavigationDoesNotLeaveTransitionsActive()
        {
            StaTest.Run(() =>
            {
                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage first = new TestPage();
                TestPage second = new TestPage();
                NavigationInTransition inTransition = new NavigationInTransition
                {
                    Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn }
                };
                TransitionService.SetNavigationInTransition(first, inTransition);
                TransitionService.SetNavigationInTransition(second, inTransition);

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    frame.ApplyTemplate();
                    window.UpdateLayout();

                    frame.Navigate(first);
                    PumpFor(50);
                    frame.Navigate(second);
                    PumpFor(1200);

                    Assert.Same(second, frame.Content);
                    Assert.True(second.IsHitTestVisible);
                    Assert.Null(ProjectionService.GetProjection(second));
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void TransitionFrame_FallsBackWhenTransitionFactoryThrows()
        {
            StaTest.Run(() =>
            {
                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage page = new TestPage();
                TransitionService.SetNavigationInTransition(page, new NavigationInTransition
                {
                    Forward = new ThrowingTransitionElement()
                });

                int failures = 0;

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    frame.ApplyTemplate();
                    window.UpdateLayout();
                    Assert.NotNull(frame.Template.FindName("FirstContentPresenter", frame));
                    Assert.NotNull(frame.Template.FindName("SecondContentPresenter", frame));
                    Assert.NotNull(TransitionService.GetNavigationInTransition(page));
                    frame.Navigate(page);
                    PumpFor(250);

                    Assert.Same(page, frame.Content);
                    Assert.Equal(1, failures);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Frame_DetectsOrientationFromSize()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationFrame frame = new PhoneApplicationFrame { Width = 480, Height = 800 };
                frame.Measure(new Size(480, 800));
                frame.Arrange(new Rect(0, 0, 480, 800));
                WpfTestHelper.PumpDispatcher();
                Assert.Equal(PageOrientation.PortraitUp, frame.Orientation);

                frame.Width = 800;
                frame.Height = 480;
                frame.Measure(new Size(800, 480));
                frame.Arrange(new Rect(0, 0, 800, 480));
                WpfTestHelper.PumpDispatcher();
                Assert.Equal(PageOrientation.LandscapeLeft, frame.Orientation);
            });
        }

        [Fact]
        public void TransitionFrame_PlaysTurnstileTransition()
        {
            StaTest.Run(() =>
            {
                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage page = new TestPage();
                TransitionService.SetNavigationInTransition(page, new NavigationInTransition
                {
                    Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn }
                });

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    frame.ApplyTemplate();
                    window.UpdateLayout();
                    frame.Navigate(page);
                    PumpFor(20);

                    Assert.Same(page, frame.Content);
                    ProjectionHost presenter = ProjectionService.GetProjectionHost(page);
                    Assert.NotNull(presenter);

                    // The 3D render path has to be active before the first
                    // animated frame, otherwise the transition starts by
                    // switching how the page is rasterized.
                    Assert.True(presenter.Prefer3D, "The presenter should be prepared for 3D during navigation.");
                    Assert.True(presenter.IsRenderingIn3D, "The presenter should render through the 3D pipeline from the start.");

                    PumpFor(90);

                    PlaneProjection projection = ProjectionService.GetProjection(page) as PlaneProjection;
                    Assert.NotNull(projection);
                    Assert.True(
                        projection.RotationY < 0 && projection.RotationY > -80,
                        "The entry transition should have started rotating the page; RotationY=" + projection.RotationY);
                    Assert.True(presenter.Prefer3D);
                    Assert.True(presenter.IsRenderingIn3D);

                    PumpFor(800);
                    Assert.Null(ProjectionService.GetProjection(page));
                    Assert.True(page.IsHitTestVisible);
                    Assert.False(presenter.Prefer3D, "The presenter should return to the 2D render path when it is idle.");
                    Assert.False(presenter.IsRenderingIn3D);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void TransitionFrame_CompletesAFeatheredExitWithoutTargets()
        {
            StaTest.Run(() =>
            {
                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage first = new TestPage();
                TestPage second = new TestPage();

                // A feathered exit on a page without feathered elements has
                // nothing to animate. It still has to complete, otherwise the
                // incoming page never becomes interactive.
                TransitionService.SetNavigationOutTransition(first, new NavigationOutTransition
                {
                    Forward = new TurnstileFeatherTransition { Mode = TurnstileFeatherTransitionMode.ForwardOut }
                });

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    frame.ApplyTemplate();
                    window.UpdateLayout();
                    frame.Navigate(first);
                    PumpFor(60);

                    frame.Navigate(second);
                    PumpFor(500);

                    Assert.Same(second, frame.Content);
                    ProjectionHost presenter = ProjectionService.GetProjectionHost(second);
                    Assert.NotNull(presenter);
                    Assert.True(second.IsHitTestVisible, "The incoming page should become interactive.");
                    Assert.True(presenter.IsHitTestVisible, "The presenter should be interactive again.");
                    Assert.False(presenter.Prefer3D, "The presenter should return to the 2D render path.");
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void TransitionFrame_AnimatesFeatheredEntryOnFirstLayout()
        {
            StaTest.Run(() =>
            {
                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage first = new TestPage();

                PhoneApplicationPage second = new PhoneApplicationPage();
                StackPanel panel = new StackPanel();
                Border[] elements = new Border[3];
                for (int i = 0; i < elements.Length; i++)
                {
                    elements[i] = new Border { Height = 40, Background = Brushes.Blue, Margin = new Thickness(8) };

                    // Mirrors XAML: the index is set before the element is
                    // connected to the page, so the effect has not subscribed
                    // yet when the incoming transition is composed.
                    TurnstileFeatherEffect.SetFeatheringIndex(elements[i], i);
                    panel.Children.Add(elements[i]);
                }

                second.Content = panel;
                TransitionService.SetNavigationInTransition(second, new NavigationInTransition
                {
                    Forward = new TurnstileFeatherTransition { Mode = TurnstileFeatherTransitionMode.ForwardIn }
                });

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    frame.ApplyTemplate();
                    window.UpdateLayout();
                    frame.Navigate(first);
                    PumpFor(60);

                    frame.Navigate(second);
                    PumpFor(120);

                    bool animated = false;
                    foreach (Border element in elements)
                    {
                        PlaneProjection projection = ProjectionService.GetProjection(element) as PlaneProjection;
                        if (projection != null && !projection.IsIdentity)
                        {
                            animated = true;
                        }
                    }

                    Assert.True(animated, "The feathered elements should be animated on the first layout of the page.");
                }
                finally
                {
                    foreach (Border element in elements)
                    {
                        TurnstileFeatherEffect.SetFeatheringIndex(element, -1);
                    }

                    window.Close();
                }
            });
        }

        [Fact]
        public void TransitionFrame_UsesAnOutTransitionSetJustBeforeNavigating()
        {
            StaTest.Run(() =>
            {
                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                TestPage first = new TestPage();
                TestPage second = new TestPage();

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    frame.ApplyTemplate();
                    window.UpdateLayout();
                    frame.Navigate(first);
                    PumpFor(60);

                    // The sample picks the transition family at navigation time
                    // instead of declaring it in XAML.
                    TransitionService.SetNavigationOutTransition(first, new NavigationOutTransition
                    {
                        Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardOut }
                    });

                    frame.Navigate(second);
                    PumpFor(40);

                    ProjectionHost outgoing = ProjectionService.GetProjectionHost(first);
                    Assert.NotNull(outgoing);
                    Assert.True(outgoing.Prefer3D, "The selected exit transition should run on the outgoing page.");

                    PlaneProjection projection = ProjectionService.GetProjection(first) as PlaneProjection;
                    Assert.NotNull(projection);
                    Assert.True(
                        projection.RotationY > 0,
                        "The turnstile exit should rotate the outgoing page; RotationY=" + projection.RotationY);

                    PumpFor(600);
                    Assert.Same(second, frame.Content);
                    Assert.True(second.IsHitTestVisible);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void TurnstileTransition_ReportsStateAndRestoresProjection()
        {
            StaTest.Run(() =>
            {
                Border element = new Border { Width = 200, Height = 100, Background = Brushes.Red };
                StackPanel panel = new StackPanel();
                panel.Children.Add(element);
                Window window = new Window { Width = 260, Height = 180, Content = panel };

                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    ITransition transition = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn }.GetTransition(element);
                    Assert.NotNull(transition);
                    Assert.Equal(ClockState.Stopped, transition.GetCurrentState());

                    transition.Begin();
                    WpfTestHelper.PumpDispatcher();
                    Assert.NotEqual(ClockState.Stopped, transition.GetCurrentState());
                    Assert.NotNull(ProjectionService.GetProjection(element));

                    PumpFor(900);
                    Assert.Equal(ClockState.Stopped, transition.GetCurrentState());
                    Assert.Null(ProjectionService.GetProjection(element));
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void SystemTrayAndApplicationServiceExposeState()
        {
            StaTest.Run(() =>
            {
                TestPage page = new TestPage();
                SystemTray.SetIsVisible(page, true);
                Assert.True(SystemTray.GetIsVisible(page));

                Microsoft.Phone.Shell.PhoneApplicationService service = Microsoft.Phone.Shell.PhoneApplicationService.Current;
                Assert.NotNull(service);
                Assert.NotNull(service.State);
                service.State["key"] = 42;
                Assert.Equal(42, service.State["key"]);
            });
        }

        private static void PumpFor(int milliseconds)
        {
            DateTime end = DateTime.UtcNow.AddMilliseconds(milliseconds);
            while (DateTime.UtcNow < end)
            {
                WpfTestHelper.PumpDispatcher();
                System.Threading.Thread.Sleep(15);
            }
        }

        private sealed class ThrowingTransitionElement : TransitionElement
        {
            public override ITransition GetTransition(UIElement element)
            {
                throw new InvalidOperationException("Intentional transition failure for tests.");
            }
        }

        private sealed class TestPage : PhoneApplicationPage
        {
            public TestPage()
            {
                Content = new Grid();
            }
        }
    }
}
