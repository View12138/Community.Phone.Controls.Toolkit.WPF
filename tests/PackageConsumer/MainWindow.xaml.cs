using System;
using System.IO;
using System.Windows;
using System.Windows.Threading;

namespace PackageConsumer
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();

            Loaded += (_, _) =>
            {
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
