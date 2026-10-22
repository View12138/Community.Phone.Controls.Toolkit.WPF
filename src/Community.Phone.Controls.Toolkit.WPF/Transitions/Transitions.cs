// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Resources;
using System.Xml;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Provides
    /// <see cref="T:Microsoft.Phone.Controls.ITransition"/>s
    /// for transition families and modes.
    /// </summary>
    /// <QualityBand>Preview</QualityBand>
    internal static class Transitions
    {
        /// <summary>
        /// The assembly that contains the XAML storyboard resources.
        /// </summary>
        private const string StoryboardAssemblyName = "Community.Phone.Controls.Toolkit.WPF";

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
        /// are slowed down by this factor. The storyboard key times themselves
        /// stay untouched; the clock speed is scaled instead. Set this to
        /// <c>1</c> to restore the original timing exactly.
        /// </remarks>
        private const double TurnstileDurationScale = 1.2;

        /// <summary>
        /// The cached XAML read from the Storyboard resources.
        /// </summary>
        private static Dictionary<string, string> _storyboardXamlCache;

        /// <summary>
        /// Creates a
        /// <see cref="T:Microsoft.Phone.Controls.ITransition"/>
        /// for a transition family, transition mode, and
        /// <see cref="T:System.Windows.UIElement"/>.
        /// </summary>
        /// <typeparam name="T">The type of the transition mode.</typeparam>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="name">The transition family.</param>
        /// <param name="mode">The transition mode.</param>
        /// <returns>The <see cref="T:Microsoft.Phone.Controls.ITransition"/>.</returns>
        private static ITransition GetEnumStoryboard<T>(UIElement element, string name, T mode)
        {
            string key = name + Enum.GetName(typeof(T), mode);
            Storyboard storyboard = GetStoryboard(key);
            if (storyboard == null)
            {
                return null;
            }

            // Port deviation: the turnstile phases are slowed down to match the
            // WP 8.1 system animation without editing the original key times.
            if (name == "Turnstile")
            {
                storyboard.SpeedRatio = 1.0 / TurnstileDurationScale;
            }

            Storyboard.SetTarget(storyboard, element);
            Projection projection = ProjectionService.GetProjection(element);
            return projection == null
                ? new Transition(element, storyboard)
                : new ProjectionTransition(element, storyboard);
        }

        /// <summary>
        /// Creates a
        /// <see cref="T:System.Windows.Media.Storyboard"/>
        /// for a particular transition family and transition mode.
        /// </summary>
        /// <param name="name">The transition family and transition mode.</param>
        /// <returns>The <see cref="T:System.Windows.Media.Storyboard"/>.</returns>
        private static Storyboard GetStoryboard(string name)
        {
            if (_storyboardXamlCache == null)
            {
                _storyboardXamlCache = new Dictionary<string, string>();
            }
            string xaml = null;
            if (_storyboardXamlCache.ContainsKey(name))
            {
                xaml = _storyboardXamlCache[name];
            }
            else
            {
                string path = "/" + StoryboardAssemblyName + ";component/Transitions/Storyboards/" + name + ".xaml";
                Uri uri = new Uri(path, UriKind.Relative);
                StreamResourceInfo streamResourceInfo = Application.GetResourceStream(uri);
                using (StreamReader streamReader = new StreamReader(streamResourceInfo.Stream))
                {
                    xaml = streamReader.ReadToEnd();
                    _storyboardXamlCache[name] = xaml;
                }
            }
            // WPF's XamlReader loads from a stream rather than a string.
            using (StringReader stringReader = new StringReader(xaml))
            using (XmlReader xmlReader = XmlReader.Create(stringReader))
            {
                return XamlReader.Load(xmlReader) as Storyboard;
            }
        }

        /// <summary>
        /// Creates an
        /// <see cref="T:Microsoft.Phone.Controls.ITransition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>
        /// for the roll transition.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <returns>The <see cref="T:Microsoft.Phone.Controls.ITransition"/>.</returns>
        public static ITransition Roll(UIElement element)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            Storyboard storyboard = GetStoryboard("Roll");
            Storyboard.SetTarget(storyboard, element);
            element.Projection = new PlaneProjection { CenterOfRotationX = 0.5, CenterOfRotationY = 0.5 };
            return new ProjectionTransition(element, storyboard);
        }

        /// <summary>
        /// Creates an
        /// <see cref="T:Microsoft.Phone.Controls.ITransition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>
        /// for the rotate transition family.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="rotateTransitionMode">The transition mode.</param>
        /// <returns>The <see cref="T:Microsoft.Phone.Controls.ITransition"/>.</returns>
        public static ITransition Rotate(UIElement element, RotateTransitionMode rotateTransitionMode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (!Enum.IsDefined(typeof(RotateTransitionMode), rotateTransitionMode))
            {
                throw new ArgumentOutOfRangeException("rotateTransitionMode");
            }
            element.Projection = new PlaneProjection { CenterOfRotationX = 0.5, CenterOfRotationY = 0.5 };

            // Takes into account the flow direction.
            rotateTransitionMode = AdjustRotateTransitionModeForFlowDirection(element, rotateTransitionMode);

            return GetEnumStoryboard<RotateTransitionMode>(element, "Rotate", rotateTransitionMode);
        }

        /// <summary>
        /// Creates an
        /// <see cref="T:Microsoft.Phone.Controls.ITransition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>
        /// for the slide transition family.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="slideTransitionMode">The transition mode.</param>
        /// <returns>The <see cref="T:Microsoft.Phone.Controls.ITransition"/>.</returns>
        public static ITransition Slide(UIElement element, SlideTransitionMode slideTransitionMode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (!Enum.IsDefined(typeof(SlideTransitionMode), slideTransitionMode))
            {
                throw new ArgumentOutOfRangeException("slideTransitionMode");
            }
            element.RenderTransform = new TranslateTransform();
            return GetEnumStoryboard<SlideTransitionMode>(element, string.Empty, slideTransitionMode);
        }

        /// <summary>
        /// Creates an
        /// <see cref="T:Microsoft.Phone.Controls.ITransition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>
        /// for the swivel transition family.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="swivelTransitionMode">The transition mode.</param>
        /// <returns>The <see cref="T:Microsoft.Phone.Controls.ITransition"/>.</returns>
        public static ITransition Swivel(UIElement element, SwivelTransitionMode swivelTransitionMode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (!Enum.IsDefined(typeof(SwivelTransitionMode), swivelTransitionMode))
            {
                throw new ArgumentOutOfRangeException("swivelTransitionMode");
            }
            element.Projection = new PlaneProjection();
            return GetEnumStoryboard<SwivelTransitionMode>(element, "Swivel", swivelTransitionMode);
        }

        /// <summary>
        /// Creates an
        /// <see cref="T:Microsoft.Phone.Controls.ITransition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>
        /// for the turnstile transition family.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="turnstileTransitionMode">The transition mode.</param>
        /// <returns>The <see cref="T:Microsoft.Phone.Controls.ITransition"/>.</returns>
        public static ITransition Turnstile(UIElement element, TurnstileTransitionMode turnstileTransitionMode)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (!Enum.IsDefined(typeof(TurnstileTransitionMode), turnstileTransitionMode))
            {
                throw new ArgumentOutOfRangeException("turnstileTransitionMode");
            }
            element.Projection = new PlaneProjection { CenterOfRotationX = TurnstileCenterOfRotationX };
            return GetEnumStoryboard<TurnstileTransitionMode>(element, "Turnstile", turnstileTransitionMode);
        }

        /// <summary>
        /// Creates an
        /// <see cref="T:Microsoft.Phone.Controls.ITransition"/>
        /// for a
        /// <see cref="T:System.Windows.UIElement"/>
        /// for the turnstile feather transition family.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="turnstileFeatherTransitionMode">The transition mode.</param>
        /// <param name="beginTime">The time at which the transition should begin.</param>
        /// <returns>The <see cref="T:Microsoft.Phone.Controls.ITransition"/>.</returns>
        public static ITransition TurnstileFeather(UIElement element, TurnstileFeatherTransitionMode turnstileFeatherTransitionMode, TimeSpan? beginTime)
        {
            if (element == null)
            {
                throw new ArgumentNullException("element");
            }
            if (!Enum.IsDefined(typeof(TurnstileFeatherTransitionMode), turnstileFeatherTransitionMode))
            {
                throw new ArgumentOutOfRangeException("turnstileFeatherTransitionMode");
            }
            element.Projection = new PlaneProjection { CenterOfRotationX = TurnstileCenterOfRotationX };
            return new FeatheredTransition(element, new Storyboard(), turnstileFeatherTransitionMode, beginTime);
        }

        /// <summary>
        /// Adjusts the rotate transition mode based on the <paramref name="element"/>'s FlowDirection.
        /// </summary>
        /// <param name="element">The <see cref="T:System.Windows.UIElement"/>.</param>
        /// <param name="rotateTransitionMode">The transition mode.</param>
        /// <returns>Returns the adjusted rotate transition mode.</returns>
        private static RotateTransitionMode AdjustRotateTransitionModeForFlowDirection(UIElement element, RotateTransitionMode rotateTransitionMode)
        {
            FrameworkElement fe = element as FrameworkElement;
            RotateTransitionMode adjustedRotateTransitionMode = rotateTransitionMode;

            if (fe != null && fe.FlowDirection == FlowDirection.RightToLeft)
            {
                switch (rotateTransitionMode)
                {
                    case RotateTransitionMode.In180Clockwise:
                        adjustedRotateTransitionMode = RotateTransitionMode.In180Counterclockwise;
                        break;
                    case RotateTransitionMode.In180Counterclockwise:
                        adjustedRotateTransitionMode = RotateTransitionMode.In180Clockwise;
                        break;
                    case RotateTransitionMode.In90Clockwise:
                        adjustedRotateTransitionMode = RotateTransitionMode.In90Counterclockwise;
                        break;
                    case RotateTransitionMode.In90Counterclockwise:
                        adjustedRotateTransitionMode = RotateTransitionMode.In90Clockwise;
                        break;
                    case RotateTransitionMode.Out180Clockwise:
                        adjustedRotateTransitionMode = RotateTransitionMode.Out180Counterclockwise;
                        break;
                    case RotateTransitionMode.Out180Counterclockwise:
                        adjustedRotateTransitionMode = RotateTransitionMode.Out180Clockwise;
                        break;
                    case RotateTransitionMode.Out90Clockwise:
                        adjustedRotateTransitionMode = RotateTransitionMode.Out90Counterclockwise;
                        break;
                    case RotateTransitionMode.Out90Counterclockwise:
                        adjustedRotateTransitionMode = RotateTransitionMode.Out90Clockwise;
                        break;
                }
            }

            return adjustedRotateTransitionMode;
        }

        /// <summary>
        /// A <see cref="T:Microsoft.Phone.Controls.Transition"/> that clears
        /// the projection it animates once the transition is over.
        /// </summary>
        /// <remarks>
        /// Silverlight left <c>UIElement.Projection</c> assigned with an
        /// identity projection, which is harmless there. In WPF a projection
        /// decides whether the element is rasterized through the 3D pipeline,
        /// so the port releases it when the transition finishes to keep idle
        /// pages on the plain 2D path.
        /// </remarks>
        private sealed class ProjectionTransition : Transition
        {
            private readonly Projection _projection;
            private bool _restored;

            public ProjectionTransition(UIElement element, Storyboard storyboard)
                : base(element, storyboard)
            {
                _projection = ProjectionService.GetProjection(element);
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
                if (ReferenceEquals(ProjectionService.GetProjection(_element), _projection))
                {
                    ProjectionService.SetProjection(_element, null);
                }
            }
        }
    }
}
