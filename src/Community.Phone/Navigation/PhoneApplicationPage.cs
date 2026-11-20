// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A WPF page that exposes the Windows Phone page lifecycle surface.
    /// </summary>
    public class PhoneApplicationPage : Page
    {
        /// <summary>
        /// Identifies the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(PageOrientation),
                typeof(PhoneApplicationPage),
                new PropertyMetadata(PageOrientation.PortraitUp, OnOrientationPropertyChanged));

        /// <summary>
        /// Identifies the <see cref="SupportedOrientations"/> property.
        /// </summary>
        public static readonly DependencyProperty SupportedOrientationsProperty =
            DependencyProperty.Register(
                "SupportedOrientations",
                typeof(SupportedPageOrientation),
                typeof(PhoneApplicationPage),
                new PropertyMetadata(SupportedPageOrientation.PortraitOrLandscape));

        /// <summary>
        /// Identifies the <see cref="ApplicationBar"/> property.
        /// </summary>
        public static readonly DependencyProperty ApplicationBarProperty =
            DependencyProperty.Register(
                "ApplicationBar",
                typeof(IApplicationBar),
                typeof(PhoneApplicationPage),
                new PropertyMetadata(null, OnApplicationBarPropertyChanged));

        private PhoneApplicationFrame _frame;
        private readonly IDictionary<string, object> _localState = new Dictionary<string, object>();

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneApplicationPage"/> class.
        /// </summary>
        public PhoneApplicationPage()
        {
            // WPF recreates a page from its URI on back navigation unless the
            // page is kept alive. The port keeps the instance so that a back
            // navigation returns to the page that was left instead of
            // constructing a new one. Applications that need a fresh page on
            // back navigation can set KeepAlive to false before navigating.
            KeepAlive = true;

            NavigationContext = NavigationContext.Empty;
        }

        /// <summary>
        /// Occurs after the page has been navigated to.
        /// </summary>
        public event EventHandler<NavigationEventArgs> NavigatedTo;

        /// <summary>
        /// Occurs when the page is being navigated away from.
        /// </summary>
        public event EventHandler<NavigatingCancelEventArgs> NavigatingFrom;

        /// <summary>
        /// Occurs after the page has been navigated away from.
        /// </summary>
        public event EventHandler<NavigationEventArgs> NavigatedFrom;

        /// <summary>
        /// Occurs when the user presses the back key.
        /// </summary>
        public event EventHandler<CancelEventArgs> BackKeyPress;

        /// <summary>
        /// Occurs when the page orientation changes.
        /// </summary>
        public event EventHandler<OrientationChangedEventArgs> OrientationChanged;

        /// <summary>
        /// Gets or sets the current orientation.
        /// </summary>
        public PageOrientation Orientation
        {
            get { return (PageOrientation)GetValue(OrientationProperty); }
            set { SetValue(OrientationProperty, value); }
        }

        /// <summary>
        /// Gets or sets the orientations supported by the page.
        /// </summary>
        public SupportedPageOrientation SupportedOrientations
        {
            get { return (SupportedPageOrientation)GetValue(SupportedOrientationsProperty); }
            set { SetValue(SupportedOrientationsProperty, value); }
        }

        /// <summary>
        /// Gets or sets the application bar associated with the page. Like the
        /// original, only the toolkit's <see cref="ApplicationBar"/> can be
        /// assigned.
        /// </summary>
        public IApplicationBar ApplicationBar
        {
            get
            {
                return (IApplicationBar)GetValue(ApplicationBarProperty);
            }

            set
            {
                if (value != null && !(value is ApplicationBar))
                {
                    throw new InvalidOperationException("PhoneApplicationPage only accepts the ApplicationBar implementation of IApplicationBar");
                }

                SetValue(ApplicationBarProperty, value);
            }
        }

        /// <summary>
        /// Gets the page state dictionary. In this WPF port the dictionary is
        /// retained for the lifetime of the frame/page pair.
        /// </summary>
        public IDictionary<string, object> State
        {
            get { return _frame == null ? _localState : _frame.GetState(this); }
        }

        /// <summary>
        /// Gets the navigation context (URI, query string and extra data) for
        /// the current navigation.
        /// </summary>
        public NavigationContext NavigationContext { get; internal set; }

        /// <summary>
        /// Gets the hosting phone frame, if any.
        /// </summary>
        public PhoneApplicationFrame Frame
        {
            get { return _frame; }
        }

        internal void ApplyFrame(PhoneApplicationFrame frame)
        {
            if (ReferenceEquals(_frame, frame))
            {
                return;
            }

            _frame = frame;
            if (frame != null && _localState.Count > 0)
            {
                IDictionary<string, object> state = frame.GetState(this);
                foreach (KeyValuePair<string, object> pair in _localState)
                {
                    state[pair.Key] = pair.Value;
                }

                _localState.Clear();
            }
        }

        /// <inheritdoc />
        protected virtual void OnNavigatedTo(NavigationEventArgs e)
        {
            EventHandler<NavigationEventArgs> handler = NavigatedTo;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <inheritdoc />
        protected virtual void OnNavigatingFrom(NavigatingCancelEventArgs e)
        {
            EventHandler<NavigatingCancelEventArgs> handler = NavigatingFrom;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <inheritdoc />
        protected virtual void OnNavigatedFrom(NavigationEventArgs e)
        {
            EventHandler<NavigationEventArgs> handler = NavigatedFrom;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnBackKeyPress(CancelEventArgs e)
        {
            EventHandler<CancelEventArgs> handler = BackKeyPress;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        protected virtual void OnOrientationChanged(OrientationChangedEventArgs e)
        {
            EventHandler<OrientationChangedEventArgs> handler = OrientationChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        internal bool RaiseBackKeyPress()
        {
            CancelEventArgs args = new CancelEventArgs();
            OnBackKeyPress(args);
            return !args.Cancel;
        }
        internal void RaiseNavigatedTo(NavigationEventArgs e)
        {
            OnNavigatedTo(e);
        }

        internal void RaiseNavigatingFrom(NavigatingCancelEventArgs e)
        {
            OnNavigatingFrom(e);
        }

        internal void RaiseNavigatedFrom(NavigationEventArgs e)
        {
            OnNavigatedFrom(e);
        }

        private static void OnApplicationBarPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PhoneApplicationPage page = (PhoneApplicationPage)d;
            if (!page.IsLoaded) { return; }
            page._frame?.UpdateApplicationBarChrome();
        }

        private static void OnOrientationPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PhoneApplicationPage page = (PhoneApplicationPage)d;
            if (!page.IsLoaded) { return; }
            page.OnOrientationChanged(new OrientationChangedEventArgs((PageOrientation)e.NewValue));
        }
    }
}
