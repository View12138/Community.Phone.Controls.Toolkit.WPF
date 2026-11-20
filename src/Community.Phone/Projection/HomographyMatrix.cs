// (c) Copyright Microsoft Corporation.
// This source is subject to the Microsoft Public License (Ms-PL).
// Please see http://go.microsoft.com/fwlink/?LinkID=131993 for details.
// All other rights reserved.

using System;
using System.Windows;

namespace Microsoft.Phone.Controls;

/// <summary>
/// A 3x3 projective transform (homography) with the same row-vector
/// convention as <see cref="System.Windows.Media.Media3D.Matrix3D"/>:
/// a point is transformed as <c>p' = (x, y, 1) * H</c> and the result is
/// divided by its third component.
/// </summary>
/// <remarks>
/// A homography is the exact 2D mapping of a planar 3D transform observed
/// through a perspective camera, so it describes a projected element
/// without approximating the perspective with a
/// <see cref="System.Windows.Media.MatrixTransform"/>.
/// </remarks>
public readonly struct HomographyMatrix : IEquatable<HomographyMatrix>
{
    private const double Epsilon = 1e-12;

    /// <summary>
    /// The identity transform.
    /// </summary>
    public static readonly HomographyMatrix Identity = new HomographyMatrix(
        1, 0, 0,
        0, 1, 0,
        0, 0, 1);

    private readonly double _m11;
    private readonly double _m12;
    private readonly double _m13;
    private readonly double _m21;
    private readonly double _m22;
    private readonly double _m23;
    private readonly double _m31;
    private readonly double _m32;
    private readonly double _m33;

    /// <summary>
    /// Initializes a new instance of the <see cref="HomographyMatrix"/>
    /// structure with the supplied row-major elements.
    /// </summary>
    public HomographyMatrix(
        double m11, double m12, double m13,
        double m21, double m22, double m23,
        double m31, double m32, double m33)
    {
        _m11 = m11;
        _m12 = m12;
        _m13 = m13;
        _m21 = m21;
        _m22 = m22;
        _m23 = m23;
        _m31 = m31;
        _m32 = m32;
        _m33 = m33;
    }

    /// <summary>Gets the first element of the first row.</summary>
    public double M11 => _m11;

    /// <summary>Gets the second element of the first row.</summary>
    public double M12 => _m12;

    /// <summary>Gets the third element of the first row.</summary>
    public double M13 => _m13;

    /// <summary>Gets the first element of the second row.</summary>
    public double M21 => _m21;

    /// <summary>Gets the second element of the second row.</summary>
    public double M22 => _m22;

    /// <summary>Gets the third element of the second row.</summary>
    public double M23 => _m23;

    /// <summary>Gets the first element of the third row.</summary>
    public double M31 => _m31;

    /// <summary>Gets the second element of the third row.</summary>
    public double M32 => _m32;

    /// <summary>Gets the third element of the third row.</summary>
    public double M33 => _m33;

    /// <summary>
    /// Gets the determinant of the matrix.
    /// </summary>
    public double Determinant =>
        (_m11 * ((_m22 * _m33) - (_m23 * _m32))) -
        (_m12 * ((_m21 * _m33) - (_m23 * _m31))) +
        (_m13 * ((_m21 * _m32) - (_m22 * _m31)));

    /// <summary>
    /// Gets a value indicating whether every element is a finite number.
    /// </summary>
    public bool IsFinite =>
        IsFiniteValue(_m11) && IsFiniteValue(_m12) && IsFiniteValue(_m13) &&
        IsFiniteValue(_m21) && IsFiniteValue(_m22) && IsFiniteValue(_m23) &&
        IsFiniteValue(_m31) && IsFiniteValue(_m32) && IsFiniteValue(_m33);

    /// <summary>
    /// Gets a value indicating whether the matrix is the identity.
    /// </summary>
    public bool IsIdentity =>
        Near(_m11, 1) && Near(_m12, 0) && Near(_m13, 0) &&
        Near(_m21, 0) && Near(_m22, 1) && Near(_m23, 0) &&
        Near(_m31, 0) && Near(_m32, 0) && Near(_m33, 1);

    /// <summary>
    /// Gets the three coefficients that produce one output component:
    /// the weights of the source X, the source Y and the constant term.
    /// </summary>
    /// <param name="output">
    /// 0 for the first output component, 1 for the second and 2 for the
    /// homogeneous weight.
    /// </param>
    public (double X, double Y, double Constant) GetOutputCoefficients(int output)
    {
        switch (output)
        {
            case 0:
                return (_m11, _m21, _m31);
            case 1:
                return (_m12, _m22, _m32);
            case 2:
                return (_m13, _m23, _m33);
            default:
                throw new ArgumentOutOfRangeException("output");
        }
    }

    /// <summary>
    /// Transforms a point, including the perspective divide.
    /// </summary>
    /// <param name="point">The point to transform.</param>
    /// <returns>
    /// The transformed point, or a point whose coordinates are
    /// <see cref="double.NaN"/> when the point maps to infinity.
    /// </returns>
    public Point Transform(Point point)
    {
        return TryTransform(point, out Point result) ? result : new Point(double.NaN, double.NaN);
    }

    /// <summary>
    /// Transforms a point, including the perspective divide.
    /// </summary>
    /// <param name="point">The point to transform.</param>
    /// <param name="result">The transformed point.</param>
    /// <returns>
    /// true when the homogeneous weight is non-zero and every resulting
    /// coordinate is finite; otherwise false.
    /// </returns>
    public bool TryTransform(Point point, out Point result)
    {
        double x = (_m11 * point.X) + (_m21 * point.Y) + _m31;
        double y = (_m12 * point.X) + (_m22 * point.Y) + _m32;
        double w = (_m13 * point.X) + (_m23 * point.Y) + _m33;

        if (!IsFiniteValue(w) || Math.Abs(w) <= Epsilon)
        {
            result = default;
            return false;
        }

        x /= w;
        y /= w;

        if (!IsFiniteValue(x) || !IsFiniteValue(y))
        {
            result = default;
            return false;
        }

        result = new Point(x, y);
        return true;
    }

    /// <summary>
    /// Calculates the inverse of the matrix.
    /// </summary>
    /// <param name="inverse">The inverted matrix.</param>
    /// <returns>
    /// true when the matrix is invertible and every element of the
    /// inverse is finite; otherwise false.
    /// </returns>
    public bool TryInvert(out HomographyMatrix inverse)
    {
        inverse = default;

        if (!IsFinite)
        {
            return false;
        }

        double a = _m11, b = _m12, c = _m13;
        double d = _m21, e = _m22, f = _m23;
        double g = _m31, h = _m32, i = _m33;

        double c00 = (e * i) - (f * h);
        double c01 = (f * g) - (d * i);
        double c02 = (d * h) - (e * g);

        double determinant = (a * c00) + (b * c01) + (c * c02);
        double scale = Math.Max(
            Math.Max(Math.Abs(a), Math.Abs(b)) + Math.Max(Math.Abs(c), Math.Abs(d)) +
            Math.Max(Math.Abs(e), Math.Abs(f)) + Math.Max(Math.Abs(g), Math.Abs(h)) + Math.Abs(i),
            1.0);

        if (!IsFiniteValue(determinant) || Math.Abs(determinant) <= Epsilon * scale * scale * scale)
        {
            return false;
        }

        double inv = 1.0 / determinant;

        HomographyMatrix result = new HomographyMatrix(
            c00 * inv,
            ((c * h) - (b * i)) * inv,
            ((b * f) - (c * e)) * inv,
            c01 * inv,
            ((a * i) - (c * g)) * inv,
            ((c * d) - (a * f)) * inv,
            c02 * inv,
            ((b * g) - (a * h)) * inv,
            ((a * e) - (b * d)) * inv);

        if (!result.IsFinite)
        {
            return false;
        }

        inverse = result;
        return true;
    }

    /// <summary>
    /// Transforms the four corners of a rectangle and returns their
    /// bounding box.
    /// </summary>
    /// <param name="bounds">The rectangle to transform.</param>
    /// <returns>
    /// The bounding box of the transformed corners, or
    /// <see cref="Rect.Empty"/> when a corner maps to infinity.
    /// </returns>
    public Rect TransformBounds(Rect bounds)
    {
        if (bounds.IsEmpty)
        {
            return Rect.Empty;
        }

        if (!TryTransform(new Point(bounds.Left, bounds.Top), out Point p0) ||
            !TryTransform(new Point(bounds.Right, bounds.Top), out Point p1) ||
            !TryTransform(new Point(bounds.Right, bounds.Bottom), out Point p2) ||
            !TryTransform(new Point(bounds.Left, bounds.Bottom), out Point p3))
        {
            return Rect.Empty;
        }

        double left = Math.Min(Math.Min(p0.X, p1.X), Math.Min(p2.X, p3.X));
        double top = Math.Min(Math.Min(p0.Y, p1.Y), Math.Min(p2.Y, p3.Y));
        double right = Math.Max(Math.Max(p0.X, p1.X), Math.Max(p2.X, p3.X));
        double bottom = Math.Max(Math.Max(p0.Y, p1.Y), Math.Max(p2.Y, p3.Y));

        if (!IsFiniteValue(left) || !IsFiniteValue(top) || !IsFiniteValue(right) || !IsFiniteValue(bottom))
        {
            return Rect.Empty;
        }

        return new Rect(left, top, right - left, bottom - top);
    }

    /// <summary>
    /// Multiplies this matrix by another matrix, applying this matrix
    /// first.
    /// </summary>
    /// <param name="other">The matrix to apply after this one.</param>
    public HomographyMatrix Multiply(HomographyMatrix other)
    {
        return new HomographyMatrix(
            (_m11 * other._m11) + (_m12 * other._m21) + (_m13 * other._m31),
            (_m11 * other._m12) + (_m12 * other._m22) + (_m13 * other._m32),
            (_m11 * other._m13) + (_m12 * other._m23) + (_m13 * other._m33),
            (_m21 * other._m11) + (_m22 * other._m21) + (_m23 * other._m31),
            (_m21 * other._m12) + (_m22 * other._m22) + (_m23 * other._m32),
            (_m21 * other._m13) + (_m22 * other._m23) + (_m23 * other._m33),
            (_m31 * other._m11) + (_m32 * other._m21) + (_m33 * other._m31),
            (_m31 * other._m12) + (_m32 * other._m22) + (_m33 * other._m32),
            (_m31 * other._m13) + (_m32 * other._m23) + (_m33 * other._m33));
    }

    /// <summary>
    /// Returns a copy of the matrix scaled so that
    /// <see cref="M33"/> is one when possible.
    /// </summary>
    public HomographyMatrix Normalize()
    {
        if (!IsFinite)
        {
            return this;
        }

        if (Math.Abs(_m33) > Epsilon && Math.Abs(_m33 - 1.0) > Epsilon)
        {
            double inv = 1.0 / _m33;
            return new HomographyMatrix(
                _m11 * inv, _m12 * inv, _m13 * inv,
                _m21 * inv, _m22 * inv, _m23 * inv,
                _m31 * inv, _m32 * inv, 1.0);
        }

        return this;
    }

    /// <summary>
    /// Returns the matrix scaled by -1, which represents the same
    /// projective transform with the opposite homogeneous sign.
    /// </summary>
    public HomographyMatrix Negate()
    {
        return new HomographyMatrix(
            -_m11, -_m12, -_m13,
            -_m21, -_m22, -_m23,
            -_m31, -_m32, -_m33);
    }

    /// <summary>Multiplies two matrices, applying <paramref name="first"/> first.</summary>
    public static HomographyMatrix operator *(HomographyMatrix first, HomographyMatrix second)
    {
        return first.Multiply(second);
    }

    /// <inheritdoc />
    public bool Equals(HomographyMatrix other)
    {
        return _m11.Equals(other._m11) && _m12.Equals(other._m12) && _m13.Equals(other._m13) &&
            _m21.Equals(other._m21) && _m22.Equals(other._m22) && _m23.Equals(other._m23) &&
            _m31.Equals(other._m31) && _m32.Equals(other._m32) && _m33.Equals(other._m33);
    }

    /// <inheritdoc />
    public override bool Equals(object obj)
    {
        return obj is HomographyMatrix other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        HashCode hash = new HashCode();
        hash.Add(_m11);
        hash.Add(_m12);
        hash.Add(_m13);
        hash.Add(_m21);
        hash.Add(_m22);
        hash.Add(_m23);
        hash.Add(_m31);
        hash.Add(_m32);
        hash.Add(_m33);
        return hash.ToHashCode();
    }

    /// <inheritdoc />
    public override string ToString()
    {
        return string.Format(
            System.Globalization.CultureInfo.InvariantCulture,
            "[[{0}, {1}, {2}], [{3}, {4}, {5}], [{6}, {7}, {8}]]",
            _m11, _m12, _m13, _m21, _m22, _m23, _m31, _m32, _m33);
    }

    private static bool Near(double value, double expected)
    {
        return Math.Abs(value - expected) <= 1e-9;
    }

    private static bool IsFiniteValue(double value)
    {
        return !double.IsNaN(value) && !double.IsInfinity(value);
    }

    public static bool operator ==(HomographyMatrix left, HomographyMatrix right)
    {
        return left.Equals(right);
    }

    public static bool operator !=(HomographyMatrix left, HomographyMatrix right)
    {
        return !(left == right);
    }
}
