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
    /// Applies an arbitrary <see cref="Matrix3D"/> projection to a
    /// <see cref="ProjectionHost"/>.
    /// </summary>
    /// <remarks>
    /// The matrix is interpreted in the normalized (0..1) coordinate space used
    /// by the Silverlight <c>Matrix3DProjection</c> type. It is converted to
    /// pixel space by the host.
    /// </remarks>
    public class Matrix3DProjection : Projection
    {
        /// <summary>
        /// Identifies the <see cref="ProjectionMatrix"/> property.
        /// </summary>
        public static readonly DependencyProperty ProjectionMatrixProperty =
            DependencyProperty.Register(
                "ProjectionMatrix",
                typeof(Matrix3D),
                typeof(Matrix3DProjection),
                new PropertyMetadata(Matrix3D.Identity, OnProjectionMatrixChanged));

        /// <summary>
        /// Gets or sets the projection matrix.
        /// </summary>
        public Matrix3D ProjectionMatrix
        {
            get { return (Matrix3D)GetValue(ProjectionMatrixProperty); }
            set { SetValue(ProjectionMatrixProperty, value); }
        }

        /// <inheritdoc />
        public override bool IsIdentity
        {
            get { return ProjectionMatrix.IsIdentity; }
        }

        /// <inheritdoc />
        public override Matrix3D ToMatrix3D(Size elementSize)
        {
            double width = Math.Max(elementSize.Width, 1);
            double height = Math.Max(elementSize.Height, 1);

            Matrix3D normalize = ProjectionMath.CreateScale(1.0 / width, 1.0 / height, 1.0);
            Matrix3D denormalize = ProjectionMath.CreateScale(width, height, 1.0);

            Matrix3D result = Matrix3D.Identity;
            result = ProjectionMath.Multiply(result, normalize);
            result = ProjectionMath.Multiply(result, ProjectionMatrix);
            result = ProjectionMath.Multiply(result, denormalize);
            return result;
        }

        private static void OnProjectionMatrixChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((Matrix3DProjection)d).OnProjectionChanged();
        }

        /// <inheritdoc />
        protected override Freezable CreateInstanceCore()
        {
            return new Matrix3DProjection();
        }
    }
}
