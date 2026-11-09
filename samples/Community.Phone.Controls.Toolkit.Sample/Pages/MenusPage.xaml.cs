using System;
using System.Globalization;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PhoneToolkitSample.Pages
{
    public partial class MenusPage : PhoneApplicationPage
    {
        private string _lastAction = "none";
        private bool _isMenuOpen;

        public MenusPage()
        {
            InitializeComponent();

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateShellStatus();
        }

        private void OnBarStateChanged(object sender, ApplicationBarStateChangedEventArgs e)
        {
            _isMenuOpen = e.IsMenuVisible;
            UpdateShellStatus();
        }

        private void OnDoneClick(object sender, EventArgs e)
        {
            _lastAction = "done";
            UpdateShellStatus();
        }

        private void OnAddClick(object sender, EventArgs e)
        {
            _lastAction = "add";
            UpdateShellStatus();
        }

        private void OnDeleteClick(object sender, EventArgs e)
        {
            _lastAction = "delete";
            UpdateShellStatus();
        }

        private void OnAboutClick(object sender, EventArgs e)
        {
            _lastAction = "about";
            UpdateShellStatus();
            MessageBox.Show("The application bar menu item was clicked.", "ApplicationBar");
        }

        private void OnMinimizeClick(object sender, EventArgs e)
        {
            ToggleApplicationBarMode();
        }

        private void OnHideBarClick(object sender, EventArgs e)
        {
            _lastAction = "hide app bar";
            PageBar.IsVisible = false;
            UpdateShellStatus();
        }

        private void OnContextMenuCopy(object sender, RoutedEventArgs e)
        {
            _lastAction = "context menu copy";
            UpdateShellStatus();
            MessageBox.Show("ContextMenu item clicked.", "ContextMenu");
        }

        private void OnShowMessage(object sender, RoutedEventArgs e)
        {
            CustomMessageBox messageBox = new CustomMessageBox
            {
                Title = "CustomMessageBox",
                Caption = "Metro dialog",
                Message = "This dialog is part of the WPF toolkit port.",
                LeftButtonContent = "cancel",
                RightButtonContent = "ok"
            };

            messageBox.Dismissed += (s, args) =>
            {
                _lastAction = "message box " + args.Result.ToString().ToLower(CultureInfo.CurrentCulture);
                UpdateShellStatus();

                if (args.Result == CustomMessageBoxResult.RightButton)
                {
                    MessageBox.Show("The right button was selected.", "CustomMessageBox");
                }
            };

            messageBox.Show();
        }

        private void OnToggleApplicationBar(object sender, RoutedEventArgs e)
        {
            PageBar.IsVisible = !PageBar.IsVisible;
            UpdateShellStatus();
        }

        private void OnToggleApplicationBarMode(object sender, RoutedEventArgs e)
        {
            ToggleApplicationBarMode();
        }

        private void OnToggleSystemTray(object sender, RoutedEventArgs e)
        {
            SystemTray.SetIsVisible(this, !SystemTray.GetIsVisible(this));
            UpdateShellStatus();
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            if (Frame != null)
            {
                Frame.GoBack();
            }
        }

        private void ToggleApplicationBarMode()
        {
            PageBar.Mode = PageBar.Mode == ApplicationBarMode.Minimized
                ? ApplicationBarMode.Default
                : ApplicationBarMode.Minimized;

            _lastAction = "mode " + PageBar.Mode.ToString().ToLower(CultureInfo.CurrentCulture);
            UpdateShellStatus();
        }

        private void UpdateShellStatus()
        {
            if (ShellStatus == null)
            {
                return;
            }

            ShellStatus.Text = string.Format(
                CultureInfo.CurrentCulture,
                "ApplicationBar: IsVisible = {0}, Mode = {1}, buttons = {2}, menu items = {3}, menu open = {4}{5}SystemTray: IsVisible = {6}, opacity = {7:F2}{5}Last action: {8}",
                PageBar.IsVisible,
                PageBar.Mode,
                PageBar.Buttons.Count,
                PageBar.MenuItems.Count,
                _isMenuOpen,
                Environment.NewLine,
                SystemTray.GetIsVisible(this),
                SystemTray.Opacity,
                _lastAction);
        }
    }
}
