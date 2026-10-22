using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using System.Windows.Threading;
using Microsoft.Phone.Controls;
using Xunit;

namespace Community.Phone.Controls.Toolkit.WPF.Tests
{
    public class ProjectionTests
    {
        [Fact]
        public void PlaneProjection_HasSilverlightDefaults()
        {
            StaTest.Run(() =>
            {
                PlaneProjection projection = new PlaneProjection();

                Assert.Equal(0.5, projection.CenterOfRotationX);
                Assert.Equal(0.5, projection.CenterOfRotationY);
                Assert.Equal(0.0, projection.CenterOfRotationZ);
                Assert.Equal(0.0, projection.RotationX);
                Assert.Equal(0.0, projection.RotationY);
                Assert.Equal(0.0, projection.RotationZ);
                Assert.Equal(0.0, projection.GlobalOffsetX);
                Assert.Equal(0.0, projection.LocalOffsetX);
                Assert.True(projection.IsIdentity);
            });
        }

        [Fact]
        public void PlaneProjection_DependencyPropertiesRoundTrip()
        {
            StaTest.Run(() =>
            {
                PlaneProjection projection = new PlaneProjection();

                projection.RotationY = -35;
                projection.GlobalOffsetZ = 120;
                projection.LocalOffsetX = 42;

                Assert.Equal(-35, projection.RotationY);
                Assert.Equal(120, projection.GlobalOffsetZ);
                Assert.Equal(42, projection.LocalOffsetX);
                Assert.False(projection.IsIdentity);
            });
        }

        [Fact]
        public void PlaneProjection_IdentityMatrixLeavesCenterAndCornersUnchanged()
        {
            StaTest.Run(() =>
            {
                PlaneProjection projection = new PlaneProjection();
                Matrix3D matrix = projection.ToMatrix3D(new Size(200, 100));

                Point3D center = matrix.Transform(new Point3D(0, 0, 0));
                Point3D topLeft = matrix.Transform(new Point3D(-100, 50, 0));

                Assert.Equal(0, center.X, 6);
                Assert.Equal(0, center.Y, 6);
                Assert.Equal(0, center.Z, 6);
                Assert.Equal(-100, topLeft.X, 6);
                Assert.Equal(50, topLeft.Y, 6);
                Assert.Equal(0, topLeft.Z, 6);
            });
        }

        [Fact]
        public void PlaneProjection_RotationYMatchesTheXamlDepthDirection()
        {
            StaTest.Run(() =>
            {
                Size size = new Size(200, 100);

                // XAML semantics: a positive RotationY brings the right edge
                // towards the viewer (+Z), a negative value pushes it away.
                // The WP turnstile depends on this: the incoming page starts
                // at -80 as a foreshortened sliver while the outgoing page
                // swings out towards the viewer at +50.
                PlaneProjection entry = new PlaneProjection
                {
                    CenterOfRotationX = 0,
                    RotationY = -80
                };
                PlaneProjection exit = new PlaneProjection
                {
                    CenterOfRotationX = 0,
                    RotationY = 50
                };

                Matrix3D entryMatrix = entry.ToMatrix3D(size);
                Matrix3D exitMatrix = exit.ToMatrix3D(size);

                Point3D recedingEdge = entryMatrix.Transform(new Point3D(100, 0, 0));
                Point3D approachingEdge = exitMatrix.Transform(new Point3D(100, 0, 0));
                Point3D pivotEdge = entryMatrix.Transform(new Point3D(-100, 0, 0));

                Assert.True(Math.Abs(recedingEdge.Z) > 100, "The rotated edge should move in depth.");
                Assert.True(recedingEdge.Z < 0, "A negative RotationY must push the free edge away from the viewer; Z=" + recedingEdge.Z);
                Assert.True(approachingEdge.Z > 0, "A positive RotationY must bring the free edge towards the viewer; Z=" + approachingEdge.Z);
                Assert.Equal(0, pivotEdge.Z, 6);
            });
        }

