using System;
using System.Runtime.ExceptionServices;
using System.Threading;
using Xunit;

// WPF tests create windows and share process-wide WPF state (theme resources,
// TiltEffect statics, SystemTray), so they run one at a time.
[assembly: CollectionBehavior(DisableTestParallelization = true)]

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    internal static class StaTest
    {
        public static void Run(Action action)
        {
            Exception exception = null;
            Thread thread = new Thread(() =>
            {
                try
                {
                    action();
                }
                catch (Exception ex)
                {
                    exception = ex;
                }
            });

            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();

            if (exception != null)
            {
                ExceptionDispatchInfo.Capture(exception).Throw();
            }
        }
    }
}
