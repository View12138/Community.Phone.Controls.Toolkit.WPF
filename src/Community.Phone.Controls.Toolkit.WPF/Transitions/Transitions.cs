// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Animation;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides <see cref="ITransition"/>s for transition families and modes.
    /// The original XAML storyboards are expressed in code so that the
    /// projection can be targeted directly on the compatible
    /// <see cref="PlaneProjection"/> object.
    /// </summary>
    internal static class Transitions
    {
        /// <summary>
        /// Horizontal center of rotation of the page-level turnstile.
        /// </summary>
        /// <remarks>
        /// The original toolkit rotates exactly around the left edge
        /// (<c>0</c>). The WP 8.1 system animation pivots slightly outside the
        /// screen edge, so the port uses a small negative offset to match the
        /// observed motion. Set this to <c>0</c> to restore the original
        /// toolkit geometry exactly.
        /// </remarks>
        private const double TurnstileCenterOfRotationX = -0.05;

        /// <summary>
        /// Duration multiplier applied to the turnstile phases.
        /// </summary>
        /// <remarks>
        /// The original storyboards run 0.25 s (exit) and 0.35 s (entry). The
        /// WP 8.1 system animation is a little slower than that, so both phases
        /// are scaled by this factor. Set this to <c>1</c> to restore the
        /// original timing exactly.
        /// </remarks>
        private const double TurnstileDurationScale = 1.2;

#if DEBUG
        private const double Rate = 1;
#else
        private const double Rate = 1;
#endif

        public static ITransition Roll(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            PlaneProjection projection = EnsurePlaneProjection(element, 0.5, 0.5);
            Storyboard storyboard = new Storyboard();
            AddRotationZ(storyboard, element, projection, new[]
            {
                KeyFrame(0.0, 0.0, Ease(EasingMode.EaseIn, 1)),
                KeyFrame(0.3, 45.0, Ease(EasingMode.EaseOut, 6)),
                KeyFrame(0.6, 90.0, null)
            });

            return new ProjectionTransition(element, storyboard, projection);
        }

        public static ITransition Rotate(UIElement element, RotateTransitionMode mode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (!Enum.IsDefined(typeof(RotateTransitionMode), mode))
            {
                throw new ArgumentOutOfRangeException("mode");
            }

            PlaneProjection projection = EnsurePlaneProjection(element, 0.5, 0.5);
            mode = AdjustRotateModeForFlowDirection(element, mode);

            Storyboard storyboard = new Storyboard();
            switch (mode)
            {
                case RotateTransitionMode.In180Clockwise:
                    AddRotationZ(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 180.0, null), KeyFrame(0.25 * Rate, 0.0, Ease(EasingMode.EaseOut, 1)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, 1.0, SineEase()) });
                    break;
                case RotateTransitionMode.In180Counterclockwise:
                    AddRotationZ(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, -180.0, null), KeyFrame(0.25 * Rate, 0.0, Ease(EasingMode.EaseOut, 1)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, 1.0, SineEase()) });
                    break;
                case RotateTransitionMode.In90Clockwise:
                    AddRotationZ(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 90.0, null), KeyFrame(0.25 * Rate, 0.0, Ease(EasingMode.EaseOut, 1)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, 1.0, SineEase()) });
                    break;
                case RotateTransitionMode.In90Counterclockwise:
                    AddRotationZ(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, -90.0, null), KeyFrame(0.25 * Rate, 0.0, Ease(EasingMode.EaseOut, 1)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, 1.0, SineEase()) });
                    break;
                case RotateTransitionMode.Out180Clockwise:
                    AddRotationZ(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, -180.0, Ease(EasingMode.EaseIn, 1)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.25 * Rate, 0.0, SineEase()) });
                    break;
                case RotateTransitionMode.Out180Counterclockwise:
                    AddRotationZ(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, 180.0, Ease(EasingMode.EaseIn, 1)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.25 * Rate, 0.0, SineEase()) });
                    break;
                case RotateTransitionMode.Out90Clockwise:
                    AddRotationZ(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, -90.0, Ease(EasingMode.EaseIn, 1)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.25 * Rate, 0.0, SineEase()) });
                    break;
                case RotateTransitionMode.Out90Counterclockwise:
                    AddRotationZ(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, 90.0, Ease(EasingMode.EaseIn, 1)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.25 * Rate, 0.0, SineEase()) });
                    break;
            }

            return new ProjectionTransition(element, storyboard, projection);
        }

        public static ITransition Slide(UIElement element, SlideTransitionMode mode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (!Enum.IsDefined(typeof(SlideTransitionMode), mode))
            {
                throw new ArgumentOutOfRangeException("mode");
            }

            TranslateTransform transform = element.RenderTransform as TranslateTransform;
            if (transform == null)
            {
                transform = new TranslateTransform();
                element.RenderTransform = transform;
            }

            Storyboard storyboard = new Storyboard();
            switch (mode)
            {
                case SlideTransitionMode.SlideDownFadeIn:
                    AddTranslate(storyboard, transform, TranslateTransform.YProperty, new[] { KeyFrame(0.0 * Rate, -200.0, null), KeyFrame(0.35 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.35 * Rate, 1.0, Ease(EasingMode.EaseOut, 6)) });
                    break;
                case SlideTransitionMode.SlideDownFadeOut:
                    AddTranslate(storyboard, transform, TranslateTransform.YProperty, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, 200.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.25 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    break;
                case SlideTransitionMode.SlideLeftFadeIn:
                    AddTranslate(storyboard, transform, TranslateTransform.XProperty, new[] { KeyFrame(0.0 * Rate, 200.0, null), KeyFrame(0.5 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.5 * Rate, 1.0, Ease(EasingMode.EaseOut, 6)) });
                    break;
                case SlideTransitionMode.SlideLeftFadeOut:
                    AddTranslate(storyboard, transform, TranslateTransform.XProperty, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.3 * Rate, -200.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.3 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    break;
                case SlideTransitionMode.SlideRightFadeIn:
                    AddTranslate(storyboard, transform, TranslateTransform.XProperty, new[] { KeyFrame(0.0 * Rate, -200.0, null), KeyFrame(0.5 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.5 * Rate, 1.0, Ease(EasingMode.EaseOut, 6)) });
                    break;
                case SlideTransitionMode.SlideRightFadeOut:
                    AddTranslate(storyboard, transform, TranslateTransform.XProperty, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.3 * Rate, 200.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.3 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    break;
                case SlideTransitionMode.SlideUpFadeIn:
                    AddTranslate(storyboard, transform, TranslateTransform.YProperty, new[] { KeyFrame(0.0 * Rate, 200.0, null), KeyFrame(0.35 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.35 * Rate, 1.0, Ease(EasingMode.EaseOut, 6)) });
                    break;
                case SlideTransitionMode.SlideUpFadeOut:
                    AddTranslate(storyboard, transform, TranslateTransform.YProperty, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, -200.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.25 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    break;
            }

            return new Transition(element, storyboard);
        }

        public static ITransition Swivel(UIElement element, SwivelTransitionMode mode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (!Enum.IsDefined(typeof(SwivelTransitionMode), mode))
            {
                throw new ArgumentOutOfRangeException("mode");
            }

            PlaneProjection projection = EnsurePlaneProjection(element, 0.5, 0.5);
            Storyboard storyboard = new Storyboard();
            switch (mode)
            {
                case SwivelTransitionMode.BackwardIn:
                    AddRotationX(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, -45.0, null), KeyFrame(0.2 * Rate, -30.0, null), KeyFrame(0.35 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.2 * Rate, 0.0, null), KeyFrame(0.21 * Rate, 1.0, null) });
                    break;
                case SwivelTransitionMode.BackwardOut:
                    AddRotationX(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, 60.0, Ease(EasingMode.EaseIn, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.24 * Rate, 1.0, null), KeyFrame(0.25 * Rate, 0.0, null) });
                    break;
                case SwivelTransitionMode.ForwardIn:
                    AddRotationX(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, -45.0, null), KeyFrame(0.35 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null) });
                    break;
                case SwivelTransitionMode.ForwardOut:
                    AddRotationX(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, 90.0, Ease(EasingMode.EaseIn, 15)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.24 * Rate, 1.0, null), KeyFrame(0.25 * Rate, 0.0, null) });
                    break;
                case SwivelTransitionMode.FullScreenIn:
                    AddRotationX(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, -30.0, null), KeyFrame(0.35 * Rate, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null) });
                    break;
                case SwivelTransitionMode.FullScreenOut:
                    AddRotationX(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate, 30.0, Ease(EasingMode.EaseIn, 1)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.24 * Rate, 1.0, null), KeyFrame(0.25 * Rate, 0.0, null) });
                    break;
            }

            ProjectionService.GetProjectionHost(element)?.Prefer3D = true;
            return new ProjectionTransition(element, storyboard, projection);
        }

        public static ITransition Turnstile(UIElement element, TurnstileTransitionMode mode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (!Enum.IsDefined(typeof(TurnstileTransitionMode), mode))
            {
                throw new ArgumentOutOfRangeException("mode");
            }

            PlaneProjection projection = EnsurePlaneProjection(element, TurnstileCenterOfRotationX, 0.5);
            Storyboard storyboard = new Storyboard();
            switch (mode)
            {
                case TurnstileTransitionMode.ForwardIn:
                    AddRotationY(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, -80.0, null), KeyFrame(0.35 * Rate * TurnstileDurationScale, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.01 * Rate * TurnstileDurationScale, 1.0, null) });
                    AddVisibility(storyboard, element);
                    break;
                case TurnstileTransitionMode.ForwardOut:
                    AddRotationY(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate * TurnstileDurationScale, 50.0, Ease(EasingMode.EaseIn, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.24 * Rate * TurnstileDurationScale, 1.0, null), KeyFrame(0.25 * Rate * TurnstileDurationScale, 0.0, null) });
                    break;
                case TurnstileTransitionMode.BackwardIn:
                    AddRotationY(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 50.0, null), KeyFrame(0.35 * Rate * TurnstileDurationScale, 0.0, Ease(EasingMode.EaseOut, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.01 * Rate * TurnstileDurationScale, 1.0, null) });
                    break;
                case TurnstileTransitionMode.BackwardOut:
                    AddRotationY(storyboard, element, projection, new[] { KeyFrame(0.0 * Rate, 0.0, null), KeyFrame(0.25 * Rate * TurnstileDurationScale, -80.0, Ease(EasingMode.EaseIn, 6)) });
                    AddOpacity(storyboard, element, new[] { KeyFrame(0.0 * Rate, 1.0, null), KeyFrame(0.24 * Rate * TurnstileDurationScale, 1.0, null), KeyFrame(0.25 * Rate * TurnstileDurationScale, 0.0, null) });
                    break;
            }

            ProjectionService.GetProjectionHost(element)?.Prefer3D = true;
            return new ProjectionTransition(element, storyboard, projection);
        }

        public static ITransition TurnstileFeather(UIElement element, TurnstileFeatherTransitionMode mode, TimeSpan? beginTime)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }

            if (!Enum.IsDefined(typeof(TurnstileFeatherTransitionMode), mode))
            {
                throw new ArgumentOutOfRangeException("mode");
            }

            PlaneProjection projection = EnsurePlaneProjection(element, TurnstileCenterOfRotationX, 0.5);
            Storyboard storyboard = new Storyboard();
            ProjectionService.GetProjectionHost(element)?.Prefer3D = true;
            return new FeatheredTransition(element, storyboard, mode, beginTime);
        }

        private static PlaneProjection EnsurePlaneProjection(UIElement element, double centerX, double centerY)
        {
            PlaneProjection projection = ProjectionService.GetProjection(element) as PlaneProjection;
            if (projection == null)
            {
                projection = new PlaneProjection();
                ProjectionService.SetProjection(element, projection);
            }

            projection.CenterOfRotationX = centerX;
            projection.CenterOfRotationY = centerY;
            projection.RotationX = 0;
            projection.RotationY = 0;
            projection.RotationZ = 0;
            projection.GlobalOffsetX = 0;
            projection.GlobalOffsetY = 0;
            projection.GlobalOffsetZ = 0;
            projection.LocalOffsetX = 0;
            projection.LocalOffsetY = 0;
            projection.LocalOffsetZ = 0;
            return projection;
        }

        private static RotateTransitionMode AdjustRotateModeForFlowDirection(UIElement element, RotateTransitionMode mode)
        {
            FrameworkElement frameworkElement = element as FrameworkElement;
            if (frameworkElement == null || frameworkElement.FlowDirection != FlowDirection.RightToLeft)
            {
                return mode;
            }

            switch (mode)
            {
                case RotateTransitionMode.In180Clockwise: return RotateTransitionMode.In180Counterclockwise;
                case RotateTransitionMode.In180Counterclockwise: return RotateTransitionMode.In180Clockwise;
                case RotateTransitionMode.In90Clockwise: return RotateTransitionMode.In90Counterclockwise;
                case RotateTransitionMode.In90Counterclockwise: return RotateTransitionMode.In90Clockwise;
                case RotateTransitionMode.Out180Clockwise: return RotateTransitionMode.Out180Counterclockwise;
                case RotateTransitionMode.Out180Counterclockwise: return RotateTransitionMode.Out180Clockwise;
                case RotateTransitionMode.Out90Clockwise: return RotateTransitionMode.Out90Counterclockwise;
                case RotateTransitionMode.Out90Counterclockwise: return RotateTransitionMode.Out90Clockwise;
                default: return mode;
            }
        }

        private static void AddRotationX(Storyboard storyboard, UIElement element, PlaneProjection projection, KeyFrameData[] frames)
        {
            AddProjectionAnimation(storyboard, element, projection, PlaneProjection.RotationXProperty, frames);
        }

        private static void AddRotationY(Storyboard storyboard, UIElement element, PlaneProjection projection, KeyFrameData[] frames)
        {
            AddProjectionAnimation(storyboard, element, projection, PlaneProjection.RotationYProperty, frames);
        }

        private static void AddRotationZ(Storyboard storyboard, UIElement element, PlaneProjection projection, KeyFrameData[] frames)
        {
            AddProjectionAnimation(storyboard, element, projection, PlaneProjection.RotationZProperty, frames);
        }

        private static void AddProjectionAnimation(
            Storyboard storyboard,
            UIElement element,
            PlaneProjection projection,
            DependencyProperty property,
            KeyFrameData[] frames)
        {
            if (projection == null)
            {
                throw new ArgumentNullException("projection");
            }

            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            animation.Duration = new Duration(TimeSpan.FromSeconds(frames[frames.Length - 1].Time));

            // Storyboard clocks do not reliably animate a Freezable that is
            // targeted directly. Target the element instead and traverse its
            // attached projection with a property path.
            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(
                animation,
                new PropertyPath("(0).(1)", ProjectionService.ProjectionProperty, property));

            for (int i = 0; i < frames.Length; i++)
            {
                EasingDoubleKeyFrame keyFrame = new EasingDoubleKeyFrame(frames[i].Value, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(frames[i].Time)));
                if (frames[i].Easing != null)
                {
                    keyFrame.EasingFunction = frames[i].Easing;
                }

                animation.KeyFrames.Add(keyFrame);
            }

            storyboard.Children.Add(animation);
        }

        private static void AddOpacity(Storyboard storyboard, UIElement element, KeyFrameData[] frames)
        {
            AddDoubleAnimation(storyboard, element, UIElement.OpacityProperty, frames);
        }

        private static void AddTranslate(Storyboard storyboard, TranslateTransform transform, DependencyProperty property, KeyFrameData[] frames)
        {
            AddDoubleAnimation(storyboard, transform, property, frames);
        }

        private static void AddDoubleAnimation(Storyboard storyboard, DependencyObject target, DependencyProperty property, KeyFrameData[] frames)
        {
            DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames();
            animation.Duration = new Duration(TimeSpan.FromSeconds(frames[frames.Length - 1].Time));
            Storyboard.SetTarget(animation, target);
            Storyboard.SetTargetProperty(animation, new PropertyPath(property));

            for (int i = 0; i < frames.Length; i++)
            {
                EasingDoubleKeyFrame keyFrame = new EasingDoubleKeyFrame(frames[i].Value, KeyTime.FromTimeSpan(TimeSpan.FromSeconds(frames[i].Time)));
                if (frames[i].Easing != null)
                {
                    keyFrame.EasingFunction = frames[i].Easing;
                }

                animation.KeyFrames.Add(keyFrame);
            }

            storyboard.Children.Add(animation);
        }

        private static void AddVisibility(Storyboard storyboard, UIElement element)
        {
            ObjectAnimationUsingKeyFrames animation = new ObjectAnimationUsingKeyFrames();
            animation.KeyFrames.Add(new DiscreteObjectKeyFrame(Visibility.Visible, KeyTime.FromTimeSpan(TimeSpan.Zero)));
            Storyboard.SetTarget(animation, element);
            Storyboard.SetTargetProperty(animation, new PropertyPath(UIElement.VisibilityProperty));
            storyboard.Children.Add(animation);
        }

        private static KeyFrameData KeyFrame(double seconds, double value, IEasingFunction easing)
        {
            return new KeyFrameData { Time = seconds, Value = value, Easing = easing };
        }

        private static IEasingFunction Ease(EasingMode mode, double exponent)
        {
            return new ExponentialEase { EasingMode = mode, Exponent = exponent };
        }

        private static IEasingFunction SineEase()
        {
            return new SineEase { EasingMode = EasingMode.EaseOut };
        }

        private struct KeyFrameData
        {
            public double Time;
            public double Value;
            public IEasingFunction Easing;
        }

        private sealed class ProjectionTransition : Transition
        {
            private readonly Projection _projection;
            private bool _restored;

            public ProjectionTransition(UIElement element, Storyboard storyboard, Projection projection)
                : base(element, storyboard)
            {
                _projection = projection;
                Completed += OnProjectionTransitionCompleted;
            }

            public override void Stop()
            {
                base.Stop();
                RestoreProjection();
            }

            private void OnProjectionTransitionCompleted(object sender, EventArgs e)
            {
                RestoreProjection();
            }

            private void RestoreProjection()
            {
                if (_restored)
                {
                    return;
                }

                _restored = true;
                if (ReferenceEquals(ProjectionService.GetProjection(Element), _projection))
                {
                    ProjectionService.SetProjection(Element, null);
                }
            }
        }
    }
}