        [Fact]
        public void PlaneProjection_RotationXAndZRespectTheCoordinateSystem()
        {
            StaTest.Run(() =>
            {
                PlaneProjection rotateX = new PlaneProjection { RotationX = 90 };
                Matrix3D xMatrix = rotateX.ToMatrix3D(new Size(200, 100));
                Point3D top = xMatrix.Transform(new Point3D(0, 50, 0));
                Point3D bottom = xMatrix.Transform(new Point3D(0, -50, 0));

                Assert.Equal(0, top.Y, 6);
                Assert.Equal(50, top.Z, 6);
                Assert.Equal(0, bottom.Y, 6);
                Assert.Equal(-50, bottom.Z, 6);

                PlaneProjection rotateZ = new PlaneProjection { RotationZ = 90 };
                Matrix3D zMatrix = rotateZ.ToMatrix3D(new Size(200, 100));
                Point3D right = zMatrix.Transform(new Point3D(10, 0, 0));
                Point3D up = zMatrix.Transform(new Point3D(0, 10, 0));

                Assert.Equal(0, right.X, 6);
                Assert.Equal(10, right.Y, 6);
                Assert.Equal(-10, up.X, 6);
                Assert.Equal(0, up.Y, 6);
            });
        }

        [Fact]
        public void PlaneProjection_CenterOfRotationMovesThePivot()
        {
            StaTest.Run(() =>
            {
                Size size = new Size(200, 100);

                PlaneProjection leftPivot = new PlaneProjection
                {
                    CenterOfRotationX = 0,
                    RotationY = 90
                };
                Matrix3D leftMatrix = leftPivot.ToMatrix3D(size);
                Point3D leftEdge = leftMatrix.Transform(new Point3D(-100, 0, 0));
                Point3D rightEdge = leftMatrix.Transform(new Point3D(100, 0, 0));

                Assert.Equal(-100, leftEdge.X, 6);
                Assert.Equal(0, leftEdge.Z, 6);
                Assert.Equal(-100, rightEdge.X, 6);
                Assert.Equal(200, rightEdge.Z, 6);

                PlaneProjection rightPivot = new PlaneProjection
                {
                    CenterOfRotationX = 1,
                    RotationY = 90
                };
                Matrix3D rightMatrix = rightPivot.ToMatrix3D(size);
                Point3D fixedRight = rightMatrix.Transform(new Point3D(100, 0, 0));
                Assert.Equal(100, fixedRight.X, 6);
                Assert.Equal(0, fixedRight.Z, 6);
            });
        }

        [Fact]
        public void PlaneProjection_LocalOffsetIsRotatedButGlobalOffsetIsNot()
        {
            StaTest.Run(() =>
            {
                Size size = new Size(200, 100);

                PlaneProjection local = new PlaneProjection
                {
                    RotationY = 90,
                    LocalOffsetX = 10
                };
                Point3D localPoint = local.ToMatrix3D(size).Transform(new Point3D(0, 0, 0));
                Assert.Equal(0, localPoint.X, 6);
                Assert.Equal(10, localPoint.Z, 6);

                PlaneProjection global = new PlaneProjection
                {
                    RotationY = 90,
                    GlobalOffsetX = 10,
                    GlobalOffsetY = 5,
                    GlobalOffsetZ = 7
                };
                Point3D globalPoint = global.ToMatrix3D(size).Transform(new Point3D(0, 0, 0));
                Assert.Equal(10, globalPoint.X, 6);
                Assert.Equal(-5, globalPoint.Y, 6);
                Assert.Equal(7, globalPoint.Z, 6);
            });
        }

        [Fact]
        public void PlaneProjection_UsesElementSizeForCentersAndScales()
        {
            StaTest.Run(() =>
            {
                PlaneProjection projection = new PlaneProjection
                {
                    CenterOfRotationX = 0.5,
                    CenterOfRotationY = 0.5,
                    RotationZ = 45
                };

                Point3D smallCenter = projection.ToMatrix3D(new Size(100, 50)).Transform(new Point3D(0, 0, 0));
                Point3D largeCenter = projection.ToMatrix3D(new Size(400, 200)).Transform(new Point3D(0, 0, 0));
                Assert.Equal(0, smallCenter.X, 6);
                Assert.Equal(0, smallCenter.Y, 6);
                Assert.Equal(0, largeCenter.X, 6);
                Assert.Equal(0, largeCenter.Y, 6);

                Point3D smallCorner = projection.ToMatrix3D(new Size(100, 50)).Transform(new Point3D(50, 25, 0));
                Point3D largeCorner = projection.ToMatrix3D(new Size(400, 200)).Transform(new Point3D(200, 100, 0));
                Assert.Equal(smallCorner.X * 4, largeCorner.X, 4);
                Assert.Equal(smallCorner.Y * 4, largeCorner.Y, 4);
            });
        }

