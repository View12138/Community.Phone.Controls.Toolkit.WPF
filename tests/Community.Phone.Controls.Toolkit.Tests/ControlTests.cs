using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Controls.LocalizedResources;
using Microsoft.Phone.Shell;
using PhoneWrapPanel = Microsoft.Phone.Controls.WrapPanel;
using PhoneContextMenu = Microsoft.Phone.Controls.ContextMenu;
using PhoneContextMenuService = Microsoft.Phone.Controls.ContextMenuService;
using PhoneDatePicker = Microsoft.Phone.Controls.DatePicker;
using PhoneTimePicker = Microsoft.Phone.Controls.TimePicker;
using Xunit;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    public class ControlTests
    {
        [Fact]
        public void Theme_ContainsMetroResources()
        {
            StaTest.Run(() =>
            {
                Assert.NotNull(WpfTestHelper.Theme["PhoneAccentBrush"]);
                Assert.NotNull(WpfTestHelper.Theme["PhoneFontFamilyNormal"]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(ToggleSwitch)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(Rating)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(PhoneTextBox)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(TransitionFrame)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(Button)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(TextBox)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(ListBox)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(ListBoxItem)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(LongListSelector)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(ComboBox)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(ComboBoxItem)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(ScrollBar)]);
                Assert.NotNull(WpfTestHelper.Theme[typeof(ProgressBar)]);
            });
        }

        [Fact]
        public void ToggleSwitch_HasTemplateAndDependencyProperties()
        {
            StaTest.Run(() =>
            {
                ToggleSwitch toggle = new ToggleSwitch { Header = "Wi-Fi" };
                WpfTestHelper.ApplyDefaultStyle(toggle);
                WpfTestHelper.Layout(toggle);

                Assert.Equal("Wi-Fi", toggle.Header);
                Assert.Equal(false, toggle.IsChecked);
                Assert.NotNull(toggle.Template);
                Assert.NotNull(toggle.Template.FindName("Switch", toggle));

                toggle.IsChecked = true;
                Assert.Equal(true, toggle.IsChecked);
            });
        }

        [Fact]
        public void ToggleSwitch_AnimatesThumbTranslation()
        {
            StaTest.Run(() =>
            {
                ToggleSwitch toggle = new ToggleSwitch { Width = 220, Height = 120 };
                Window window = new Window { Width = 300, Height = 200, Content = toggle };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    Microsoft.Phone.Controls.Primitives.ToggleSwitchButton switchButton =
                        (Microsoft.Phone.Controls.Primitives.ToggleSwitchButton)toggle.Template.FindName("Switch", toggle);
                    Assert.NotNull(switchButton);
                    TranslateTransform thumb = (TranslateTransform)switchButton.Template.FindName("ThumbTranslation", switchButton);
                    Assert.NotNull(thumb);
                    Assert.Equal(0, thumb.X);

                    switchButton.IsChecked = true;
                    PumpFor(70);
                    Assert.True(thumb.X > 0 && thumb.X < 69, "The switch thumb should animate between the two ends.");
                    PumpFor(300);
                    Assert.Equal(69, thumb.X, 1);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Rating_PointerPositionUpdatesValue()
        {
            StaTest.Run(() =>
            {
                Rating rating = new Rating { Width = 200, Height = 40, RatingItemCount = 5, Value = 1 };
                Window window = new Window { Width = 300, Height = 160, Content = rating };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    window.UpdateLayout();

                    // Same computation the mouse handlers run, without depending on
                    // the OS pointer (a locked or covered desktop used to make this
                    // test skip or fail).
                    rating.UpdateValueFromMouse(new Point(150, 20));
                    Assert.Equal(4, rating.Value);

                    rating.UpdateValueFromMouse(new Point(190, 20));
                    Assert.Equal(5, rating.Value);

                    rating.UpdateValueFromMouse(new Point(10, 20));
                    Assert.Equal(1, rating.Value);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void Rating_CreatesConfiguredNumberOfItems()
        {
            StaTest.Run(() =>
            {
                Rating rating = new Rating { RatingItemCount = 4, Value = 2 };
                WpfTestHelper.ApplyDefaultStyle(rating);
                WpfTestHelper.Layout(rating);

                Assert.Equal(4, rating.RatingItemCount);
                Assert.Equal(2, rating.Value);
                Grid filled = (Grid)rating.Template.FindName("FilledGridElement", rating);
                Grid unfilled = (Grid)rating.Template.FindName("UnfilledGridElement", rating);
                Assert.NotNull(filled);
                Assert.NotNull(unfilled);
                Assert.Equal(4, filled.Children.Count);
                Assert.Equal(4, unfilled.Children.Count);
            });
        }

        [Fact]
        public void Rating_HalfItemIncrementKeepsValueInItemUnits()
        {
            StaTest.Run(() =>
            {
                Rating rating = new Rating
                {
                    Width = 200,
                    Height = 40,
                    RatingItemCount = 5,
                    AllowHalfItemIncrement = true,
                    Value = 1
                };
                WpfTestHelper.ApplyDefaultStyle(rating);
                WpfTestHelper.Layout(rating);

                Assert.Equal(200, rating.ActualWidth);

                // 80% of the width is inside the fifth item.
                Assert.Equal(4.0, CalculateValue(rating, 160));
                Assert.Equal(4.5, CalculateValue(rating, 175));

                // 50% of the width is exactly the half of the third item.
                Assert.Equal(2.5, CalculateValue(rating, 100));

                // 10% of the width selects the first half item.
                Assert.Equal(0.5, CalculateValue(rating, 20));
            });
        }

        private static double CalculateValue(Rating rating, double x)
        {
            System.Reflection.MethodInfo method = typeof(Rating).GetMethod(
                "PerformValueCalculation",
                System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
            method.Invoke(rating, new object[] { new Point(x, rating.ActualHeight / 2), rating });
            return rating.Value;
        }

        [Fact]
        public void WrapPanel_WrapsChildrenAndHonorsItemSize()
        {
            StaTest.Run(() =>
            {
                PhoneWrapPanel panel = new PhoneWrapPanel { Width = 100, Height = 100, ItemWidth = 40, ItemHeight = 20 };
                for (int i = 0; i < 5; i++)
                {
                    panel.Children.Add(new Border { Width = 40, Height = 20 });
                }

                panel.Measure(new Size(100, 100));
                panel.Arrange(new Rect(0, 0, 100, 100));

                Point first = ((FrameworkElement)panel.Children[0]).TransformToAncestor(panel).Transform(new Point(0, 0));
                Point second = ((FrameworkElement)panel.Children[1]).TransformToAncestor(panel).Transform(new Point(0, 0));
                Point third = ((FrameworkElement)panel.Children[2]).TransformToAncestor(panel).Transform(new Point(0, 0));

                Assert.Equal(0, first.X);
                Assert.Equal(0, first.Y);
                Assert.Equal(40, second.X);
                Assert.Equal(0, second.Y);
                Assert.Equal(0, third.X);
                Assert.Equal(20, third.Y);
            });
        }

        [Fact]
        public void ListPicker_SelectionRoundTrips()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker();
                picker.Items.Add("one");
                picker.Items.Add("two");
                picker.Items.Add("three");

                picker.SelectedIndex = 1;
                Assert.Equal("two", picker.SelectedItem);

                picker.SelectedItem = "three";
                Assert.Equal(2, picker.SelectedIndex);
            });
        }

        [Fact]
        public void PhoneTextBox_ShowsHintWhenEmpty()
        {
            StaTest.Run(() =>
            {
                PhoneTextBox textBox = new PhoneTextBox { Hint = "Type here", Width = 300, Height = 60 };
                WpfTestHelper.ApplyDefaultStyle(textBox);
                WpfTestHelper.Layout(textBox, 300, 60);

                Assert.Equal("Type here", textBox.Hint);
                Assert.Equal(Visibility.Visible, textBox.ActualHintVisibility);

                textBox.Text = "abc";
                Assert.Equal(Visibility.Collapsed, textBox.ActualHintVisibility);
            });
        }

        [Fact]
        public void ExpanderView_TogglesAndRaisesEvents()
        {
            StaTest.Run(() =>
            {
                ExpanderView view = new ExpanderView { Header = "Header", Width = 300 };
                view.Items.Add("one");
                view.Items.Add("two");
                WpfTestHelper.ApplyDefaultStyle(view);
                WpfTestHelper.Layout(view, 300, 400);

                int expanded = 0;
                int collapsed = 0;
                view.Expanded += delegate { expanded++; };
                view.Collapsed += delegate { collapsed++; };

                Assert.False(view.IsExpanded);
                view.IsExpanded = true;
                WpfTestHelper.PumpDispatcher();
                Assert.True(view.IsExpanded);
                Assert.Equal(1, expanded);

                view.IsExpanded = false;
                WpfTestHelper.PumpDispatcher();
                Assert.False(view.IsExpanded);
                Assert.Equal(1, collapsed);
            });
        }

        [Fact]
        public void ToggleSwitch_SwitchButtonDrivesEvents()
        {
            StaTest.Run(() =>
            {
                ToggleSwitch toggle = new ToggleSwitch { Header = "Wi-Fi" };
                WpfTestHelper.ApplyDefaultStyle(toggle);
                WpfTestHelper.Layout(toggle);

                int checkedCount = 0;
                int clickCount = 0;
                toggle.Checked += delegate { checkedCount++; };
                toggle.Click += delegate { clickCount++; };

                Microsoft.Phone.Controls.Primitives.ToggleSwitchButton switchButton =
                    (Microsoft.Phone.Controls.Primitives.ToggleSwitchButton)toggle.Template.FindName("Switch", toggle);
                Assert.NotNull(switchButton);

                switchButton.IsChecked = true;
                Assert.True(toggle.IsChecked);
                Assert.Equal(1, checkedCount);

                switchButton.RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent));
                Assert.Equal(1, clickCount);
            });
        }

        [Fact]
        public void PhoneTextBox_InnerEditorIsVisibleAndReceivesFocus()
        {
            StaTest.Run(() =>
            {
                PhoneTextBox textBox = new PhoneTextBox { Hint = "Type", Width = 300, Height = 60 };
                Window window = new Window { Width = 400, Height = 200, Content = textBox };
                try
                {
                    window.Show();
                    window.Activate();
                    window.UpdateLayout();
                    PumpFor(100);

                    TextBox inner = (TextBox)textBox.Template.FindName("Text", textBox);
                    Assert.NotNull(inner);
                    Assert.True(inner.IsVisible, "The inner editor must be visible; otherwise clicks focus an editor that is not rendered.");
                    Keyboard.Focus(textBox);
                    WpfTestHelper.PumpDispatcher();
                    Assert.True(
                        inner.IsKeyboardFocusWithin,
                        string.Format(
                            "Focus on the PhoneTextBox should be forwarded to its editor. outer={0}, inner={1}, focused={2}",
                            textBox.IsKeyboardFocused,
                            inner.IsKeyboardFocused,
                            Keyboard.FocusedElement));

                    inner.Text = "typed";
                    Assert.Equal("typed", textBox.Text);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void PhoneTextBox_TemplatePartsAndTwoWayText()
        {
            StaTest.Run(() =>
            {
                PhoneTextBox textBox = new PhoneTextBox
                {
                    Hint = "Type",
                    LengthIndicatorVisible = true,
                    DisplayedMaxLength = 10,
                    MaxLength = 10,
                    HidesActionItemWhenEmpty = true
                };
                WpfTestHelper.ApplyDefaultStyle(textBox);
                WpfTestHelper.Layout(textBox, 300, 80);

                TextBox inner = (TextBox)textBox.Template.FindName("Text", textBox);
                Assert.NotNull(inner);
                inner.Text = "abc";
                Assert.Equal("abc", textBox.Text);

                TextBlock length = (TextBlock)textBox.Template.FindName("LengthIndicator", textBox);
                Assert.NotNull(length);
                Assert.Equal("3/10", length.Text);

                Border actionIcon = (Border)textBox.Template.FindName("ActionIconBorder", textBox);
                Assert.NotNull(actionIcon);
                Assert.Equal(Visibility.Collapsed, actionIcon.Visibility);

                textBox.ActionIcon = new DrawingImage(
                    new GeometryDrawing(Brushes.White, null, new RectangleGeometry(new Rect(0, 0, 1, 1))));
                Assert.Equal(Visibility.Visible, actionIcon.Visibility);

                int taps = 0;
                textBox.ActionIconTapped += delegate { taps++; };
                actionIcon.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Left)
                {
                    RoutedEvent = UIElement.MouseLeftButtonDownEvent
                });
                Assert.Equal(1, taps);
            });
        }

        [Fact]
        public void Rating_ValueChangesRaiseEventAndUpdateItems()
        {
            StaTest.Run(() =>
            {
                Rating rating = new Rating { RatingItemCount = 5 };
                WpfTestHelper.ApplyDefaultStyle(rating);
                WpfTestHelper.Layout(rating);

                int changes = 0;
                rating.ValueChanged += delegate { changes++; };

                rating.Value = 3.5;
                Assert.Equal(3.5, rating.Value);
                Assert.Equal(1, changes);

                Grid filled = (Grid)rating.Template.FindName("FilledGridElement", rating);
                Grid unfilled = (Grid)rating.Template.FindName("UnfilledGridElement", rating);
                Assert.Equal(5, filled.Children.Count);
                Assert.Equal(5, unfilled.Children.Count);
            });
        }

        [Fact]
        public void ListPicker_ExpandedModeSelectsItemAndCollapses()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker { Width = 320 };
                picker.Items.Add("one");
                picker.Items.Add("two");
                picker.Items.Add("three");

                WpfTestHelper.ApplyDefaultStyle(picker);
                Window window = new Window { Width = 400, Height = 300, Content = picker };
                try
                {
                    window.Show();
                    WpfTestHelper.Layout(picker, 320, 200);

                    int changes = 0;
                    picker.SelectionChanged += delegate { changes++; };

                    Assert.True(picker.Open());
                    Assert.Equal(ListPickerMode.Expanded, picker.ListPickerMode);

                    picker.SelectedIndex = 1;
                    WpfTestHelper.PumpDispatcher();

                    Assert.Equal(1, picker.SelectedIndex);
                    Assert.Equal("two", picker.SelectedItem);
                    Assert.Equal(ListPickerMode.Normal, picker.ListPickerMode);
                    Assert.Equal(1, changes);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ListPicker_FullModeWithoutFrameStaysUsable()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker();
                for (int i = 0; i < 6; i++)
                {
                    picker.Items.Add("item " + i);
                }

                WpfTestHelper.ApplyDefaultStyle(picker);
                WpfTestHelper.Layout(picker);

                Assert.True(picker.Open());
                Assert.Equal(ListPickerMode.Normal, picker.ListPickerMode);
                Assert.Equal(0, picker.SelectedIndex);
            });
        }

        [Fact]
        public void ListPicker_FullModeNavigatesToPickerPageAndCommitsSelection()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker { Width = 320 };
                for (int i = 0; i < 7; i++)
                {
                    picker.Items.Add("item " + i);
                }

                PhoneApplicationPage hostPage = new PhoneApplicationPage { Content = picker };
                WpfTestHelper.ApplyTheme(hostPage);
                WpfTestHelper.ApplyDefaultStyle(picker);

                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(hostPage);
                    WpfTestHelper.PumpDispatcher();
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(picker.Open());
                    WpfTestHelper.PumpDispatcher();
                    WpfTestHelper.PumpDispatcher();

                    ListPickerPage page = frame.Content as ListPickerPage;
                    Assert.NotNull(page);
                    Assert.Equal(7, page.Items.Count);
                    Assert.Equal(SelectionMode.Single, page.SelectionMode);
                    Assert.Equal(6, page.Items.IndexOf("item 6"));

                    // The page must stay open until the user acts on it (it must not navigate away on its own).
                    WpfTestHelper.PumpFor(600);

                    Assert.Same(page, frame.Content);
                    Assert.Equal(ListPickerMode.Full, picker.ListPickerMode);

                    page.SelectedItem = "item 5";
                    frame.NavigationService.GoBack();
                    WpfTestHelper.PumpDispatcher();
                    WpfTestHelper.PumpDispatcher();

                    Assert.Same(hostPage, frame.Content);
                    Assert.Equal("item 5", picker.SelectedItem);
                    Assert.Equal(5, picker.SelectedIndex);
                    Assert.Equal(ListPickerMode.Normal, picker.ListPickerMode);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ListPicker_FullModePageUsesTheApplicationBarForSingleSelection()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker { Width = 320 };
                for (int i = 0; i < 7; i++)
                {
                    picker.Items.Add("item " + i);
                }

                PhoneApplicationPage hostPage = new PhoneApplicationPage { Content = picker };
                WpfTestHelper.ApplyTheme(hostPage);
                WpfTestHelper.ApplyDefaultStyle(picker);

                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(hostPage);
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(picker.Open());
                    Assert.True(WpfTestHelper.PumpUntil(() => frame.Content is ListPickerPage));

                    ListPickerPage page = (ListPickerPage)frame.Content;
                    ApplicationBar bar = page.ApplicationBar as ApplicationBar;
                    Assert.NotNull(bar);
                    Assert.Equal(2, bar.Buttons.Count);

                    // Single selection commits on tap, so the original hides the done/cancel bar.
                    Assert.False(bar.IsVisible);

                    ApplicationBarPresenter presenter =
                        (ApplicationBarPresenter)frame.Template.FindName("ApplicationBar", frame);
                    Assert.Same(bar, presenter.ApplicationBar);
                    Assert.Equal(Visibility.Collapsed, presenter.Visibility);

                    // The page's own bar replaces whatever the host page had.
                    Assert.Equal(0, ((FrameworkElement)frame.Template.FindName("ApplicationBarHost", frame)).Height);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ListPicker_FullModePageCommitsFromTheApplicationBarDoneButton()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker { Width = 320, SelectionMode = SelectionMode.Multiple };
                for (int i = 0; i < 7; i++)
                {
                    picker.Items.Add("item " + i);
                }

                PhoneApplicationPage hostPage = new PhoneApplicationPage { Content = picker };
                WpfTestHelper.ApplyTheme(hostPage);
                WpfTestHelper.ApplyDefaultStyle(picker);

                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(hostPage);
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(picker.Open());
                    Assert.True(WpfTestHelper.PumpUntil(() => frame.Content is ListPickerPage));

                    ListPickerPage page = (ListPickerPage)frame.Content;
                    ApplicationBar bar = page.ApplicationBar as ApplicationBar;
                    Assert.NotNull(bar);

                    Assert.True(bar.IsVisible);

                    ApplicationBarIconButton done = (ApplicationBarIconButton)bar.Buttons[0];
                    ApplicationBarIconButton cancel = (ApplicationBarIconButton)bar.Buttons[1];
                    Assert.Equal(ControlResources.DateTimePickerDoneText, done.Text);
                    Assert.Equal(ControlResources.DateTimePickerCancelText, cancel.Text);
                    Assert.True(done.IsEnabled);

                    ApplicationBarPresenter presenter =
                        (ApplicationBarPresenter)frame.Template.FindName("ApplicationBar", frame);
                    Assert.Same(bar, presenter.ApplicationBar);
                    Assert.Equal(Visibility.Visible, presenter.Visibility);
                    Assert.Equal(72, presenter.ActualHeight);

                    // The check/cancel icons ship as library resources and must resolve.
                    List<ApplicationBarIconButtonView> views = presenter.ButtonElements.OfType<ApplicationBarIconButtonView>().ToList();
                    Assert.Equal(2, views.Count);
                    foreach (ApplicationBarIconButtonView view in views)
                    {
                        Image icon = ((Grid)view.Child).Children.OfType<Image>().First();
                        Assert.NotNull(icon.Source);
                    }

                    page.Picker.SelectedItems.Add("item 3");
                    done.ClickEvent();

                    Assert.True(WpfTestHelper.PumpUntil(() => ReferenceEquals(frame.Content, hostPage)));
                    Assert.Equal(ListPickerMode.Normal, picker.ListPickerMode);
                    Assert.Contains("item 3", picker.SelectedItems.Cast<object>());
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ListPicker_FullModePageAnimatesEveryItemInView()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker { Width = 320, SelectionMode = SelectionMode.Multiple };
                for (int i = 0; i < 4; i++)
                {
                    picker.Items.Add("item " + i);
                }

                PhoneApplicationPage hostPage = new PhoneApplicationPage { Content = picker };
                WpfTestHelper.ApplyTheme(hostPage);
                WpfTestHelper.ApplyDefaultStyle(picker);

                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(hostPage);
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(picker.Open());
                    Assert.True(WpfTestHelper.PumpUntil(() => frame.Content is ListPickerPage));

                    ListPickerPage page = (ListPickerPage)frame.Content;
                    page.Picker.UpdateLayout();
                    WpfTestHelper.PumpDispatcher();

                    List<ProjectionHost> hosts = new List<ProjectionHost>();
                    foreach (object item in page.Items)
                    {
                        ListBoxItem container = (ListBoxItem)page.Picker.ItemContainerGenerator.ContainerFromItem(item);
                        Assert.NotNull(container);

                        // Item containers cannot be wrapped in a projection
                        // host, so the page template hosts the content instead.
                        ProjectionHost host = container.Template.FindName("ProjectionHost", container) as ProjectionHost;
                        Assert.NotNull(host);
                        hosts.Add(host);
                    }

                    // The open animation starts once the list has generated its
                    // containers, so wait for it and observe the edge-on frames
                    // (before the fix no projection was applied at all).
                    double minimumRotation = 0;
                    Assert.True(
                        WpfTestHelper.PumpUntil(() =>
                        {
                            foreach (ProjectionHost host in hosts)
                            {
                                PlaneProjection projection = host.Projection as PlaneProjection;
                                if (null == projection)
                                {
                                    return false;
                                }

                                minimumRotation = Math.Min(minimumRotation, projection.RotationX);
                            }

                            return minimumRotation < -5;
                        }),
                        "No item was animated.");

                    WpfTestHelper.PumpFor(1500);
                    Assert.All(hosts, host => Assert.Equal(0, ((PlaneProjection)host.Projection).RotationX, 1));
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ListPicker_FullModePageWithManyItemsScrollsToTheSelectionAndAnimatesIt()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker
                {
                    Width = 320,
                    ExpansionMode = ExpansionMode.FullScreenOnly,
                };
                for (int i = 0; i < 120; i++)
                {
                    picker.Items.Add("item " + i);
                }

                picker.SelectedItem = "item 60";

                PhoneApplicationPage hostPage = new PhoneApplicationPage { Content = picker };
                WpfTestHelper.ApplyTheme(hostPage);
                WpfTestHelper.ApplyDefaultStyle(picker);

                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(hostPage);
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(picker.Open());
                    Assert.True(WpfTestHelper.PumpUntil(() => frame.Content is ListPickerPage));

                    // The list virtualizes, so only the containers around the
                    // view port exist. Used to throw NullReferenceException
                    // (ContainerFromIndex returns null for the rest).
                    ListPickerPage page = (ListPickerPage)frame.Content;
                    ListBox list = page.Picker;
                    WpfTestHelper.PumpUntil(() => list.ItemContainerGenerator.ContainerFromItem("item 60") != null);

                    FrameworkElement container = (FrameworkElement)list.ItemContainerGenerator.ContainerFromItem("item 60");
                    Assert.NotNull(container);

                    ScrollViewer scrollViewer = WpfTestHelper.FindDescendants<ScrollViewer>(list).First();
                    Assert.True(WpfTestHelper.PumpUntil(() =>
                    {
                        GeneralTransform transform = container.TransformToVisual(scrollViewer);
                        Point position = transform.Transform(new Point(0, 0));
                        return (position.Y + container.ActualHeight > 0) && (position.Y < scrollViewer.ActualHeight);
                    }), "The selected item was not scrolled into view.");

                    // The animation must cover the items the user sees, not the
                    // first ones of the (virtualized) list.
                    double observedRotation = 0;
                    Assert.True(
                        WpfTestHelper.PumpUntil(() =>
                        {
                            PlaneProjection projection = GetItemProjection((ListBoxItem)container);
                            if (null == projection)
                            {
                                return false;
                            }

                            observedRotation = Math.Min(observedRotation, projection.RotationX);
                            return observedRotation != 0;
                        }),
                        "The selected item was not animated.");

                    WpfTestHelper.PumpFor(1500);

                    // No realized container may be left edge-on: a stale -90
                    // would make it invisible when it scrolls into view.
                    for (int i = 0; i < list.Items.Count; i++)
                    {
                        if (list.ItemContainerGenerator.ContainerFromIndex(i) is ListBoxItem item)
                        {
                            PlaneProjection projection = GetItemProjection(item);
                            if (null != projection)
                            {
                                Assert.Equal(0, projection.RotationX, 1);
                            }
                        }
                    }
                }
                finally
                {
                    window.Close();
                }
            });
        }

        private static PlaneProjection GetItemProjection(ListBoxItem container)
        {
            ProjectionHost host = container.Template.FindName("ProjectionHost", container) as ProjectionHost;
            return host == null ? null : host.Projection as PlaneProjection;
        }

        [Fact]
        public void ListPicker_FullModeSingleSelectionUsesThePhoneItemStyle()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker
                {
                    Width = 320,
                    ExpansionMode = ExpansionMode.FullScreenOnly,
                };
                for (int i = 0; i < 4; i++)
                {
                    picker.Items.Add("item " + i);
                }

                PhoneApplicationPage hostPage = new PhoneApplicationPage { Content = picker };
                WpfTestHelper.ApplyTheme(hostPage);
                WpfTestHelper.ApplyDefaultStyle(picker);

                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(hostPage);
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(picker.Open());
                    Assert.True(WpfTestHelper.PumpUntil(() => frame.Content is ListPickerPage));

                    ListPickerPage page = (ListPickerPage)frame.Content;
                    page.Picker.UpdateLayout();
                    WpfTestHelper.PumpDispatcher();

                    Assert.Same(page.Resources["ListBoxItemStyle"], page.Picker.ItemContainerStyle);

                    ListBoxItem container = (ListBoxItem)page.Picker.ItemContainerGenerator.ContainerFromIndex(1);
                    Assert.NotNull(container);
                    Assert.True(container.ActualHeight < 110, "The desktop ListBoxItem padding is applied: " + container.ActualHeight);
                    Assert.Equal(Colors.Transparent, ((SolidColorBrush)container.Background).Color);

                    page.Picker.SelectedIndex = 1;
                    WpfTestHelper.PumpFor(100);

                    ContentControl content = container.Template.FindName("ContentContainer", container) as ContentControl;
                    Assert.NotNull(content);
                    SolidColorBrush foreground = content.Foreground as SolidColorBrush;
                    Assert.NotNull(foreground);
                    SolidColorBrush accent = WpfTestHelper.Theme["PhoneAccentBrush"] as SolidColorBrush;
                    Assert.NotNull(accent);
                    Assert.Equal(accent.Color, foreground.Color);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ListPicker_MultipleSelectionCheckBoxUsesThePhoneStyle()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker { Width = 320, SelectionMode = SelectionMode.Multiple };
                picker.Items.Add("one");
                picker.Items.Add("two");

                PhoneApplicationPage hostPage = new PhoneApplicationPage { Content = picker };
                WpfTestHelper.ApplyTheme(hostPage);
                WpfTestHelper.ApplyDefaultStyle(picker);

                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(hostPage);
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(picker.Open());
                    Assert.True(WpfTestHelper.PumpUntil(() => frame.Content is ListPickerPage));

                    ListPickerPage page = (ListPickerPage)frame.Content;
                    page.Picker.UpdateLayout();
                    WpfTestHelper.PumpDispatcher();

                    ListBoxItem container = (ListBoxItem)page.Picker.ItemContainerGenerator.ContainerFromIndex(0);
                    Assert.NotNull(container);
                    CheckBox checkBox = WpfTestHelper.FindDescendants<CheckBox>(container).FirstOrDefault();
                    Assert.NotNull(checkBox);

                    // The page's check box style replaces the theme style, so
                    // it must supply the phone colors itself.
                    SolidColorBrush background = checkBox.Background as SolidColorBrush;
                    Assert.NotNull(background);
                    Assert.Equal(Colors.Transparent, background.Color);

                    SolidColorBrush border = checkBox.BorderBrush as SolidColorBrush;
                    SolidColorBrush expectedBorder = WpfTestHelper.Theme["PhoneRadioCheckBoxBorderBrush"] as SolidColorBrush;
                    Assert.NotNull(border);
                    Assert.NotNull(expectedBorder);
                    Assert.Equal(expectedBorder.Color, border.Color);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ListPicker_ReturnsFromFullModeShowsTheSelectedItem()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker
                {
                    Width = 320,
                    ExpansionMode = ExpansionMode.FullScreenOnly,
                };
                for (int i = 0; i < 6; i++)
                {
                    picker.Items.Add("item " + i);
                }

                PhoneApplicationPage hostPage = new PhoneApplicationPage { Content = picker };
                WpfTestHelper.ApplyTheme(hostPage);
                WpfTestHelper.ApplyDefaultStyle(picker);

                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(hostPage);
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(picker.Open());
                    Assert.True(WpfTestHelper.PumpUntil(() => frame.Content is ListPickerPage));

                    ListPickerPage page = (ListPickerPage)frame.Content;
                    page.SelectedItem = "item 5";
                    frame.NavigationService.GoBack();

                    Assert.True(WpfTestHelper.PumpUntil(() => ReferenceEquals(frame.Content, hostPage)));
                    WpfTestHelper.PumpFor(600);

                    Assert.Equal("item 5", picker.SelectedItem);

                    ItemsPresenter presenter = WpfTestHelper.FindDescendants<ItemsPresenter>(picker).First();
                    TranslateTransform transform = presenter.RenderTransform as TranslateTransform;
                    Assert.NotNull(transform);

                    // -4 would mean the presenter still shows the first item.
                    Assert.True(transform.Y < -50, "The picker still shows the first item (Y=" + transform.Y + ").");
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ListPicker_OpenWithoutItemsExpands()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker();
                WpfTestHelper.ApplyDefaultStyle(picker);
                WpfTestHelper.Layout(picker);
                Assert.True(picker.Open());
                Assert.Equal(ListPickerMode.Expanded, picker.ListPickerMode);
            });
        }

        [Fact]
        public void ListPicker_PageWithPickersLoadsWithoutAnimationErrors()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker { Header = "Transition family", Margin = new Thickness(0, 0, 0, 16) };
                picker.Items.Add("Turnstile");
                picker.Items.Add("Slide");
                picker.Items.Add("Rotate");
                RecurringDaysPicker daysPicker = new RecurringDaysPicker { Header = "Repeats", Margin = new Thickness(0, 0, 0, 16) };

                StackPanel panel = new StackPanel();
                panel.Children.Add(picker);
                panel.Children.Add(daysPicker);

                PhoneApplicationPage page = new PhoneApplicationPage
                {
                    Content = new ScrollViewer { Content = panel, VerticalScrollBarVisibility = ScrollBarVisibility.Auto }
                };
                WpfTestHelper.ApplyTheme(page);

                TransitionFrame frame = new TransitionFrame();
                Window window = new Window { Width = 480, Height = 800, Content = frame };
                try
                {
                    window.Show();
                    frame.Navigate(page);
                    for (int i = 0; i < 8; i++)
                    {
                        WpfTestHelper.PumpDispatcher();
                    }

                    Canvas host = picker.Template.FindName("ItemsPresenterHost", picker) as Canvas;
                    Assert.NotNull(host);
                    Assert.False(double.IsNaN(host.Height));
                    Assert.True(host.Height > 0);

                    // Multiple selection collapses the items presenter and shows the summary text instead.
                    Canvas daysHost = daysPicker.Template.FindName("ItemsPresenterHost", daysPicker) as Canvas;
                    Assert.NotNull(daysHost);
                    Assert.Equal(Visibility.Collapsed, daysHost.Visibility);

                    Assert.True(picker.ActualHeight > 40);
                    Assert.True(daysPicker.ActualHeight > 40);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ListPicker_HighlightedVisualStateUsesBrushResources()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker { Width = 320 };
                picker.Items.Add("one");

                WpfTestHelper.ApplyDefaultStyle(picker);

                Window window = new Window { Width = 480, Height = 800, Content = picker };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(VisualStateManager.GoToState(picker, "Highlighted", false));
                    for (int i = 0; i < 5; i++)
                    {
                        WpfTestHelper.PumpDispatcher();
                    }

                    Border border = picker.Template.FindName("Border", picker) as Border;
                    Assert.NotNull(border);
                    SolidColorBrush background = Assert.IsType<SolidColorBrush>(border.Background);
                    Assert.Equal(Colors.White, background.Color);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void CustomMessageBox_ShowDismissRaisesLifecycleEvents()
        {
            StaTest.Run(() =>
            {
                CustomMessageBox box = new CustomMessageBox
                {
                    Title = "Title",
                    Message = "Message",
                    LeftButtonContent = "Cancel",
                    RightButtonContent = "OK"
                };

                int dismissing = 0;
                int dismissed = 0;
                CustomMessageBoxResult result = CustomMessageBoxResult.None;
                box.Dismissing += delegate { dismissing++; };
                box.Dismissed += delegate(object sender, DismissedEventArgs e)
                {
                    dismissed++;
                    result = e.Result;
                };

                // Show() is modal: it runs a nested dispatcher loop, so the
                // dismissal is driven by a timer that only ticks inside it. The
                // dialog must also work without an Application (test host).
                DispatcherTimer dismissTimer = new DispatcherTimer(DispatcherPriority.Background, box.Dispatcher);
                dismissTimer.Interval = TimeSpan.FromMilliseconds(150);
                dismissTimer.Tick += delegate
                {
                    dismissTimer.Stop();
                    box.Dismiss();
                };

                dismissTimer.Start();
                box.Show();

                // Show() returns once the close transition completed and the
                // window closed, so the events have already been raised.
                Assert.Equal(1, dismissing);
                Assert.Equal(1, dismissed);
                Assert.Equal(CustomMessageBoxResult.None, result);
            });
        }

        [Fact]
        public void DateTimePickers_FormatValuesAndRaiseValueChanged()
        {
            StaTest.Run(() =>
            {
                PhoneDatePicker date = new PhoneDatePicker { ValueStringFormat = "{0:yyyy-MM-dd}" };
                PhoneTimePicker time = new PhoneTimePicker { ValueStringFormat = "{0:HH:mm}" };
                int dateChanges = 0;
                int timeChanges = 0;
                date.ValueChanged += delegate { dateChanges++; };
                time.ValueChanged += delegate { timeChanges++; };

                date.Value = new DateTime(2020, 1, 2);
                time.Value = new DateTime(2020, 1, 2, 13, 45, 0);

                Assert.Equal("2020-01-02", date.ValueString);
                Assert.Equal("13:45", time.ValueString);
                Assert.Equal(1, dateChanges);
                Assert.Equal(1, timeChanges);
            });
        }

        [Fact]
        public void HubTile_PropertiesAndGroupFreeze()
        {
            StaTest.Run(() =>
            {
                HubTile tile = new HubTile
                {
                    Title = "News",
                    Message = "More",
                    Notification = "1",
                    DisplayNotification = true,
                    GroupTag = "group-a"
                };

                // A tile joins the service pools from its first SizeChanged, which
                // is what FreezeGroup walks; without a layout pass the group call
                // has nothing to freeze.
                Window window = new Window { Width = 260, Height = 260, Content = tile };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    Assert.Equal("News", tile.Title);
                    Assert.True(tile.DisplayNotification);
                    Assert.False(tile.IsFrozen);

                    HubTileService.FreezeGroup("group-a");
                    Assert.True(tile.IsFrozen);
                    HubTileService.UnfreezeGroup("group-a");
                    Assert.False(tile.IsFrozen);

                    HubTileService.FreezeHubTile(tile);
                    HubTileService.UnfreezeHubTile(tile);
                    Assert.False(tile.IsFrozen);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void HubTile_ShowsTheSourceImageAndFlipsToTheBackPanel()
        {
            StaTest.Run(() =>
            {
                HubTile tile = new HubTile
                {
                    Title = "News",
                    Message = "More",
                    DisplayNotification = false,
                    Source = CreateSolidBitmap(Colors.Red)
                };

                Window window = new Window { Width = 260, Height = 260, Content = tile };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    window.UpdateLayout();
                    WpfTestHelper.PumpDispatcher();

                    // HubTileService flips tiles from its own timer, so freeze this
                    // one and observe only the states the test sets.
                    HubTileService.FreezeHubTile(tile);

                    Grid titlePanel = (Grid)tile.Template.FindName("TitlePanel", tile);
                    FrameworkElement viewport = (FrameworkElement)tile.Template.FindName("Viewport", tile);
                    PlaneProjection projection = ProjectionService.GetProjection(viewport) as PlaneProjection;
                    Border image = (Border)tile.Template.FindName("Image", tile);
                    FrameworkElement backPanel = (FrameworkElement)tile.Template.FindName("BackPanel", tile);

                    Assert.NotNull(titlePanel);
                    Assert.NotNull(projection);

                    // The template slides a two-tile-high surface through a
                    // one-tile-high viewport, so Expanded has to move the second
                    // band (the Source image) into view.
                    double height = tile.ActualHeight;
                    Assert.True(
                        WpfTestHelper.PumpUntil(() => Math.Abs(SurfaceOffset(titlePanel) + height) < 0.5),
                        "Expanded must slide the image band into the viewport.");
                    Assert.True(
                        CountRedPixels(tile) > 1000,
                        "The Expanded tile has to show the Source image.");

                    tile.State = ImageState.Semiexpanded;
                    Assert.True(
                        WpfTestHelper.PumpUntil(
                            () => Math.Abs(SurfaceOffset(titlePanel) + (0.4566 * height)) < 1.0),
                        "Semiexpanded must slide the surface to the original offset.");

                    tile.State = ImageState.Flipped;
                    Assert.True(
                        WpfTestHelper.PumpUntil(() => Math.Abs(Math.Abs(projection.RotationX) - 180.0) < 1.0),
                        "The flip has to rotate the viewport to 180 degrees.");
                    Assert.Equal(Visibility.Collapsed, image.Visibility);
                    Assert.Equal(Visibility.Visible, backPanel.Visibility);
                    Assert.Equal(0, CountRedPixels(tile));
                    Assert.True(
                        CountOpaquePixels(tile) > 1000,
                        "A projection past 90 degrees has to keep rendering its content.");

                    tile.State = ImageState.Expanded;
                    Assert.True(
                        WpfTestHelper.PumpUntil(() => CountRedPixels(tile) > 1000),
                        "Returning to Expanded has to show the image again.");
                    Assert.Equal(Visibility.Visible, image.Visibility);
                    Assert.Equal(Visibility.Collapsed, backPanel.Visibility);
                }
                finally
                {
                    HubTileService.UnfreezeHubTile(tile);
                    window.Close();
                }
            });
        }

        [Fact]
        public void HubTile_FlipDrawsOutsideTheTileBounds()
        {
            StaTest.Run(() =>
            {
                // The phone toolkit projects the tile's rasterized content and
                // lets the rotated result draw outside the tile: the near half
                // of the flip is magnified and overlaps the tile's neighbours.
                // Only the content may be clipped, so neither the tile nor the
                // template's wrapper panel may carry a clip in the 3D path.
                const int TileLeft = 70;
                const int TileTop = 80;

                HubTile tile = new HubTile
                {
                    Title = "News",
                    Message = "More",
                    DisplayNotification = false,
                    Source = CreateSolidBitmap(Colors.Red)
                };

                Canvas canvas = new Canvas();
                Canvas.SetLeft(tile, TileLeft);
                Canvas.SetTop(tile, TileTop);
                canvas.Children.Add(tile);

                Grid root = new Grid
                {
                    Width = 320,
                    Height = 340,
                    Background = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Left,
                    VerticalAlignment = VerticalAlignment.Top
                };
                root.Children.Add(canvas);

                Window window = new Window { Width = 380, Height = 400, Content = root };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    window.UpdateLayout();
                    WpfTestHelper.PumpDispatcher();

                    HubTileService.FreezeHubTile(tile);

                    FrameworkElement viewport = (FrameworkElement)tile.Template.FindName("Viewport", tile);
                    PlaneProjection projection = ProjectionService.GetProjection(viewport) as PlaneProjection;
                    Assert.NotNull(projection);

                    Rect tileBounds = new Rect(TileLeft, TileTop, tile.ActualWidth, tile.ActualHeight);
                    Assert.True(tileBounds.Width > 100, "The tile has to be laid out before the flip.");

                    Assert.True(
                        WpfTestHelper.PumpUntil(() => CountRedPixelsInside(root, tileBounds) > 1000),
                        "The Expanded tile has to show the Source image.");

                    // The band of the sliding surface that has left the
                    // viewport stays clipped while the tile is flat.
                    Assert.Equal(0, CountRedPixelsOutside(root, tileBounds));

                    projection.BeginAnimation(PlaneProjection.RotationXProperty, null);
                    projection.RotationX = 45;
                    Assert.True(
                        WpfTestHelper.PumpUntil(() => CountRedPixelsInside(root, tileBounds) > 500),
                        "The rotated tile has to keep rendering its content.");

                    Assert.True(
                        CountRedPixelsOutside(root, tileBounds) > 20,
                        "The projected half of the flip has to draw outside the tile bounds.");
                }
                finally
                {
                    HubTileService.UnfreezeHubTile(tile);
                    window.Close();
                }
            });
        }

        private static double SurfaceOffset(Grid titlePanel)
        {
            TranslateTransform transform = titlePanel.RenderTransform as TranslateTransform;
            return transform == null ? double.NaN : transform.Y;
        }

        private static BitmapSource CreateSolidBitmap(Color color)
        {
            const int Size = 8;
            int stride = Size * 4;
            byte[] pixels = new byte[stride * Size];
            for (int i = 0; i < pixels.Length; i += 4)
            {
                pixels[i] = color.B;
                pixels[i + 1] = color.G;
                pixels[i + 2] = color.R;
                pixels[i + 3] = 255;
            }

            BitmapSource bitmap = BitmapSource.Create(Size, Size, 96, 96, PixelFormats.Pbgra32, null, pixels, stride);
            bitmap.Freeze();
            return bitmap;
        }

        private static byte[] RenderPixels(FrameworkElement element)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                Math.Max(1, (int)Math.Ceiling(element.ActualWidth)),
                Math.Max(1, (int)Math.Ceiling(element.ActualHeight)),
                96,
                96,
                PixelFormats.Pbgra32);
            bitmap.Render(element);

            int stride = bitmap.PixelWidth * 4;
            byte[] buffer = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(buffer, stride, 0);
            return buffer;
        }

        private static int CountRedPixels(FrameworkElement element)
        {
            byte[] buffer = RenderPixels(element);

            int count = 0;
            for (int i = 0; i < buffer.Length; i += 4)
            {
                if (buffer[i + 3] > 30 && buffer[i + 2] > 190 && buffer[i] < 70 && buffer[i + 1] < 70)
                {
                    count++;
                }
            }

            return count;
        }

        private static int CountRedPixelsInside(FrameworkElement element, Rect bounds)
        {
            return CountRedPixels(element, bounds, true);
        }

        private static int CountRedPixelsOutside(FrameworkElement element, Rect bounds)
        {
            return CountRedPixels(element, bounds, false);
        }

        private static int CountRedPixels(FrameworkElement element, Rect bounds, bool inside)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                Math.Max(1, (int)Math.Ceiling(element.ActualWidth)),
                Math.Max(1, (int)Math.Ceiling(element.ActualHeight)),
                96,
                96,
                PixelFormats.Pbgra32);
            bitmap.Render(element);

            int stride = bitmap.PixelWidth * 4;
            byte[] buffer = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(buffer, stride, 0);

            int count = 0;
            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    if (bounds.Contains(x + 0.5, y + 0.5) != inside)
                    {
                        continue;
                    }

                    int offset = (y * bitmap.PixelWidth + x) * 4;
                    if (buffer[offset + 3] > 30 && buffer[offset + 2] > 190 && buffer[offset] < 70 && buffer[offset + 1] < 70)
                    {
                        count++;
                    }
                }
            }

            return count;
        }

        private static int CountOpaquePixels(FrameworkElement element)
        {
            byte[] buffer = RenderPixels(element);

            int count = 0;
            for (int i = 3; i < buffer.Length; i += 4)
            {
                if (buffer[i] > 30)
                {
                    count++;
                }
            }

            return count;
        }

        [Fact]
        public void ContextMenuService_AttachesMenu()
        {
            StaTest.Run(() =>
            {
                Border element = new Border();
                PhoneContextMenu menu = new PhoneContextMenu();
                PhoneContextMenuService.SetContextMenu(element, menu);

                Assert.Same(menu, PhoneContextMenuService.GetContextMenu(element));
            });
        }

        [Fact]
        public void ContextMenu_OpensAndCloses()
        {
            StaTest.Run(() =>
            {
                Border element = new Border { Width = 200, Height = 100, Background = Brushes.White };
                PhoneContextMenu menu = new PhoneContextMenu();
                menu.Items.Add(new Microsoft.Phone.Controls.MenuItem { Header = "Open" });
                PhoneContextMenuService.SetContextMenu(element, menu);

                Window window = new Window { Width = 300, Height = 200, Content = element };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    element.RaiseEvent(new MouseButtonEventArgs(Mouse.PrimaryDevice, 0, MouseButton.Right)
                    {
                        RoutedEvent = UIElement.PreviewMouseRightButtonDownEvent
                    });
                    WpfTestHelper.PumpDispatcher();
                    Assert.True(menu.IsOpen);
                    Assert.Same(element, menu.Owner);

                    menu.IsOpen = false;
                    WpfTestHelper.PumpDispatcher();
                    Assert.False(menu.IsOpen);
                }
                finally
                {
                    menu.IsOpen = false;
                    window.Close();
                }
            });
        }

        [Fact]
        public void DateTimePickers_HaveExpectedDefaultsAndFormatting()
        {
            StaTest.Run(() =>
            {
                PhoneDatePicker datePicker = new PhoneDatePicker();
                PhoneTimePicker timePicker = new PhoneTimePicker();

                Assert.NotNull(datePicker.Value);
                Assert.NotNull(timePicker.Value);
                Assert.False(string.IsNullOrEmpty(datePicker.ValueString));
                Assert.False(string.IsNullOrEmpty(timePicker.ValueString));
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

        [Fact]
        public void RecurringDaysPicker_SummarizesWeekdays()
        {
            StaTest.Run(() =>
            {
                System.Globalization.CultureInfo oldCulture = System.Globalization.CultureInfo.CurrentCulture;
                System.Globalization.CultureInfo oldUiCulture = System.Globalization.CultureInfo.CurrentUICulture;
                System.Globalization.CultureInfo oldResourceCulture = Microsoft.Phone.Controls.LocalizedResources.ControlResources.Culture;
                try
                {
                    System.Globalization.CultureInfo.CurrentCulture = new System.Globalization.CultureInfo("en-US");
                    System.Globalization.CultureInfo.CurrentUICulture = new System.Globalization.CultureInfo("en-US");
                    Microsoft.Phone.Controls.LocalizedResources.ControlResources.Culture = new System.Globalization.CultureInfo("en-US");
                    RecurringDaysPicker picker = new RecurringDaysPicker();
                    ArrayList selection = new ArrayList
                    {
                        "Monday", "Tuesday", "Wednesday", "Thursday", "Friday"
                    };

                    object summary = picker.GetType()
                        .GetMethod("SummarizeDaysOfWeek", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)
                        .Invoke(picker, new object[] { selection });

                    Assert.Contains("weekday", summary.ToString(), StringComparison.OrdinalIgnoreCase);
                }
                finally
                {
                    System.Globalization.CultureInfo.CurrentCulture = oldCulture;
                    System.Globalization.CultureInfo.CurrentUICulture = oldUiCulture;
                    Microsoft.Phone.Controls.LocalizedResources.ControlResources.Culture = oldResourceCulture;
                }
            });
        }
    }
}
