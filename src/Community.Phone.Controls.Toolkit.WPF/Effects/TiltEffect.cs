// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides attached properties for adding a tilt effect to controls
    /// within a container.
    /// </summary>
    /// <remarks>
    /// In WPF, the tilt is implemented by wrapping the tiltable element in a
    /// <see cref="ProjectionHost"/> and animating a real
    /// <see cref="PlaneProjection"/>. Mouse input is supported in addition to
    /// touch manipulation.
    /// </remarks>
    public partial class TiltEffect : DependencyObject
    {
        private const double MaxAngle = 0.3;
        private const double MaxDepression = 25;
        private static readonly TimeSpan TiltReturnAnimationDelay = TimeSpan.FromMilliseconds(200);
        private static readonly TimeSpan TiltReturnAnimationDuration = TimeSpan.FromMilliseconds(100);

        private static FrameworkElement _currentTiltElement;
        private static ProjectionHost _currentHost;
        private static GeneralTransform _containerToHost;
        private static PlaneProjection _currentProjection;
        private static bool _wrappedByTilt;
        private static Point _currentTiltElementCenter;
        private static Storyboard _tiltReturnStoryboard;
        private static bool _tiltReturnStoryboardBegun;
        private static DoubleAnimation _tiltReturnXAnimation;
        private static DoubleAnimation _tiltReturnYAnimation;
        private static DoubleAnimation _tiltReturnZAnimation;
        private static bool _wasPauseAnimation;

        /// <summary>
        /// Gets or sets a value indicating whether a slightly more accurate
        /// easing function is used for tilt return animations.
        /// </summary>
        public static bool UseLogarithmicEase { get; set; }

        /// <summary>
        /// Gets the default list of items that are tiltable.
        /// </summary>
        public static List<Type> TiltableItems { get; private set; }

        static TiltEffect()
        {
            TiltableItems = new List<Type>
            {
                typeof(ButtonBase),
                typeof(MenuItem),
                typeof(ListBoxItem),
                typeof(ProjectionHost)
            };
        }

        private TiltEffect()
        {
        }

        /// <summary>
        /// Identifies the IsTiltEnabled attached property.
        /// </summary>
        public static readonly DependencyProperty IsTiltEnabledProperty =
            DependencyProperty.RegisterAttached(
                "IsTiltEnabled",
                typeof(bool),
                typeof(TiltEffect),
                new PropertyMetadata(false, OnIsTiltEnabledChanged));

        /// <summary>
        /// Gets the IsTiltEnabled attached property.
        /// </summary>
        public static bool GetIsTiltEnabled(DependencyObject source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return (bool)source.GetValue(IsTiltEnabledProperty);
        }

        /// <summary>
        /// Sets the IsTiltEnabled attached property.
        /// </summary>
        public static void SetIsTiltEnabled(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(IsTiltEnabledProperty, value);
        }

        /// <summary>
        /// Identifies the SuppressTilt attached property.
        /// </summary>
        public static readonly DependencyProperty SuppressTiltProperty =
            DependencyProperty.RegisterAttached("SuppressTilt", typeof(bool), typeof(TiltEffect), null);

        /// <summary>
        /// Gets the SuppressTilt attached property.
        /// </summary>
        public static bool GetSuppressTilt(DependencyObject source)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            return (bool)source.GetValue(SuppressTiltProperty);
        }

        /// <summary>
        /// Sets the SuppressTilt attached property.
        /// </summary>
        public static void SetSuppressTilt(DependencyObject source, bool value)
        {
            if (source == null)
            {
                throw new ArgumentNullException("source");
            }

            source.SetValue(SuppressTiltProperty, value);
        }

        private static void OnIsTiltEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement element = target as FrameworkElement;
            if (element == null)
            {
                return;
            }

            if ((bool)args.NewValue)
            {
                element.IsManipulationEnabled = true;
                element.ManipulationStarted += OnManipulationStarted;
                element.ManipulationDelta += OnManipulationDelta;
                element.ManipulationCompleted += OnManipulationCompleted;
                element.PreviewMouseDown += OnMouseLeftButtonDown;
                element.PreviewMouseMove += OnMouseMove;
                element.PreviewMouseUp += OnMouseLeftButtonUp;
                element.LostMouseCapture += OnLostMouseCapture;
                element.Loaded -= OnTiltElementLoaded;
                element.Loaded += OnTiltElementLoaded;

                if (element.IsLoaded)
                {
                    EnsureTiltHost(element);
                }
            }
            else
            {
                element.ManipulationStarted -= OnManipulationStarted;
                element.ManipulationDelta -= OnManipulationDelta;
                element.ManipulationCompleted -= OnManipulationCompleted;
                element.PreviewMouseDown -= OnMouseLeftButtonDown;
                element.PreviewMouseMove -= OnMouseMove;
                element.PreviewMouseUp -= OnMouseLeftButtonUp;
                element.LostMouseCapture -= OnLostMouseCapture;
                element.Loaded -= OnTiltElementLoaded;
                ProjectionService.ReleaseProjectionHost(element);
            }
        }

        private static void OnTiltElementLoaded(object sender, RoutedEventArgs e)
        {
            EnsureTiltHost(sender as FrameworkElement);
        }

        /// <summary>
        /// Wraps an element that is itself tiltable in a projection host as
        /// soon as it is loaded. Doing it up front keeps the visual tree
        /// untouched while a pointer is pressed, which would otherwise
        /// interrupt the pressed control's input state.
        /// </summary>
        private static void EnsureTiltHost(FrameworkElement element)
        {
            if (element == null || element is ProjectionHost || IsSuppressed(element) || !IsTiltable(element))
            {
                return;
            }

            ProjectionService.EnsureProjectionHost(element);
        }

        private static bool IsTiltable(FrameworkElement element)
        {
            foreach (Type type in TiltableItems)
            {
                if (type.IsAssignableFrom(element.GetType()))
                {
                    return true;
                }
            }

            return false;
        }

        private static void OnManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            TryStartTiltEffect(
                e.OriginalSource as FrameworkElement ?? sender as FrameworkElement,
                e.ManipulationOrigin,
                e.ManipulationContainer as FrameworkElement ?? sender as FrameworkElement);
        }

        private static void OnManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ContinueTiltEffect(sender as FrameworkElement, e.ManipulationOrigin, e.ManipulationContainer as FrameworkElement);
        }

        private static void OnManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            EndTiltEffect();
        }

        private static void OnMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            FrameworkElement container = sender as FrameworkElement;
            if (container == null)
            {
                return;
            }

            Point point = e.GetPosition(container);
            FrameworkElement source = e.OriginalSource as FrameworkElement ?? container;
            TryStartTiltEffect(source, point, container);
        }

        private static void OnMouseMove(object sender, MouseEventArgs e)
        {
            FrameworkElement container = sender as FrameworkElement;
            if (container == null || _currentHost == null || e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            ContinueTiltEffect(container, e.GetPosition(container), container);
        }

        private static void OnMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            EndTiltEffect();
        }

        private static void OnLostMouseCapture(object sender, MouseEventArgs e)
        {
            EndTiltEffect();
        }

        private static bool TryStartTiltEffect(FrameworkElement source, Point point, FrameworkElement container)
        {
            if (source == null || container == null)
            {
                return false;
            }

            DependencyObject current = source;
            FrameworkElement tiltable = null;
            while (current != null)
            {
                FrameworkElement element = current as FrameworkElement;
                if (element != null)
                {
                    foreach (Type type in TiltableItems)
                    {
                        if (type.IsAssignableFrom(element.GetType()))
                        {
                            tiltable = element;
                            break;
                        }
                    }
                }

                if (tiltable != null)
                {
                    break;
                }

                current = VisualTreeHelper.GetParent(current);
            }

            if (tiltable == null || IsSuppressed(tiltable))
            {
                return false;
            }

            ProjectionHost host = tiltable as ProjectionHost;
            bool wrapped = false;
            if (host == null)
            {
                host = ProjectionService.GetProjectionHost(tiltable);
                if (host == null || !ReferenceEquals(host.Child, tiltable))
                {
                    // Only a host that directly contains the element can tilt
                    // it; an ancestor host would tilt the whole page.
                    host = ProjectionService.EnsureProjectionHost(tiltable);
                    wrapped = host != null;
                }
            }

            if (host == null || host.Projection != null)
            {
                if (wrapped)
                {
                    ProjectionService.ReleaseProjectionHost(tiltable);
                }

                return false;
            }

            BeginTiltEffect(host, tiltable, wrapped, point, container);
            return true;
        }

        private static bool IsSuppressed(FrameworkElement element)
        {
            DependencyObject current = element;
            while (current != null)
            {
                if (current.ReadLocalValue(SuppressTiltProperty) is bool)
                {
                    return (bool)current.GetValue(SuppressTiltProperty);
                }

                current = VisualTreeHelper.GetParent(current);
            }

            return false;
        }

        private static void BeginTiltEffect(ProjectionHost host, FrameworkElement tiltable, bool wrapped, Point point, FrameworkElement container)
        {
            if (_tiltReturnStoryboard != null)
            {
                StopTiltReturnStoryboardAndCleanup();
            }

            _currentHost = host;
            _wrappedByTilt = wrapped;
            _currentTiltElement = tiltable;
            _currentProjection = new PlaneProjection();

            try
            {
                // The transform has to be captured while the element is still
                // rendered in 2D: a 3D hosted element has no 2D transform to
                // its host, and the pointer position is needed for every
                // update of the tilt.
                _containerToHost = container.TransformToVisual(host);
                Point elementPoint = _containerToHost.Transform(point);
                _currentTiltElementCenter = new Point(host.ActualWidth / 2, host.ActualHeight / 2);

                // Switch the render path before the first tilted frame so the
                // animation does not change appearance one frame in.
                host.Prefer3D = true;
                ProjectionService.SetProjection(tiltable, _currentProjection);

                PrepareTiltReturnStoryboard(host);
                ApplyTiltEffect(elementPoint, _currentTiltElementCenter);
            }
            catch (ArgumentException)
            {
                StopTiltReturnStoryboardAndCleanup();
            }
            catch (InvalidOperationException)
            {
                StopTiltReturnStoryboardAndCleanup();
            }
        }

        private static void PrepareTiltReturnStoryboard(FrameworkElement element)
        {
            if (_tiltReturnStoryboard == null)
            {
                _tiltReturnStoryboard = new Storyboard();
                _tiltReturnStoryboard.Completed += OnTiltReturnStoryboardCompleted;

                _tiltReturnXAnimation = new DoubleAnimation
                {
                    BeginTime = TiltReturnAnimationDelay,
                    To = 0,
                    Duration = TiltReturnAnimationDuration
                };
                Storyboard.SetTargetProperty(_tiltReturnXAnimation, new PropertyPath(PlaneProjection.RotationXProperty));

                _tiltReturnYAnimation = new DoubleAnimation
                {
                    BeginTime = TiltReturnAnimationDelay,
                    To = 0,
                    Duration = TiltReturnAnimationDuration
                };
                Storyboard.SetTargetProperty(_tiltReturnYAnimation, new PropertyPath(PlaneProjection.RotationYProperty));

                _tiltReturnZAnimation = new DoubleAnimation
                {
                    BeginTime = TiltReturnAnimationDelay,
                    To = 0,
                    Duration = TiltReturnAnimationDuration
                };
                Storyboard.SetTargetProperty(_tiltReturnZAnimation, new PropertyPath(PlaneProjection.GlobalOffsetZProperty));

                if (UseLogarithmicEase)
                {
                    _tiltReturnXAnimation.EasingFunction = new LogarithmicEase();
                    _tiltReturnYAnimation.EasingFunction = new LogarithmicEase();
                    _tiltReturnZAnimation.EasingFunction = new LogarithmicEase();
                }

                _tiltReturnStoryboard.Children.Add(_tiltReturnXAnimation);
                _tiltReturnStoryboard.Children.Add(_tiltReturnYAnimation);
                _tiltReturnStoryboard.Children.Add(_tiltReturnZAnimation);
            }

            Storyboard.SetTarget(_tiltReturnXAnimation, _currentProjection);
            Storyboard.SetTarget(_tiltReturnYAnimation, _currentProjection);
            Storyboard.SetTarget(_tiltReturnZAnimation, _currentProjection);
        }

        private static void ContinueTiltEffect(FrameworkElement source, Point point, FrameworkElement container)
        {
            if (_currentHost == null || container == null)
            {
                return;
            }

            try
            {
                // The element is rendered in 3D while the tilt runs, so the
                // mapping captured when the tilt started is used instead of
                // recomputing it from the (now 3D hosted) element.
                GeneralTransform transform = _containerToHost ?? container.TransformToVisual(_currentHost);
                Point elementPoint = transform.Transform(point);
                if (!new Rect(0, 0, _currentHost.ActualWidth, _currentHost.ActualHeight).Contains(elementPoint))
                {
                    PauseTiltEffect();
                }
                else
                {
                    ApplyTiltEffect(elementPoint, _currentTiltElementCenter);
                }
            }
            catch (ArgumentException)
            {
            }
            catch (InvalidOperationException)
            {
            }
        }

        private static void ApplyTiltEffect(Point touchPoint, Point centerPoint)
        {
            if (_currentProjection == null)
            {
                return;
            }

            if (_tiltReturnStoryboard != null && _tiltReturnStoryboardBegun)
            {
                _tiltReturnStoryboard.Stop();
                _tiltReturnStoryboardBegun = false;
                _wasPauseAnimation = false;
            }

            double normalizedX = Math.Min(Math.Max(touchPoint.X / (centerPoint.X * 2), 0), 1);
            double normalizedY = Math.Min(Math.Max(touchPoint.Y / (centerPoint.Y * 2), 0), 1);
            if (double.IsNaN(normalizedX) || double.IsNaN(normalizedY))
            {
                return;
            }

            double xMagnitude = Math.Abs(normalizedX - 0.5);
            double yMagnitude = Math.Abs(normalizedY - 0.5);
            double xDirection = -Math.Sign(normalizedX - 0.5);
            double yDirection = Math.Sign(normalizedY - 0.5);
            double angleMagnitude = xMagnitude + yMagnitude;
            double xAngleContribution = angleMagnitude > 0 ? xMagnitude / angleMagnitude : 0;

            double angle = angleMagnitude * MaxAngle * 180 / Math.PI;
            double depression = (1 - angleMagnitude) * MaxDepression;

            _currentProjection.RotationY = angle * xAngleContribution * xDirection;
            _currentProjection.RotationX = angle * (1 - xAngleContribution) * yDirection;
            _currentProjection.GlobalOffsetZ = -depression;
        }

        private static void EndTiltEffect()
        {
            if (_currentHost != null)
            {
                _currentHost.ManipulationCompleted -= OnManipulationCompleted;
            }

            if (_tiltReturnStoryboard != null)
            {
                _wasPauseAnimation = false;
                if (!_tiltReturnStoryboardBegun)
                {
                    _tiltReturnStoryboard.Begin();
                    _tiltReturnStoryboardBegun = true;
                }
            }
            else
            {
                StopTiltReturnStoryboardAndCleanup();
            }
        }

        private static void OnTiltReturnStoryboardCompleted(object sender, EventArgs e)
        {
            _tiltReturnStoryboardBegun = false;

            if (_wasPauseAnimation)
            {
                if (_currentProjection != null)
                {
                    _currentProjection.RotationX = 0;
                    _currentProjection.RotationY = 0;
                    _currentProjection.GlobalOffsetZ = 0;
                }
            }
            else
            {
                StopTiltReturnStoryboardAndCleanup();
            }
        }

        private static void PauseTiltEffect()
        {
            if (_tiltReturnStoryboard != null && !_wasPauseAnimation)
            {
                if (_tiltReturnStoryboardBegun)
                {
                    _tiltReturnStoryboard.Stop();
                    _tiltReturnStoryboardBegun = false;
                }

                _wasPauseAnimation = true;
                _tiltReturnStoryboard.Begin();
                _tiltReturnStoryboardBegun = true;
            }
        }

        private static void StopTiltReturnStoryboardAndCleanup()
        {
            if (_tiltReturnStoryboard != null && _tiltReturnStoryboardBegun)
            {
                _tiltReturnStoryboard.Stop();
                _tiltReturnStoryboardBegun = false;
            }

            if (_currentHost != null)
            {
                _currentHost.Projection = null;
                _currentHost.Prefer3D = false;
            }

            if (_currentTiltElement != null)
            {
                ProjectionService.SetProjection(_currentTiltElement, null);
            }

            if (_wrappedByTilt && _currentTiltElement != null)
            {
                ProjectionService.ReleaseProjectionHost(_currentTiltElement);
            }

            _currentHost = null;
            _containerToHost = null;
            _currentTiltElement = null;
            _currentProjection = null;
            _wrappedByTilt = false;
            _wasPauseAnimation = false;
            _tiltReturnStoryboardBegun = false;
            _tiltReturnStoryboard = null;
            _tiltReturnXAnimation = null;
            _tiltReturnYAnimation = null;
            _tiltReturnZAnimation = null;
        }

        private sealed class LogarithmicEase : EasingFunctionBase
        {
            protected override double EaseInCore(double normalizedTime)
            {
                return Math.Log10(9 * normalizedTime + 1);
            }

            protected override Freezable CreateInstanceCore()
            {
                return new LogarithmicEase();
            }
        }
    }
}
