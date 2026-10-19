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
        public void PlaneProjection_RotationYPushesRightEdgeInDepth()
        {
            StaTest.Run(() =>
            {
                PlaneProjection projection = new PlaneProjection
                {
                    CenterOfRotationX = 0,
                    RotationY = -80
                };

                Matrix3D matrix = projection.ToMatrix3D(new Size(200, 100));
                Point3D left = matrix.Transform(new Point3D(-100, 0, 0));
                Point3D right = matrix.Transform(new Point3D(100, 0, 0));

                Assert.True(Math.Abs(right.Z) > 100, "The rotated edge should move in depth.");
                Assert.True(Math.Abs(right.Z) > Math.Abs(left.Z), "The pivot edge should stay fixed in depth.");
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
                Assert.Equal(-200, rightEdge.Z, 6);

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
                Assert.Equal(-10, localPoint.Z, 6);

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
                    Child = content
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
        public void ProjectionHost_RendersAtHighDpiWithoutChangingLayoutUnits()
        {
            StaTest.Run(() =>
            {
                ProjectionHost host = new ProjectionHost
                {
                    Width = 160,
                    Height = 80,
                    Child = new Border { Background = Brushes.Lime, Width = 160, Height = 80 },
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
                    Child = new Border { Background = Brushes.Red },
                    Projection = new PlaneProjection { RotationY = -30 }
                };
                ProjectionHost second = new ProjectionHost
                {
                    Width = 100,
                    Height = 80,
                    Child = new Border { Background = Brushes.Blue },
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
                    Child = button,
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
                    Child = content
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
                    Child = content,
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
                    Child = content,
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
