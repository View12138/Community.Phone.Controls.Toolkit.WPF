// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;

#if WINDOWS_PHONE

#endif

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// This code provides attached properties for adding a 'tilt' effect to all 
    /// controls within a container.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    [SuppressMessage("Microsoft.Design", "CA1052:StaticHolderTypesShouldBeSealed", Justification = "Cannot be static and derive from DependencyObject.")]
    public partial class TiltEffect : DependencyObject
    {
        /// <summary>
        /// Maximum amount of tilt, in radians.
        /// </summary>
        private const double MaxAngle = 0.3;

        /// <summary>
        /// Maximum amount of depression, in pixels
        /// </summary>
        private const double MaxDepression = 25;

        /// <summary>
        /// Delay between releasing an element and the tilt release animation 
        /// playing.
        /// </summary>
        private static readonly TimeSpan TiltReturnAnimationDelay = TimeSpan.FromMilliseconds(200);

        /// <summary>
        /// Duration of tilt release animation.
        /// </summary>
        private static readonly TimeSpan TiltReturnAnimationDuration = TimeSpan.FromMilliseconds(100);

        /// <summary>
        /// The control that is currently being tilted.
        /// </summary>
        private static FrameworkElement currentTiltElement;

        /// <summary>
        /// The container whose pointer events drive the current tilt.
        /// </summary>
        private static FrameworkElement currentTiltContainer;

        /// <summary>
        /// Mapping from the tilt container to the tilted control, captured 
        /// while the control is still rendered in 2D.
        /// </summary>
        private static GeneralTransform currentTiltContainerToHost;

        /// <summary>
        /// The single instance of a storyboard used for all tilts.
        /// </summary>
        private static Storyboard tiltReturnStoryboard;

        /// <summary>
        /// The single instance of an X rotation used for all tilts.
        /// </summary>
        private static DoubleAnimation tiltReturnXAnimation;

        /// <summary>
        /// The single instance of a Y rotation used for all tilts.
        /// </summary>
        private static DoubleAnimation tiltReturnYAnimation;

        /// <summary>
        /// The single instance of a Z depression used for all tilts.
        /// </summary>
        private static DoubleAnimation tiltReturnZAnimation;

        /// <summary>
        /// Whether the tilt return storyboard has been started.
        /// </summary>
        private static bool tiltReturnStoryboardBegun;

        /// <summary>
        /// The bounds of the content that is currently being tilted, in the
        /// control's coordinates. Like the original toolkit, a tilt rotates
        /// about the centre of the control's first visual child (what the
        /// control actually shows), not about the centre of its layout slot.
        /// </summary>
        private static Rect currentTiltElementBounds;

        /// <summary>
        /// Whether the animation just completed was for a 'pause' or not.
        /// </summary>
        private static bool wasPauseAnimation = false;

        /// <summary>
        /// Whether to use a slightly more accurate (but slightly slower) tilt 
        /// animation easing function.
        /// </summary>
        public static bool UseLogarithmicEase { get; set; }

        /// <summary>
        /// Default list of items that are tiltable.
        /// </summary>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "Tiltable", Justification = "By design.")]
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists", Justification = "Keeping it simple.")]
        public static List<Type> TiltableItems { get; private set; }

        #region Constructor and Static Constructor
        /// <summary>
        /// This is not a constructable class, but it cannot be static because 
        /// it derives from DependencyObject.
        /// </summary>
        private TiltEffect()
        {
        }

        /// <summary>
        /// Initialize the static properties
        /// </summary>
        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline", Justification = "Need to initialize the tiltable items property.")]
        static TiltEffect()
        {
            // The tiltable items list.
            TiltableItems = new List<Type>()
            {
                typeof(ButtonBase),
                typeof(ListBoxItem),
                typeof(ListPicker),
                typeof(MenuItem),
                typeof(LongListSelector)
            };
        }

        #endregion

        #region Dependency properties

        /// <summary>
        /// Whether the tilt effect is enabled on a container (and all its 
        /// children).
        /// </summary>
        public static readonly DependencyProperty IsTiltEnabledProperty = DependencyProperty.RegisterAttached(
          "IsTiltEnabled",
          typeof(bool),
          typeof(TiltEffect),
          new PropertyMetadata(OnIsTiltEnabledChanged)
          );

        /// <summary>
        /// Gets the IsTiltEnabled dependency property from an object.
        /// </summary>
        /// <param name="source">The object to get the property from.</param>
        /// <returns>The property's value.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static bool GetIsTiltEnabled(DependencyObject source)
        {
            return (bool)source.GetValue(IsTiltEnabledProperty);
        }

        /// <summary>
        /// Sets the IsTiltEnabled dependency property on an object.
        /// </summary>
        /// <param name="source">The object to set the property on.</param>
        /// <param name="value">The value to set.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static void SetIsTiltEnabled(DependencyObject source, bool value)
        {
            source.SetValue(IsTiltEnabledProperty, value);
        }

        /// <summary>
        /// Suppresses the tilt effect on a single control that would otherwise 
        /// be tilted.
        /// </summary>
        public static readonly DependencyProperty SuppressTiltProperty = DependencyProperty.RegisterAttached(
          "SuppressTilt",
          typeof(bool),
          typeof(TiltEffect),
          null
          );

        /// <summary>
        /// Gets the SuppressTilt dependency property from an object.
        /// </summary>
        /// <param name="source">The object to get the property from.</param>
        /// <returns>The property's value.</returns>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static bool GetSuppressTilt(DependencyObject source)
        {
            return (bool)source.GetValue(SuppressTiltProperty);
        }

        /// <summary>
        /// Sets the SuppressTilt dependency property from an object.
        /// </summary>
        /// <param name="source">The object to get the property from.</param>
        /// <param name="value">The property's value.</param>
        [SuppressMessage("Microsoft.Design", "CA1062:Validate arguments of public methods", MessageId = "0", Justification = "Standard pattern.")]
        public static void SetSuppressTilt(DependencyObject source, bool value)
        {
            source.SetValue(SuppressTiltProperty, value);
        }

        /// <summary>
        /// Property change handler for the IsTiltEnabled dependency property.
        /// </summary>
        /// <param name="target">The element that the property is atteched to.</param>
        /// <param name="args">Event arguments.</param>
        /// <remarks>
        /// Adds or removes event handlers from the element that has been 
        /// (un)registered for tilting.
        /// </remarks>
        static void OnIsTiltEnabledChanged(DependencyObject target, DependencyPropertyChangedEventArgs args)
        {
            FrameworkElement fe = target as FrameworkElement;
            if (fe != null)
            {
                // Add / remove the event handler if necessary
                if ((bool)args.NewValue == true)
                {
                    fe.ManipulationStarted += TiltEffect_ManipulationStarted;
                    fe.PreviewMouseDown += TiltEffect_MouseDown;
                    fe.Loaded -= OnTiltElementLoaded;
                    fe.Loaded += OnTiltElementLoaded;

                    if (fe.IsLoaded)
                    {
                        EnsureTiltHost(fe);
                    }
                }
                else
                {
                    fe.ManipulationStarted -= TiltEffect_ManipulationStarted;
                    fe.PreviewMouseDown -= TiltEffect_MouseDown;
                    fe.Loaded -= OnTiltElementLoaded;
                    ProjectionService.ReleaseProjectionHost(fe);
                }
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

        /// <summary>
        /// Looks up the tree to find either an explicit DO or DO NOT suppress.
        /// </summary>
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

        /// <summary>
        /// Gets the bounds of the content a control shows, in the control's
        /// coordinates. The original toolkit tilted the control's first visual
        /// child, so the rotation is centred on what is actually visible
        /// instead of on the (possibly much larger) layout slot.
        /// </summary>
        private static Rect GetTiltBounds(FrameworkElement element)
        {
            if (VisualTreeHelper.GetChildrenCount(element) > 0)
            {
                FrameworkElement content = VisualTreeHelper.GetChild(element, 0) as FrameworkElement;
                if (content != null && content.ActualWidth > 0 && content.ActualHeight > 0)
                {
                    try
                    {
                        Rect bounds = content.TransformToAncestor(element).TransformBounds(
                            new Rect(0, 0, content.ActualWidth, content.ActualHeight));

                        if (bounds.Width > 0 && bounds.Height > 0)
                        {
                            return bounds;
                        }
                    }
                    catch (ArgumentException)
                    {
                    }
                    catch (InvalidOperationException)
                    {
                    }
                }
            }

            return new Rect(0, 0, element.ActualWidth, element.ActualHeight);
        }

        #endregion

        #region Top-level manipulation event handlers

        /// <summary>
        /// Event handler for ManipulationStarted.
        /// </summary>
        /// <param name="sender">sender of the event - this will be the tilt 
        /// container (eg, entire page).</param>
        /// <param name="e">Event arguments.</param>
        private static void TiltEffect_ManipulationStarted(object sender, ManipulationStartedEventArgs e)
        {
            TryStartTiltEffect(sender as FrameworkElement, e.OriginalSource as FrameworkElement, e.ManipulationOrigin, e.ManipulationContainer);
        }

        private static void TiltEffect_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton != MouseButton.Left)
            {
                return;
            }

            FrameworkElement container = sender as FrameworkElement;
            TryStartTiltEffect(container, e.OriginalSource as FrameworkElement, e.GetPosition(container), container);
        }

        /// <summary>
        /// Event handler for ManipulationDelta
        /// </summary>
        /// <param name="sender">sender of the event - this will be the tilting 
        /// object (eg a button).</param>
        /// <param name="e">Event arguments.</param>
        private static void TiltEffect_ManipulationDelta(object sender, ManipulationDeltaEventArgs e)
        {
            ContinueTiltEffect(sender as FrameworkElement, e.ManipulationOrigin, e.ManipulationContainer);
        }

        private static void TiltEffect_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.LeftButton != MouseButtonState.Pressed)
            {
                return;
            }

            FrameworkElement container = sender as FrameworkElement;
            ContinueTiltEffect(container, e.GetPosition(container), container);
        }

        /// <summary>
        /// Event handler for ManipulationCompleted.
        /// </summary>
        /// <param name="sender">sender of the event - this will be the tilting 
        /// object (eg a button).</param>
        /// <param name="e">Event arguments.</param>
        static void TiltEffect_ManipulationCompleted(object sender, ManipulationCompletedEventArgs e)
        {
            EndTiltEffect(currentTiltElement);
        }
        private static void TiltEffect_MouseUp(object sender, MouseEventArgs e)
        {
            if (e is MouseButtonEventArgs args && args.ChangedButton != MouseButton.Left)
            {
                return;
            }

            EndTiltEffect(currentTiltElement);
        }

        #endregion

        #region Core tilt logic


        /// <summary>
        /// Checks if the manipulation should cause a tilt, and if so starts the 
        /// tilt effect.
        /// </summary>
        /// <param name="source">The source of the manipulation (the tilt 
        /// container, eg entire page).</param>
        /// <param name="e">The args from the ManipulationStarted event.</param>
        static void TryStartTiltEffect(FrameworkElement source, ManipulationStartedEventArgs e)
        {
            TryStartTiltEffect(source, e.OriginalSource as FrameworkElement, e.ManipulationOrigin, e.ManipulationContainer);
        }
        /// <summary>
        /// Checks if the manipulation should cause a tilt, and if so starts the 
        /// tilt effect.
        /// </summary>
        /// <param name="source">The source of the manipulation (the tilt 
        /// container, eg entire page).</param>
        /// <param name="OriginalSource">The args from the ManipulationStarted event.</param>
        /// <param name="ManipulationOrigin">The args from the ManipulationStarted event.</param>
        /// <param name="ManipulationContainer">The args from the ManipulationStarted event.</param>
        static void TryStartTiltEffect(FrameworkElement source, FrameworkElement OriginalSource, Point ManipulationOrigin, IInputElement ManipulationContainer)
        {
            FrameworkElement container = ManipulationContainer as FrameworkElement ?? source;
            FrameworkElement ancestor = OriginalSource;

            if (ancestor == null || container == null)
            {
                return;
            }

            // Clean up first: the cleanup may release the host this tilt is
            // about to use.
            if (tiltReturnStoryboard != null)
            {
                StopTiltReturnStoryboardAndCleanup();
            }

            while (ancestor != null)
            {
                if (IsTiltable(ancestor) && !IsSuppressed(ancestor))
                {
#if WP8
                    if (ancestor is LongListSelector)
                    {
                        StartTiltEffectOnLLS((LongListSelector)ancestor, OriginalSource, ManipulationOrigin, ManipulationContainer);
                        return;
                    }
#endif
                    GeneralTransform containerToElement;
                    Point tiltTouchPoint;
                    Rect elementBounds;

                    try
                    {
                        // The mapping has to be captured while the control is
                        // still rendered in 2D: a 3D hosted element has no 2D
                        // transform to its host.
                        containerToElement = container.TransformToVisual(ancestor);
                        tiltTouchPoint = containerToElement.Transform(ManipulationOrigin);
                        elementBounds = GetTiltBounds(ancestor);
                    }
                    catch (ArgumentException)
                    {
                        return;
                    }
                    catch (InvalidOperationException)
                    {
                        return;
                    }

                    BeginTiltEffect(ancestor, tiltTouchPoint, elementBounds, container, containerToElement);
                    return;
                }

                ancestor = ancestor.GetVisualParent();
            }
        }


