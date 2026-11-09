using Microsoft.Phone.Controls;
using PhoneToolkitSample8;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace PhoneToolkitSample
{
    public partial class App : Application
    {
        private readonly CustomMessageBox MessageBox;
        public App()
        {
            DispatcherUnhandledException += OnDispatcherUnhandledException;
            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
            TaskScheduler.UnobservedTaskException += TaskScheduler_UnobservedTaskException;
            MessageBox = new CustomMessageBox()
            {
                Caption = "Do you like this sample?",
                Message = "There are tons of things you can do using custom message boxes. To learn more, be sure to check out the source code at Codeplex.",
                LeftButtonContent = null,
                RightButtonContent = "close",
                IsFullScreen = false,
                IsCenterScreen = false
            };
        }


        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            try
            {
                e.Handled = true;
                WriteError(e.Exception);
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
        }

        private void TaskScheduler_UnobservedTaskException(object sender, UnobservedTaskExceptionEventArgs e)
        {
            WriteError(e.Exception);
            e.SetObserved();
        }

        private void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            if (e.ExceptionObject is Exception)
            {
                WriteError((Exception)e.ExceptionObject);
            }
            else
            {
                WriteError(new Exception(e.ExceptionObject.ToString()));
            }
        }

        private void OnStartup(object sender, StartupEventArgs e)
        {
            try
            {
                MainWindow window = new MainWindow();
                window.Loaded += Window_Loaded;
                window.Show();
            }
            catch (Exception ex)
            {
                WriteError(ex);
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            if (sender is MainWindow window)
            {
                window.Loaded -= Window_Loaded;
                window.PhoneFrame.Navigate(new MainPage());
            }
        }

        private void WriteError(Exception ex)
        {
            MessageBox.Caption = ex.Message;
            MessageBox.Message = ex.ToString();
            MessageBox.Show();
        }
    }
}
