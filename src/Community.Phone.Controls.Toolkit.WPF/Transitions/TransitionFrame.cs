// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Navigation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Enables navigation transitions for <see cref="PhoneApplicationPage"/>s.
    /// </summary>
    [TemplatePart(Name = FirstTemplatePartName, Type = typeof(ProjectionHost))]
    [TemplatePart(Name = SecondTemplatePartName, Type = typeof(ProjectionHost))]
    public class TransitionFrame : PhoneApplicationFrame
    {
        private const string FirstTemplatePartName = "FirstContentPresenter";
        private const string SecondTemplatePartName = "SecondContentPresenter";

        internal static readonly CacheMode BitmapCacheMode = CreateBitmapCache();

        private ProjectionHost _firstContentPresenter;
        private ProjectionHost _secondContentPresenter;
        private ProjectionHost _newContentPresenter;
        private ProjectionHost _oldContentPresenter;

        private bool _isForwardNavigation;
        private bool _useFirstAsNew;
        private bool _readyToTransitionToNewContent;
        private bool _contentReady;
        private bool _performingExitTransition;
        private bool _navigationStopped;
        private object _lastPresentedContent;
        private ITransition _storedNewTransition;
        private NavigationInTransition _storedNavigationInTransition;
        private ITransition _storedOldTransition;
        private NavigationOutTransition _storedNavigationOutTransition;

        /// <summary>
        /// Initializes a new instance of the <see cref="TransitionFrame"/> class.
        /// </summary>
        public TransitionFrame()
        {
            Navigating += OnNavigating;
            NavigationStopped += OnNavigationStopped;
        }

        static TransitionFrame()
        {
            DefaultStyleKeyProperty.OverrideMetadata(
                typeof(TransitionFrame),
                new FrameworkPropertyMetadata(typeof(TransitionFrame)));
        }

        /// <summary>
        /// Occurs when a navigation transition could not be created or
        /// started. The frame falls back to an untransitioned navigation.
        /// </summary>
        public event EventHandler<TransitionFailedEventArgs> TransitionFailed;

        private static CacheMode CreateBitmapCache()
        {
            BitmapCache cache = new BitmapCache();
            cache.Freeze();
            return cache;
        }

        private void FlipPresenters()
        {
            _newContentPresenter = _useFirstAsNew ? _firstContentPresenter : _secondContentPresenter;
            _oldContentPresenter = _useFirstAsNew ? _secondContentPresenter : _firstContentPresenter;
            _useFirstAsNew = !_useFirstAsNew;
        }

        private void OnNavigating(object sender, NavigatingCancelEventArgs e)
        {
            _isForwardNavigation = e.NavigationMode != NavigationMode.Back;

            UIElement oldElement = Content as UIElement;
            if (oldElement == null)
            {
                return;
            }

            EnsureLastTransitionIsComplete();
            FlipPresenters();

            TransitionElement oldTransitionElement = null;
            NavigationOutTransition navigationOutTransition = null;
            ITransition oldTransition = null;

            navigationOutTransition = TransitionService.GetNavigationOutTransition(oldElement);
            if (navigationOutTransition != null)
            {
                oldTransitionElement = _isForwardNavigation
                    ? navigationOutTransition.Forward
                    : navigationOutTransition.Backward;
            }

            if (oldTransitionElement != null)
            {
                try
                {
                    oldTransition = oldTransitionElement.GetTransition(oldElement);
                }
                catch (Exception exception)
                {
                    RaiseTransitionFailed(oldElement, exception, true);
                    navigationOutTransition = null;
                }
            }

            if (oldTransition != null)
            {
                EnsureStoppedTransition(oldTransition);

                _storedNavigationOutTransition = navigationOutTransition;
                _storedOldTransition = oldTransition;
                oldTransition.Completed += OnExitTransitionCompleted;

                _performingExitTransition = true;

                try
                {
                    PerformTransition(navigationOutTransition, _oldContentPresenter, oldTransition);
                    PrepareContentPresenterForCompositor(_oldContentPresenter);
                }
                catch (Exception exception)
                {
                    RaiseTransitionFailed(oldElement, exception, true);
                    _performingExitTransition = false;
                    _readyToTransitionToNewContent = true;
                    oldTransition.Completed -= OnExitTransitionCompleted;
                    CompleteTransition(navigationOutTransition, _oldContentPresenter, oldTransition);
                    _storedNavigationOutTransition = null;
                    _storedOldTransition = null;
                }
            }
            else
            {
                _readyToTransitionToNewContent = true;
            }
        }

        private void OnNavigationStopped(object sender, NavigationEventArgs e)
        {
            _navigationStopped = true;
        }

        private void EnsureLastTransitionIsComplete()
        {
            _readyToTransitionToNewContent = false;
            _contentReady = false;
            _navigationStopped = false;

            if (_performingExitTransition)
            {
                Debug.Assert(_storedOldTransition != null || _storedNavigationOutTransition == null);

                if (_oldContentPresenter != null)
                {
                    _oldContentPresenter.Child = null;
                }

                if (_storedOldTransition != null)
                {
                    _storedOldTransition.Completed -= OnExitTransitionCompleted;
                    _storedOldTransition.Stop();
                }

                _storedNavigationOutTransition = null;
                _storedOldTransition = null;

                if (_storedNewTransition != null)
                {
                    _storedNewTransition.Stop();
                    _storedNewTransition = null;
                    _storedNavigationInTransition = null;
                }

                _performingExitTransition = false;
            }
        }

        private void OnExitTransitionCompleted(object sender, EventArgs e)
        {
            if (!ReferenceEquals(sender, _storedOldTransition) || !_performingExitTransition)
            {
                return;
            }

            _readyToTransitionToNewContent = true;
            _performingExitTransition = false;

            if (_navigationStopped)
            {
                CompleteTransition(_storedNavigationOutTransition, _oldContentPresenter, _storedOldTransition);
                _navigationStopped = false;
            }
            else
            {
                CompleteTransition(_storedNavigationOutTransition, _oldContentPresenter, _storedOldTransition);
            }

            if (_storedOldTransition != null)
            {
                _storedOldTransition.Completed -= OnExitTransitionCompleted;
            }

            _storedNavigationOutTransition = null;
            _storedOldTransition = null;

            if (_contentReady)
            {
                ITransition newTransition = _storedNewTransition;
                NavigationInTransition navigationInTransition = _storedNavigationInTransition;

                _storedNewTransition = null;
                _storedNavigationInTransition = null;

                TransitionNewContent(newTransition, navigationInTransition);
            }
        }

        /// <inheritdoc />
        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            _firstContentPresenter = GetTemplateChild(FirstTemplatePartName) as ProjectionHost;
            _secondContentPresenter = GetTemplateChild(SecondTemplatePartName) as ProjectionHost;
            _newContentPresenter = _secondContentPresenter;
            _oldContentPresenter = _firstContentPresenter;
            _useFirstAsNew = true;
            _readyToTransitionToNewContent = true;
            _lastPresentedContent = null;

            if (Content != null)
            {
                // Host the initial content in the presenter that is currently
                // considered new. OnNavigating flips the presenters, so the
                // initial content must already be in the presenter that will
                // become the outgoing presenter when the first navigation
                // starts.
                OnContentChanged(null, Content);
            }
        }

        /// <inheritdoc />
        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);

            _contentReady = true;

            UIElement oldElement = oldContent as UIElement;
            UIElement newElement = newContent as UIElement;

            if (_firstContentPresenter == null || _secondContentPresenter == null || newElement == null)
            {
                return;
            }

            if (ReferenceEquals(newElement, _lastPresentedContent))
            {
                return;
            }

            _lastPresentedContent = newElement;

            NavigationInTransition navigationInTransition = null;
            ITransition newTransition = null;

            if (newElement != null)
            {
                navigationInTransition = TransitionService.GetNavigationInTransition(newElement);
                TransitionElement newTransitionElement = null;
                if (navigationInTransition != null)
                {
                    newTransitionElement = _isForwardNavigation
                        ? navigationInTransition.Forward
                        : navigationInTransition.Backward;
                }

                if (newTransitionElement != null)
                {
                    try
                    {
                        newElement.UpdateLayout();
                        newTransition = newTransitionElement.GetTransition(newElement);
                        PrepareContentPresenterForCompositor(_newContentPresenter);
                    }
                    catch (Exception exception)
                    {
                        RaiseTransitionFailed(newElement, exception, false);
                        navigationInTransition = null;
                        newTransition = null;
                    }
                }
            }

            _newContentPresenter.Opacity = 0;
            _newContentPresenter.Visibility = Visibility.Visible;
            _newContentPresenter.Child = newElement;

            _oldContentPresenter.Opacity = 1;
            _oldContentPresenter.Visibility = Visibility.Visible;
            _oldContentPresenter.Child = oldElement;

            if (_readyToTransitionToNewContent)
            {
                TransitionNewContent(newTransition, navigationInTransition);
            }
            else
            {
                _storedNewTransition = newTransition;
                _storedNavigationInTransition = navigationInTransition;
            }
        }

        private void TransitionNewContent(ITransition newTransition, NavigationInTransition navigationInTransition)
        {
            if (_oldContentPresenter != null)
            {
                _oldContentPresenter.Visibility = Visibility.Collapsed;
                _oldContentPresenter.Child = null;
            }

            if (newTransition == null)
            {
                RestoreContentPresenterInteractivity(_newContentPresenter);
                return;
            }

            EnsureStoppedTransition(newTransition);
            newTransition.Completed += delegate
            {
                CompleteTransition(navigationInTransition, _newContentPresenter, newTransition);
            };

            _readyToTransitionToNewContent = false;
            _storedNavigationInTransition = null;
            _storedNewTransition = null;

            try
            {
                PerformTransition(navigationInTransition, _newContentPresenter, newTransition);
            }
            catch (Exception exception)
            {
                RaiseTransitionFailed(_newContentPresenter == null ? null : _newContentPresenter.Child, exception, false);
                RestoreContentPresenterInteractivity(_newContentPresenter);
                _readyToTransitionToNewContent = true;
            }
        }

        private static void EnsureStoppedTransition(ITransition transition)
        {
            if (transition != null && transition.GetCurrentState() != System.Windows.Media.Animation.ClockState.Stopped)
            {
                transition.Stop();
            }
        }

        private static void PerformTransition(NavigationTransition navigationTransition, ProjectionHost presenter, ITransition transition)
        {
            if (navigationTransition != null)
            {
                navigationTransition.OnBeginTransition();
            }

            if (presenter != null && presenter.Opacity != 1)
            {
                presenter.Opacity = 1;
            }

            if (transition != null)
            {
                transition.Begin();
            }
        }

        private static void CompleteTransition(NavigationTransition navigationTransition, ProjectionHost presenter, ITransition transition)
        {
            if (transition != null)
            {
                transition.Stop();
            }

            RestoreContentPresenterInteractivity(presenter);

            if (navigationTransition != null)
            {
                navigationTransition.OnEndTransition();
            }
        }

        private static void PrepareContentPresenterForCompositor(ProjectionHost presenter, bool applyBitmapCache = true)
        {
            if (presenter != null)
            {
                if (applyBitmapCache)
                {
                    presenter.CacheMode = BitmapCacheMode;
                }

                presenter.IsHitTestVisible = false;
            }
        }

        private static void RestoreContentPresenterInteractivity(ProjectionHost presenter)
        {
            if (presenter != null)
            {
                presenter.CacheMode = null;

                if (presenter.Opacity != 1)
                {
                    presenter.Opacity = 1;
                }

                presenter.IsHitTestVisible = true;
            }
        }

        private void RaiseTransitionFailed(UIElement element, Exception exception, bool isExit)
        {
            EventHandler<TransitionFailedEventArgs> handler = TransitionFailed;
            if (handler != null)
            {
                handler(this, new TransitionFailedEventArgs(element, exception, isExit));
            }
        }
    }

    /// <summary>
    /// Provides data for the <see cref="TransitionFrame.TransitionFailed"/>
    /// event.
    /// </summary>
    public class TransitionFailedEventArgs : EventArgs
    {
        internal TransitionFailedEventArgs(UIElement element, Exception exception, bool isExit)
        {
            Element = element;
            Exception = exception;
            IsExit = isExit;
        }

        /// <summary>
        /// Gets the page or element whose transition failed.
        /// </summary>
        public UIElement Element { get; private set; }

        /// <summary>
        /// Gets the exception that caused the fallback.
        /// </summary>
        public Exception Exception { get; private set; }

        /// <summary>
        /// Gets a value indicating whether the failed transition was an exit
        /// transition.
        /// </summary>
        public bool IsExit { get; private set; }
    }
}
