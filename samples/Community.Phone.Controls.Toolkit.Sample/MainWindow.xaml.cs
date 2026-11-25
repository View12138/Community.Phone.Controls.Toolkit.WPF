using PhoneToolkitSample.Placement;
using PhoneToolkitSample8;
using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;

namespace PhoneToolkitSample
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        protected override void OnSourceInitialized(EventArgs e)
        {
            base.OnSourceInitialized(e);

            try
            {
                // Load window placement details for previous application session from application settings
                // Note - if window was closed on a monitor that is now disconnected from the computer,
                //        SetWindowPlacement will place the window onto a visible monitor.
                var wp = Properties.Settings.Default.WindowPlacement;
                if (wp != new WindowPlacement())
                {
                    wp.length = Marshal.SizeOf(typeof(WindowPlacement));
                    wp.flags = 0;
                    wp.showCmd = (wp.showCmd == SwShowminimized ? SwShownormal : wp.showCmd);
                    var hwnd = new WindowInteropHelper(this).Handle;
                    SetWindowPlacement(hwnd, ref wp);
                }
            }
            catch
            {
                // ignored
            }
        }

        // WARNING - Not fired when Application.SessionEnding is fired
        protected override void OnClosing(CancelEventArgs e)
        {
            base.OnClosing(e);

            // Persist window placement details to application settings
            WindowPlacement wp;
            var hwnd = new WindowInteropHelper(this).Handle;
            GetWindowPlacement(hwnd, out wp);
            Properties.Settings.Default.WindowPlacement = wp;
            Properties.Settings.Default.Save();
        }
        #region Win32 API declarations to set and get window placement

        [DllImport("user32.dll")]
        private static extern bool SetWindowPlacement(IntPtr hWnd, [In] ref WindowPlacement lpwndpl);

        [DllImport("user32.dll")]
        private static extern bool GetWindowPlacement(IntPtr hWnd, out WindowPlacement lpwndpl);

        private const int SwShownormal = 1;
        private const int SwShowminimized = 2;

        #endregion

        public Microsoft.Phone.Controls.TransitionFrame PhoneFrame
        {
            get { return (Microsoft.Phone.Controls.TransitionFrame)FindName("Frame"); }
        }

        private void Back_Button_Click(object sender, RoutedEventArgs e)
        {
            PhoneFrame.GoBack();
        }

        private void Home_Button_Click(object sender, RoutedEventArgs e)
        {
            if (PhoneFrame.Content.GetType() != typeof(MainPage))
            {
                PhoneFrame.Navigate(new MainPage());
            }
        }

        private void Search_Button_Click(object sender, RoutedEventArgs e)
        {

        }

        private void Window_KeyUp(object sender, System.Windows.Input.KeyEventArgs e)
        {
            if (e.Key == System.Windows.Input.Key.F1)
            {
                PhoneFrame.GoBack();
            }
            if (e.Key == System.Windows.Input.Key.F2)
            {
                if (PhoneFrame.Content.GetType() != typeof(MainPage))
                {
                    PhoneFrame.Navigate(new MainPage());
                }
            }
            if (e.Key == System.Windows.Input.Key.F1)
            {

            }
        }
    }
}
