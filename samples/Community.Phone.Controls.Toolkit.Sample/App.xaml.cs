using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PhoneToolkitSample.Resources;
using PhoneToolkitSample8;
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Threading;

namespace PhoneToolkitSample
{
    public partial class App : Application
    {

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public static PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Get or set UICulture/language for the sample app
        /// </summary>
        public static CultureInfo UICultureOverride { get; set; }

        /// <summary>
        /// Get or set regional format for the sample app
        /// </summary>
        public static CultureInfo RegionalCultureOverride { get; set; }

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

            // Language display initialization
            InitializeLanguage();
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
            if (e.ExceptionObject is Exception exception)
            {
                WriteError(exception);
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
                this.InitializeTheme();
                MainWindow window = new MainWindow();
                this.RootVisual = window.PhoneFrame;
                RootFrame = window.PhoneFrame;
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


        private void InitializeLanguage()
        {
            try
            {

                // Set the font to match the display language defined by the
                // ResourceLanguage resource string for each supported language.
                //
                // Fall back to the font of the neutral language if the Display
                // language of the phone is not supported.
                //
                // If a compiler error is hit then ResourceLanguage is missing from
                // the resource file.
                //RootFrame.Language = XmlLanguage.GetLanguage(AppResources.ResourceLanguage);

                App.UICultureOverride = new CultureInfo(AppResources.ResourceLanguage);
                App.RegionalCultureOverride = new CultureInfo(AppResources.ResourceLanguage);

                // Set the FlowDirection of all elements under the root frame based
                // on the ResourceFlowDirection resource string for each
                // supported language.
                //
                // If a compiler error is hit then ResourceFlowDirection is missing from
                // the resource file.
                FlowDirection flow = (FlowDirection)Enum.Parse(typeof(FlowDirection), AppResources.ResourceFlowDirection);
                //RootFrame.FlowDirection = flow;
            }
            catch
            {
                // If an exception is caught here it is most likely due to either
                // ResourceLangauge not being correctly set to a supported language
                // code or ResourceFlowDirection is set to a value other than LeftToRight
                // or RightToLeft.

                if (Debugger.IsAttached)
                {
                    Debugger.Break();
                }

                throw;
            }
        }
    }
}
