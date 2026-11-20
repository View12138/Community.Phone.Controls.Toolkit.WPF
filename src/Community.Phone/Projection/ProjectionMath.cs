// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Microsoft.Phone.Controls
{
    /// <summary>
    /// Matrix helpers shared by the projection backends.
    /// </summary>
    public static class ProjectionMath
    {
        /// <summary>
        /// The fixed perspective depth used by the XAML <c>PlaneProjection</c>
        /// implementation, in device independent pixels.
        /// </summary>
        public const double DefaultPerspectiveDepth = 999.0;

        /// <summary>
        /// Creates a translation matrix using the row-vector convention.
        /// </summary>
        public static Matrix3D CreateTranslation(double x, double y, double z)
        {
            return new Matrix3D(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                x, y, z, 1);
        }

        /// <summary>
        /// Creates a scale matrix using the row-vector convention.
        /// </summary>
        public static Matrix3D CreateScale(double x, double y, double z)
        {
            return new Matrix3D(
                x, 0, 0, 0,
                0, y, 0, 0,
                0, 0, z, 0,
                0, 0, 0, 1);
        }

        /// <summary>
        /// Creates the combined X, Y and Z rotation matrix used by
        /// <see cref="PlaneProjection"/>.
        /// </summary>
        public static Matrix3D CreateRotationMatrix(double rotationX, double rotationY, double rotationZ)
        {
            Matrix3D result = Matrix3D.Identity;
            result *= CreateRotationX(rotationX);
            result *= CreateRotationY(rotationY);
            result *= CreateRotationZ(rotationZ);
            return result;
        }

        /// <summary>
        /// Creates a rotation around the X axis, in degrees.
        /// </summary>
        public static Matrix3D CreateRotationX(double degrees)
        {
            double radians = degrees * Math.PI / 180.0;
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);
            return new Matrix3D(
                1, 0, 0, 0,
                0, c, s, 0,
                0, -s, c, 0,
                0, 0, 0, 1);
        }

        /// <summary>
        /// Creates a rotation around the Y axis, in degrees.
        /// </summary>
        public static Matrix3D CreateRotationY(double degrees)
        {
            double radians = degrees * Math.PI / 180.0;
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);
            return new Matrix3D(
                c, 0, -s, 0,
                0, 1, 0, 0,
                s, 0, c, 0,
                0, 0, 0, 1);
        }

        /// <summary>
        /// Creates a rotation around the Z axis, in degrees.
        /// </summary>
        public static Matrix3D CreateRotationZ(double degrees)
        {
            double radians = degrees * Math.PI / 180.0;
            double c = Math.Cos(radians);
            double s = Math.Sin(radians);
            return new Matrix3D(
                c, s, 0, 0,
                -s, c, 0, 0,
                0, 0, 1, 0,
                0, 0, 0, 1);
        }

        /// <summary>
        /// Creates the 2D homography that maps a point of an element's own
        /// coordinate space (origin at the top left, Y pointing down) to the
        /// same point after <paramref name="projectionMatrix"/> has been
        /// applied in the element's centered, Y-up plane space and observed by
        /// the fixed perspective camera of the projection backend.
        /// </summary>
        /// <param name="projectionMatrix">
        /// The matrix returned by <see cref="Projection.ToMatrix3D(Size)"/>.
        /// </param>
        /// <param name="elementSize">The arranged size of the element.</param>
        /// <param name="perspectiveDepth">
        /// The camera distance, in device independent pixels.
        /// </param>
        /// <returns>
        /// The forward homography. Its determinant becomes zero when the plane
        /// is viewed edge-on.
        /// </returns>
        public static HomographyMatrix CreateHomography(Matrix3D projectionMatrix, Size elementSize, double perspectiveDepth)
        {
            double width = Math.Max(elementSize.Width, 0);
            double height = Math.Max(elementSize.Height, 0);
            double depth = Math.Max(1.0, perspectiveDepth);

            // Element space (origin top left, Y down, size WxH) to the plane
            // space used by the 3D scene: origin at the center, Y up.
            double halfWidth = width / 2.0;
            double halfHeight = height / 2.0;

            // The matrix is applied with the row-vector convention:
            // (X, Y, 0, 1) * M. Because the plane lives at Z = 0 and W = 1, the
            // four clip components are linear combinations of (X, Y, 1).
            double xToX = projectionMatrix.M11;
            double yToX = projectionMatrix.M21;
            double cToX = projectionMatrix.OffsetX;

            double xToY = projectionMatrix.M12;
            double yToY = projectionMatrix.M22;
            double cToY = projectionMatrix.OffsetY;

            double xToZ = projectionMatrix.M13;
            double yToZ = projectionMatrix.M23;
            double cToZ = projectionMatrix.OffsetZ;

            double xToW = projectionMatrix.M14;
            double yToW = projectionMatrix.M24;
            double cToW = projectionMatrix.M44;

            // A PerspectiveCamera at (0, 0, depth) looking down -Z with a
            // viewport of WxH and a horizontal field of view of
            // 2 * atan(W / (2 * depth)) maps the point (X', Y', Z') of the
            // plane to (W/2 + depth * X' / (depth - Z'),
            //     H/2 - depth * Y' / (depth - Z')).
            // Multiplying numerator and denominator by W' turns this into the
            // homogeneous form below, where clipW = depth * W' - Z'.
            //
            // Rows hold the source component, columns the destination
            // component, which is what the row-vector convention of
            // HomographyMatrix requires.
            double clipXByX = (depth * xToX) - (halfWidth * xToZ) + (halfWidth * depth * xToW);
            double clipXByY = (depth * yToX) - (halfWidth * yToZ) + (halfWidth * depth * yToW);
            double clipXByC = (depth * cToX) - (halfWidth * cToZ) + (halfWidth * depth * cToW);

            double clipYByX = (-depth * xToY) - (halfHeight * xToZ) + (halfHeight * depth * xToW);
            double clipYByY = (-depth * yToY) - (halfHeight * yToZ) + (halfHeight * depth * yToW);
            double clipYByC = (-depth * cToY) - (halfHeight * cToZ) + (halfHeight * depth * cToW);

            double clipWByX = -xToZ + (depth * xToW);
            double clipWByY = -yToZ + (depth * yToW);
            double clipWByC = -cToZ + (depth * cToW);

            HomographyMatrix plane = new HomographyMatrix(
                clipXByX, clipYByX, clipWByX,
                clipXByY, clipYByY, clipWByY,
                clipXByC, clipYByC, clipWByC);

            // Plane space to element space: X = x - W/2, Y = H/2 - y.
            HomographyMatrix localToPlane = new HomographyMatrix(
                1, 0, 0,
                0, -1, 0,
                -halfWidth, halfHeight, 1);

            HomographyMatrix homography = localToPlane.Multiply(plane).Normalize();

            // A homography is only defined up to a scale. Fix the sign so that
            // the homogeneous weight is positive at the element center, which
            // lets the shader test a single sign to drop everything behind the
            // camera.
            (double X, double Y, double Constant) weight = homography.GetOutputCoefficients(2);
            double centerWeight = (weight.X * halfWidth) + (weight.Y * halfHeight) + weight.Constant;

            return centerWeight < 0 ? homography.Negate() : homography;
        }

        /// <summary>
        /// Returns how far the plane of a projected element is in front of the
        /// camera, in device independent pixels.
        /// </summary>
        /// <param name="projectionMatrix">
        /// The matrix returned by <see cref="Projection.ToMatrix3D(Size)"/>.
        /// </param>
        /// <param name="perspectiveDepth">The camera distance.</param>
        /// <returns>
        /// A positive distance while the plane is in front of the camera, zero
        /// or a negative distance once it reaches or passes the camera.
        /// </returns>
        public static double GetPlaneDistance(Matrix3D projectionMatrix, double perspectiveDepth)
        {
            double depth = Math.Max(1.0, perspectiveDepth);
            Point3D center = projectionMatrix.Transform(new Point3D(0, 0, 0));
            return depth - center.Z;
        }
    }
}
