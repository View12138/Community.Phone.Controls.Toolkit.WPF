// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Threading;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides attached properties to feather <see cref="FrameworkElement"/>s
    /// in and out during page transitions.
    /// </summary>
    public sealed class TurnstileFeatherEffect : DependencyObject
    {
        private const double FeatheringCenterOfRotationX = -0.2;
        private const double ForwardInFeatheringDuration = 350.0;
        private const double ForwardInFeatheringAngle = -80.0;
        private const double ForwardInFeatheringDelay = 40.0;
        private const double ForwardOutFeatheringDuration = 250.0;
        private const double ForwardOutFeatheringAngle = 50.0;
        private const double ForwardOutFeatheringDelay = 50.0;
        private const double BackwardInFeatheringDuration = 350.0;
        private const double BackwardInFeatheringAngle = 50.0;
        private const double BackwardInFeatheringDelay = 50.0;
        private const double BackwardOutFeatheringDuration = 250.0;
        private const double BackwardOutFeatheringAngle = -80.0;
        private const double BackwardOutFeatheringDelay = 40.0;

        private static readonly ExponentialEase FeatheringEaseIn = CreateEase(EasingMode.EaseIn);

        private static readonly ExponentialEase FeatheringEaseOut = CreateEase(EasingMode.EaseOut);

        private static readonly List<TargetState> Targets = new List<TargetState>();

        private static Dispatcher TargetsDispatcher;

        private static readonly IList<Type> NonPermittedTypesInternal = new List<Type>
        {
            typeof(PhoneApplicationFrame),
            typeof(PhoneApplicationPage),
            typeof(ProjectionHost)
        };

        /// <summary>
        /// Gets the list of types that cannot be feathered.
        /// </summary>
        public static IList<Type> NonPermittedTypes
        {
            get { return NonPermittedTypesInternal; }
        }

        /// <summary>
        /// Gets the feathering index of an element.
        /// </summary>
        public static int GetFeatheringIndex(DependencyObject obj)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            return (int)obj.GetValue(FeatheringIndexProperty);
        }

        /// <summary>
        /// Sets the feathering index of an element.
        /// </summary>
        public static void SetFeatheringIndex(DependencyObject obj, int value)
        {
            if (obj == null)
            {
                throw new ArgumentNullException("obj");
            }

            obj.SetValue(FeatheringIndexProperty, value);
        }

        /// <summary>
        /// Identifies the FeatheringIndex attached property.
        /// </summary>
        public static readonly DependencyProperty FeatheringIndexProperty =
            DependencyProperty.RegisterAttached(
                "FeatheringIndex",
                typeof(int),
                typeof(TurnstileFeatherEffect),
                new PropertyMetadata(-1, OnFeatheringIndexChanged));

        private static void OnFeatheringIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            FrameworkElement element = d as FrameworkElement;
            if (element == null)
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture, "The dependency object must be of type {0}.", typeof(FrameworkElement)));
            }

            CheckTypePermission(element);

            if ((int)e.NewValue < 0)
            {
                element.SizeChanged -= OnTargetSizeChanged;
                element.Loaded -= OnTargetLoaded;
                element.Unloaded -= OnTargetUnloaded;
                Unsubscribe(element, true);
            }
            else
            {
                element.SizeChanged -= OnTargetSizeChanged;
                element.SizeChanged += OnTargetSizeChanged;
                element.Loaded -= OnTargetLoaded;
                element.Loaded += OnTargetLoaded;
                element.Unloaded -= OnTargetUnloaded;
                element.Unloaded += OnTargetUnloaded;
                Subscribe(element);
            }
        }

        private static void OnTargetLoaded(object sender, RoutedEventArgs e)
        {
            Subscribe((FrameworkElement)sender);
        }

        private static void OnTargetSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Subscribe((FrameworkElement)sender);
        }

        private static void OnTargetUnloaded(object sender, RoutedEventArgs e)
        {
            FrameworkElement element = sender as FrameworkElement;
            if (element == null)
            {
                return;
            }

            // Wrapping the element (done when it subscribes) raises Unloaded.
            // Removing the reference here would undo the subscription, so a
            // transition would then find no targets for the page.
            if (element.Parent is ProjectionHost)
            {
                return;
            }

            Unsubscribe(element, false);
        }

        private static void CheckTypePermission(FrameworkElement element)
        {
            Type type = element.GetType();
            if (NonPermittedTypesInternal.Contains(type))
            {
                throw new InvalidOperationException(
                    string.Format(CultureInfo.InvariantCulture, "Objects of type {0} cannot be feathered.", type));
            }
        }

        private static void Subscribe(FrameworkElement element)
        {
            // Wrap the element up front so a transition never has to modify
            // the visual tree while it is animating (the wrapper is kept until
            // the element stops being feathered). The wrap is deferred: this
            // runs from SizeChanged, and re-parenting inside a layout pass can
            // make the layout cycle.
            if (!element.IsLoaded || element.Parent is ProjectionHost)
            {
                return;
            }

            element.Dispatcher.BeginInvoke(
                DispatcherPriority.Background,
                new Action(delegate
                {
                    if (element.IsLoaded && !(element.Parent is ProjectionHost))
                    {
                        ProjectionService.EnsureProjectionHost(element);
                    }
                }));
        }

        private static void Unsubscribe(FrameworkElement element, bool releaseHost)
        {
            if (releaseHost)
            {
                ProjectionService.ReleaseProjectionHost(element);
            }
        }

        private static List<FrameworkElement> CollectTargets(DependencyObject root)
        {
            List<FrameworkElement> elements = new List<FrameworkElement>();
            CollectTargets(root, elements);
            return elements;
        }

        private static void CollectTargets(DependencyObject root, List<FrameworkElement> elements)
        {
            int count = VisualTreeHelper.GetChildrenCount(root);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(root, i);
                FrameworkElement element = child as FrameworkElement;
                if (element != null)
                {
                    int index = GetFeatheringIndex(element);
                    if (index >= 0)
                    {
                        // Insert in order; the scan is stable, so elements that
                        // share an index keep their visual tree order.
                        int insertAt = elements.Count;
                        while (insertAt > 0 && GetFeatheringIndex(elements[insertAt - 1]) > index)
                        {
                            insertAt--;
                        }

                        elements.Insert(insertAt, element);
                    }
                }

                CollectTargets(child, elements);
            }
        }

        internal static void ComposeStoryboard(Storyboard storyboard, TimeSpan? beginTime, TurnstileFeatherTransitionMode mode, UIElement pageElement)
        {
            DropTargetsFromAnotherDispatcher();
            Cleanup();

            PhoneApplicationPage page = pageElement as PhoneApplicationPage;
            if (page == null)
            {
                return;
            }

            // The targets are collected from the page instead of from the
            // attached property notifications: an incoming page is composed
            // before it is connected to the visual tree, so the notifications
            // for its elements have not run yet.
            List<FrameworkElement> elements = CollectTargets(page);

            int counter = 0;
            foreach (FrameworkElement element in elements)
            {
                if (!IsOnScreen(page, element))
                {
                    continue;
                }

                ProjectionHost host = ProjectionService.EnsureProjectionHost(element);
                if (host == null)
                {
                    continue;
                }

                // Prepare the 3D render path up front so the feathering does
                // not change the element's rasterization on its first frame.
                host.Prefer3D = true;

                PlaneProjection projection = new PlaneProjection
                {
                    CenterOfRotationX = FeatheringCenterOfRotationX,
                    CenterOfRotationY = 0.5
                };

                ProjectionService.SetProjection(element, projection);

                TargetState state = new TargetState
                {
                    Element = element,
                    Host = host,
                    Projection = projection,
                    OriginalOpacity = element.Opacity
                };
                Targets.Add(state);
                TargetsDispatcher = Dispatcher.CurrentDispatcher;

                switch (mode)
                {
                    case TurnstileFeatherTransitionMode.ForwardIn:
                        element.Opacity = 0;
                        AddProjectionAnimation(storyboard, element, projection, ForwardInFeatheringAngle, 0, ForwardInFeatheringDuration, ForwardInFeatheringDelay * counter, FeatheringEaseOut);
                        AddOpacityAnimation(storyboard, element, 0, state.OriginalOpacity, 0, ForwardInFeatheringDelay * counter);
                        break;
                    case TurnstileFeatherTransitionMode.ForwardOut:
                        AddProjectionAnimation(storyboard, element, projection, 0, ForwardOutFeatheringAngle, ForwardOutFeatheringDuration, ForwardOutFeatheringDelay * counter, FeatheringEaseIn);
                        AddOpacityAnimation(storyboard, element, state.OriginalOpacity, 0, 0, ForwardOutFeatheringDelay * counter + ForwardOutFeatheringDuration);
                        break;
                    case TurnstileFeatherTransitionMode.BackwardIn:
                        element.Opacity = 0;
                        AddProjectionAnimation(storyboard, element, projection, BackwardInFeatheringAngle, 0, BackwardInFeatheringDuration, BackwardInFeatheringDelay * counter, FeatheringEaseOut);
                        AddOpacityAnimation(storyboard, element, 0, state.OriginalOpacity, 0, BackwardInFeatheringDelay * counter);
                        break;
                    case TurnstileFeatherTransitionMode.BackwardOut:
                        AddProjectionAnimation(storyboard, element, projection, 0, BackwardOutFeatheringAngle, BackwardOutFeatheringDuration, BackwardOutFeatheringDelay * counter, FeatheringEaseIn);
                        AddOpacityAnimation(storyboard, element, state.OriginalOpacity, 0, 0, BackwardOutFeatheringDelay * counter + BackwardOutFeatheringDuration);
                        break;
                }

                counter++;
            }

            storyboard.BeginTime = beginTime;
            storyboard.Completed -= OnStoryboardCompleted;
            storyboard.Completed += OnStoryboardCompleted;
        }

        internal static void Cleanup()
        {
            DropTargetsFromAnotherDispatcher();

            foreach (TargetState state in Targets)
            {
                if (state.Element != null)
                {
                    state.Element.Opacity = state.OriginalOpacity;
                    ProjectionService.SetProjection(state.Element, null);
                }

                if (state.Host != null)
                {
                    // The wrapper is kept so the next transition does not have
                    // to re-parent the element; it is released by Unsubscribe.
                    state.Host.Prefer3D = false;
                }
            }

            Targets.Clear();
        }

        private static void DropTargetsFromAnotherDispatcher()
        {
            if (Targets.Count == 0 || ReferenceEquals(TargetsDispatcher, Dispatcher.CurrentDispatcher))
            {
                return;
            }

            // Each UI thread has its own visual tree. Targets left over from a
            // previous dispatcher (a closed window or a test that ran on its
            // own thread) must not be touched from this one.
            Targets.Clear();
            TargetsDispatcher = null;
        }

        private static ExponentialEase CreateEase(EasingMode mode)
        {
            ExponentialEase ease = new ExponentialEase { EasingMode = mode, Exponent = 6 };
            ease.Freeze();
            return ease;
        }

        private static void OnStoryboardCompleted(object sender, EventArgs e)
        {
            Cleanup();
        }

        private static void AddProjectionAnimation(
            Storyboard storyboard,
            FrameworkElement element,
            PlaneProjection projection,
            double from,
            double to,
            double durationMilliseconds,
            double beginMilliseconds,
            IEasingFunction easing)
        {
            if (element == null || projection == null)
            {
                return;
            }

            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMilliseconds),
                BeginTime = TimeSpan.FromMilliseconds(beginMilliseconds),
                EasingFunction = easing,
                FillBehavior = FillBehavior.HoldEnd
            };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(
                animation,
                new PropertyPath("(0).(1)", ProjectionService.ProjectionProperty, PlaneProjection.RotationYProperty));
            storyboard.Children.Add(animation);
        }

        private static void AddOpacityAnimation(
            Storyboard storyboard,
            UIElement element,
            double from,
            double to,
            double durationMilliseconds,
            double beginMilliseconds)
        {
            DoubleAnimation animation = new DoubleAnimation
            {
                From = from,
                To = to,
                Duration = TimeSpan.FromMilliseconds(durationMilliseconds),
                BeginTime = TimeSpan.FromMilliseconds(beginMilliseconds),
                FillBehavior = FillBehavior.HoldEnd
            };

            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.OpacityProperty));
            storyboard.Children.Add(animation);
        }

        private static bool IsOnScreen(FrameworkElement page, FrameworkElement element)
        {
            if (page.ActualWidth <= 0 || page.ActualHeight <= 0)
            {
                return false;
            }

            try
            {
                GeneralTransform transform = element.TransformToVisual(page);
                Point origin = transform.Transform(new Point(0, 0));
                Point corner = transform.Transform(new Point(element.ActualWidth, element.ActualHeight));

                return corner.X > 0
                    && origin.X < page.ActualWidth
                    && corner.Y > 0
                    && origin.Y < page.ActualHeight;
            }
            catch (ArgumentException)
            {
                return false;
            }
        }

        private sealed class TargetState
        {
            public FrameworkElement Element;
            public ProjectionHost Host;
            public PlaneProjection Projection;
            public double OriginalOpacity;
        }
    }
}
