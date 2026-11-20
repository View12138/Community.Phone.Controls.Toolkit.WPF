// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using System.Windows.Media.Media3D;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// replacing its <see cref="UIElement.Effect"/> with a
    /// <see cref="PerspectiveShaderEffect"/>.
    /// </summary>
    /// <remarks>
    /// The element keeps its place in the visual and logical tree: nothing is
    /// re-parented and no layout property is touched. The renderer owns exactly
    /// one effect instance for the lifetime of the element and only rewrites its
    /// shader constants, so an animated projection never allocates a new effect
    /// or a new pixel shader.
    /// </remarks>
    internal sealed class ShaderProjectionRenderer
    {
        /// <summary>
        /// Upper bound of the padded output surface, per axis. A projection
        /// close to edge-on can push a corner arbitrarily far away; the surface
        /// is cropped instead of allocating an unbounded render target.
        /// </summary>
        internal const double MaxOutputExtent = 4096.0;

        private readonly UIElement _element;
        private PerspectiveShaderEffect _effect;
        private Projection _projection;
        private Effect _previousEffect;
        private bool _hasPreviousEffect;
        private Effect _blockedEffect;
        private bool _attached;
        private bool _released;
        private bool _restoreHitTestVisible;
        private bool _savedHitTestVisible;

        private Size _frameSize;
        private Rect _frameOutput;
        private HomographyMatrix _frameInverse;
        private bool _frameEnabled;
        private bool _hasFrame;


        private void OnProjectionChanged(object sender, EventArgs e) => Invalidate();
        private void OnSizeChanged(object sender, SizeChangedEventArgs e) => Invalidate();
        private void OnLayoutUpdated(object sender, EventArgs e) => Invalidate();
        public ShaderProjectionRenderer(UIElement element)
        {
            _element = element ?? throw new ArgumentNullException("element");

            if (element is FrameworkElement frameworkElement)
            {
                frameworkElement.SizeChanged += OnSizeChanged;
            }
            else
            {
                element.LayoutUpdated += OnLayoutUpdated;
            }

            _projection = element.Projection;
            _projection?.ProjectionChanged += OnProjectionChanged;
        }


        /// <summary>
        /// Recomputes the homography, the output bounds and the shader
        /// constants, and detaches the effect when nothing is projected.
        /// </summary>
        public void Invalidate()
        {
            if (_released)
            {
                return;
            }

            Size size = _element.RenderSize;
            if (_element is FrameworkElement element)
            {
                size = new Size(element.ActualWidth + element.Margin.Left + element.Margin.Right, 
                    element.ActualHeight + element.Margin.Top + element.Margin.Bottom);
            }
            Projection projection = _element.Projection;

            if (projection == null || size.Width <= 0 || size.Height <= 0)
            {
                Detach();
                return;
            }

            if (projection.IsIdentity && !_element.PreferShader)
            {
                Detach();
                return;
            }

            Matrix3D matrix = projection.ToMatrix3D(size);

            // The camera culls everything that is not in front of it. Testing
            // the plane center keeps a plane that straddles the camera from
            // rendering the mirrored ghost the homography alone would produce.
            if (ProjectionMath.GetPlaneDistance(matrix, _element.PerspectiveDepth) <= 0)
            {
                ApplyFrame(size, HomographyMatrix.Identity, HomographyMatrix.Identity, new Rect(size), false);
                return;
            }

            HomographyMatrix forward = ProjectionMath.CreateHomography(matrix, size, _element.PerspectiveDepth);
            if (!forward.TryInvert(out HomographyMatrix inverse))
            {
                ApplyFrame(size, HomographyMatrix.Identity, HomographyMatrix.Identity, new Rect(size), false);
                return;
            }

            Rect output = ComputeOutput(forward.TransformBounds(new Rect(size)), size);
            ApplyFrame(size, inverse, forward, output, true);
        }

        /// <summary>
        /// Restores the element's previous effect and stops listening to the
        /// projection.
        /// </summary>
        public void Release()
        {
            if (_released)
            {
                return;
            }

            _released = true;

            if (_element is FrameworkElement frameworkElement)
            {
                frameworkElement.SizeChanged -= OnSizeChanged;
            }
            else
            {
                _element.LayoutUpdated -= OnLayoutUpdated;
            }

            if (_projection != null)
            {
                _projection.ProjectionChanged -= OnProjectionChanged;
                _projection = null;
            }

            Detach();
        }

        private void ApplyFrame(Size size, HomographyMatrix inverse, HomographyMatrix forward, Rect output, bool enabled = false)
        {
            if (_hasFrame && _frameEnabled == enabled && _frameSize == size && _frameOutput == output && _frameInverse.Equals(inverse))
            {
                return;
            }

            if (!EnsureAttached())
            {
                return;
            }

            _effect.Update(inverse, forward, output, size, enabled);

            _frameSize = size;
            _frameOutput = output;
            _frameInverse = inverse;
            _frameEnabled = enabled;
            _hasFrame = true;
        }

        private bool EnsureAttached()
        {
            if (_attached)
            {
                if (ReferenceEquals(_element.Effect, _effect))
                {
                    return true;
                }

                _attached = false;
                _hasPreviousEffect = false;
                _previousEffect = null;
            }

            _effect ??= new PerspectiveShaderEffect();

            Effect current = _element.Effect;
            if (current != null && !ReferenceEquals(current, _effect))
            {
                if (ReferenceEquals(_blockedEffect, current))
                {
                    return false;
                }

                if (_element.EffectConflictBehavior != EffectConflictBehavior.Replace)
                {
                    _blockedEffect = current;
                    return false;
                }

                _blockedEffect = null;
                _previousEffect = current;
                _hasPreviousEffect = true;
            }

            _element.Effect = _effect;
            _attached = true;
            ApplyHitTestPolicy(true);
            return true;
        }

        private void Detach()
        {
            _hasFrame = false;

            // Restoring input must not depend on the effect still being ours:
            // the element must never keep IsHitTestVisible = false because the
            // shader was replaced while it was attached.
            ApplyHitTestPolicy(false);

            if (!_attached)
            {
                return;
            }

            _attached = false;

            if (ReferenceEquals(_element.Effect, _effect))
            {
                _element.Effect = _hasPreviousEffect ? _previousEffect : null;
            }

            _hasPreviousEffect = false;
            _previousEffect = null;
        }

        private void ApplyHitTestPolicy(bool attached)
        {
            if (attached && _element.HitTestPolicy == HitTestPolicy.Disabled)
            {
                if (!_restoreHitTestVisible)
                {
                    _savedHitTestVisible = _element.IsHitTestVisible;
                    _restoreHitTestVisible = true;
                }

                _element.IsHitTestVisible = false;
                return;
            }

            if (_restoreHitTestVisible)
            {
                _restoreHitTestVisible = false;
                _element.IsHitTestVisible = _savedHitTestVisible;
            }
        }


        private static Rect ComputeOutput(Rect projected, Size size)
        {
            double maxWidth = Math.Max(MaxOutputExtent, size.Width);
            double maxHeight = Math.Max(MaxOutputExtent, size.Height);

            if (projected.IsEmpty)
            {
                // A corner maps to infinity: bound the surface symmetrically.
                double padX = Math.Max(0, (maxWidth - size.Width) / 2.0);
                double padY = Math.Max(0, (maxHeight - size.Height) / 2.0);
                return new Rect(-padX, -padY, size.Width + (2 * padX), size.Height + (2 * padY));
            }

            double left = Math.Min(0, projected.Left);
            double top = Math.Min(0, projected.Top);
            double right = Math.Max(size.Width, projected.Right);
            double bottom = Math.Max(size.Height, projected.Bottom);

            left = Math.Max(left, size.Width - maxWidth);
            top = Math.Max(top, size.Height - maxHeight);
            right = Math.Min(right, maxWidth);
            bottom = Math.Min(bottom, maxHeight);

            if (right <= left || bottom <= top)
            {
                return new Rect(size);
            }

            return new Rect(left, top, right - left, bottom - top);
        }
    }
}