#if WP8
        /// <summary>
        /// Starts the tilt effect on LLS items or sticky header.
        /// </summary>
        private static void StartTiltEffectOnLLS(LongListSelector lls, ManipulationStartedEventArgs e)
        {
            StartTiltEffectOnLLS(lls, e.OriginalSource as FrameworkElement, e.ManipulationOrigin, e.ManipulationContainer);
        }
        private static void StartTiltEffectOnLLS(LongListSelector lls, FrameworkElement OriginalSource, Point ManipulationOrigin, IInputElement ManipulationContainer)
        {
            FrameworkElement parent = OriginalSource;
            ContentPresenter itemContainer = null;
            int containerCount = 0;

            // Walk from the source up to the selector, keeping the outermost
            // ContentPresenter: that is the item container. The WPF port hosts
            // the items in an ItemsControl instead of the phone's Canvas, so
            // there is no ViewportControl type in the ancestor chain.
            while (parent != null && parent != lls)
            {
                if (parent is ContentPresenter presenter)
                {
                    itemContainer = presenter;
                    containerCount++;
                }

                parent = parent.GetVisualParent();
            }

            if (parent == lls && itemContainer != null && containerCount >= 1)
            {
                // The tilted element is the content of the item container.
                FrameworkElement element = itemContainer.GetVisualChildren().FirstOrDefault() as FrameworkElement;

                // Starts tilt only if we found an element on which tilt is not suppressed.
                if (element != null)
                {
                    object suppressTilt = element.ReadLocalValue(SuppressTiltProperty);

                    if ((suppressTilt is bool) == false || (bool)suppressTilt == false)
                    {
                        FrameworkElement container = ManipulationContainer as FrameworkElement;

                        // The mapping has to be captured while the element is
                        // still rendered in 2D.
                        GeneralTransform containerToElement = container.TransformToVisual(element);

                        // Touch point relative to the element being tilted.
                        Point tiltTouchPoint = containerToElement.Transform(ManipulationOrigin);

                        // Bounds of the visible content, in element coordinates.
                        Rect elementBounds = GetTiltBounds(element);

                        BeginTiltEffect(element, tiltTouchPoint, elementBounds, container, containerToElement);
                    }
                }
            }
        }
