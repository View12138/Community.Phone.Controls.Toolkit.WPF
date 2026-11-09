using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PackageConsumer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
                PhoneFrame.ApplyTemplate();
                if (!(PhoneFrame.Template.FindName("ApplicationBar", PhoneFrame) is ApplicationBarPresenter))
                {
                    throw new InvalidOperationException("Community.Phone did not apply the phone frame template.");
                }

                string resultPath = Environment.GetEnvironmentVariable("PACKAGE_CONSUMER_RESULT");
                if (!string.IsNullOrEmpty(resultPath))
                {
                    File.WriteAllText(resultPath, "ok");
                }

                DispatcherTimer timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(1.5) };
                timer.Tick += (_, _) =>
                {
                    timer.Stop();
                    Application.Current.Shutdown(0);
                };
                timer.Start();
            };
        }
    }
}
