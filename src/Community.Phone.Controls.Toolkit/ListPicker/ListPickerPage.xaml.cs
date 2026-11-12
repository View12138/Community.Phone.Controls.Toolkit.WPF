// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Threading;
using Microsoft.Phone.Controls.LocalizedResources;
using Microsoft.Phone.Shell;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Displays the list of items and allows single or multiple selection.
    /// </summary>
    public partial class ListPickerPage : PhoneApplicationPage
    {
        private const string StateKey_Value = "ListPickerPage_State_Value";
        private const string ProjectionHostPartName = "ProjectionHost";

        /// <summary>
        /// The number of dispatcher turns to wait for the list to generate its
        /// containers before starting the open animation anyway.
        /// </summary>
        private const int MaxOpenAnimationRetries = 50;

        private PageOrientation _lastOrientation;

        private IList<WeakReference> _itemsToAnimate;

        private bool _waitingForListLayout;

        private int _openAnimationRetries;

        /// <summary>
        /// Gets or sets the string of text to display as the header of the page.
        /// </summary>
        public string HeaderText { get; set; }

        /// <summary>
        /// Gets or sets the list of items to display.
        /// </summary>
        public IList Items { get; private set; }

        /// <summary>
        /// Gets or sets the selection mode.
        /// </summary>
        public SelectionMode SelectionMode { get; set; }

        /// <summary>
        /// Gets or sets the selected item.
        /// </summary>
        public object SelectedItem { get; set; }

        /// <summary>
        /// Gets or sets the list of items to select.
        /// </summary>
        public IList SelectedItems { get; private set; }

        /// <summary>
        /// Gets or sets the item template
        /// </summary>
        public DataTemplate FullModeItemTemplate { get; set; }

        /// <summary>
        /// Whether the picker page is open or not.
        /// </summary>
        private bool IsOpen
        {
            get { return (bool)GetValue(IsOpenProperty); }
            set { SetValue(IsOpenProperty, value); }
        }

        private static readonly DependencyProperty IsOpenProperty =
            DependencyProperty.Register("IsOpen",
                                        typeof(bool),
                                        typeof(ListPickerPage),
                                        new PropertyMetadata(false, new PropertyChangedCallback(OnIsOpenChanged)));

        private static void OnIsOpenChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {
            (o as ListPickerPage).OnIsOpenChanged();
        }

        private void OnIsOpenChanged()
        {
            UpdateVisualState(true);
        }

        /// <summary>
        /// Creates a list picker page.
        /// </summary>
        public ListPickerPage()
        {
            InitializeComponent();

            Items = new List<object>();
            SelectedItems = new List<object>();

            Loaded += OnLoaded;
            Unloaded += OnUnloaded;

            ThemeManager.ApplyTheme(this, false);
            ThemeManager.ThemeChanged += (s, e) =>
            {
                ThemePalette palette = (e.Theme == ApplicationTheme.Light ? ThemePalette.Light : ThemePalette.Dark).WithAccent(e.AccentColor);
                ThemeManager.ApplyTheme(Resources, palette);
            };
        }

        private void OnLoaded(object sender, RoutedEventArgs e)
        {
            OrientationChanged += OnOrientationChanged;
            _lastOrientation = Orientation;

            // Customize the ApplicationBar Buttons by providing the right text
            if (null != ApplicationBar)
            {
                foreach (object obj in ApplicationBar.Buttons)
                {
                    IApplicationBarIconButton button = obj as IApplicationBarIconButton;
                    if (null != button)
                    {
                        if ("DONE" == button.Text)
                        {
                            button.Text = ControlResources.DateTimePickerDoneText;
                            button.Click += OnDoneButtonClick;
                        }
                        else if ("CANCEL" == button.Text)
                        {
                            button.Text = ControlResources.DateTimePickerCancelText;
                            button.Click += OnCancelButtonClick;
                        }
                    }
                }
            }

            // Scroll the selected item into view, needs to be done before the
            // animations are setup because the animations are only attached to
            // the items in view. The list may not have containers yet, in which
            // case the request is queued; BeginOpenAnimationWhenListIsReady
            // repeats it once the list is laid out.
            ScrollToSelectedItem();

            PlaneProjection headerProjection = ProjectionService.GetProjection(HeaderTitle) as PlaneProjection;
            if (null == headerProjection)
            {
                headerProjection = new PlaneProjection();
                ProjectionService.SetProjection(HeaderTitle, headerProjection);
            }
            headerProjection.RotationX = -90;

            Picker.Opacity = 1;

            // The item containers are generated by layout passes, and
            // ScrollIntoView only completes after several of them. Wait for the
            // list to settle before setting up the animation: with a long,
            // virtualized list the containers realized during Loaded belong to
            // the previous scroll position, so the animation would cover the
            // wrong items (and leave the visible ones hidden or flat).
            BeginOpenAnimationWhenListIsReady();
        }

        /// <summary>
        /// Hides the items in view and starts the open animation once the list
        /// has finished scrolling and generating its containers.
        /// </summary>
        private void BeginOpenAnimationWhenListIsReady()
        {
            _waitingForListLayout = true;
            _openAnimationRetries = 0;
            LayoutUpdated += OnListLayoutUpdated;
            ScheduleOpenAnimation();

            // A layout pass runs ScrollIntoView; the panels realize the
            // containers for the new offset in a later pass. The retry loop
            // below covers the case where no further pass is raised.
        }

        private void ScheduleOpenAnimation()
        {
            Dispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() =>
            {
                if (!_waitingForListLayout)
                {
                    return;
                }

                // ScrollIntoView issued while the page is loading is queued
                // until the list has containers, and that queued request can be
                // lost. Once the list is laid out it works synchronously, so
                // repeat it until the selected item is on screen.
                ScrollToSelectedItem();
                Picker.UpdateLayout();

                if (IsListReadyForOpenAnimation() || (++_openAnimationRetries >= MaxOpenAnimationRetries))
                {
                    StartOpenAnimation();
                    return;
                }

                ScheduleOpenAnimation();
            }));
        }

        private void OnListLayoutUpdated(object sender, EventArgs e)
        {
            if (_waitingForListLayout && IsListReadyForOpenAnimation())
            {
                StartOpenAnimation();
            }
        }

        private void ScrollToSelectedItem()
        {
            object item = GetScrollTarget();
            if (null != item)
            {
                Picker.ScrollIntoView(item);
            }
        }

        /// <summary>
        /// Gets the item whose container must be on screen before the open
        /// animation starts, or <c>null</c> when nothing (that the list can
        /// show) is selected.
        /// </summary>
        private object GetScrollTarget()
        {
            object item = null;
            if (SelectionMode == SelectionMode.Single)
            {
                item = SelectedItem;
            }
            else if (SelectedItems.Count > 0)
            {
                item = SelectedItems[0];
            }

            return (null != item && Items.Contains(item)) ? item : null;
        }

        /// <summary>
        /// Gets whether the virtualizing panel has realized the containers for
        /// the final scroll position. Until it has, it still holds the
        /// containers of the previous position (the stale ones can still be
        /// laid out as visible), and setting up the animation would target the
        /// wrong items. The item scrolled to by <see cref="OnLoaded"/> is the
        /// one that proves the new position is on screen.
        /// </summary>
        private bool IsListReadyForOpenAnimation()
        {
            ScrollViewer scrollViewer = Picker.GetVisualChildren().OfType<ScrollViewer>().FirstOrDefault();
            if ((null == scrollViewer) || (scrollViewer.ActualHeight <= 0))
            {
                return true;
            }

            object scrolledToItem = GetScrollTarget();
            if (null != scrolledToItem)
            {
                FrameworkElement container = Picker.ItemContainerGenerator.ContainerFromItem(scrolledToItem) as FrameworkElement;
                return (null != container) && IsItemContainerInView(container, scrollViewer);
            }

            for (int index = 0; index < Picker.Items.Count; index++)
            {
                FrameworkElement container = Picker.ItemContainerGenerator.ContainerFromIndex(index) as FrameworkElement;
                if ((null != container) && IsItemContainerInView(container, scrollViewer))
                {
                    return true;
                }
            }

            return Picker.Items.Count == 0;
        }

        private static bool IsItemContainerInView(FrameworkElement container, ScrollViewer scrollViewer)
        {
            GeneralTransform transform;
            try
            {
                transform = container.TransformToVisual(scrollViewer);
            }
            catch (ArgumentException)
            {
                // Not in the visual tree; treat it as eligible.
                return true;
            }
            catch (InvalidOperationException)
            {
                // Not connected; treat it as eligible.
                return true;
            }

            Rect bounds = new Rect(
                transform.Transform(new Point()),
                transform.Transform(new Point(container.ActualWidth, container.ActualHeight)));

            return (bounds.Bottom > 0) && (bounds.Top < scrollViewer.ActualHeight);
        }

        private void StartOpenAnimation()
        {
            if (!_waitingForListLayout)
            {
                return;
            }

            _waitingForListLayout = false;
            LayoutUpdated -= OnListLayoutUpdated;

            // Generate the containers for the final scroll position (and
            // post the pending work), hide them, then let UpdateVisualState
            // animate them in.
            Picker.UpdateLayout();
            SetupListItems(-90);
            IsOpen = true;
        }

        private void OnUnloaded(object sender, RoutedEventArgs e)
        {
            OrientationChanged -= OnOrientationChanged;
            LayoutUpdated -= OnListLayoutUpdated;
            _waitingForListLayout = false;
        }

        private void SetupListItems(double degree)
        {
            // Containers collected by a previous call may no longer be in view
            // (the list scrolled between the two calls). Reset their projection
            // so a stale -90 does not leave them invisible when they scroll
            // back into view.
            if (null != _itemsToAnimate)
            {
                for (int i = 0; i < _itemsToAnimate.Count; i++)
                {
                    FrameworkElement previous = _itemsToAnimate[i].Target as FrameworkElement;
                    if (null != previous)
                    {
                        GetOrCreateProjection(previous).RotationX = 0;
                    }
                }
            }

            _itemsToAnimate = Picker.GetItemsInViewPort();

            for (int i = 0; i < _itemsToAnimate.Count; i++)
            {
                FrameworkElement item = (FrameworkElement)_itemsToAnimate[i].Target;
                if (null != item)
                {
                    GetOrCreateProjection(item).RotationX = degree;
                }
            }
        }

        /// <summary>
        /// Gets the projection that animates an item. Item containers cannot be
        /// wrapped in a <see cref="ProjectionHost"/> (that would break container
        /// generation and virtualization), so the page templates host their
        /// content in a <see cref="ProjectionHost"/> instead. Anything else
        /// (the header, for example) uses the attached projection service.
        /// </summary>
        private static PlaneProjection GetOrCreateProjection(FrameworkElement item)
        {
            ProjectionHost host = GetProjectionHost(item);
            if (null != host)
            {
                PlaneProjection hostProjection = host.Projection as PlaneProjection;
                if (null == hostProjection)
                {
                    hostProjection = new PlaneProjection();
                    host.Projection = hostProjection;
                }

                return hostProjection;
            }

            PlaneProjection projection = ProjectionService.GetProjection(item) as PlaneProjection;
            if (null == projection)
            {
                projection = new PlaneProjection();
                ProjectionService.SetProjection(item, projection);
            }

            return projection;
        }

        private static ProjectionHost GetProjectionHost(FrameworkElement item)
        {
            Control control = item as Control;
            if (null != control && null != control.Template)
            {
                return control.Template.FindName(ProjectionHostPartName, control) as ProjectionHost;
            }

            return null;
        }

        /// <summary>
        /// Called when a page becomes the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            base.OnNavigatedTo(e);

            // Restore Value if returning to application (to avoid inconsistent state)
            if (State.ContainsKey(StateKey_Value))
            {
                State.Remove(StateKey_Value);

                // Back out from picker page for consistency with behavior of core pickers in this scenario
                if (NavigationService.CanGoBack)
                {
                    NavigationService.GoBack();
                    return;
                }
            }

            // Automatically uppercase the text for the header.
            if (null != HeaderText)
            {
                HeaderTitle.Text = HeaderText.ToUpper(CultureInfo.CurrentCulture);
            }

            Picker.DataContext = Items;

            Picker.SelectionMode = SelectionMode;

            if (null != FullModeItemTemplate)
            {
                Picker.ItemTemplate = FullModeItemTemplate;
            }

            if (SelectionMode == SelectionMode.Single)
            {
                ApplicationBar.IsVisible = false;

                Picker.ItemContainerStyle = (Style)Resources["ListBoxItemStyle"];
                Picker.SelectedItem = SelectedItem;
            }
            else
            {
                ApplicationBar.IsVisible = true;
                Picker.ItemContainerStyle = (Style)Resources["ListBoxItemCheckedStyle"];

                foreach (object item in Items)
                {
                    if (null != SelectedItems && SelectedItems.Contains(item))
                    {
                        Picker.SelectedItems.Add(item);
                    }
                }
            }
        }

        private void OnDoneButtonClick(object sender, EventArgs e)
        {
            // Commit the value and close
            SelectedItem = Picker.SelectedItem;
            SelectedItems = Picker.SelectedItems;
            ClosePickerPage();
        }

        private void OnCancelButtonClick(object sender, EventArgs e)
        {
            // Close without committing a value
            SelectedItem = null;
            SelectedItems = null;
            ClosePickerPage();
        }

        /// <summary>
        /// Called when the Back key is pressed.
        /// </summary>
        /// <param name="e">Event arguments.</param>
        protected override void OnBackKeyPress(CancelEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            // Cancel back action so we can play the Close state animation (then go back)
            e.Cancel = true;
            SelectedItem = null;
            SelectedItems = null;
            ClosePickerPage();
        }

        private void ClosePickerPage()
        {
            // Prevent user from selecting an item as the picker is closing,
            // disabling the control would cause the UI to change so instead
            // it's hidden from hittesting.
            Picker.IsHitTestVisible = false;

            IsOpen = false;
        }

        private void OnClosedStoryboardCompleted(object sender, EventArgs e)
        {
            // Close the picker page
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
        }

        /// <summary>
        /// Called when a page is no longer the active page in a frame.
        /// </summary>
        /// <param name="e">An object that contains the event data.</param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            if (null == e)
            {
                throw new ArgumentNullException("e");
            }

            base.OnNavigatedFrom(e);

            // Save Value if navigating away from application
            if (e.Uri.IsExternalNavigation())
            {
                State[StateKey_Value] = StateKey_Value;
            }
        }

        private void OnOrientationChanged(object sender, OrientationChangedEventArgs e)
        {
            PageOrientation newOrientation = e.Orientation;

            RotateTransition transitionElement = new RotateTransition();

            // Adjust padding if possible

            if (null != MainGrid)
            {
                switch (newOrientation)
                {
                    case PageOrientation.Portrait:
                    case PageOrientation.PortraitUp:
                        HeaderTitle.Margin = new Thickness(24, 12, 12, 12);
                        Picker.Margin = new Thickness(24, 12, 0, 0);

                        transitionElement.Mode = (_lastOrientation == PageOrientation.LandscapeLeft) ?
                        RotateTransitionMode.In90Counterclockwise : RotateTransitionMode.In90Clockwise;

                        break;
                    case PageOrientation.Landscape:
                    case PageOrientation.LandscapeLeft:
                        HeaderTitle.Margin = new Thickness(24, 24, 0, 0);
                        Picker.Margin = new Thickness(24, 24, 0, 0);

                        transitionElement.Mode = (_lastOrientation == PageOrientation.LandscapeRight) ?
                        RotateTransitionMode.In180Counterclockwise : RotateTransitionMode.In90Clockwise;
                        break;
                    case PageOrientation.LandscapeRight:
                        HeaderTitle.Margin = new Thickness(24, 24, 0, 0);
                        Picker.Margin = new Thickness(24, 24, 0, 0);

                        transitionElement.Mode = (_lastOrientation == PageOrientation.PortraitUp) ?
                        RotateTransitionMode.In90Counterclockwise : RotateTransitionMode.In180Clockwise;
                        break;
                }
            }

            PhoneApplicationPage phoneApplicationPage = (PhoneApplicationPage)(((PhoneApplicationFrame)Window.GetWindow(this).Content)).Content;
            ITransition transition = transitionElement.GetTransition(phoneApplicationPage);
            transition.Completed += delegate
            {
                transition.Stop();
            };
            transition.Begin();

            _lastOrientation = newOrientation;
        }

        private void UpdateVisualState(bool useTransitions)
        {
            if (useTransitions)
            {
                // The original re-applies the current vertical offset here to
                // stop the picker from moving. That does not apply to WPF (there
                // is no inertia to stop) and it cancels the scroll requested by
                // ScrollIntoView: the offset is read before the virtualizing
                // panel has applied the request, so the list jumps back to the
                // top and the animation covers the first items.

                // Make sure the containers that are currently in view exist and
                // have been laid out before their projections are set up.
                Picker.UpdateLayout();
                SetupListItems(IsOpen ? -90 : 0);

                Storyboard mainBoard = new Storyboard();

                Storyboard headerBoard = AnimationForElement(HeaderTitle, 0);
                mainBoard.Children.Add(headerBoard);

                for (int i = 0; i < _itemsToAnimate.Count; i++)
                {
                    FrameworkElement element = (FrameworkElement)_itemsToAnimate[i].Target;
                    Storyboard board = AnimationForElement(element, i + 1);
                    mainBoard.Children.Add(board);
                }

                if (!IsOpen)
                {
                    mainBoard.Completed += OnClosedStoryboardCompleted;
                }

                mainBoard.Begin();
            }
            else if (!IsOpen)
            {
                OnClosedStoryboardCompleted(null, null);
            }
        }

        private Storyboard AnimationForElement(FrameworkElement element, int index)
        {
            double delay = 30;
            double duration = (IsOpen) ? 350 : 250;
            double from = (IsOpen) ? -45 : 0;
            double to = (IsOpen) ? 0 : 90;
            ExponentialEase ee = new ExponentialEase()
            {
                EasingMode = (IsOpen) ? EasingMode.EaseOut : EasingMode.EaseIn,
                Exponent = 5,
            };

            DoubleAnimation anim = new DoubleAnimation()
            {
                Duration = new Duration(TimeSpan.FromMilliseconds(duration)),
                From = from,
                To = to,
                EasingFunction = ee,
            };

            GetOrCreateProjection(element);
            ProjectionHost host = GetProjectionHost(element);
            DependencyObject animationTarget = (DependencyObject)host ?? element;
            DependencyProperty projectionProperty = (null != host) ? ProjectionHost.ProjectionProperty : ProjectionService.ProjectionProperty;

            Storyboard.SetTarget(anim, animationTarget);
            Storyboard.SetTargetProperty(anim, new PropertyPath("(0).(1)", projectionProperty, PlaneProjection.RotationXProperty));

            Storyboard board = new Storyboard();
            board.BeginTime = TimeSpan.FromMilliseconds(delay * index);
            board.Children.Add(anim);

            return board;
        }

        private void OnPickerTapped(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            // We listen to the tap event because SelectionChanged does not fire if the user picks the already selected item.

            // Only close the page in Single Selection mode.
            if (SelectionMode == SelectionMode.Single)
            {
                // Commit the value and close
                SelectedItem = Picker.SelectedItem;
                ClosePickerPage();
            }
        }
    }
}
