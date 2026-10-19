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

        private static readonly Dictionary<PhoneApplicationPage, List<WeakReference>> PagesToReferences =
            new Dictionary<PhoneApplicationPage, List<WeakReference>>();

        private static readonly List<TargetState> Targets = new List<TargetState>();

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
                element.Unloaded -= OnTargetUnloaded;
                Unsubscribe(element);
            }
            else
            {
                element.SizeChanged -= OnTargetSizeChanged;
                element.SizeChanged += OnTargetSizeChanged;
                element.Unloaded -= OnTargetUnloaded;
                element.Unloaded += OnTargetUnloaded;
                Subscribe(element);
            }
        }

        private static void OnTargetSizeChanged(object sender, SizeChangedEventArgs e)
        {
            Subscribe((FrameworkElement)sender);
        }

        private static void OnTargetUnloaded(object sender, RoutedEventArgs e)
        {
            Unsubscribe((FrameworkElement)sender);
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
            PhoneApplicationPage page = FindAncestor<PhoneApplicationPage>(element);
            if (page == null)
            {
                return;
            }

            List<WeakReference> references;
            if (!PagesToReferences.TryGetValue(page, out references))
            {
                references = new List<WeakReference>();
                PagesToReferences.Add(page, references);
            }

            references.RemoveAll(r => r.Target == null || ReferenceEquals(r.Target, element));
            references.Add(new WeakReference(element));
            references.Sort(CompareReferences);
        }

        private static void Unsubscribe(FrameworkElement element)
        {
            PhoneApplicationPage page = FindAncestor<PhoneApplicationPage>(element);
            if (page == null)
            {
                return;
            }

            List<WeakReference> references;
            if (PagesToReferences.TryGetValue(page, out references))
            {
                references.RemoveAll(r => r.Target == null || ReferenceEquals(r.Target, element));
                if (references.Count == 0)
                {
                    PagesToReferences.Remove(page);
                }
            }
        }

        private static int CompareReferences(WeakReference x, WeakReference y)
        {
            FrameworkElement first = x.Target as FrameworkElement;
            FrameworkElement second = y.Target as FrameworkElement;
            int firstIndex = first == null ? int.MaxValue : GetFeatheringIndex(first);
            int secondIndex = second == null ? int.MaxValue : GetFeatheringIndex(second);
            return firstIndex.CompareTo(secondIndex);
        }

        internal static void ComposeStoryboard(Storyboard storyboard, TimeSpan? beginTime, TurnstileFeatherTransitionMode mode, UIElement pageElement)
        {
            Cleanup();

            PhoneApplicationPage page = pageElement as PhoneApplicationPage;
            if (page == null)
            {
                return;
            }

            List<WeakReference> references;
            if (!PagesToReferences.TryGetValue(page, out references))
            {
                return;
            }

            references.Sort(CompareReferences);

            int counter = 0;
            foreach (WeakReference reference in references)
            {
                FrameworkElement element = reference.Target as FrameworkElement;
                if (element == null || !IsOnScreen(page, element))
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
            foreach (TargetState state in Targets)
            {
                if (state.Element != null)
                {
                    state.Element.Opacity = state.OriginalOpacity;
                    ProjectionService.SetProjection(state.Element, null);
                }

                if (state.Host != null)
                {
                    state.Host.Prefer3D = false;

                    if (state.Element != null)
                    {
                        ProjectionService.ReleaseProjectionHost(state.Element);
                    }
                }
            }

            Targets.Clear();
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

        private static T FindAncestor<T>(DependencyObject element) where T : DependencyObject
        {
            DependencyObject current = element;
            while (current != null)
            {
                T match = current as T;
                if (match != null)
                {
                    return match;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return null;
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
