// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Renders an <see cref="IApplicationBar"/>: the icon buttons, the menu
    /// button and the menu. In Windows Phone this chrome is drawn by the
    /// compositor; the WPF port draws it with this control, whose template lives
    /// in Themes/Controls/ApplicationBar.xaml and which
    /// <see cref="Microsoft.Phone.Controls.PhoneApplicationFrame"/> hosts under
    /// the page content. This type is additive (it has no Windows Phone
    /// counterpart) and is documented as such.
    /// </summary>
    public class ApplicationBarPresenter : Control
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
        /// The height of the menu button slot, measured from the top of the
        /// icon row; it fills the minimized bar.
        /// </summary>
        internal const double MenuButtonHeight = 30;

        private const string ButtonsPartName = "PART_Buttons";
        private const string MenuItemsPartName = "PART_MenuItems";
        private const string MenuButtonPartName = "PART_MenuButton";

        /// <summary>
        /// Identifies the <see cref="ApplicationBar"/> property.
        /// </summary>
        public static readonly DependencyProperty ApplicationBarProperty =
            DependencyProperty.Register(
                "ApplicationBar",
                typeof(IApplicationBar),
                typeof(ApplicationBarPresenter),
                new PropertyMetadata(null, OnApplicationBarPropertyChanged));

        private StackPanel _buttons;
        private StackPanel _menuItems;
        private Border _menuButton;

        private ApplicationBar _subscribedBar;
        private bool _isExpanded;
        private double _reservedHeight;

        static ApplicationBarPresenter()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(ApplicationBarPresenter),
                new FrameworkPropertyMetadata(typeof(ApplicationBarPresenter)));
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ApplicationBarPresenter"/> class.
        /// </summary>
        public ApplicationBarPresenter()
        {
            HorizontalAlignment = HorizontalAlignment.Stretch;
            VerticalAlignment = VerticalAlignment.Bottom;
            Visibility = Visibility.Collapsed;
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
        /// (the bar then floats over the page), like the original. The expanded
        /// bar is taller than the reserved height, so it covers the page while
        /// it is open instead of reflowing it.
        /// </summary>
        internal double ReservedHeight
        {
            get { return _reservedHeight; }
        }

        /// <summary>
        /// Gets a value indicating whether the bar is expanded.
        /// </summary>
        internal bool IsMenuOpen
        {
            get { return _isExpanded; }
        }

        /// <summary>
        /// Gets the elements rendered for the bar items (icon buttons).
        /// </summary>
        internal UIElementCollection ButtonElements
        {
            get { return _buttons == null ? null : _buttons.Children; }
        }

        /// <summary>
        /// Gets the elements rendered for the menu items.
        /// </summary>
        internal UIElementCollection MenuElements
        {
            get { return _menuItems == null ? null : _menuItems.Children; }
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

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            DetachMenuButton();

            base.OnApplyTemplate();

            _buttons = GetTemplateChild(ButtonsPartName) as StackPanel;
            _menuItems = GetTemplateChild(MenuItemsPartName) as StackPanel;
            _menuButton = GetTemplateChild(MenuButtonPartName) as Border;

            AttachMenuButton();
            UpdateChrome();
        }

        /// <summary>
        /// Expands the bar, like tapping the menu button: the bar grows upward
        /// to show the icon labels and the menu items.
        /// </summary>
        internal void OpenMenu()
        {
            IApplicationBar bar = ApplicationBar;
            if (bar == null || !bar.IsMenuEnabled || _isExpanded || !HasItems(bar))
            {
                return;
            }

            _isExpanded = true;
            UpdateChrome();

            ApplicationBar applicationBar = bar as ApplicationBar;
            if (applicationBar != null)
            {
                applicationBar.OnEnterMenu();
            }
        }

        /// <summary>
        /// Collapses the bar back to its resting height.
        /// </summary>
        internal void CloseMenu()
        {
            if (!_isExpanded)
            {
                return;
            }

            _isExpanded = false;
            UpdateChrome();

            ApplicationBar applicationBar = ApplicationBar as ApplicationBar;
            if (applicationBar != null)
            {
                applicationBar.OnExitMenu();
            }
        }

        /// <summary>
        /// Updates the chrome from the current <see cref="ApplicationBar"/>.
        /// </summary>
        internal void UpdateChrome()
        {
            IApplicationBar bar = ApplicationBar;

            if (bar == null || !bar.IsVisible)
            {
                CollapseWithoutNotification();

                Visibility = Visibility.Collapsed;
                Height = 0;
                ClearItems();
                UpdateReservedHeight(0);
                return;
            }

            if (_isExpanded && !HasItems(bar))
            {
                CollapseWithoutNotification();
            }

            bool minimized = bar.Mode == ApplicationBarMode.Minimized;
            bool showButtons = !minimized || _isExpanded;
            double itemsHeight = _isExpanded ? bar.MenuItems.Count * ApplicationBarMenuItemView.RowHeight : 0;
            double iconRowHeight = showButtons
                ? ApplicationBarIconButtonView.IconAreaHeight + (_isExpanded ? ApplicationBarIconButtonView.LabelHeight : 0)
                : 0;
            double height = Math.Max(GetBarHeight(bar), iconRowHeight + itemsHeight);

            Visibility = Visibility.Visible;
            Height = height;
            Background = new SolidColorBrush(bar.BackgroundColor);
            Opacity = bar.Opacity;
            Foreground = new SolidColorBrush(bar.ForegroundColor);

            if (_buttons != null)
            {
                _buttons.Height = iconRowHeight;
                _buttons.Visibility = showButtons ? Visibility.Visible : Visibility.Collapsed;
            }

            UpdateButtons(bar, showButtons);
            UpdateMenuItems(bar);
            UpdateMenuButton(height, itemsHeight, minimized);

            UpdateReservedHeight(GetReservedHeight(bar));
        }

        private static void OnApplicationBarPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ApplicationBarPresenter presenter = (ApplicationBarPresenter)d;
            presenter.OnApplicationBarChanged((IApplicationBar)e.OldValue, (IApplicationBar)e.NewValue);
        }

        private static bool HasItems(IApplicationBar bar)
        {
            return bar.Buttons.Count > 0 || bar.MenuItems.Count > 0;
        }

        private void OnApplicationBarChanged(IApplicationBar oldBar, IApplicationBar newBar)
        {
            if (_subscribedBar != null)
            {
                _subscribedBar.Changed -= OnBarChanged;
                _subscribedBar = null;
            }

            if (_isExpanded)
            {
                _isExpanded = false;
                ApplicationBar oldApplicationBar = oldBar as ApplicationBar;
                if (oldApplicationBar != null)
                {
                    oldApplicationBar.OnExitMenu();
                }
            }

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

        private void CollapseWithoutNotification()
        {
            if (_isExpanded)
            {
                _isExpanded = false;
                ApplicationBar applicationBar = ApplicationBar as ApplicationBar;
                if (applicationBar != null)
                {
                    applicationBar.OnExitMenu();
                }
            }
        }

        private void ClearItems()
        {
            if (_buttons != null)
            {
                _buttons.Children.Clear();
            }

            if (_menuItems != null)
            {
                _menuItems.Children.Clear();
            }
        }

        private void UpdateButtons(IApplicationBar bar, bool showButtons)
        {
            if (_buttons == null)
            {
                return;
            }

            _buttons.Children.Clear();

            if (!showButtons)
            {
                return;
            }

            for (int i = 0; i < bar.Buttons.Count; i++)
            {
                ApplicationBarItemContainer container = GetButtonContainer(bar, i);
                if (container == null)
                {
                    continue;
                }

                ApplicationBarIconButtonView view = new ApplicationBarIconButtonView(container)
                {
                    Foreground = Foreground,
                    VerticalAlignment = VerticalAlignment.Top,
                };

                _buttons.Children.Add(view);
            }
        }

        private void UpdateMenuItems(IApplicationBar bar)
        {
            if (_menuItems == null)
            {
                return;
            }

            _menuItems.Children.Clear();

            if (!_isExpanded)
            {
                return;
            }

            for (int i = 0; i < bar.MenuItems.Count; i++)
            {
                ApplicationBarItemContainer container = GetMenuItemContainer(bar, i);
                if (container == null)
                {
                    continue;
                }

                ApplicationBarMenuItemView view = new ApplicationBarMenuItemView(container)
                {
                    Foreground = Foreground,
                };

                view.Activated += OnMenuItemActivated;
                _menuItems.Children.Add(view);
            }
        }

        private void UpdateMenuButton(double barHeight, double itemsHeight, bool minimized)
        {
            if (_menuButton == null)
            {
                return;
            }

            if (minimized && !_isExpanded)
            {
                _menuButton.VerticalAlignment = VerticalAlignment.Stretch;
                _menuButton.Height = double.NaN;
                _menuButton.Margin = new Thickness(0);
            }
            else
            {
                // Keep the button at the top of the icon row, so it stays put
                // while the bar grows upward.
                _menuButton.VerticalAlignment = VerticalAlignment.Bottom;
                _menuButton.Height = MenuButtonHeight;
                _menuButton.Margin = new Thickness(
                    0,
                    0,
                    0,
                    Math.Max(barHeight - itemsHeight - MenuButtonHeight, 0));
            }
        }

        private void AttachMenuButton()
        {
            if (_menuButton == null)
            {
                return;
            }

            _menuButton.MouseLeftButtonUp += OnMenuButtonClick;
            _menuButton.KeyDown += OnMenuButtonKeyDown;
        }

        private void DetachMenuButton()
        {
            if (_menuButton == null)
            {
                return;
            }

            _menuButton.MouseLeftButtonUp -= OnMenuButtonClick;
            _menuButton.KeyDown -= OnMenuButtonKeyDown;
            _menuButton = null;
        }

        private void OnMenuButtonClick(object sender, MouseButtonEventArgs e)
        {
            ToggleMenu();
            e.Handled = true;
        }

        private void OnMenuButtonKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter || e.Key == Key.Space)
            {
                ToggleMenu();
                e.Handled = true;
            }
            else if (e.Key == Key.Escape && _isExpanded)
            {
                CloseMenu();
                e.Handled = true;
            }
        }

        private void ToggleMenu()
        {
            if (_isExpanded)
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
