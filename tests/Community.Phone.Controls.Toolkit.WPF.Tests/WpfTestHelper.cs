using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    internal static class WpfTestHelper
    {
        [ThreadStatic]
        private static ResourceDictionary _theme;

        public static ResourceDictionary Theme
        {
            get
            {
                if (_theme == null)
                {
                    _theme = new ResourceDictionary
                    {
                        Source = new Uri(
                            "pack://application:,,,/Community.Phone.Controls.Toolkit.WPF;component/Themes/Generic.xaml",
                            UriKind.Absolute)
                    };
                }

                return _theme;
            }
        }

        public static void ApplyTheme(FrameworkElement element)
        {
            if (element.Resources.MergedDictionaries.Contains(Theme))
            {
                return;
            }

            element.Resources.MergedDictionaries.Add(Theme);
        }

        public static void ApplyDefaultStyle(Control control)
        {
            Style style = Theme[control.GetType()] as Style;
            if (style != null)
            {
                control.Style = style;
            }

            control.ApplyTemplate();
        }

        public static void Layout(FrameworkElement element, double width = 480, double height = 800)
        {
            element.Measure(new Size(width, height));
            element.Arrange(new Rect(0, 0, width, height));
            element.UpdateLayout();
        }

        public static void PumpDispatcher()
        {
            DispatcherFrame frame = new DispatcherFrame();
            Dispatcher.CurrentDispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new DispatcherOperationCallback(f =>
                {
                    ((DispatcherFrame)f).Continue = false;
                    return null;
                }),
                frame);
            Dispatcher.PushFrame(frame);
        }

        /// <summary>
        /// Pumps the dispatcher until the condition holds, so tests wait for the
        /// observable result instead of a fixed duration. Returns <c>false</c>
        /// when the timeout expires.
        /// </summary>
        public static bool PumpUntil(Func<bool> condition, int timeoutMilliseconds = 3000)
        {
            if (condition == null)
            {
                throw new ArgumentNullException("condition");
            }

            DateTime end = DateTime.UtcNow.AddMilliseconds(timeoutMilliseconds);
            while (DateTime.UtcNow < end)
            {
                if (condition())
                {
                    return true;
                }

                PumpDispatcher();
                System.Threading.Thread.Sleep(10);
            }

            return condition();
        }

        /// <summary>
        /// Pumps the dispatcher for a fixed duration.
        /// </summary>
        public static void PumpFor(int milliseconds)
        {
            DateTime end = DateTime.UtcNow.AddMilliseconds(milliseconds);
            while (DateTime.UtcNow < end)
            {
                PumpDispatcher();
                System.Threading.Thread.Sleep(10);
            }
        }
    }
}
