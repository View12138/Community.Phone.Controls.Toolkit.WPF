using System;
using System.Globalization;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PhoneToolkitSample.Pages
{
    public partial class MenusPage : PhoneApplicationPage
    {
        private readonly ApplicationBar _applicationBar;

        public MenusPage()
        {
            InitializeComponent();

            _applicationBar = new ApplicationBar { IsVisible = true };
            _applicationBar.MenuItems.Add(new ApplicationBarMenuItem { Text = "about" });
            ApplicationBar = _applicationBar;

            Loaded += OnLoaded;
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            UpdateShellStatus();
        }

        private void OnContextMenuCopy(object sender, RoutedEventArgs e)
        {
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
                if (args.Result == CustomMessageBoxResult.RightButton)
                {
                    MessageBox.Show("The right button was selected.", "CustomMessageBox");
                }
            };

            messageBox.Show();
        }

        private void OnToggleApplicationBar(object sender, RoutedEventArgs e)
        {
            _applicationBar.IsVisible = !_applicationBar.IsVisible;
            UpdateShellStatus();
        }

        private void OnToggleSystemTray(object sender, RoutedEventArgs e)
        {
            SystemTray.SetIsVisible(this, !SystemTray.GetIsVisible(this));
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
                "ApplicationBar.IsVisible = {0}, buttons = {1}, menu items = {2}{3}SystemTray.IsVisible = {4}, opacity = {5:F2}",
                _applicationBar.IsVisible,
                _applicationBar.Buttons.Count,
                _applicationBar.MenuItems.Count,
                Environment.NewLine,
                SystemTray.GetIsVisible(this),
                SystemTray.Opacity);
        }

        private void OnGoBack(object sender, RoutedEventArgs e)
        {
            if (Frame != null)
            {
                Frame.GoBack();
            }
        }
    }
}
