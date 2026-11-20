// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;
using System.Windows.Media.Media3D;

namespace Microsoft.Phone.Controls;

/// <summary>
/// Represents a perspective projection of a plane in three-dimensional
/// space. This is a WPF implementation of the Silverlight
/// <c>PlaneProjection</c> API.
/// </summary>
public class PlaneProjection : Projection
{
    /// <summary>
    /// Identifies the <see cref="CenterOfRotationX"/> property.
    /// </summary>
    public static readonly DependencyProperty CenterOfRotationXProperty =
        DependencyProperty.Register("CenterOfRotationX", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.5, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="CenterOfRotationY"/> property.
    /// </summary>
    public static readonly DependencyProperty CenterOfRotationYProperty =
        DependencyProperty.Register("CenterOfRotationY", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.5, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="CenterOfRotationZ"/> property.
    /// </summary>
    public static readonly DependencyProperty CenterOfRotationZProperty =
        DependencyProperty.Register("CenterOfRotationZ", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.0, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="GlobalOffsetX"/> property.
    /// </summary>
    public static readonly DependencyProperty GlobalOffsetXProperty =
        DependencyProperty.Register("GlobalOffsetX", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.0, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="GlobalOffsetY"/> property.
    /// </summary>
    public static readonly DependencyProperty GlobalOffsetYProperty =
        DependencyProperty.Register("GlobalOffsetY", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.0, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="GlobalOffsetZ"/> property.
    /// </summary>
    public static readonly DependencyProperty GlobalOffsetZProperty =
        DependencyProperty.Register("GlobalOffsetZ", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.0, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="LocalOffsetX"/> property.
    /// </summary>
    public static readonly DependencyProperty LocalOffsetXProperty =
        DependencyProperty.Register("LocalOffsetX", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.0, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="LocalOffsetY"/> property.
    /// </summary>
    public static readonly DependencyProperty LocalOffsetYProperty =
        DependencyProperty.Register("LocalOffsetY", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.0, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="LocalOffsetZ"/> property.
    /// </summary>
    public static readonly DependencyProperty LocalOffsetZProperty =
        DependencyProperty.Register("LocalOffsetZ", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.0, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="RotationX"/> property.
    /// </summary>
    public static readonly DependencyProperty RotationXProperty =
        DependencyProperty.Register("RotationX", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.0, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="RotationY"/> property.
    /// </summary>
    public static readonly DependencyProperty RotationYProperty =
        DependencyProperty.Register("RotationY", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.0, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Identifies the <see cref="RotationZ"/> property.
    /// </summary>
    public static readonly DependencyProperty RotationZProperty =
        DependencyProperty.Register("RotationZ", typeof(double), typeof(PlaneProjection), new PropertyMetadata(0.0, (d, e) => ((PlaneProjection)d).OnProjectionChanged()));

    /// <summary>
    /// Gets or sets the x-coordinate of the center of rotation.
    /// </summary>
    public double CenterOfRotationX
    {
        get { return (double)GetValue(CenterOfRotationXProperty); }
        set { SetValue(CenterOfRotationXProperty, value); }
    }

    /// <summary>
    /// Gets or sets the y-coordinate of the center of rotation.
    /// </summary>
    public double CenterOfRotationY
    {
        get { return (double)GetValue(CenterOfRotationYProperty); }
        set { SetValue(CenterOfRotationYProperty, value); }
    }

    /// <summary>
    /// Gets or sets the z-coordinate of the center of rotation.
    /// </summary>
    public double CenterOfRotationZ
    {
        get { return (double)GetValue(CenterOfRotationZProperty); }
        set { SetValue(CenterOfRotationZProperty, value); }
    }

    /// <summary>
    /// Gets or sets the global x offset.
    /// </summary>
    public double GlobalOffsetX
    {
        get { return (double)GetValue(GlobalOffsetXProperty); }
        set { SetValue(GlobalOffsetXProperty, value); }
    }

    /// <summary>
    /// Gets or sets the global y offset.
    /// </summary>
    public double GlobalOffsetY
    {
        get { return (double)GetValue(GlobalOffsetYProperty); }
        set { SetValue(GlobalOffsetYProperty, value); }
    }

    /// <summary>
    /// Gets or sets the global z offset.
    /// </summary>
    public double GlobalOffsetZ
    {
        get { return (double)GetValue(GlobalOffsetZProperty); }
        set { SetValue(GlobalOffsetZProperty, value); }
    }

    /// <summary>
    /// Gets or sets the local x offset.
    /// </summary>
    public double LocalOffsetX
    {
        get { return (double)GetValue(LocalOffsetXProperty); }
        set { SetValue(LocalOffsetXProperty, value); }
    }

    /// <summary>
    /// Gets or sets the local y offset.
    /// </summary>
    public double LocalOffsetY
    {
        get { return (double)GetValue(LocalOffsetYProperty); }
        set { SetValue(LocalOffsetYProperty, value); }
    }

    /// <summary>
    /// Gets or sets the local z offset.
    /// </summary>
    public double LocalOffsetZ
    {
        get { return (double)GetValue(LocalOffsetZProperty); }
        set { SetValue(LocalOffsetZProperty, value); }
    }

    /// <summary>
    /// Gets or sets the rotation around the x-axis, in degrees.
    /// </summary>
    public double RotationX
    {
        get { return (double)GetValue(RotationXProperty); }
        set { SetValue(RotationXProperty, value); }
    }

    /// <summary>
    /// Gets or sets the rotation around the y-axis, in degrees.
    /// </summary>
    public double RotationY
    {
        get { return (double)GetValue(RotationYProperty); }
        set { SetValue(RotationYProperty, value); }
    }

    /// <summary>
    /// Gets or sets the rotation around the z-axis, in degrees.
    /// </summary>
    public double RotationZ
    {
        get { return (double)GetValue(RotationZProperty); }
        set { SetValue(RotationZProperty, value); }
    }

    /// <inheritdoc />
    public override bool IsIdentity =>
        RotationX == 0 && RotationY == 0 && RotationZ == 0
        && GlobalOffsetX == 0 && GlobalOffsetY == 0 && GlobalOffsetZ == 0
        && LocalOffsetX == 0 && LocalOffsetY == 0 && LocalOffsetZ == 0;

    /// <inheritdoc />
    public override Matrix3D ToMatrix3D(Size elementSize)
    {
        double width = Math.Max(elementSize.Width, 0);
        double height = Math.Max(elementSize.Height, 0);

        Point3D center = new Point3D((CenterOfRotationX - 0.5) * width, (0.5 - CenterOfRotationY) * height, CenterOfRotationZ);

        Vector3D localOffset = new Vector3D(LocalOffsetX, -LocalOffsetY, LocalOffsetZ);
        Vector3D globalOffset = new Vector3D(GlobalOffsetX, -GlobalOffsetY, GlobalOffsetZ);

        Matrix3D rotation = ProjectionMath.CreateRotationMatrix(RotationX, -RotationY, RotationZ);

        Matrix3D result = Matrix3D.Identity;
        result *= ProjectionMath.CreateTranslation(-center.X, -center.Y, -center.Z);
        result *= ProjectionMath.CreateTranslation(localOffset.X, localOffset.Y, localOffset.Z);
        result *= rotation;
        result *= ProjectionMath.CreateTranslation(center.X, center.Y, center.Z);
        result *= ProjectionMath.CreateTranslation(globalOffset.X, globalOffset.Y, globalOffset.Z);
        return result;
    }

    /// <inheritdoc />
    protected override Freezable CreateInstanceCore()
    {
        return new PlaneProjection();
    }
}