#endif

        /// <summary>
        /// Begins the tilt effect by preparing the control and doing the 
        /// initial animation.
        /// </summary>
        /// <param name="element">The element to tilt.</param>
        /// <param name="touchPoint">The touch point, in element coordinates.</param>
        /// <param name="bounds">The bounds of the visible content, in element
        /// coordinates; the tilt rotates about its centre.</param>
        /// <param name="container">The container that drives the tilt.</param>
        /// <param name="containerToElement">Mapping from the container to the
        /// element, captured before the element is rendered in 3D.</param>
        static void BeginTiltEffect(FrameworkElement element, Point touchPoint, Rect bounds, FrameworkElement container, GeneralTransform containerToElement)
        {
            if (tiltReturnStoryboard != null)
            {
                StopTiltReturnStoryboardAndCleanup();
            }

            if (PrepareControlForTilt(element, bounds, container) == false)
            {
                return;
            }

            currentTiltElement = element;
            currentTiltContainer = container;
            currentTiltContainerToHost = containerToElement;
            currentTiltElementBounds = bounds;
            PrepareTiltReturnStoryboard(element);

            ApplyTiltEffect(currentTiltElement, touchPoint, currentTiltElementBounds);
        }

        /// <summary>
        /// Prepares a control to be tilted by setting up a plane projection and
        /// some event handlers.
        /// </summary>
        /// <param name="element">The control that is to be tilted.</param>
        /// <param name="bounds">The bounds of the visible content, in the
        /// control's coordinates.</param>
        /// <param name="container">The container that drives the tilt.</param>
        /// <returns>true if successful; false otherwise.</returns>
        /// <remarks>
        /// This method is conservative; it will fail any attempt to tilt a 
        /// control that already has a projection on it.
        /// </remarks>
        static bool PrepareControlForTilt(FrameworkElement element, Rect bounds, FrameworkElement container)
        {
            // Prevents interference with any existing projection
            if (ProjectionService.GetProjection(element) != null)
            {
                return false;
            }

            PlaneProjection projection = new PlaneProjection();

            // The projection plane is the control's layout slot, but the
            // rotation happens about the centre of the visible content, like
            // the original toolkit (which tilted the first visual child).
            double slotWidth = element.ActualWidth;
            double slotHeight = element.ActualHeight;

            if (bounds.Width > 0 && bounds.Height > 0 && slotWidth > 0 && slotHeight > 0)
            {
                projection.CenterOfRotationX = (bounds.Left + (bounds.Width / 2)) / slotWidth;
                projection.CenterOfRotationY = (bounds.Top + (bounds.Height / 2)) / slotHeight;
            }

            // Assigning a projection wraps the control in a ProjectionHost on
            // demand; the host is removed again when the projection is cleared.
            ProjectionService.SetProjection(element, projection);

            ProjectionHost host = ProjectionService.GetDirectProjectionHost(element);
            if (host == null)
            {
                // The control cannot be projected (an item container, for
                // example); undo the assignment and do nothing.
                ProjectionService.SetProjection(element, null);
                return false;
            }

            // Keep the render path in 3D until the cleanup so the return
            // animation does not switch rasterization halfway through.
            host.Prefer3D = true;

            element.ManipulationDelta += TiltEffect_ManipulationDelta;
            element.ManipulationCompleted += TiltEffect_ManipulationCompleted;
            container.PreviewMouseMove += TiltEffect_MouseMove;
            container.PreviewMouseUp += TiltEffect_MouseUp;
            container.LostMouseCapture += TiltEffect_MouseUp;

            return true;
        }

        /// <summary>
        /// Removes modifications made by PrepareControlForTilt.
        /// </summary>
        /// <param name="element">THe control to be un-prepared.</param>
        /// <param name="container">The container that drives the tilt.</param>
        /// <remarks>
        /// This method is basic; it does not do anything to detect if the 
        /// control being un-prepared was previously prepared.
        /// </remarks>
        private static void RevertPrepareControlForTilt(FrameworkElement element, FrameworkElement container)
        {
            element.ManipulationDelta -= TiltEffect_ManipulationDelta;
            element.ManipulationCompleted -= TiltEffect_ManipulationCompleted;

            if (container != null)
            {
                container.PreviewMouseMove -= TiltEffect_MouseMove;
                container.PreviewMouseUp -= TiltEffect_MouseUp;
                container.LostMouseCapture -= TiltEffect_MouseUp;
            }

            ProjectionHost host = ProjectionService.GetDirectProjectionHost(element);
            if (host != null)
            {
                host.Prefer3D = false;
            }

            // Clearing the projection also removes a host that was created on
            // demand for this tilt.
            ProjectionService.SetProjection(element, null);
        }

        /// <summary>
        /// Creates the tilt return storyboard (if not already created) and 
        /// targets it to the projection.
        /// </summary>
        /// <param name="element">The framework element to prepare for
        /// projection.</param>
        static void PrepareTiltReturnStoryboard(FrameworkElement element)
        {
            if (tiltReturnStoryboard == null)
            {
                tiltReturnStoryboard = new Storyboard();
                tiltReturnStoryboard.Completed += TiltReturnStoryboard_Completed;

                tiltReturnXAnimation = new DoubleAnimation();
                Storyboard.SetTargetProperty(tiltReturnXAnimation, new PropertyPath(PlaneProjection.RotationXProperty));
                tiltReturnXAnimation.BeginTime = TiltReturnAnimationDelay;
                tiltReturnXAnimation.To = 0;
                tiltReturnXAnimation.Duration = TiltReturnAnimationDuration;

                tiltReturnYAnimation = new DoubleAnimation();
                Storyboard.SetTargetProperty(tiltReturnYAnimation, new PropertyPath(PlaneProjection.RotationYProperty));
                tiltReturnYAnimation.BeginTime = TiltReturnAnimationDelay;
                tiltReturnYAnimation.To = 0;
                tiltReturnYAnimation.Duration = TiltReturnAnimationDuration;

                tiltReturnZAnimation = new DoubleAnimation();
                Storyboard.SetTargetProperty(tiltReturnZAnimation, new PropertyPath(PlaneProjection.GlobalOffsetZProperty));
                tiltReturnZAnimation.BeginTime = TiltReturnAnimationDelay;
                tiltReturnZAnimation.To = 0;
                tiltReturnZAnimation.Duration = TiltReturnAnimationDuration;

                if (UseLogarithmicEase)
                {
                    tiltReturnXAnimation.EasingFunction = new LogarithmicEase();
                    tiltReturnYAnimation.EasingFunction = new LogarithmicEase();
                    tiltReturnZAnimation.EasingFunction = new LogarithmicEase();
                }

                tiltReturnStoryboard.Children.Add(tiltReturnXAnimation);
                tiltReturnStoryboard.Children.Add(tiltReturnYAnimation);
                tiltReturnStoryboard.Children.Add(tiltReturnZAnimation);
            }

            Storyboard.SetTarget(tiltReturnXAnimation, element.Projection);
            Storyboard.SetTarget(tiltReturnYAnimation, element.Projection);
            Storyboard.SetTarget(tiltReturnZAnimation, element.Projection);
        }

        /// <summary>
        /// Continues a tilt effect that is currently applied to an element, 
        /// presumably because the user moved their finger.
        /// </summary>
        /// <param name="element">The element being tilted.</param>
        /// <param name="e">The manipulation event args.</param>
        static void ContinueTiltEffect(FrameworkElement element, ManipulationDeltaEventArgs e)
        {
            ContinueTiltEffect(element, e.ManipulationOrigin, e.ManipulationContainer);
        }
        static void ContinueTiltEffect(FrameworkElement element, Point ManipulationOrigin, IInputElement ManipulationContainer)
        {
            FrameworkElement container = ManipulationContainer as FrameworkElement;
            if (container == null || element == null)
            {
                return;
            }

            if (container == null || element == null)
            {
                return;
            }

            Point tiltTouchPoint;

            try
            {
                // The element is rendered in 3D while the tilt runs, so the
                // mapping captured when the tilt started is used instead of
                // recomputing it from the (now 3D hosted) element.
                GeneralTransform transform = currentTiltContainerToHost;
                if (transform == null)
                {
                    ProjectionHost host = ProjectionService.GetDirectProjectionHost(element);
                    if (host == null)
                    {
                        return;
                    }

                    transform = container.TransformToVisual(host);
                }

                tiltTouchPoint = transform.Transform(ManipulationOrigin);
            }
            catch (ArgumentException)
            {
                return;
            }
            catch (InvalidOperationException)
            {
                return;
            }

            // If touch moved outside the visible content, then pause the tilt
            // (but don't cancel it)
            if (currentTiltElementBounds.Contains(tiltTouchPoint) != true)
            {
                PauseTiltEffect();
            }
            else
            {
                // Apply the updated tilt effect
                ApplyTiltEffect(currentTiltElement, tiltTouchPoint, currentTiltElementBounds);
            }
        }

        /// <summary>
        /// Ends the tilt effect by playing the animation.
        /// </summary>
        /// <param name="element">The element being tilted.</param>
        private static void EndTiltEffect(FrameworkElement element)
        {
            if (element != null)
            {
                element.ManipulationCompleted -= TiltEffect_ManipulationCompleted;
                element.ManipulationDelta -= TiltEffect_ManipulationDelta;
            }

            if (currentTiltContainer != null)
            {
                currentTiltContainer.PreviewMouseMove -= TiltEffect_MouseMove;
                currentTiltContainer.PreviewMouseUp -= TiltEffect_MouseUp;
                currentTiltContainer.LostMouseCapture -= TiltEffect_MouseUp;
            }

            if (tiltReturnStoryboard != null)
            {
                wasPauseAnimation = false;
                if (!tiltReturnStoryboardBegun)
                {
                    tiltReturnStoryboard.Begin();
                    tiltReturnStoryboardBegun = true;
                }
            }
            else
            {
                StopTiltReturnStoryboardAndCleanup();
            }
        }

        /// <summary>
        /// Handler for the storyboard complete event.
        /// </summary>
        /// <param name="sender">sender of the event.</param>
        /// <param name="e">event args.</param>
        private static void TiltReturnStoryboard_Completed(object sender, EventArgs e)
        {
            tiltReturnStoryboardBegun = false;

            if (wasPauseAnimation)
            {
                ResetTiltEffect(currentTiltElement);
            }
            else
            {
                StopTiltReturnStoryboardAndCleanup();
            }
        }

        /// <summary>
        /// Resets the tilt effect on the control, making it appear 'normal'
        /// again.
        /// </summary>
        /// <param name="element">The element to reset the tilt on.</param>
        /// <remarks>
        /// This method doesn't turn off the tilt effect or cancel any current
        /// manipulation; it just temporarily cancels the effect.
        /// </remarks>
        private static void ResetTiltEffect(FrameworkElement element)
        {
            PlaneProjection projection = element != null ? element.Projection as PlaneProjection : null;
            if (projection == null)
            {
                return;
            }

            projection.RotationY = 0;
            projection.RotationX = 0;
            projection.GlobalOffsetZ = 0;
        }

        /// <summary>
        /// Stops the tilt effect and release resources applied to the currently
        /// tilted control.
        /// </summary>
        private static void StopTiltReturnStoryboardAndCleanup()
        {
            if (tiltReturnStoryboard != null)
            {
                tiltReturnStoryboard.Stop();
            }

            if (currentTiltElement != null)
            {
                // Clearing the projection also removes the host that was
                // created on demand for this tilt.
                RevertPrepareControlForTilt(currentTiltElement, currentTiltContainer);
            }

            currentTiltElement = null;
            currentTiltContainer = null;
            currentTiltContainerToHost = null;
            wasPauseAnimation = false;
            tiltReturnStoryboardBegun = false;
            tiltReturnStoryboard = null;
            tiltReturnXAnimation = null;
            tiltReturnYAnimation = null;
            tiltReturnZAnimation = null;
        }

        /// <summary>
        /// Pauses the tilt effect so that the control returns to the 'at rest'
        /// position, but doesn't stop the tilt effect (handlers are still 
        /// attached).
        /// </summary>
        private static void PauseTiltEffect()
        {
            if ((tiltReturnStoryboard != null) && !wasPauseAnimation)
            {
                if (tiltReturnStoryboardBegun)
                {
                    tiltReturnStoryboard.Stop();
                    tiltReturnStoryboardBegun = false;
                }

                wasPauseAnimation = true;
                tiltReturnStoryboard.Begin();
                tiltReturnStoryboardBegun = true;
            }
        }

        /// <summary>
        /// Resets the storyboard to not running.
        /// </summary>
        private static void ResetTiltReturnStoryboard()
        {
            tiltReturnStoryboard.Stop();
            tiltReturnStoryboardBegun = false;
            wasPauseAnimation = false;
        }

        /// <summary>
        /// Applies the tilt effect to the control.
        /// </summary>
        /// <param name="element">the control to tilt.</param>
        /// <param name="touchPoint">The touch point, in the element's
        /// coordinates.</param>
        /// <param name="bounds">The bounds of the visible content, in the
        /// element's coordinates.</param>
        private static void ApplyTiltEffect(FrameworkElement element, Point touchPoint, Rect bounds)
        {
            // Stop any active animation
            ResetTiltReturnStoryboard();

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            // Get the relative point of the touch inside the content bounds
            Point normalizedPoint = new Point(
                Math.Min(Math.Max((touchPoint.X - bounds.Left) / bounds.Width, 0), 1),
                Math.Min(Math.Max((touchPoint.Y - bounds.Top) / bounds.Height, 0), 1));

            if (double.IsNaN(normalizedPoint.X) || double.IsNaN(normalizedPoint.Y))
            {
                return;
            }

            // Shell values
            double xMagnitude = Math.Abs(normalizedPoint.X - 0.5);
            double yMagnitude = Math.Abs(normalizedPoint.Y - 0.5);
            double xDirection = -Math.Sign(normalizedPoint.X - 0.5);
            double yDirection = Math.Sign(normalizedPoint.Y - 0.5);
            double angleMagnitude = xMagnitude + yMagnitude;
            double xAngleContribution = xMagnitude + yMagnitude > 0 ? xMagnitude / (xMagnitude + yMagnitude) : 0;

            double angle = angleMagnitude * MaxAngle * 180 / Math.PI;
            double depression = (1 - angleMagnitude) * MaxDepression;

            // RotationX and RotationY are the angles of rotations about the x- 
            // or y-*axis*; to achieve a rotation in the x- or y-*direction*, we 
            // need to swap the two. That is, a rotation to the left about the 
            // y-axis is a rotation to the left in the x-direction, and a 
            // rotation up about the x-axis is a rotation up in the y-direction.
            PlaneProjection projection = element.Projection as PlaneProjection;
            if (projection == null)
            {
                return;
            }

            projection.RotationY = angle * xAngleContribution * xDirection;
            projection.RotationX = angle * (1 - xAngleContribution) * yDirection;
            projection.GlobalOffsetZ = -depression;
        }

        #endregion

        #region Custom easing function

        /// <summary>
        /// Provides an easing function for the tilt return.
        /// </summary>
        private class LogarithmicEase : EasingFunctionBase
        {
            /// <summary>
            /// Computes the easing function.
            /// </summary>
            /// <param name="normalizedTime">The time.</param>
            /// <returns>The eased value.</returns>
            protected override double EaseInCore(double normalizedTime)
            {
                // ln(t + 1) / ln(2)
                return Math.Log(normalizedTime + 1) / 0.693147181;
            }

            protected override Freezable CreateInstanceCore() => new LogarithmicEase();
        }

        #endregion
    }
}
