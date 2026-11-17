// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;
using System.Windows.Threading;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Renders an <see cref="IApplicationBar"/>: the icon buttons, the menu
    /// button and the menu. In Windows Phone this chrome is drawn by the
    /// compositor; the WPF port draws it with this control, which
    /// <see cref="Microsoft.Phone.Controls.PhoneApplicationFrame"/> hosts under
    /// the page content. This type is additive (it has no Windows Phone
    /// counterpart) and is documented as such.
    /// </summary>
    public class ApplicationBarPresenter : Border
    {
        /// <summary>
        /// The height of the bar in the default mode.
        /// </summary>
        internal const double DefaultBarHeight = 72;

        /// <summary>
        /// The height of the bar in the minimized mode.
        /// </summary>
        internal const double MiniBarHeight = 30;

        /// <summary>
        /// The width of the menu button slot.
        /// </summary>
        internal const double MenuButtonWidth = 48;

        /// <summary>
        /// Identifies the <see cref="ApplicationBar"/> property.
        /// </summary>
        public static readonly DependencyProperty ApplicationBarProperty =
            DependencyProperty.Register(
                "ApplicationBar",
                typeof(IApplicationBar),
                typeof(ApplicationBarPresenter),
                new PropertyMetadata(null, OnApplicationBarPropertyChanged));

        private static readonly Brush TransparentBrush = CreateFrozenBrush(Color.FromArgb(0, 0, 0, 0));
        private static readonly Brush HighlightBrush = CreateFrozenBrush(Color.FromArgb(0x33, 0xFF, 0xFF, 0xFF));

        private readonly Grid _bar;
        private readonly StackPanel _buttons;
        private readonly Popup _menuPopup;
        private readonly Border _menuBorder;
        private readonly StackPanel _menuPanel;

        private ApplicationBar _subscribedBar;
        private bool _isMenuOpen;
        private double _reservedHeight;

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarPresenter"/> class.
        /// </summary>
        public ApplicationBarPresenter()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Bottom;
            Visibility = Visibility.Collapsed;

            _bar = new Grid
            {
                VerticalAlignment = VerticalAlignment.Bottom,
                Background = TransparentBrush,
            };

            _buttons = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Stretch,
            };

            _bar.Children.Add(_buttons);
            Child = _bar;

            _menuPanel = new StackPanel { Orientation = Orientation.Vertical };
            _menuBorder = new Border
            {
                Child = _menuPanel,
                Focusable = true,
                UseLayoutRounding = true,
            };
            _menuBorder.PreviewKeyDown += OnMenuPreviewKeyDown;

            _menuPopup = new Popup
            {
                Placement = PlacementMode.Top,
                PlacementTarget = this,
                StaysOpen = false,
                AllowsTransparency = true,
                PopupAnimation = PopupAnimation.Slide,
                Child = _menuBorder,
            };
            _menuPopup.Closed += OnMenuPopupClosed;
        }

        /// <summary>
        /// Occurs when the height the page layout must reserve for the bar changed.
        /// </summary>
        internal event EventHandler ReservedHeightChanged;

        /// <summary>
        /// Gets or sets the application bar rendered by this presenter.
        /// </summary>
        public IApplicationBar ApplicationBar
        {
            get { return (IApplicationBar)GetValue(ApplicationBarProperty); }
            set { SetValue(ApplicationBarProperty, value); }
        }

        /// <summary>
        /// Gets the height the page layout must reserve for the bar. It is the
        /// bar height when the bar is visible and opaque, and zero otherwise
        /// (the bar then floats over the page), like the original.
        /// </summary>
        internal double ReservedHeight
        {
            get { return _reservedHeight; }
        }

        /// <summary>
        /// Gets a value indicating whether the menu is open.
        /// </summary>
        internal bool IsMenuOpen
        {
            get { return _isMenuOpen; }
        }

        /// <summary>
        /// Gets the elements rendered for the bar items (icon buttons, then
        /// the menu button when the bar has menu items).
        /// </summary>
        internal UIElementCollection ButtonElements
        {
            get { return _buttons.Children; }
        }

        /// <summary>
        /// Gets the elements rendered for the menu items.
        /// </summary>
        internal UIElementCollection MenuElements
        {
            get { return _menuPanel.Children; }
        }

        /// <summary>
        /// Gets the height of the bar for the given bar.
        /// </summary>
        /// <param name="bar">The bar.</param>
        /// <returns>The bar height.</returns>
        internal static double GetBarHeight(IApplicationBar bar)
        {
            if (bar != null && bar.Mode == ApplicationBarMode.Minimized)
            {
                return MiniBarHeight;
            }

            return DefaultBarHeight;
        }

        /// <summary>
        /// Gets the height the page layout must reserve for the given bar.
        /// </summary>
        /// <param name="bar">The bar.</param>
        /// <returns>The reserved height.</returns>
        internal static double GetReservedHeight(IApplicationBar bar)
        {
            if (bar == null)
            {
                return 0;
            }

            ApplicationBar applicationBar = bar as ApplicationBar;
            if (applicationBar != null)
            {
                return applicationBar.ReservedHeight;
            }

            bool reserves = bar.IsVisible
                && bar.Opacity >= 1.0
                && (bar.BackgroundColor.A >= byte.MaxValue || bar.BackgroundColor.A == 0);

            return reserves ? GetBarHeight(bar) : 0;
        }

        /// <summary>
        /// Opens the menu, like tapping the menu button.
        /// </summary>
        internal void OpenMenu()
        {
            IApplicationBar bar = ApplicationBar;
            if (bar == null || !bar.IsMenuEnabled || bar.MenuItems.Count == 0 || _isMenuOpen)
            {
                return;
            }

            Brush foreground = new SolidColorBrush(bar.ForegroundColor);
            UpdateMenuItems(bar, foreground);
            _menuBorder.Background = new SolidColorBrush(bar.BackgroundColor);
            _menuBorder.Width = Math.Max(ActualWidth, 1);
            _menuBorder.Opacity = bar.Opacity;

            _isMenuOpen = true;
            _menuPopup.IsOpen = true;

            ApplicationBar applicationBar = bar as ApplicationBar;
            if (applicationBar != null)
            {
                applicationBar.OnEnterMenu();
            }

            Dispatcher.BeginInvoke(DispatcherPriority.Input, new Action(() => _menuBorder.Focus()));
        }

        /// <summary>
        /// Closes the menu, like tapping an item or tapping outside of it.
        /// </summary>
        internal void CloseMenu()
        {
            if (!_isMenuOpen)
            {
                return;
            }

            _isMenuOpen = false;
            _menuPopup.IsOpen = false;

            ApplicationBar applicationBar = ApplicationBar as ApplicationBar;
            if (applicationBar != null)
            {
                applicationBar.OnExitMenu();
            }
        }

        /// <summary>
        /// Gets the default font family of the bar, from the phone theme when
        /// it is available.
        /// </summary>
        /// <returns>The font family.</returns>
        internal static FontFamily GetDefaultFontFamily()
        {
            try
            {
                Application application = Application.Current;
                if (application != null)
                {
                    FontFamily family = application.TryFindResource("PhoneFontFamilyNormal") as FontFamily;
                    if (family != null)
                    {
                        return family;
                    }
                }
            }
            catch (InvalidOperationException)
            {
                // No resource scope yet; fall through to the system font.
            }

            return new FontFamily("Segoe UI");
        }

        /// <summary>
        /// Updates the chrome from the current <see cref="ApplicationBar"/>.
        /// </summary>
        internal void UpdateChrome()
        {
            IApplicationBar bar = ApplicationBar;

            if (bar == null || !bar.IsVisible)
            {
                CloseMenu();
                Visibility = Visibility.Collapsed;
                Height = 0;
                _buttons.Children.Clear();
                UpdateReservedHeight(0);
                return;
            }

            double barHeight = GetBarHeight(bar);
            Visibility = Visibility.Visible;
            Height = barHeight;
            _bar.Height = barHeight;
            _bar.Background = new SolidColorBrush(bar.BackgroundColor);
            _bar.Opacity = bar.Opacity;

            Brush foreground = new SolidColorBrush(bar.ForegroundColor);
            UpdateButtons(bar, foreground);

            if (_isMenuOpen)
            {
                UpdateMenuItems(bar, foreground);
                _menuBorder.Background = new SolidColorBrush(bar.BackgroundColor);
                _menuBorder.Opacity = bar.Opacity;
            }

            UpdateReservedHeight(GetReservedHeight(bar));
        }

        private static void OnApplicationBarPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ApplicationBarPresenter presenter = (ApplicationBarPresenter)d;
            presenter.OnApplicationBarChanged((IApplicationBar)e.OldValue, (IApplicationBar)e.NewValue);
        }

        private static Brush CreateFrozenBrush(Color color)
        {
            SolidColorBrush brush = new SolidColorBrush(color);
            brush.Freeze();
            return brush;
        }

        private void OnApplicationBarChanged(IApplicationBar oldBar, IApplicationBar newBar)
        {
            if (_subscribedBar != null)
            {
                _subscribedBar.Changed -= OnBarChanged;
                _subscribedBar = null;
            }

            CloseMenu();

            _subscribedBar = newBar as ApplicationBar;
            if (_subscribedBar != null)
            {
                _subscribedBar.Changed += OnBarChanged;
            }

            UpdateChrome();
        }

        private void OnBarChanged(object sender, EventArgs e)
        {
            UpdateChrome();
        }

        private void UpdateButtons(IApplicationBar bar, Brush foreground)
        {
            _buttons.Children.Clear();

            bool minimized = bar.Mode == ApplicationBarMode.Minimized;
            if (!minimized)
            {
                for (int i = 0; i < bar.Buttons.Count; i++)
                {
                    ApplicationBarItemContainer container = GetButtonContainer(bar, i);
                    if (container == null)
                    {
                        continue;
                    }

                    ApplicationBarIconButtonView view = new ApplicationBarIconButtonView(container)
                    {
                        ForegroundBrush = foreground,
                        VerticalAlignment = VerticalAlignment.Stretch,
                    };

                    _buttons.Children.Add(view);
                }
            }

            if (bar.IsMenuEnabled && bar.MenuItems.Count > 0)
            {
                _buttons.Children.Add(CreateMenuButton(foreground, minimized));
            }
        }

        private void UpdateMenuItems(IApplicationBar bar, Brush foreground)
        {
            _menuPanel.Children.Clear();

            for (int i = 0; i < bar.MenuItems.Count; i++)
            {
                ApplicationBarItemContainer container = GetMenuItemContainer(bar, i);
                if (container == null)
                {
                    continue;
                }

                ApplicationBarMenuItemView view = new ApplicationBarMenuItemView(container)
                {
                    ForegroundBrush = foreground,
                };

                view.Activated += OnMenuItemActivated;
                _menuPanel.Children.Add(view);
            }
        }

        private Border CreateMenuButton(Brush foreground, bool minimized)
        {
            Border button = new Border
            {
                Width = MenuButtonWidth,
                Background = TransparentBrush,
                Cursor = Cursors.Hand,
                Focusable = true,
                HorizontalAlignment = HorizontalAlignment.Right,
                VerticalAlignment = minimized ? VerticalAlignment.Stretch : VerticalAlignment.Top,
                Height = minimized ? double.NaN : 30,
            };

            StackPanel dots = new StackPanel
            {
                Orientation = Orientation.Horizontal,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            for (int i = 0; i < 3; i++)
            {
                dots.Children.Add(new Rectangle
                {
                    Width = 3,
                    Height = 3,
                    Margin = new Thickness(2, 0, 2, 0),
                    Fill = foreground,
                });
            }

            button.Child = dots;
            AutomationProperties.SetName(button, "menu");

            button.MouseEnter += delegate { button.Background = HighlightBrush; };
            button.MouseLeave += delegate { button.Background = TransparentBrush; };
            button.MouseLeftButtonUp += delegate(object sender, MouseButtonEventArgs e)
            {
                ToggleMenu();
                e.Handled = true;
            };
            button.KeyDown += delegate(object sender, KeyEventArgs e)
            {
                if (e.Key == Key.Enter || e.Key == Key.Space)
                {
                    ToggleMenu();
                    e.Handled = true;
                }
            };

            return button;
        }

        private void ToggleMenu()
        {
            if (_isMenuOpen)
            {
                CloseMenu();
            }
            else
            {
                OpenMenu();
            }
        }

        private void OnMenuItemActivated(object sender, EventArgs e)
        {
            CloseMenu();
        }

        private void OnMenuPreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                CloseMenu();
                e.Handled = true;
            }
        }

        private void OnMenuPopupClosed(object sender, EventArgs e)
        {
            if (!_isMenuOpen)
            {
                return;
            }

            _isMenuOpen = false;

            ApplicationBar applicationBar = ApplicationBar as ApplicationBar;
            if (applicationBar != null)
            {
                applicationBar.OnExitMenu();
            }
        }

        private void UpdateReservedHeight(double height)
        {
            if (height == _reservedHeight)
            {
                return;
            }

            _reservedHeight = height;

            EventHandler handler = ReservedHeightChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }

        private static ApplicationBarItemContainer GetButtonContainer(IApplicationBar bar, int index)
        {
            ApplicationBar applicationBar = bar as ApplicationBar;
            return applicationBar == null ? null : applicationBar.GetButtonContainer(index);
        }

        private static ApplicationBarItemContainer GetMenuItemContainer(IApplicationBar bar, int index)
        {
            ApplicationBar applicationBar = bar as ApplicationBar;
            return applicationBar == null ? null : applicationBar.GetMenuItemContainer(index);
        }
    }
}
