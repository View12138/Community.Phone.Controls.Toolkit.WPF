// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows.Media.Media3D;

namespace Microsoft.Phone.Controls
{
    internal static class ProjectionMath
    {
        public static Matrix3D CreateTranslation(double x, double y, double z)
        {
            return new Matrix3D(
                1, 0, 0, 0,
                0, 1, 0, 0,
                0, 0, 1, 0,
                x, y, z, 1);
        }

        public static Matrix3D CreateScale(double x, double y, double z)
        {
            return new Matrix3D(
                x, 0, 0, 0,
                0, y, 0, 0,
                0, 0, z, 0,
                0, 0, 0, 1);
        }

        public static Matrix3D CreateRotationMatrix(double rotationX, double rotationY, double rotationZ)
        {
            Matrix3D result = Matrix3D.Identity;
            result = Multiply(result, CreateRotationX(rotationX));
            result = Multiply(result, CreateRotationY(rotationY));
            result = Multiply(result, CreateRotationZ(rotationZ));
            return result;
        }

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
        /// Multiplies two row-vector matrices (apply <paramref name="first"/>
        /// followed by <paramref name="second"/>).
        /// </summary>
        public static Matrix3D Multiply(Matrix3D first, Matrix3D second)
        {
            return Matrix3D.Multiply(first, second);
        }
    }
}
