// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Markup;
using System.Windows.Media;
using System.Windows.Media.Media3D;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Hosts a single <see cref="UIElement"/> and renders it either directly
    /// (when the projection is the identity) or on an interactive plane inside
    /// a <see cref="Viewport3D"/>.
    /// </summary>
    /// <remarks>
    /// WPF has no built-in <c>UIElement.Projection</c> property. Wrapping the
    /// content in a <see cref="ProjectionHost"/> is the supported way to apply
    /// a <see cref="Projection"/> while preserving hit testing and input.
    /// </remarks>
    [ContentProperty("Child")]
    public class ProjectionHost : FrameworkElement
    {
        #region Child DependencyProperty
        /// <summary>
        /// Gets or sets the content to project.
        /// </summary>
        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }
        /// <summary>
        /// Identifies the <see cref="Child"/> property.
        /// </summary>
        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register("Child", typeof(UIElement), typeof(ProjectionHost), new PropertyMetadata(null, OnChildChanged));
        #endregion

        #region Projection DependencyProperty
        /// <summary>
        /// Gets or sets the projection applied to the content.
        /// </summary>
        public Projection Projection
        {
            get { return (Projection)GetValue(ProjectionProperty); }
            set { SetValue(ProjectionProperty, value); }
        }
        /// <summary>
        /// Identifies the <see cref="Projection"/> property.
        /// </summary>
        public static readonly DependencyProperty ProjectionProperty =
            DependencyProperty.Register("Projection", typeof(Projection), typeof(ProjectionHost), new PropertyMetadata(null, OnProjectionChanged));
        #endregion

        #region PerspectiveDepth DependencyProperty
        /// <summary>
        /// Gets or sets the distance, in device independent pixels, between the
        /// viewer and the unprojected plane. Smaller values produce a stronger
        /// perspective.
        /// </summary>
        public double PerspectiveDepth
        {
            get { return (double)GetValue(PerspectiveDepthProperty); }
            set { SetValue(PerspectiveDepthProperty, value); }
        }
        /// <summary>
        /// Identifies the <see cref="PerspectiveDepth"/> property.
        /// </summary>
        public static readonly DependencyProperty PerspectiveDepthProperty =
            DependencyProperty.Register("PerspectiveDepth", typeof(double), typeof(ProjectionHost), new PropertyMetadata(DefaultPerspectiveDepth, OnPerspectiveDepthChanged));
        #endregion

        #region Prefer3D DependencyProperty
        /// <summary>
        /// Gets or sets a value indicating whether the content is rendered
        /// through the 3D pipeline even while the projection is the identity.
        /// </summary>
        /// <remarks>
        /// Switching between the 2D and the 3D render path restyles the
        /// content (the 3D path rasterizes it, so ClearType is replaced by
        /// grayscale anti-aliasing). Transitions set this before the first
        /// animated frame so that the whole animation is rendered by a single
        /// path instead of changing appearance on its first frame.
        /// </remarks>
        public bool Prefer3D
        {
            get { return (bool)GetValue(Prefer3DProperty); }
            set { SetValue(Prefer3DProperty, value); }
        }
        /// <summary>
        /// Identifies the <see cref="Prefer3D"/> property.
        /// </summary>
        public static readonly DependencyProperty Prefer3DProperty =
            DependencyProperty.Register("Prefer3D", typeof(bool), typeof(ProjectionHost), new PropertyMetadata(false, OnPrefer3DChanged));
        #endregion

        /// <summary>
        /// The perspective depth used by the XAML <c>PlaneProjection</c>
        /// implementation. The value is a fixed camera distance in device
        /// independent pixels rather than a field of view, which is why the
        /// perspective stays consistent for every element size.
        /// </summary>
        private const double DefaultPerspectiveDepth = 999.0;
        private const double NearPlaneDistance = 0.1;
        private const double FarPlaneDistance = 1000000.0;

        private readonly Viewport3D _viewport;
        private readonly Viewport2DVisual3D _visual3D;
        private readonly PerspectiveCamera _camera;
        private readonly MeshGeometry3D _mesh;
        private readonly ProjectedSurface _surface;
        private Projection _subscribedProjection;
        private bool _using3D;
        private Size _geometrySize = Size.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionHost"/> class.
        /// </summary>
        public ProjectionHost()
        {
            _mesh = new MeshGeometry3D();
            _surface = new ProjectedSurface();
            _camera = new PerspectiveCamera
            {
                NearPlaneDistance = NearPlaneDistance,
                FarPlaneDistance = FarPlaneDistance,
                UpDirection = new Vector3D(0, 1, 0),
                LookDirection = new Vector3D(0, 0, -1)
            };

            DiffuseMaterial material = new DiffuseMaterial(Brushes.White);
            Viewport2DVisual3D.SetIsVisualHostMaterial(material, true);

            _visual3D = new Viewport2DVisual3D
            {
                Geometry = _mesh,
                Material = material
            };

            _viewport = new Viewport3D
            {
                Camera = _camera,
                ClipToBounds = true,
                IsHitTestVisible = true
            };
            _viewport.Children.Add(new ModelVisual3D
            {
                Content = new AmbientLight(Colors.White)
            });
            _viewport.Children.Add(_visual3D);
        }

        /// <summary>
        /// Gets a value indicating whether the content is currently rendered
        /// through the 3D pipeline.
        /// </summary>
        public bool IsRenderingIn3D => _using3D;

        /// <inheritdoc />
        protected override int VisualChildrenCount => _using3D || Child != null ? 1 : 0;

        /// <inheritdoc />
        protected override Visual GetVisualChild(int index)
        {
            if (_using3D)
            {
                if (index != 0)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return _viewport;
            }

            if (index != 0 || Child == null)
            {
                throw new ArgumentOutOfRangeException("index");
            }

            return Child;
        }

        /// <inheritdoc />
        protected override Size MeasureOverride(Size availableSize)
        {
            UIElement child = Child;
            if (child == null)
            {
                return new Size();
            }

            child.Measure(availableSize);

            if (_using3D)
            {
                _surface.Measure(availableSize);
                _viewport.Measure(availableSize);
            }

            return child.DesiredSize;
        }

        /// <inheritdoc />
        protected override Size ArrangeOverride(Size finalSize)
        {
            UIElement child = Child;
            if (child != null)
            {
                if (_using3D)
                {
                    // Viewport2DVisual3D lays its visual out at its desired size,
                    // so the surface is pinned to the host's slot.
                    if (_surface.Width != finalSize.Width || _surface.Height != finalSize.Height)
                    {
                        _surface.Width = finalSize.Width;
                        _surface.Height = finalSize.Height;
                    }

                    _surface.Arrange(new Rect(finalSize));
                }
                else
                {
                    child.Arrange(new Rect(finalSize));
                }
            }

            if (_using3D)
            {
                _viewport.Measure(finalSize);
                _viewport.Arrange(new Rect(finalSize));
            }

            UpdateScene(finalSize);
            return finalSize;
        }

        private static void OnChildChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProjectionHost)d).OnChildChanged((UIElement)e.OldValue, (UIElement)e.NewValue);
        }

        private void OnChildChanged(UIElement oldChild, UIElement newChild)
        {
            if (oldChild != null)
            {
                FrameworkElement oldElement = oldChild as FrameworkElement;
                if (oldElement != null)
                {
                    oldElement.SizeChanged -= OnChildSizeChanged;
                }

                if (_using3D)
                {
                    if (ReferenceEquals(_surface.Content, oldChild))
                    {
                        _surface.Content = null;
                    }
                }
                else
                {
                    RemoveVisualChild(oldChild);
                }

                RemoveLogicalChild(oldChild);
            }

            if (newChild != null)
            {
                AddLogicalChild(newChild);
                FrameworkElement newElement = newChild as FrameworkElement;
                if (newElement != null)
                {
                    newElement.SizeChanged += OnChildSizeChanged;
                }

                if (ShouldUse3D())
                {
                    Ensure3D();
                }
                else
                {
                    RemoveVisualChildIfNeeded();
                    AddVisualChild(newChild);
                    _using3D = false;
                }
            }
            else
            {
                Ensure2D();
            }

            InvalidateMeasure();
            InvalidateArrange();
        }

        private static void OnProjectionChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((ProjectionHost)d).OnProjectionChanged((Projection)e.OldValue, (Projection)e.NewValue);
        }

        private void OnProjectionChanged(Projection oldProjection, Projection newProjection)
        {
            if (_subscribedProjection != null)
            {
                _subscribedProjection.ProjectionChanged -= OnProjectionPropertyChanged;
                _subscribedProjection = null;
            }

            if (newProjection != null)
            {
                newProjection.ProjectionChanged += OnProjectionPropertyChanged;
                _subscribedProjection = newProjection;
            }

            if (ShouldUse3D())
            {
                Ensure3D();
            }
            else
            {
                Ensure2D();
            }

            UpdateScene(RenderSize);
        }

        private static void OnPerspectiveDepthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProjectionHost host = (ProjectionHost)d;
            host.UpdateScene(host.RenderSize);
        }

        private static void OnPrefer3DChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProjectionHost host = (ProjectionHost)d;

            if (host.ShouldUse3D())
            {
                host.Ensure3D();
            }
            else
            {
                host.Ensure2D();
            }

            host.UpdateScene(host.RenderSize);
        }

        private void OnProjectionPropertyChanged(object sender, EventArgs e)
        {
            // A storyboard can animate a projection from identity to a
            // non-identity transform (or back) without replacing the
            // Projection object. The host has to switch render paths in that
            // case as well, otherwise the animation is calculated but never
            // displayed.
            if (ShouldUse3D())
            {
                Ensure3D();
            }
            else
            {
                Ensure2D();
            }

            UpdateScene(RenderSize);
        }

        private void OnChildSizeChanged(object sender, SizeChangedEventArgs e)
        {
            UpdateScene(RenderSize);
        }

        private bool ShouldUse3D()
        {
            if (Child == null)
            {
                return false;
            }

            if (Prefer3D)
            {
                return true;
            }

            Projection projection = Projection;
            return projection != null && !projection.IsIdentity;
        }

        private void Ensure3D()
        {
            if (_using3D || Child == null)
            {
                return;
            }

            RemoveVisualChildIfNeeded();

            // The projected visual is the surface, not the content itself:
            // Viewport2DVisual3D maps the visual's painted bounds onto the
            // quad, so content that does not fill its layout slot (for
            // example a left-aligned CheckBox or a short TextBlock) would
            // otherwise be stretched across the projected plane.
            _surface.Content = Child;
            _visual3D.Visual = _surface;
            AddVisualChild(_viewport);
            _using3D = true;
            InvalidateMeasure();
            InvalidateArrange();
            UpdateScene(RenderSize);
        }

        private void Ensure2D()
        {
            if (!_using3D)
            {
                return;
            }

            _visual3D.Visual = null;
            _surface.Content = null;

            RemoveVisualChild(_viewport);
            _using3D = false;

            if (Child != null)
            {
                AddVisualChild(Child);
            }

            InvalidateMeasure();
            InvalidateArrange();
        }

        private void RemoveVisualChildIfNeeded()
        {
            UIElement child = Child;
            if (child != null && !_using3D && ReferenceEquals(VisualTreeHelper.GetParent(child), this))
            {
                RemoveVisualChild(child);
            }
        }

        private void UpdateScene(Size hostSize)
        {
            if (!_using3D || Child == null)
            {
                return;
            }

            // The projected plane is the host's layout slot, not the size the
            // child happened to be arranged at. A child that is smaller than
            // its host (or that does not paint its whole slot) must keep its
            // size and position on the plane, and the rotation centers are
            // relative to the slot.
            Size planeSize = hostSize;
            if (planeSize.Width <= 0 || planeSize.Height <= 0)
            {
                planeSize = Child.RenderSize;
            }

            if (planeSize.Width <= 0 || planeSize.Height <= 0)
            {
                return;
            }

            UpdateGeometry(planeSize);
            UpdateCamera(hostSize, planeSize);
            UpdateTransform(planeSize);
        }

        private void UpdateGeometry(Size contentSize)
        {
            if (_geometrySize == contentSize)
            {
                return;
            }

            _geometrySize = contentSize;

            double halfWidth = contentSize.Width / 2.0;
            double halfHeight = contentSize.Height / 2.0;

            Point3DCollection positions = new Point3DCollection
            {
                new Point3D(-halfWidth, halfHeight, 0),
                new Point3D(halfWidth, halfHeight, 0),
                new Point3D(halfWidth, -halfHeight, 0),
                new Point3D(-halfWidth, -halfHeight, 0)
            };

            PointCollection textureCoordinates = new PointCollection
            {
                new Point(0, 0),
                new Point(1, 0),
                new Point(1, 1),
                new Point(0, 1)
            };

            Int32Collection triangleIndices = new Int32Collection
            {
                0, 2, 1,
                0, 3, 2
            };

            _mesh.Positions = positions;
            _mesh.TextureCoordinates = textureCoordinates;
            _mesh.TriangleIndices = triangleIndices;
        }

        private void UpdateCamera(Size hostSize, Size contentSize)
        {
            double viewportWidth = hostSize.Width > 0 ? hostSize.Width : contentSize.Width;
            double depth = Math.Max(1.0, PerspectiveDepth);

            // WPF's PerspectiveCamera field of view is horizontal, and the
            // vertical field of view is derived from the viewport aspect
            // ratio. Deriving the field of view from the viewport width keeps
            // the unprojected plane exactly one-to-one with the layout slot,
            // which is what the XAML PlaneProjection (fixed perspective depth)
            // does. Using the height here scales every projected element down
            // by the aspect ratio.
            double fieldOfView = 2.0 * Math.Atan(viewportWidth / (2.0 * depth)) * 180.0 / Math.PI;
            _camera.FieldOfView = Math.Max(1.0, Math.Min(179.0, fieldOfView));
            _camera.Position = new Point3D(0, 0, depth);
        }

        private void UpdateTransform(Size contentSize)
        {
            Projection projection = Projection;
            if (projection == null)
            {
                _visual3D.Transform = Transform3D.Identity;
                return;
            }

            _visual3D.Transform = new MatrixTransform3D(projection.ToMatrix3D(contentSize));
        }

        /// <summary>
        /// Hosts the projected content inside the 3D scene and paints a
        /// transparent rectangle over the whole layout slot.
        /// </summary>
        /// <remarks>
        /// <see cref="Viewport2DVisual3D"/> maps the projected visual's painted
        /// bounds onto the quad. Content that does not paint its whole layout
        /// slot (a left-aligned <see cref="CheckBox"/>, a short
        /// <see cref="TextBlock"/>, ...) would be stretched to fill the plane,
        /// so the surface paints a transparent rectangle of the slot size to
        /// keep the mapping one-to-one.
        /// </remarks>
        private sealed class ProjectedSurface : FrameworkElement
        {
            private readonly VisualCollection _visuals;
            private UIElement _content;

            public ProjectedSurface()
            {
                _visuals = new VisualCollection(this);
            }

            public UIElement Content
            {
                get
                {
                    return _content;
                }

                set
                {
                    if (ReferenceEquals(_content, value))
                    {
                        return;
                    }

                    if (_content != null)
                    {
                        _visuals.Remove(_content);
                    }

                    _content = value;

                    if (value != null)
                    {
                        _visuals.Add(value);
                    }

                    InvalidateMeasure();
                    InvalidateArrange();
                }
            }

            protected override int VisualChildrenCount => _content != null ? 1 : 0;

            protected override Visual GetVisualChild(int index)
            {
                if (index != 0 || _content == null)
                {
                    throw new ArgumentOutOfRangeException("index");
                }

                return _content;
            }

            protected override Size MeasureOverride(Size availableSize)
            {
                UIElement content = _content;
                if (content == null)
                {
                    return new Size();
                }

                content.Measure(availableSize);
                return content.DesiredSize;
            }

            protected override Size ArrangeOverride(Size finalSize)
            {
                if (_content != null)
                {
                    _content.Arrange(new Rect(finalSize));
                }

                return finalSize;
            }

            protected override void OnRender(DrawingContext drawingContext)
            {
                drawingContext.DrawRectangle(Brushes.Transparent, null, new Rect(RenderSize));
            }
        }
    }
}
