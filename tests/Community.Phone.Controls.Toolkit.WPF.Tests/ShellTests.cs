using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Xunit;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    public class ShellTests
    {
        private static ApplicationBarIconButton CreateIconButton(string text)
        {
            return new ApplicationBarIconButton(new Uri("/Images/" + text + ".png", UriKind.Relative))
            {
                Text = text,
            };
        }

        [Fact]
        public void ApplicationBarItemsRaiseClicksThroughTheirViews()
        {
            StaTest.Run(() =>
            {
                ApplicationBar bar = new ApplicationBar();
                ApplicationBarIconButton button = CreateIconButton("save");
                ApplicationBarMenuItem menuItem = new ApplicationBarMenuItem("settings");
                bar.Buttons.Add(button);
                bar.MenuItems.Add(menuItem);

                ApplicationBarPresenter presenter = new ApplicationBarPresenter { ApplicationBar = bar };
                Window window = new Window { Width = 480, Height = 200, Content = presenter };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    int buttonClicks = 0;
                    int menuClicks = 0;
                    button.Click += delegate { buttonClicks++; };
                    menuItem.Click += delegate { menuClicks++; };

                    ApplicationBarIconButtonView buttonView =
                        presenter.ButtonElements.OfType<ApplicationBarIconButtonView>().Single();
                    buttonView.Activate();

                    presenter.OpenMenu();
                    WpfTestHelper.PumpDispatcher();
                    ApplicationBarMenuItemView menuView =
                        presenter.MenuElements.OfType<ApplicationBarMenuItemView>().Single();
                    menuView.Activate();

                    Assert.Equal(1, buttonClicks);
                    Assert.Equal(1, menuClicks);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ApplicationBarIconButtonDrawsThePhoneCircle()
        {
            StaTest.Run(() =>
            {
                ApplicationBar bar = new ApplicationBar();
                bar.Buttons.Add(new ApplicationBarIconButton(
                    new Uri("/Community.Phone.Controls.Toolkit.WPF;component/Content/ApplicationBar.Check.png", UriKind.Relative))
                {
                    Text = "done",
                });

                ApplicationBarPresenter presenter = new ApplicationBarPresenter { ApplicationBar = bar };
                Window window = new Window { Width = 480, Height = 200, Content = presenter };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    ApplicationBarIconButtonView view =
                        presenter.ButtonElements.OfType<ApplicationBarIconButtonView>().Single();

                    Border circle = WpfTestHelper.FindDescendants<Border>(view)
                        .FirstOrDefault(border => border.CornerRadius.TopLeft > 0);
                    Assert.NotNull(circle);
                    Assert.Equal(ApplicationBarIconButtonView.IconSize, circle.Width, 1);
                    Assert.Equal(ApplicationBarIconButtonView.IconSize, circle.Height, 1);
                    Assert.Equal(new CornerRadius(ApplicationBarIconButtonView.IconSize / 2), circle.CornerRadius);
                    Assert.Equal(new Thickness(ApplicationBarIconButtonView.CircleThickness), circle.BorderThickness);

                    SolidColorBrush borderBrush = circle.BorderBrush as SolidColorBrush;
                    Assert.NotNull(borderBrush);
                    Assert.Equal(Colors.White, borderBrush.Color);

                    // The 48 DIP icon is the circle; the glyph inside it is smaller.
                    Image image = WpfTestHelper.FindDescendants<Image>(view).Single();
                    Assert.Equal(ApplicationBarIconButtonView.GlyphSize, image.Width, 1);
                    Assert.Equal(ApplicationBarIconButtonView.GlyphSize, image.Height, 1);
                    Assert.NotNull(image.Source);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ApplicationBarDisabledItemsDoNotRaiseEvents()
        {
            StaTest.Run(() =>
            {
                ApplicationBar bar = new ApplicationBar();
                ApplicationBarIconButton button = CreateIconButton("save");
                button.IsEnabled = false;
                ApplicationBarMenuItem menuItem = new ApplicationBarMenuItem("settings") { IsEnabled = false };
                bar.Buttons.Add(button);
                bar.MenuItems.Add(menuItem);

                int events = 0;
                button.Click += delegate { events++; };
                menuItem.Click += delegate { events++; };

                ApplicationBarPresenter presenter = new ApplicationBarPresenter { ApplicationBar = bar };
                Window window = new Window { Width = 480, Height = 200, Content = presenter };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    ApplicationBarItemView buttonView = presenter.ButtonElements.OfType<ApplicationBarItemView>().First();
                    Assert.False(buttonView.IsEnabled);

                    presenter.OpenMenu();
                    WpfTestHelper.PumpDispatcher();
                    ApplicationBarItemView menuView = presenter.MenuElements.OfType<ApplicationBarItemView>().First();
                    Assert.False(menuView.IsEnabled);

                    buttonView.Activate();
                    menuView.Activate();

                    Assert.Equal(0, events);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ApplicationBarEnforcesItemLimitsAndValidation()
        {
            ApplicationBar full = new ApplicationBar();
            for (int i = 0; i < ApplicationBar.MaxIconButtons; i++)
            {
                full.Buttons.Add(CreateIconButton("button" + i));
            }

            Assert.Throws<InvalidOperationException>(() => full.Buttons.Add(CreateIconButton("extra")));
            Assert.Throws<ArgumentNullException>(() => full.MenuItems.Add(null));

            ApplicationBarIconButton shared = CreateIconButton("shared");
            ApplicationBar owner = new ApplicationBar();
            owner.Buttons.Add(shared);
            Assert.Throws<InvalidOperationException>(() => owner.Buttons.Add(shared));

            ApplicationBar bar = new ApplicationBar();
            Assert.Throws<ArgumentNullException>(() => bar.Buttons.Add(new ApplicationBarIconButton { Text = "no icon" }));
            Assert.Throws<InvalidOperationException>(() => bar.MenuItems.Add(new ApplicationBarMenuItem()));
            Assert.Throws<InvalidOperationException>(() => bar.MenuItems.Add(new ApplicationBarMenuItem("two\nlines")));
            Assert.Throws<InvalidOperationException>(() => bar.MenuItems.Add(new ApplicationBarMenuItem("   ")));
            Assert.Throws<ArgumentException>(() => bar.Buttons.Add(new ApplicationBarMenuItem("wrong list")));
            Assert.Empty(bar.MenuItems);
        }

        [Fact]
        public void ApplicationBarMenuAcceptsTheMaximumNumberOfItems()
        {
            ApplicationBar bar = new ApplicationBar();
            for (int i = 0; i < ApplicationBar.MaxMenuItems; i++)
            {
                bar.MenuItems.Add(new ApplicationBarMenuItem("item" + i));
            }

            Assert.Equal(ApplicationBar.MaxMenuItems, bar.MenuItems.Count);
            Assert.Throws<InvalidOperationException>(() => bar.MenuItems.Add(new ApplicationBarMenuItem("extra")));
        }

        [Fact]
        public void ApplicationBarReservedHeightFollowsTheOriginalRule()
        {
            StaTest.Run(() =>
            {
                ApplicationBar bar = new ApplicationBar();
                ApplicationBarPresenter presenter = new ApplicationBarPresenter { ApplicationBar = bar };

                Assert.True(bar.IsVisible);
                Assert.Equal(72, bar.DefaultSize);
                Assert.Equal(30, bar.MiniSize);
                Assert.Equal(bar.DefaultSize, presenter.ReservedHeight);

                bar.Mode = ApplicationBarMode.Minimized;
                Assert.Equal(bar.MiniSize, presenter.ReservedHeight);
                bar.Mode = ApplicationBarMode.Default;

                bar.Opacity = 0.5;
                Assert.Equal(0.5, bar.Opacity);
                Assert.Equal(0, presenter.ReservedHeight);

                bar.Opacity = 4;
                Assert.Equal(1.0, bar.Opacity);
                Assert.Equal(72, presenter.ReservedHeight);

                bar.Opacity = -1;
                Assert.Equal(0.0, bar.Opacity);
                Assert.Equal(0, presenter.ReservedHeight);

                bar.Opacity = 1.0;
                bar.IsVisible = false;
                Assert.Equal(0, presenter.ReservedHeight);
                Assert.Equal(Visibility.Collapsed, presenter.Visibility);

                bar.IsVisible = true;
                bar.BackgroundColor = Colors.Transparent;
                Assert.Equal(72, presenter.ReservedHeight);

                bar.BackgroundColor = Color.FromArgb(128, 0, 0, 0);
                Assert.Equal(0, presenter.ReservedHeight);

                bar.BackgroundColor = Colors.Black;
                Assert.Equal(72, presenter.ReservedHeight);
            });
        }

        [Fact]
        public void ApplicationBarPresenterOpensAndClosesTheMenu()
        {
            StaTest.Run(() =>
            {
                ApplicationBar bar = new ApplicationBar();
                ApplicationBarMenuItem first = new ApplicationBarMenuItem("first");
                ApplicationBarMenuItem second = new ApplicationBarMenuItem("second");
                bar.MenuItems.Add(first);
                bar.MenuItems.Add(second);

                ApplicationBarPresenter presenter = new ApplicationBarPresenter { ApplicationBar = bar };
                Window window = new Window { Width = 480, Height = 200, Content = presenter };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    List<bool> states = new List<bool>();
                    bar.StateChanged += delegate(object sender, ApplicationBarStateChangedEventArgs e)
                    {
                        states.Add(e.IsMenuVisible);
                    };

                    int clicks = 0;
                    second.Click += delegate { clicks++; };

                    presenter.OpenMenu();
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(presenter.IsMenuOpen);
                    Assert.Equal(2, presenter.MenuElements.Count);
                    Assert.Equal(ApplicationBarMenuItemView.RowHeight, presenter.MenuElements[0].RenderSize.Height);
                    Assert.Equal(new[] { true }, states.ToArray());

                    ApplicationBarMenuItemView secondView = presenter.MenuElements.OfType<ApplicationBarMenuItemView>().Last();
                    secondView.Activate();
                    WpfTestHelper.PumpDispatcher();

                    Assert.False(presenter.IsMenuOpen);
                    Assert.Equal(1, clicks);
                    Assert.Equal(new[] { true, false }, states.ToArray());
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void PhoneApplicationFrameHostsThePageApplicationBar()
        {
            StaTest.Run(() =>
            {
                ApplicationBar bar = new ApplicationBar();
                bar.Buttons.Add(CreateIconButton("save"));
                bar.MenuItems.Add(new ApplicationBarMenuItem("about"));

                PhoneApplicationPage page = new PhoneApplicationPage();
                page.ApplicationBar = bar;
                WpfTestHelper.ApplyTheme(page);

                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(page);
                    WpfTestHelper.PumpDispatcher();
                    WpfTestHelper.PumpDispatcher();

                    Assert.Same(bar, frame.ApplicationBar);

                    FrameworkElement host = (FrameworkElement)frame.Template.FindName("ApplicationBarHost", frame);
                    ApplicationBarPresenter presenter = (ApplicationBarPresenter)frame.Template.FindName("ApplicationBar", frame);
                    Assert.NotNull(host);
                    Assert.NotNull(presenter);

                    Assert.Same(bar, presenter.ApplicationBar);
                    Assert.Equal(Visibility.Visible, presenter.Visibility);
                    Assert.Equal(72, host.Height);
                    Assert.Equal(72, presenter.ActualHeight);
                    Assert.Single(presenter.ButtonElements.OfType<ApplicationBarIconButtonView>());

                    // The page is laid out above the bar, so the bar never covers page content.
                    Assert.Equal(frame.ActualHeight - 72, page.ActualHeight, 1);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void PhoneApplicationFrameHidesTheBarWhenThePageHasNone()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationPage page = new PhoneApplicationPage();
                WpfTestHelper.ApplyTheme(page);

                PhoneApplicationFrame frame = new PhoneApplicationFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(page);
                    WpfTestHelper.PumpDispatcher();

                    Assert.Null(frame.ApplicationBar);

                    FrameworkElement host = (FrameworkElement)frame.Template.FindName("ApplicationBarHost", frame);
                    ApplicationBarPresenter presenter = (ApplicationBarPresenter)frame.Template.FindName("ApplicationBar", frame);
                    Assert.NotNull(host);
                    Assert.NotNull(presenter);
                    Assert.Equal(0, host.Height);
                    Assert.Equal(Visibility.Collapsed, presenter.Visibility);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void PhoneApplicationPageRejectsForeignApplicationBarImplementations()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationPage page = new PhoneApplicationPage();
                Assert.Throws<InvalidOperationException>(() => page.ApplicationBar = new ForeignApplicationBar());
                Assert.Null(page.ApplicationBar);

                ApplicationBar bar = new ApplicationBar();
                page.ApplicationBar = bar;
                Assert.Same(bar, page.ApplicationBar);
            });
        }

        [Fact]
        public void SystemTrayStatePublishesPropertyChanges()
        {
            SystemTrayState state = SystemTrayState.Current;
            int changes = 0;
            PropertyChangedEventHandler handler = delegate { changes++; };
            state.PropertyChanged += handler;
            try
            {
                state.Opacity = 0.5;
                state.BackgroundColor = Colors.Red;
                state.ForegroundColor = Colors.Yellow;
                state.IsVisible = true;
                state.ProgressIndicator.IsVisible = true;
                state.ProgressIndicator.Text = "Loading";

                Assert.Equal(0.5, state.Opacity);
                Assert.Equal(Colors.Red, state.BackgroundColor);
                Assert.Equal(Colors.Yellow, state.ForegroundColor);
                Assert.True(state.IsVisible);
                Assert.True(state.ProgressIndicator.IsVisible);
                Assert.True(changes >= 6);
            }
            finally
            {
                state.PropertyChanged -= handler;
                state.IsVisible = false;
                state.Opacity = 1.0;
                state.BackgroundColor = Colors.Black;
                state.ForegroundColor = Colors.White;
                state.ProgressIndicator.IsVisible = false;
                state.ProgressIndicator.Text = string.Empty;
            }
        }

        private sealed class ForeignApplicationBar : IApplicationBar
        {
            public bool IsVisible { get; set; }

            public double Opacity { get; set; }

            public bool IsMenuEnabled { get; set; }

            public Color BackgroundColor { get; set; }

            public Color ForegroundColor { get; set; }

            public ApplicationBarMode Mode { get; set; }

            public double DefaultSize
            {
                get { return 72; }
            }

            public double MiniSize
            {
                get { return 30; }
            }

            public System.Collections.IList Buttons
            {
                get { return new object[0]; }
            }

            public System.Collections.IList MenuItems
            {
                get { return new object[0]; }
            }

            public event EventHandler<ApplicationBarStateChangedEventArgs> StateChanged
            {
                add { }
                remove { }
            }
        }
    }
}
