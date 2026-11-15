// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Shell;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// A WPF navigation host that mirrors the Windows Phone
    /// <c>PhoneApplicationFrame</c> surface.
    /// </summary>
    public class PhoneApplicationFrame : Frame
    {
        /// <summary>
        /// The name of the template part that reserves room for the
        /// application bar.
        /// </summary>
        private const string ApplicationBarHostPartName = "ApplicationBarHost";

        /// <summary>
        /// The name of the template part that draws the application bar.
        /// </summary>
        private const string ApplicationBarPartName = "ApplicationBar";

        /// <summary>
        /// Identifies the <see cref="Orientation"/> property.
        /// </summary>
        public static readonly DependencyProperty OrientationProperty =
            DependencyProperty.Register(
                "Orientation",
                typeof(PageOrientation),
                typeof(PhoneApplicationFrame),
                new PropertyMetadata(PageOrientation.PortraitUp, OnOrientationChanged));

        /// <summary>
        /// Identifies the <see cref="SupportedOrientations"/> property.
        /// </summary>
        public static readonly DependencyProperty SupportedOrientationsProperty =
            DependencyProperty.Register(
                "SupportedOrientations",
                typeof(SupportedPageOrientation),
                typeof(PhoneApplicationFrame),
                new PropertyMetadata(SupportedPageOrientation.PortraitOrLandscape));

        private readonly Dictionary<Page, IDictionary<string, object>> _pageStates =
            new Dictionary<Page, IDictionary<string, object>>();

        private PhoneApplicationPage _currentPage;
        private object _pendingNavigationExtraData;

        /// <summary>
        /// Initializes a new instance of the <see cref="PhoneApplicationFrame"/> class.
        /// </summary>
        public PhoneApplicationFrame()
        {
            SizeChanged += OnFrameSizeChanged;
            Navigating += OnFrameNavigating;
            Navigated += OnFrameNavigated;
            NavigationStopped += OnFrameNavigationStopped;
        }

        static PhoneApplicationFrame()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(PhoneApplicationFrame),
                new FrameworkPropertyMetadata(typeof(PhoneApplicationFrame)));
        }

        /// <summary>
        /// Occurs when the frame orientation changes.
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
        /// Gets or sets the orientations supported by the frame.
        /// </summary>
        public SupportedPageOrientation SupportedOrientations
        {
            get { return (SupportedPageOrientation)GetValue(SupportedOrientationsProperty); }
            set { SetValue(SupportedOrientationsProperty, value); }
        }

        /// <summary>
        /// Gets the application bar for the current page.
        /// </summary>
        public IApplicationBar ApplicationBar
        {
            get
            {
                PhoneApplicationPage page = Content as PhoneApplicationPage;
                return page == null ? null : page.ApplicationBar;
            }
        }

        /// <summary>
        /// Gets or sets the element that reserves room for the application bar.
        /// </summary>
        private FrameworkElement _applicationBarHost;

        /// <summary>
        /// Gets or sets the presenter that draws the application bar.
        /// </summary>
        private ApplicationBarPresenter _applicationBarPresenter;

        /// <summary>
        /// Gets a value indicating whether the current orientation is portrait.
        /// </summary>
        public bool IsPortrait
        {
            get { return (Orientation & PageOrientation.Portrait) == Orientation; }
        }

        /// <summary>
        /// Navigates to a page and supplies query-string style parameters.
        /// </summary>
        /// <param name="page">The page to navigate to.</param>
        /// <param name="parameters">The navigation parameters.</param>
        /// <returns><c>true</c> if the navigation was initiated.</returns>
        public bool Navigate(PhoneApplicationPage page, IDictionary<string, string> parameters)
        {
            if (page == null)
            {
                throw new ArgumentNullException("page");
            }

            _pendingNavigationExtraData = parameters;
            bool navigated = Navigate((object)page);
            if (!navigated)
            {
                _pendingNavigationExtraData = null;
            }

            return navigated;
        }

        /// <summary>
        /// Returns to the previous page in the journal.
        /// </summary>
        public new void GoBack()
        {
            PhoneApplicationPage page = Content as PhoneApplicationPage;
            if (page != null && !page.RaiseBackKeyPress())
            {
                return;
            }

            if (CanGoBack)
            {
                base.GoBack();
            }
        }

        /// <summary>
        /// Applies the template and hooks up the application bar chrome.
        /// </summary>
        public override void OnApplyTemplate()
        {
            if (_applicationBarPresenter != null)
            {
                _applicationBarPresenter.ReservedHeightChanged -= OnApplicationBarReservedHeightChanged;
                _applicationBarPresenter.ApplicationBar = null;
            }

            base.OnApplyTemplate();

            _applicationBarHost = GetTemplateChild(ApplicationBarHostPartName) as FrameworkElement;
            _applicationBarPresenter = GetTemplateChild(ApplicationBarPartName) as ApplicationBarPresenter;

            if (_applicationBarPresenter != null)
            {
                _applicationBarPresenter.ReservedHeightChanged += OnApplicationBarReservedHeightChanged;
            }

            UpdateApplicationBarChrome();
        }

        /// <summary>
        /// Pushes the current page's application bar into the chrome.
        /// </summary>
        internal void UpdateApplicationBarChrome()
        {
            if (_applicationBarPresenter != null)
            {
                _applicationBarPresenter.ApplicationBar = ApplicationBar;
            }

            ApplyApplicationBarReservedHeight();
        }

        private void ApplyApplicationBarReservedHeight()
        {
            if (_applicationBarHost == null)
            {
                return;
            }

            double height = _applicationBarPresenter == null ? 0 : _applicationBarPresenter.ReservedHeight;
            _applicationBarHost.Height = height;
        }

        private void OnApplicationBarReservedHeightChanged(object sender, EventArgs e)
        {
            ApplyApplicationBarReservedHeight();
        }

        /// <summary>
        /// Gets the persisted state dictionary for a page.
        /// </summary>
        internal IDictionary<string, object> GetState(Page page)
        {
            IDictionary<string, object> state;
            if (!_pageStates.TryGetValue(page, out state))
            {
                state = new Dictionary<string, object>();
                _pageStates[page] = state;
            }

            return state;
        }

        private void OnFrameSizeChanged(object sender, SizeChangedEventArgs e)
        {
            //PageOrientation orientation = e.NewSize.Width > e.NewSize.Height
            //    ? PageOrientation.LandscapeLeft
            //    : PageOrientation.PortraitUp;

            //if (orientation != Orientation)
            //{
            //    Orientation = orientation;
            //}
        }

        private void OnFrameNavigated(object sender, NavigationEventArgs e)
        {
            PhoneApplicationPage page = e.Content as PhoneApplicationPage;
            if (page == null)
            {
                if (_currentPage != null)
                {
                    _currentPage.RaiseNavigatedFrom(e);
                    _currentPage = null;
                }

                _pendingNavigationExtraData = null;
                UpdateApplicationBarChrome();
                return;
            }

            if (_currentPage != null && !ReferenceEquals(_currentPage, page))
            {
                _currentPage.RaiseNavigatedFrom(e);
            }

            _currentPage = page;
            page.ApplyFrame(this);
            page.Orientation = Orientation;

            object extraData = _pendingNavigationExtraData ?? e.ExtraData;
            page.NavigationContext = NavigationContext.FromNavigationEventArgs(e.Uri, extraData);
            _pendingNavigationExtraData = null;

            UpdateApplicationBarChrome();

            page.RaiseNavigatedTo(e);
        }

        private void OnFrameNavigating(object sender, NavigatingCancelEventArgs e)
        {
            if (_currentPage == null)
            {
                return;
            }

            _currentPage.RaiseNavigatingFrom(e);
        }

        private void OnFrameNavigationStopped(object sender, NavigationEventArgs e)
        {
            _pendingNavigationExtraData = null;
        }

        private static void OnOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {            PhoneApplicationFrame frame = (PhoneApplicationFrame)d;
            PageOrientation orientation = (PageOrientation)e.NewValue;

            PhoneApplicationPage page = frame.Content as PhoneApplicationPage;
            if (page != null)
            {
                page.Orientation = orientation;
            }

            EventHandler<OrientationChangedEventArgs> handler = frame.OrientationChanged;
            if (handler != null)
            {
                handler(frame, new OrientationChangedEventArgs(orientation));
            }
        }
    }
}
