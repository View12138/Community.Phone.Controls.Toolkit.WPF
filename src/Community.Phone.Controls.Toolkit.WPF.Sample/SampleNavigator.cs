using System.Collections.Generic;
using System.Windows;
using System.Windows.Media;
using Microsoft.Phone.Controls;

namespace PhoneToolkitSample
{
    /// <summary>
    /// The transition families the sample can demonstrate.
    /// </summary>
    internal enum TransitionKind
    {
        Turnstile,
        Slide,
        Rotate,
        Swivel,
        Roll,
        Feather
    }

    /// <summary>
    /// Navigates between sample pages and applies the transition family of the
    /// target sample.
    /// </summary>
    /// <remarks>
    /// Transitions belong to a page, but the outgoing transition is taken from
    /// the page being left. That is why the sample sets both sides explicitly:
    /// otherwise leaving the main page towards the feathering sample would run
    /// the main page's turnstile exit instead of a feathered exit.
    /// </remarks>
    internal static class SampleNavigator
    {
        private static readonly List<PhoneApplicationPage> History = new List<PhoneApplicationPage>();

        public static void Navigate(PhoneApplicationPage source, PhoneApplicationPage target, TransitionKind kind)
        {
            if (source != null)
            {
                TransitionService.SetNavigationOutTransition(source, CreateOutTransition(kind, source));
                History.Add(source);
            }

            if (target != null)
            {
                TransitionService.SetNavigationInTransition(target, CreateInTransition(kind));
            }

            source.NavigationService.Navigate(target);
        }

        /// <summary>
        /// Navigates back while keeping the transition family of the sample.
        /// </summary>
        /// <remarks>
        /// The page that becomes visible again is an existing instance, so its
        /// incoming transition is replaced before the navigation starts;
        /// otherwise the family declared in its XAML would win.
        /// </remarks>
        public static void GoBack(PhoneApplicationPage source, TransitionKind kind)
        {
            if (source == null)
            {
                return;
            }

            TransitionService.SetNavigationOutTransition(source, CreateOutTransition(kind, source));

            if (History.Count > 0)
            {
                PhoneApplicationPage previous = History[History.Count - 1];
                History.RemoveAt(History.Count - 1);
                TransitionService.SetNavigationInTransition(previous, CreateInTransition(kind));
            }

            if (source.Frame != null)
            {
                source.Frame.GoBack();
            }
        }

        /// <summary>
        /// Gets a value indicating whether a page has elements that participate
        /// in feathered transitions.
        /// </summary>
        /// <remarks>
        /// A feathered transition on a page without feathered elements has
        /// nothing to animate, so the sample falls back to the turnstile exit
        /// instead of leaving the page static while the next page feathers in.
        /// </remarks>
        private static bool HasFeatheredElements(DependencyObject page)
        {
            if (page == null)
            {
                return false;
            }

            int count = VisualTreeHelper.GetChildrenCount(page);
            for (int i = 0; i < count; i++)
            {
                DependencyObject child = VisualTreeHelper.GetChild(page, i);
                if (child is FrameworkElement && TurnstileFeatherEffect.GetFeatheringIndex(child) >= 0)
                {
                    return true;
                }

                if (HasFeatheredElements(child))
                {
                    return true;
                }
            }

            return false;
        }

        private static NavigationInTransition CreateInTransition(TransitionKind kind)
        {
            NavigationInTransition transition = new NavigationInTransition();

            switch (kind)
            {
                case TransitionKind.Slide:
                    transition.Forward = new SlideTransition { Mode = SlideTransitionMode.SlideLeftFadeIn };
                    transition.Backward = new SlideTransition { Mode = SlideTransitionMode.SlideRightFadeIn };
                    break;
                case TransitionKind.Rotate:
                    transition.Forward = new RotateTransition { Mode = RotateTransitionMode.In90Clockwise };
                    transition.Backward = new RotateTransition { Mode = RotateTransitionMode.In90Counterclockwise };
                    break;
                case TransitionKind.Swivel:
                    transition.Forward = new SwivelTransition { Mode = SwivelTransitionMode.FullScreenIn };
                    transition.Backward = new SwivelTransition { Mode = SwivelTransitionMode.BackwardIn };
                    break;
                case TransitionKind.Roll:
                    transition.Forward = new RollTransition();
                    transition.Backward = new RollTransition();
                    break;
                case TransitionKind.Feather:
                    transition.Forward = new TurnstileFeatherTransition { Mode = TurnstileFeatherTransitionMode.ForwardIn };
                    transition.Backward = new TurnstileFeatherTransition { Mode = TurnstileFeatherTransitionMode.BackwardIn };
                    break;
                default:
                    transition.Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardIn };
                    transition.Backward = new TurnstileTransition { Mode = TurnstileTransitionMode.BackwardIn };
                    break;
            }

            return transition;
        }

        private static NavigationOutTransition CreateOutTransition(TransitionKind kind, DependencyObject source)
        {
            NavigationOutTransition transition = new NavigationOutTransition();

            switch (kind)
            {
                case TransitionKind.Slide:
                    transition.Forward = new SlideTransition { Mode = SlideTransitionMode.SlideLeftFadeOut };
                    transition.Backward = new SlideTransition { Mode = SlideTransitionMode.SlideRightFadeOut };
                    break;
                case TransitionKind.Rotate:
                    transition.Forward = new RotateTransition { Mode = RotateTransitionMode.Out90Clockwise };
                    transition.Backward = new RotateTransition { Mode = RotateTransitionMode.Out90Counterclockwise };
                    break;
                case TransitionKind.Swivel:
                    transition.Forward = new SwivelTransition { Mode = SwivelTransitionMode.FullScreenOut };
                    transition.Backward = new SwivelTransition { Mode = SwivelTransitionMode.BackwardOut };
                    break;
                case TransitionKind.Roll:
                    transition.Forward = new RollTransition();
                    transition.Backward = new RollTransition();
                    break;
                case TransitionKind.Feather:
                    if (HasFeatheredElements(source))
                    {
                        transition.Forward = new TurnstileFeatherTransition { Mode = TurnstileFeatherTransitionMode.ForwardOut };
                        transition.Backward = new TurnstileFeatherTransition { Mode = TurnstileFeatherTransitionMode.BackwardOut };
                        break;
                    }

                    transition.Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardOut };
                    transition.Backward = new TurnstileTransition { Mode = TurnstileTransitionMode.BackwardOut };
                    break;
                default:
                    transition.Forward = new TurnstileTransition { Mode = TurnstileTransitionMode.ForwardOut };
                    transition.Backward = new TurnstileTransition { Mode = TurnstileTransitionMode.BackwardOut };
                    break;
            }

            return transition;
        }
    }
}
