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
        /// <summary>
        /// Identifies the <see cref="Child"/> property.
        /// </summary>
        public static readonly DependencyProperty ChildProperty =
            DependencyProperty.Register(
                "Child",
                typeof(UIElement),
                typeof(ProjectionHost),
                new PropertyMetadata(null, OnChildChanged));

        /// <summary>
        /// Identifies the <see cref="Projection"/> property.
        /// </summary>
        public static readonly DependencyProperty ProjectionProperty =
            DependencyProperty.Register(
                "Projection",
                typeof(Projection),
                typeof(ProjectionHost),
                new PropertyMetadata(null, OnProjectionChanged));

        /// <summary>
        /// Identifies the <see cref="FieldOfView"/> property.
        /// </summary>
        public static readonly DependencyProperty FieldOfViewProperty =
            DependencyProperty.Register(
                "FieldOfView",
                typeof(double),
                typeof(ProjectionHost),
                new PropertyMetadata(57.6, OnFieldOfViewChanged));

        private const double NearPlaneDistance = 0.1;
        private const double FarPlaneDistance = 1000000.0;

        private readonly Viewport3D _viewport;
        private readonly Viewport2DVisual3D _visual3D;
        private readonly PerspectiveCamera _camera;
        private readonly MeshGeometry3D _mesh;
        private Projection _subscribedProjection;
        private bool _using3D;
        private Size _geometrySize = Size.Empty;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProjectionHost"/> class.
        /// </summary>
        public ProjectionHost()
        {
            _mesh = new MeshGeometry3D();
            _camera = new PerspectiveCamera
            {
                FieldOfView = FieldOfView,
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
        /// Gets or sets the content to project.
        /// </summary>
        public UIElement Child
        {
            get { return (UIElement)GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        /// <summary>
        /// Gets or sets the projection applied to the content.
        /// </summary>
        public Projection Projection
        {
            get { return (Projection)GetValue(ProjectionProperty); }
            set { SetValue(ProjectionProperty, value); }
        }

        /// <summary>
        /// Gets or sets the perspective camera field of view, in degrees.
        /// </summary>
        public double FieldOfView
        {
            get { return (double)GetValue(FieldOfViewProperty); }
            set { SetValue(FieldOfViewProperty, value); }
        }

        /// <summary>
        /// Gets a value indicating whether the content is currently rendered
        /// through the 3D pipeline.
        /// </summary>
        public bool IsRenderingIn3D
        {
            get { return _using3D; }
        }

        /// <inheritdoc />
        protected override int VisualChildrenCount
        {
            get
            {
                if (_using3D)
                {
                    return 1;
                }

                return Child == null ? 0 : 1;
            }
        }

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
                child.Arrange(new Rect(finalSize));
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
                    if (ReferenceEquals(_visual3D.Visual, oldChild))
                    {
                        _visual3D.Visual = null;
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

        private static void OnFieldOfViewChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ProjectionHost host = (ProjectionHost)d;
            host._camera.FieldOfView = Math.Max(1.0, Math.Min(179.0, (double)e.NewValue));
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
            Projection projection = Projection;
            return Child != null && projection != null && !projection.IsIdentity;
        }

        private void Ensure3D()
        {
            if (_using3D || Child == null)
            {
                return;
            }

            RemoveVisualChildIfNeeded();
            _visual3D.Visual = Child;
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

            if (ReferenceEquals(_visual3D.Visual, Child))
            {
                _visual3D.Visual = null;
            }

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

            Size contentSize = Child.RenderSize;
            if (contentSize.Width <= 0 || contentSize.Height <= 0)
            {
                contentSize = hostSize;
            }

            if (contentSize.Width <= 0 || contentSize.Height <= 0)
            {
                return;
            }

            UpdateGeometry(contentSize);
            UpdateCamera(hostSize, contentSize);
            UpdateTransform(contentSize);
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
            double viewportHeight = hostSize.Height > 0 ? hostSize.Height : contentSize.Height;
            double fovRadians = Math.Max(1.0, Math.Min(179.0, FieldOfView)) * Math.PI / 180.0;
            double distance = viewportHeight / (2.0 * Math.Tan(fovRadians / 2.0));

            if (distance <= NearPlaneDistance)
            {
                distance = NearPlaneDistance * 2.0;
            }

            _camera.Position = new Point3D(0, 0, distance);
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
    }
}
