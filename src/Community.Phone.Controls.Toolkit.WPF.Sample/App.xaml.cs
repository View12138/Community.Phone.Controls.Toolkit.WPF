using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace PhoneToolkitSample
{
    public partial class App : Application
    {
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                MainWindow window = new MainWindow();
                window.Show();
                window.PhoneFrame.Navigate(new Pages.HomePage());
            }
            catch (Exception ex)
            {
                WriteError(ex);
                Shutdown(1);
            }
        }

        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            WriteError(e.Exception);
            e.Handled = true;
            Shutdown(1);
        }

        private static void WriteError(Exception ex)
        {
            File.WriteAllText(
                Path.Combine(Path.GetTempPath(), "community-phone-toolkit-sample-error.txt"),
                ex.ToString());
        }
    }
}
