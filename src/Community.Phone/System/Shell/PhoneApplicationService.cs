// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Threading;

namespace Microsoft.Phone.Shell
{
    /// <summary>
    /// Provides application lifetime notifications and state storage compatible
    /// with the Windows Phone <c>PhoneApplicationService</c> API.
    /// </summary>
    public sealed class PhoneApplicationService
    {
        private static readonly PhoneApplicationService Instance = new PhoneApplicationService();
        private static bool _initialized;
        private static bool _launchingRaised;

        private PhoneApplicationService()
        {
            State = new Dictionary<string, object>();
        }

        /// <summary>
        /// Gets the current application service.
        /// </summary>
        public static PhoneApplicationService Current
        {
            get
            {
                EnsureInitialized();
                return Instance;
            }
        }

        /// <summary>
        /// Gets application state that survives page navigation and application
        /// deactivation in the same process.
        /// </summary>
        public IDictionary<string, object> State { get; private set; }

        /// <summary>
        /// Occurs when the application is launched.
        /// </summary>
        public event EventHandler<LaunchingEventArgs> Launching;

        /// <summary>
        /// Occurs when the application is activated.
        /// </summary>
        public event EventHandler<ActivatedEventArgs> Activated;

        /// <summary>
        /// Occurs when the application is deactivated.
        /// </summary>
        public event EventHandler<DeactivatedEventArgs> Deactivated;

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        public event EventHandler<ClosingEventArgs> Closing;

        /// <summary>
        /// Gets or sets the user idle detection mode. WPF has no equivalent
        /// system policy; the value is retained for API compatibility.
        /// </summary>
        public UserIdleDetectionMode UserIdleDetectionMode { get; set; }

        /// <summary>
        /// Gets or sets the application idle detection mode. WPF has no
        /// equivalent system policy; the value is retained for API compatibility.
        /// </summary>
        public ApplicationIdleDetectionMode ApplicationIdleDetectionMode { get; set; }

        private static void EnsureInitialized()
        {
            if (_initialized)
            {
                return;
            }

            _initialized = true;

            Application application = Application.Current;
            if (application == null)
            {
                return;
            }

            application.Startup += OnApplicationStartup;
            application.Exit += OnApplicationExit;

            Action raiseLaunching = () =>
            {
                if (_launchingRaised)
                {
                    return;
                }

                _launchingRaised = true;
                EventHandler<LaunchingEventArgs> handler = Instance.Launching;
                if (handler != null)
                {
                    handler(Instance, new LaunchingEventArgs());
                }
            };

            application.Dispatcher.BeginInvoke(DispatcherPriority.Loaded, raiseLaunching);
        }

        private static void OnApplicationStartup(object sender, StartupEventArgs e)
        {
            if (_launchingRaised)
            {
                return;
            }

            _launchingRaised = true;
            EventHandler<LaunchingEventArgs> handler = Instance.Launching;
            if (handler != null)
            {
                handler(Instance, new LaunchingEventArgs());
            }

            Window window = Application.Current == null ? null : Application.Current.MainWindow;
            if (window != null)
            {
                window.Activated += OnWindowActivated;
                window.Deactivated += OnWindowDeactivated;
            }
        }

        private static void OnWindowActivated(object sender, EventArgs e)
        {
            EventHandler<ActivatedEventArgs> handler = Instance.Activated;
            if (handler != null)
            {
                handler(Instance, new ActivatedEventArgs());
            }
        }

        private static void OnWindowDeactivated(object sender, EventArgs e)
        {
            EventHandler<DeactivatedEventArgs> handler = Instance.Deactivated;
            if (handler != null)
            {
                handler(Instance, new DeactivatedEventArgs());
            }
        }

        private static void OnApplicationExit(object sender, ExitEventArgs e)
        {
            EventHandler<ClosingEventArgs> handler = Instance.Closing;
            if (handler != null)
            {
                handler(Instance, new ClosingEventArgs());
            }
        }
    }

    /// <summary>
    /// Event arguments for <see cref="PhoneApplicationService.Launching"/>.
    /// </summary>
    public class LaunchingEventArgs : EventArgs
    {
        /// <summary>
        /// Gets or sets a value indicating whether the application instance was
        /// preserved.
        /// </summary>
        public bool IsApplicationInstancePreserved { get; set; }
    }

    /// <summary>
    /// Event arguments for <see cref="PhoneApplicationService.Activated"/>.
    /// </summary>
    public class ActivatedEventArgs : EventArgs
    {
    }

    /// <summary>
    /// Event arguments for <see cref="PhoneApplicationService.Deactivated"/>.
    /// </summary>
    public class DeactivatedEventArgs : EventArgs
    {
    }

    /// <summary>
    /// Event arguments for <see cref="PhoneApplicationService.Closing"/>.
    /// </summary>
    public class ClosingEventArgs : EventArgs
    {
    }

    /// <summary>
    /// Specifies whether user idle detection is enabled.
    /// </summary>
    public enum UserIdleDetectionMode
    {
        /// <summary>
        /// User idle detection is enabled.
        /// </summary>
        Enabled,

        /// <summary>
        /// User idle detection is disabled.
        /// </summary>
        Disabled
    }

    /// <summary>
    /// Specifies whether application idle detection is enabled.
    /// </summary>
    public enum ApplicationIdleDetectionMode
    {
        /// <summary>
        /// Application idle detection is enabled.
        /// </summary>
        Enabled,

        /// <summary>
        /// Application idle detection is disabled.
        /// </summary>
        Disabled
    }
}
