using System;
using System.Collections;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Microsoft.Phone.Controls;
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
        public void Rating_MouseClickChangesValue()
        {
            StaTest.Run(() =>
            {
                Rating rating = new Rating { Width = 200, Height = 40, RatingItemCount = 5, Value = 1 };
                Window window = new Window
                {
                    Width = 300,
                    Height = 160,
                    Left = 120,
                    Top = 120,
                    WindowStartupLocation = WindowStartupLocation.Manual,
                    ShowInTaskbar = false,
                    Topmost = true,
                    Content = rating
                };
                try
                {
                    window.Show();
                    window.Activate();
                    PumpFor(150);

                    // Real pointer input depends on this machine's desktop
                    // (a lock screen or another window on top blocks it), so
                    // retry a few times and skip the assertion when the
                    // pointer cannot be placed over the control.
                    Point point = rating.PointToScreen(new Point(150, 20));
                    Point local = default(Point);
                    for (int attempt = 0; attempt < 5; attempt++)
                    {
                        SetCursorPos((int)point.X, (int)point.Y);
                        PumpFor(80);
                        local = Mouse.GetPosition(rating);
                        if (local.X > 50)
                        {
                            break;
                        }
                    }

                    if (local.X <= 50)
                    {
                        return;
                    }

                    mouse_event(MouseLeftDown, 0, 0, 0, UIntPtr.Zero);
                    mouse_event(MouseLeftUp, 0, 0, 0, UIntPtr.Zero);
                    PumpFor(200);

                    Assert.Equal(4, rating.Value);
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
        public void ListPicker_PopupSelectsItemAndCloses()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker { Width = 320, Height = 60 };
                picker.Items.Add("one");
                picker.Items.Add("two");
                picker.Items.Add("three");

                WpfTestHelper.ApplyDefaultStyle(picker);
                Window window = new Window { Width = 400, Height = 300, Content = picker };
                Popup popup = null;
                try
                {
                    window.Show();
                    WpfTestHelper.Layout(picker, 320, 60);

                    popup = (Popup)picker.Template.FindName("PickerPopup", picker);
                    ListBox list = (ListBox)picker.Template.FindName("PickerListBox", picker);
                    Assert.NotNull(popup);
                    Assert.NotNull(list);

                    int changes = 0;
                    picker.SelectionChanged += delegate { changes++; };

                    Assert.True(picker.Open());
                    Assert.True(popup.IsOpen);

                    list.SelectedIndex = 1;
                    WpfTestHelper.PumpDispatcher();

                    Assert.Equal(1, picker.SelectedIndex);
                    Assert.Equal("two", picker.SelectedItem);
                    Assert.False(popup.IsOpen);
                    Assert.Equal(1, changes);
                }
                finally
                {
                    if (popup != null)
                    {
                        popup.IsOpen = false;
                    }

                    window.Close();
                }
            });
        }

        [Fact]
        public void ListPicker_OpenFailsWithoutItems()
        {
            StaTest.Run(() =>
            {
                ListPicker picker = new ListPicker();
                WpfTestHelper.ApplyDefaultStyle(picker);
                WpfTestHelper.Layout(picker);
                Assert.False(picker.Open());
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

                box.Show();
                WpfTestHelper.PumpDispatcher();
                box.Dismiss();

                // The message box swivels out before the window closes, so the
                // dismissed event arrives after the transition.
                PumpFor(700);

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

                Assert.Equal("News", tile.Title);
                Assert.True(tile.DisplayNotification);
                Assert.False(tile.IsFrozen);

                HubTileService.FreezeGroup("group-a");
                HubTileService.FreezeHubTile(tile);
                Assert.True(tile.IsFrozen);
                HubTileService.UnfreezeGroup("group-a");
                HubTileService.UnfreezeHubTile(tile);
                Assert.False(tile.IsFrozen);
            });
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

        private const uint MouseLeftDown = 0x0002;
        private const uint MouseLeftUp = 0x0004;

        [DllImport("user32.dll")]
        private static extern bool SetCursorPos(int x, int y);

        [DllImport("user32.dll")]
        private static extern void mouse_event(uint flags, uint dx, uint dy, uint data, UIntPtr extraInfo);

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