        [Fact]
        public void Matrix3DProjection_TranslatesInNormalizedCoordinates()
        {
            StaTest.Run(() =>
            {
                Matrix3D translation = new Matrix3D(
                    1, 0, 0, 0,
                    0, 1, 0, 0,
                    0, 0, 1, 0,
                    0.1, 0.2, 0, 1);

                Matrix3DProjection projection = new Matrix3DProjection
                {
                    ProjectionMatrix = translation
                };

                Point3D transformed = projection.ToMatrix3D(new Size(200, 100)).Transform(new Point3D(0, 0, 0));
                Assert.Equal(20, transformed.X, 6);
                Assert.Equal(20, transformed.Y, 6);
                Assert.False(projection.IsIdentity);
            });
        }

        [Fact]
        public void ProjectionHost_UpdatesTheSceneWhenTheProjectionIsAnimated()
        {
            StaTest.Run(() =>
            {
                Border content = new Border { Background = Brushes.Red, Width = 200, Height = 100 };
                ProjectionHost host = new ProjectionHost
                {
                    Width = 200,
                    Height = 100,
                    Content = content
                };
                PlaneProjection projection = new PlaneProjection { RotationY = 1 };
                host.Projection = projection;

                Window window = new Window { Width = 260, Height = 160, Content = host };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    Matrix3D before = GetProjectedTransform(host).Matrix;
                    projection.BeginAnimation(
                        PlaneProjection.RotationYProperty,
                        new DoubleAnimation
                        {
                            From = 0,
                            To = -80,
                            Duration = TimeSpan.FromMilliseconds(300),
                            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseIn }
                        });

                    PumpFor(150);
                    Matrix3D during = GetProjectedTransform(host).Matrix;
                    Assert.False(AreClose(before, during), "The projected transform should change while the animation runs.");

                    PumpFor(400);
                    Assert.Equal(-80, projection.RotationY, 2);
                }
                finally
                {
                    projection.BeginAnimation(PlaneProjection.RotationYProperty, null);
                    window.Close();
                }
            });
        }

        [Fact]
        public void Storyboard_AnimatesProjectionTarget()
        {
            StaTest.Run(() =>
            {
                Border content = new Border { Background = Brushes.Red, Width = 200, Height = 100 };
                ProjectionHost host = new ProjectionHost
                {
                    Width = 200,
                    Height = 100,
                    Content = content
                };
                PlaneProjection projection = new PlaneProjection();
                ProjectionService.SetProjection(content, projection);

                Window window = new Window { Width = 260, Height = 160, Content = host };
                Storyboard storyboard = null;
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    storyboard = new Storyboard();
                    DoubleAnimationUsingKeyFrames animation = new DoubleAnimationUsingKeyFrames
                    {
                        Duration = TimeSpan.FromMilliseconds(300)
                    };
                    animation.KeyFrames.Add(new EasingDoubleKeyFrame(0, KeyTime.FromTimeSpan(TimeSpan.Zero)));
                    animation.KeyFrames.Add(new EasingDoubleKeyFrame(50, KeyTime.FromTimeSpan(TimeSpan.FromMilliseconds(300))));
                    Storyboard.SetTarget(animation, content);
                    Storyboard.SetTargetProperty(
                        animation,
                        ProjectionService.GetProjectionPropertyPath(PlaneProjection.RotationYProperty));
                    storyboard.Children.Add(animation);

                    DoubleAnimation opacity = new DoubleAnimation(1, 0, TimeSpan.FromMilliseconds(300));
                    Storyboard.SetTarget(opacity, content);
                    Storyboard.SetTargetProperty(opacity, new PropertyPath(UIElement.OpacityProperty));
                    storyboard.Children.Add(opacity);

                    storyboard.Begin(content, true);

                    PumpFor(120);
                    Assert.True(content.Opacity < 0.9, "The storyboard clock should be ticking.");
                    Assert.NotEqual(0, projection.RotationY, 2);
                }
                finally
                {
                    if (storyboard != null)
                    {
                        storyboard.Stop(content);
                        storyboard.Remove(content);
                    }

                    window.Close();
                }
            });
        }

        [Fact]
        public void ProjectionHost_Enters3DWhenIdentityProjectionIsAnimated()
        {
            StaTest.Run(() =>
            {
                Border content = new Border { Background = Brushes.Orange, Width = 200, Height = 100 };
                ProjectionHost host = new ProjectionHost
                {
                    Width = 200,
                    Height = 100,
                    Content = content
                };
                PlaneProjection projection = new PlaneProjection();
                host.Projection = projection;

                Window window = new Window { Width = 260, Height = 160, Content = host };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    Assert.False(host.IsRenderingIn3D, "An identity projection should initially use the 2D path.");

                    projection.BeginAnimation(
                        PlaneProjection.RotationYProperty,
                        new DoubleAnimation
                        {
                            From = 0,
                            To = -45,
                            Duration = TimeSpan.FromMilliseconds(300)
                        });

                    PumpFor(100);

                    Assert.True(host.IsRenderingIn3D, "Animating an identity projection must switch the host to the 3D path.");
                    Assert.NotEqual(0, projection.RotationY, 2);
                    Assert.True(RenderNonTransparentPixels(host) > 0);
                }
                finally
                {
                    projection.BeginAnimation(PlaneProjection.RotationYProperty, null);
                    window.Close();
                }
            });
        }

        [Fact]
        public void ProjectionHost_RendersAtHighDpiWithoutChangingLayoutUnits()
        {
            StaTest.Run(() =>
            {
                ProjectionHost host = new ProjectionHost
                {
                    Width = 160,
                    Height = 80,
                    Content = new Border { Background = Brushes.Lime, Width = 160, Height = 80 },
                    Projection = new PlaneProjection { RotationY = -25 }
                };
                host.Measure(new Size(160, 80));
                host.Arrange(new Rect(0, 0, 160, 80));
                host.UpdateLayout();

                int baselinePixels = RenderNonTransparentPixels(host);
                RenderTargetBitmap bitmap = new RenderTargetBitmap(240, 120, 144, 144, PixelFormats.Pbgra32);
                bitmap.Render(host);

                Assert.Equal(240, bitmap.PixelWidth);
                Assert.Equal(120, bitmap.PixelHeight);
                Assert.True(baselinePixels > 0, "The baseline 96 DPI render should contain pixels.");
                Assert.True(CountPixels(bitmap) > 0, "The projected content should render at a non-96 DPI.");
            });
        }

        [Fact]
        public void ProjectionHost_RendersIndependentProjectionsForMultipleHosts()
        {
            StaTest.Run(() =>
            {
                ProjectionHost first = new ProjectionHost
                {
                    Width = 100,
                    Height = 80,
                    Content = new Border { Background = Brushes.Red },
                    Projection = new PlaneProjection { RotationY = -30 }
                };
                ProjectionHost second = new ProjectionHost
                {
                    Width = 100,
                    Height = 80,
                    Content = new Border { Background = Brushes.Blue },
                    Projection = new PlaneProjection { RotationX = 30 }
                };

                StackPanel panel = new StackPanel();
                panel.Children.Add(first);
                panel.Children.Add(second);
                panel.Measure(new Size(120, 200));
                panel.Arrange(new Rect(0, 0, 120, 200));
                panel.UpdateLayout();

                Assert.True(first.IsRenderingIn3D);
                Assert.True(second.IsRenderingIn3D);
                Assert.NotEqual(GetProjectedTransform(first).Matrix, GetProjectedTransform(second).Matrix);
                Assert.True(RenderNonTransparentPixels(first) > 0);
                Assert.True(RenderNonTransparentPixels(second) > 0);
            });
        }

        [Fact]
        public void ProjectionHost_KeepsProjectedContentFocusable()
        {
            StaTest.Run(() =>
            {
                Button button = new Button { Content = "Focus me", Width = 160, Height = 60 };
                ProjectionHost host = new ProjectionHost
                {
                    Width = 200,
                    Height = 100,
                    Content = button,
                    Projection = new PlaneProjection { RotationY = -12 }
                };

                Window window = new Window { Width = 300, Height = 200, Content = host };
                try
                {
                    window.Show();
                    window.Activate();
                    WpfTestHelper.PumpDispatcher();

                    bool focused = button.Focus();
                    Keyboard.Focus(button);
                    WpfTestHelper.PumpDispatcher();

                    Assert.True(focused || button.IsKeyboardFocusWithin, "The button inside the 3D host should accept focus.");
                    Assert.True(button.IsKeyboardFocusWithin);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ProjectionHost_RendersContentInTwoAndThreeDimensions()
        {
            StaTest.Run(() =>
            {
                Border content = new Border
                {
                    Background = Brushes.Red,
                    Width = 160,
                    Height = 80
                };

                ProjectionHost host = new ProjectionHost
                {
                    Width = 160,
                    Height = 80,
                    Content = content
                };

                host.Measure(new Size(160, 80));
                host.Arrange(new Rect(0, 0, 160, 80));
                host.UpdateLayout();

                Assert.False(host.IsRenderingIn3D);

                int flatPixels = RenderNonTransparentPixels(host);
                Assert.True(flatPixels > 0, "The 2D rendering should contain visible pixels.");

                host.Projection = new PlaneProjection { RotationY = -30 };
                host.UpdateLayout();

                Assert.True(host.IsRenderingIn3D);
                int projectedPixels = RenderNonTransparentPixels(host);
                Assert.True(projectedPixels > 0, "The 3D rendering should contain visible pixels.");

                host.Projection = new PlaneProjection();
                host.UpdateLayout();
                Assert.False(host.IsRenderingIn3D);
            });
        }

        [Fact]
        public void ProjectionHost_KeepsTheProjectedPlaneOneToOneWithTheLayoutSlot()
        {
            StaTest.Run(() =>
            {
                // A tall slot exposes a camera that derives its field of view
                // from the wrong axis: the projected page then shrinks to
                // width * (width / height) and snaps back when the transition
                // finishes.
                ProjectionHost host = new ProjectionHost
                {
                    Width = 200,
                    Height = 400,
                    Content = new Border { Background = Brushes.Red, Width = 200, Height = 400 },
                    Projection = new PlaneProjection { RotationX = 4 }
                };

                host.Measure(new Size(200, 400));
                host.Arrange(new Rect(0, 0, 200, 400));
                host.UpdateLayout();

                Assert.True(host.IsRenderingIn3D);

                RenderTargetBitmap bitmap = Render(host);
                Assert.InRange(GetWidestRowSpan(bitmap), 199, 200);
            });
        }

        [Fact]
        public void ProjectionHost_AppliesTheXamlPerspectiveDepth()
        {
            StaTest.Run(() =>
            {
                // A 480x800 page hinged at its left edge and rotated -80
                // degrees, which is the first frame of the WP turnstile entry
                // transition. The XAML perspective depth is 999 DIP, so the
                // free edge sits 480 * sin(80) = 472.7 DIP behind the hinge and
                // is scaled by 999 / (999 + 472.7) = 0.6788.
                const double Width = 480;
                const double Height = 800;
                const double Depth = 999;
                double radians = 80 * Math.PI / 180;
                double scale = Depth / (Depth + (Width * Math.Sin(radians)));

                // The perspective divide happens about the middle of the
                // host, not about the hinge, so the shifted rotation origin
                // participates in the projection.
                double freeEdgeX = (Width / 2) + (((Width * Math.Cos(radians)) - (Width / 2)) * scale);
                double freeEdgeHeight = Height * scale;

                ProjectionHost host = new ProjectionHost
                {
                    Width = Width,
                    Height = Height,
                    Content = new Border { Background = Brushes.Red, Width = Width, Height = Height },
                    Projection = new PlaneProjection
                    {
                        CenterOfRotationX = 0,
                        CenterOfRotationY = 0.5,
                        RotationY = -80
                    }
                };

                host.Measure(new Size(Width, Height));
                host.Arrange(new Rect(0, 0, Width, Height));
                host.UpdateLayout();

                Assert.True(host.IsRenderingIn3D);

                RenderTargetBitmap bitmap = Render(host);
                int rightmost = GetRightmostOccupiedColumn(bitmap);

                Assert.InRange(GetColumnExtent(bitmap, 0, 2), 796, 800);
                Assert.InRange(rightmost, (int)freeEdgeX - 3, (int)freeEdgeX + 3);
                Assert.InRange(
                    GetColumnExtent(bitmap, rightmost, rightmost),
                    (int)freeEdgeHeight - 6,
                    (int)freeEdgeHeight + 6);
            });
        }

        [Fact]
        public void ProjectionHost_PreservesTextureOrientation()
        {
            StaTest.Run(() =>
            {
                Grid content = new Grid { Width = 160, Height = 80 };
                content.RowDefinitions.Add(new RowDefinition());
                content.RowDefinitions.Add(new RowDefinition());
                Border top = new Border { Background = Brushes.Red };
                Border bottom = new Border { Background = Brushes.Blue };
                Grid.SetRow(bottom, 1);
                content.Children.Add(top);
                content.Children.Add(bottom);

                ProjectionHost host = new ProjectionHost
                {
                    Width = 160,
                    Height = 80,
                    Content = content,
                    Projection = new PlaneProjection { RotationX = 10 }
                };

                host.Measure(new Size(160, 80));
                host.Arrange(new Rect(0, 0, 160, 80));
                host.UpdateLayout();

                RenderTargetBitmap bitmap = Render(host);
                Color topColor = GetPixel(bitmap, 80, 12);
                Color bottomColor = GetPixel(bitmap, 80, 68);

                Assert.True(topColor.R > topColor.B, "The top of the source content must remain at the top.");
                Assert.True(bottomColor.B > bottomColor.R, "The bottom of the source content must remain at the bottom.");
            });
        }

        [Fact]
        public void ProjectionHost_ForwardsHitTestingToProjectedContent()
        {
            StaTest.Run(() =>
            {
                TextBlock text = new TextBlock { Text = "Press", Width = 160, Height = 80 };
                Border content = new Border { Background = Brushes.White, Child = text };
                ProjectionHost host = new ProjectionHost
                {
                    Width = 160,
                    Height = 80,
                    Content = content,
                    Projection = new PlaneProjection { RotationY = -15 }
                };

                host.Measure(new Size(160, 80));
                host.Arrange(new Rect(0, 0, 160, 80));
                host.UpdateLayout();

                Assert.True(host.IsRenderingIn3D);
                HitTestResult hit = VisualTreeHelper.HitTest(host, new Point(80, 40));
                Assert.NotNull(hit);
                Assert.True(
                    ReferenceEquals(hit.VisualHit, text)
                    || hit.VisualHit is TextBlock
                    || hit.VisualHit is Border
                    || hit.VisualHit is Viewport2DVisual3D,
                    "Unexpected hit visual: " + hit.VisualHit);
            });
        }

        [Fact]
        public void ProjectionService_WrapsAnElementThatReceivesAProjection()
        {
            StaTest.Run(() =>
            {
                StackPanel panel = new StackPanel { Width = 320, Height = 220 };
                Border content = new Border { Background = Brushes.Red, Width = 200, Height = 100 };
                panel.Children.Add(content);

                Window window = new Window { Width = 400, Height = 300, Content = panel };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();
                    panel.UpdateLayout();

                    Assert.Null(ProjectionService.GetProjectionHost(content));

                    PlaneProjection projection = new PlaneProjection { RotationY = 30 };
                    content.Projection = projection;
                    WpfTestHelper.PumpDispatcher();

                    ProjectionHost host = ProjectionService.GetProjectionHost(content);
                    Assert.NotNull(host);
                    Assert.Same(content, host.Content);
                    Assert.Same(projection, host.Projection);
                    Assert.True(host.IsRenderingIn3D);
                    Assert.Same(projection, content.Projection);

                    content.Projection = null;

                    Assert.Null(ProjectionService.GetProjection(content));
                    Assert.Same(panel, VisualTreeHelper.GetParent(content));
                    Assert.Null(host.Content);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ProjectionService_AppliesAProjectionDeclaredBeforeTheElementLoads()
        {
            StaTest.Run(() =>
            {
                StackPanel panel = new StackPanel { Width = 320, Height = 220 };
                Border content = new Border { Width = 200, Height = 100, Background = Brushes.Red };
                PlaneProjection projection = new PlaneProjection { RotationX = 20 };
                content.Projection = projection;

                Assert.Null(ProjectionService.GetProjectionHost(content));

                panel.Children.Add(content);
                Window window = new Window { Width = 400, Height = 300, Content = panel };
                try
                {
                    window.Show();
                    WpfTestHelper.PumpDispatcher();

                    ProjectionHost host = ProjectionService.GetProjectionHost(content);
                    Assert.NotNull(host);
                    Assert.Same(content, host.Content);
                    Assert.Same(projection, host.Projection);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ProjectionService_DoesNotProjectThePageHostOfAContainedElement()
        {
            StaTest.Run(() =>
            {
                PhoneApplicationPage page = new PhoneApplicationPage();
                StackPanel panel = new StackPanel { Margin = new Thickness(12) };
                Button button = new Button { Content = "Projected", Width = 200, Height = 60 };
                panel.Children.Add(button);
                page.Content = panel;

                TransitionFrame frame = new TransitionFrame { Width = 480, Height = 800 };
                Window window = new Window { Width = 520, Height = 860, Content = frame };
                try
                {
                    window.Show();
                    frame.Content = page;
                    PumpFor(400);

                    ProjectionHost pageHost = ProjectionService.GetProjectionHost(page);
                    Assert.NotNull(pageHost);
                    Assert.Null(pageHost.Projection);

                    PlaneProjection projection = new PlaneProjection { RotationY = 25 };
                    button.Projection = projection;
                    WpfTestHelper.PumpDispatcher();

                    ProjectionHost buttonHost = ProjectionService.GetProjectionHost(button);
                    Assert.NotNull(buttonHost);
                    Assert.NotSame(pageHost, buttonHost);
                    Assert.Same(projection, buttonHost.Projection);
                    Assert.Null(pageHost.Projection);
                }
                finally
                {
                    window.Close();
                }
            });
        }

        [Fact]
        public void ProjectionHost_DoesNotStretchContentThatPaintsLessThanItsSlot()
        {
            StaTest.Run(() =>
            {
                // A left-aligned 32x32 block inside a 200x50 slot: the control
                // paints only a strip of its slot. Viewport2DVisual3D maps the
                // painted bounds onto the quad, so the host must compensate or
                // the block would be stretched across the whole plane.
                Grid content = new Grid { Width = 200, Height = 50 };
                content.Children.Add(new Border
                {
                    Width = 32,
                    Height = 32,
                    Background = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Left
                });

                ProjectionHost host = new ProjectionHost { Width = 200, Height = 50, Content = content, Prefer3D = true };
                host.Measure(new Size(200, 50));
                host.Arrange(new Rect(0, 0, 200, 50));
                host.UpdateLayout();

                Assert.True(host.IsRenderingIn3D);

                RenderTargetBitmap bitmap = new RenderTargetBitmap(200, 50, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(host);

                int minX = int.MaxValue;
                int maxX = -1;
                for (int y = 0; y < 50; y++)
                {
                    for (int x = 0; x < 200; x++)
                    {
                        Color pixel = GetPixel(bitmap, x, y);
                        if (pixel.R > 200 && pixel.G > 200 && pixel.B > 200)
                        {
                            minX = Math.Min(minX, x);
                            maxX = Math.Max(maxX, x);
                        }
                    }
                }

                Assert.Equal(0, minX);
                Assert.Equal(31, maxX);
            });
        }

        [Fact]
        public void ProjectionHost_ProjectsTheLayoutSlotNotTheChildSize()
        {
            StaTest.Run(() =>
            {
                // The child is smaller than the host slot: the plane (and the
                // rotation centers) must be the slot, so the child keeps its
                // 2D size and position.
                Border content = new Border
                {
                    Width = 32,
                    Height = 32,
                    Background = Brushes.White,
                    HorizontalAlignment = HorizontalAlignment.Left
                };

                ProjectionHost host = new ProjectionHost { Width = 200, Height = 50, Content = content, Prefer3D = true };
                host.Measure(new Size(200, 50));
                host.Arrange(new Rect(0, 0, 200, 50));
                host.UpdateLayout();

                Assert.True(host.IsRenderingIn3D);

                RenderTargetBitmap bitmap = new RenderTargetBitmap(200, 50, 96, 96, PixelFormats.Pbgra32);
                bitmap.Render(host);

                int minX = int.MaxValue;
                int maxX = -1;
                int minY = int.MaxValue;
                int maxY = -1;
                for (int y = 0; y < 50; y++)
                {
                    for (int x = 0; x < 200; x++)
                    {
                        Color pixel = GetPixel(bitmap, x, y);
                        if (pixel.R > 200 && pixel.G > 200 && pixel.B > 200)
                        {
                            minX = Math.Min(minX, x);
                            maxX = Math.Max(maxX, x);
                            minY = Math.Min(minY, y);
                            maxY = Math.Max(maxY, y);
                        }
                    }
                }

                Assert.Equal(0, minX);
                Assert.Equal(31, maxX);
                Assert.Equal(9, minY);
                Assert.Equal(40, maxY);
            });
        }

        private static MatrixTransform3D GetProjectedTransform(ProjectionHost host)
        {
            Viewport3D viewport = VisualTreeHelper.GetChild(host, 0) as Viewport3D;
            Assert.NotNull(viewport);
            foreach (Visual3D visual in viewport.Children)
            {
                Viewport2DVisual3D plane = visual as Viewport2DVisual3D;
                if (plane != null)
                {
                    MatrixTransform3D transform = plane.Transform as MatrixTransform3D;
                    Assert.NotNull(transform);
                    return transform;
                }
            }

            throw new InvalidOperationException("The host did not contain a Viewport2DVisual3D.");
        }

        private static bool AreClose(Matrix3D first, Matrix3D second)
        {
            Point3D a = first.Transform(new Point3D(80, 30, 0));
            Point3D b = second.Transform(new Point3D(80, 30, 0));
            return (a - b).Length < 0.01;
        }

        private static void PumpFor(int milliseconds)
        {
            DateTime end = DateTime.UtcNow.AddMilliseconds(milliseconds);
            while (DateTime.UtcNow < end)
            {
                WpfTestHelper.PumpDispatcher();
                System.Threading.Thread.Sleep(15);
            }
        }

        private static int GetRightmostOccupiedColumn(RenderTargetBitmap bitmap)
        {
            int stride = bitmap.PixelWidth * 4;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            for (int x = bitmap.PixelWidth - 1; x >= 0; x--)
            {
                for (int y = 0; y < bitmap.PixelHeight; y++)
                {
                    if (pixels[(y * stride) + (x * 4) + 3] != 0)
                    {
                        return x;
                    }
                }
            }

            return -1;
        }

        private static int GetColumnExtent(RenderTargetBitmap bitmap, int fromX, int toX)
        {
            int stride = bitmap.PixelWidth * 4;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            int extent = 0;
            for (int x = Math.Max(0, fromX); x <= Math.Min(bitmap.PixelWidth - 1, toX); x++)
            {
                int top = -1;
                int bottom = -1;
                for (int y = 0; y < bitmap.PixelHeight; y++)
                {
                    if (pixels[(y * stride) + (x * 4) + 3] != 0)
                    {
                        if (top < 0)
                        {
                            top = y;
                        }

                        bottom = y;
                    }
                }

                if (top >= 0)
                {
                    extent = Math.Max(extent, bottom - top + 1);
                }
            }

            return extent;
        }

        private static int GetWidestRowSpan(RenderTargetBitmap bitmap)
        {
            int stride = bitmap.PixelWidth * 4;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            int widest = 0;
            for (int y = 0; y < bitmap.PixelHeight; y++)
            {
                int left = -1;
                int right = -1;
                for (int x = 0; x < bitmap.PixelWidth; x++)
                {
                    if (pixels[(y * stride) + (x * 4) + 3] != 0)
                    {
                        if (left < 0)
                        {
                            left = x;
                        }

                        right = x;
                    }
                }

                if (left >= 0)
                {
                    widest = Math.Max(widest, right - left + 1);
                }
            }

            return widest;
        }

        private static int CountPixels(RenderTargetBitmap bitmap)
        {
            int stride = bitmap.PixelWidth * 4;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);
            int count = 0;
            for (int i = 3; i < pixels.Length; i += 4)
            {
                if (pixels[i] != 0)
                {
                    count++;
                }
            }

            return count;
        }

        private static RenderTargetBitmap Render(FrameworkElement element)
        {
            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                Math.Max(1, (int)Math.Ceiling(element.ActualWidth)),
                Math.Max(1, (int)Math.Ceiling(element.ActualHeight)),
                96,
                96,
                PixelFormats.Pbgra32);
            bitmap.Render(element);
            return bitmap;
        }

        private static Color GetPixel(RenderTargetBitmap bitmap, int x, int y)
        {
            byte[] pixel = new byte[4];
            bitmap.CopyPixels(new Int32Rect(x, y, 1, 1), pixel, 4, 0);
            return Color.FromArgb(pixel[3], pixel[2], pixel[1], pixel[0]);
        }

        private static int RenderNonTransparentPixels(FrameworkElement element)

        {
            element.Measure(new Size(element.Width, element.Height));
            element.Arrange(new Rect(0, 0, element.Width, element.Height));
            element.UpdateLayout();

            RenderTargetBitmap bitmap = new RenderTargetBitmap(
                Math.Max(1, (int)Math.Ceiling(element.ActualWidth)),
                Math.Max(1, (int)Math.Ceiling(element.ActualHeight)),
                96,
                96,
                PixelFormats.Pbgra32);
            bitmap.Render(element);

            int stride = bitmap.PixelWidth * 4;
            byte[] pixels = new byte[stride * bitmap.PixelHeight];
            bitmap.CopyPixels(pixels, stride, 0);

            int count = 0;
            for (int i = 3; i < pixels.Length; i += 4)
            {
                if (pixels[i] != 0)
                {
                    count++;
                }
            }

            return count;
        }
    }
}
