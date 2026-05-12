using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Numerics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Core.Vectors;

public readonly struct Vector3D : IEquatable<Vector3D>
{
    public static Vector3D Zero { get; } = new Vector3D(0, 0, 0);
    public static Vector3D XAxis { get; } = new Vector3D(1, 0, 0);
    public static Vector3D YAxis { get; } = new Vector3D(0, 1, 0);
    public static Vector3D ZAxis { get; } = new Vector3D(0, 0, 1);
    public static Vector3D[] Axes { get; } = [XAxis, YAxis, ZAxis];

    public double X { get; }
    public double Y { get; }
    public double Z { get; }

    public Vector3D(double x, double y, double z)
    {
        X = x;
        Y = y;
        Z = z;
    }
    public Vector3D(Vector2D vec, double z)
    {
        X = vec.X;
        Y = vec.Y;
        Z = z;
    }
    public Vector3D(ReadOnlySpan<double> arr)
    {
#if DEBUG
        if (arr.Length != 3) throw new ArgumentException();
#endif
        X = arr[0];
        Y = arr[1];
        Z = arr[2];
    }
    public double Distance(Vector3D b) => Distance(this, b);

    public double SqrDistance(Vector3D b) => SqrDistance(this, b);
    public void Deconstruct(out double x, out double y, out double z)
        => (x, y, z) = (X, Y, Z);

    public double this[int k]
    {
        get
        {
            return k switch
            {
                0 => X,
                1 => Y,
                2 => Z,
                _ => throw new Exception("get: Vector3D out of range"),
            };
        }
    }
    public ReadOnlySpan<double> AsSpan() => MemoryMarshal.Cast<Vector3D, double>(MemoryMarshal.CreateReadOnlySpan(in this, 1));
    //public double[] AsArray() => [X, Y, Z];
    public Vector2D As2D() => new Vector2D(X, Y);

    public double Norm => Math.Sqrt(NormSqr);
    public double NormSqr => X * X + Y * Y + Z * Z;

    public double MaxNorm => Math.Max(Math.Abs(X), Math.Max(Math.Abs(Y), Math.Abs(Z)));

    public Vector3D Projection(Vector3D p) => (this * p) * p;

    public Vector3D Normalize() => this / Norm;

    public Vector3D Round(int digits) => new Vector3D(Math.Round(X, digits), Math.Round(Y, digits), Math.Round(Z, digits));

    public override string ToString() => $"Vec({X}, {Y}, {Z})";

    public override bool Equals(object? obj) => obj is Vector3D v && Equals(v);

    public override int GetHashCode() => HashCode.Combine(X, Y, Z);

    public bool Equals(Vector3D a) => a.X == X && a.Y == Y && a.Z == Z;

    public static bool TryParse(string line, out Vector3D res)
    {
        double x, y, z;
        var words = line.Split([' ', '\t', ',', '(', ')', '<', '>'], StringSplitOptions.RemoveEmptyEntries);
        if (words[0] == "Vec")
        {
            if (words.Length != 4 || !double.TryParse(words[1], out x) || !double.TryParse(words[2], out y)
                || !double.TryParse(words[3], out z))
            {
                res = Zero;
                return false;
            }
            res = new Vector3D(x, y, z);
            return true;
        }
        if (words.Length != 3 || !double.TryParse(words[0], out x) || !double.TryParse(words[1], out y)
            || !double.TryParse(words[2], out z))
        {
            res = Zero;
            return false;
        }

        res = new Vector3D(x, y, z);
        return true;
    }
    public static Vector3D Vec(double x, double y, double z) => new Vector3D(x, y, z);

    public static Vector3D Parse(string line)
    {
        Vector3D res;
        if (!TryParse(line, out res))
            throw new FormatException($"Can't parse Vector3D from {line}");
        return res;
    }

    #region Static operators

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator -(Vector3D a) => new Vector3D(-a.X, -a.Y, -a.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator +(Vector3D a) => a;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double operator *(Vector3D a, Vector3D b) => a.X * b.X + a.Y * b.Y + a.Z * b.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator *(double a, Vector3D b) => new Vector3D(a * b.X, a * b.Y, a * b.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator *(Vector3D b, double a) => new Vector3D(a * b.X, a * b.Y, a * b.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator /(Vector3D a, double v) => new Vector3D(a.X / v, a.Y / v, a.Z / v);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator +(Vector3D a, Vector3D b) => new Vector3D(a.X + b.X, a.Y + b.Y, a.Z + b.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D operator -(Vector3D a, Vector3D b) => new Vector3D(a.X - b.X, a.Y - b.Y, a.Z - b.Z);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator ==(Vector3D a, Vector3D b) => a.X == b.X && a.Y == b.Y && a.Z == b.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool operator !=(Vector3D a, Vector3D b) => a.X != b.X || a.Y != b.Y || a.Z != b.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Cross(Vector3D v1, Vector3D v2) =>
        new Vector3D(v1.Y * v2.Z - v2.Y * v1.Z, v1.Z * v2.X - v1.X * v2.Z, v1.X * v2.Y - v1.Y * v2.X);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Mixed(Vector3D v1, Vector3D v2, Vector3D v3) =>
        (v1.Y * v2.Z - v2.Y * v1.Z) * v3.X + (v1.Z * v2.X - v1.X * v2.Z) * v3.Y + (v1.X * v2.Y - v1.Y * v2.X) * v3.Z;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Sum(Vector3D a, Vector3D b) => a + b;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Min(Vector3D a, Vector3D b) =>
        new Vector3D(Math.Min(a.X, b.X), Math.Min(a.Y, b.Y), Math.Min(a.Z, b.Z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static Vector3D Max(Vector3D a, Vector3D b) =>
        new Vector3D(Math.Max(a.X, b.X), Math.Max(a.Y, b.Y), Math.Max(a.Z, b.Z));

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double Distance(Vector3D a, Vector3D b) => (a - b).Norm;

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static double SqrDistance(Vector3D a, Vector3D b)
    {
        var diff = a - b;
        return diff * diff;
    }
    #endregion

    #region EqualityComparer

    private class EqualityComparer : IEqualityComparer<Vector3D>
    {
        public int Digits { get; set; }

        public bool Equals(Vector3D v1, Vector3D v2)
        {
            return v1.Round(Digits) == v2.Round(Digits);
        }

        public int GetHashCode(Vector3D obj)
        {
            return obj.Round(Digits).GetHashCode();
        }
    }

    public static IEqualityComparer<Vector3D> CreateComparer(int digits = 7)
    {
        return new EqualityComparer { Digits = digits };
    }
    #endregion

    public static Vector3D Abs(Vector3D value) => throw new NotSupportedException();
    public static bool IsCanonical(Vector3D value) => true;
    public static bool IsComplexNumber(Vector3D value) => false;
    public static bool IsEvenInteger(Vector3D value) => false;
    public static bool IsFinite(Vector3D value) => double.IsFinite(value.X) && double.IsFinite(value.Y) && double.IsFinite(value.Z);
    public static bool IsImaginaryNumber(Vector3D value) => false;
    public static bool IsInfinity(Vector3D value) => double.IsInfinity(value.X) || double.IsInfinity(value.Y) || double.IsInfinity(value.Z);
    public static bool IsInteger(Vector3D value) => false;
    public static bool IsNaN(Vector3D value) => double.IsNaN(value.X) || double.IsNaN(value.Y) || double.IsNaN(value.Z);
    public static bool IsNegative(Vector3D value) => false;
    public static bool IsNegativeInfinity(Vector3D value) => false;
    public static bool IsNormal(Vector3D value) => value.Norm == 1.0;
    public static bool IsOddInteger(Vector3D value) => false;
    public static bool IsPositive(Vector3D value) => false;
    public static bool IsPositiveInfinity(Vector3D value) => false;
    public static bool IsRealNumber(Vector3D value) => false;
    public static bool IsSubnormal(Vector3D value) => false;
    public static bool IsZero(Vector3D value) => value == default;
    public static Vector3D MaxMagnitude(Vector3D x, Vector3D y) => MaxMagnitudeNumber(x, y);
    public static Vector3D MaxMagnitudeNumber(Vector3D x, Vector3D y) => x.X > y.X || (x.X == y.X && x.Y > y.Y) || (x.X == y.X && x.Y == y.Y && x.Z > y.Z) ? x : y;
    public static Vector3D MinMagnitude(Vector3D x, Vector3D y) => MinMagnitudeNumber(x, y);
    public static Vector3D MinMagnitudeNumber(Vector3D x, Vector3D y) => x.X < y.X || (x.X == y.X && x.Y < y.Y) || (x.X == y.X && x.Y == y.Y && x.Z < y.Z) ? x : y;
    public static Vector3D Parse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider) => Parse(s.ToString(), style, provider);
    public static Vector3D Parse(string s, NumberStyles style, IFormatProvider? provider) => Parse(s);
    public static bool TryParse(ReadOnlySpan<char> s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector3D result) => TryParse(s.ToString(), style, provider, out result);
    public static bool TryParse([NotNullWhen(true)] string? s, NumberStyles style, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector3D result) => TryParse(s!, out result);

    public static Vector3D One => throw new NotSupportedException();
    public static int Radix => throw new NotSupportedException();

    public bool TryFormat(Span<char> destination, out int charsWritten, ReadOnlySpan<char> format, IFormatProvider? provider)
    {
        var s = ToString();
        charsWritten = s.Length;
        return s.AsSpan().TryCopyTo(destination);
    }
    public string ToString(string? format, IFormatProvider? formatProvider) => ToString();
    public static Vector3D Parse(ReadOnlySpan<char> s, IFormatProvider? provider) => Parse(s.ToString());
    public static bool TryParse(ReadOnlySpan<char> s, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector3D result) => TryParse(s.ToString(), out result);
    public static Vector3D Parse(string s, IFormatProvider? provider) => Parse(s);
    public static bool TryParse([NotNullWhen(true)] string? s, IFormatProvider? provider, [MaybeNullWhen(false)] out Vector3D result) => TryParse(s!, out result);
    public static Vector3D AdditiveIdentity => Zero;
    public static Vector3D operator --(Vector3D value) => throw new NotSupportedException();
    public static Vector3D operator /(Vector3D left, Vector3D right) => throw new NotSupportedException();
    public static Vector3D operator ++(Vector3D value) => throw new NotSupportedException();

    public static Vector3D MultiplicativeIdentity => throw new NotSupportedException();



    public static bool TryConvertFromSaturating<TOther>(TOther value, [MaybeNullWhen(false)] out Vector3D result) where TOther : INumberBase<TOther>
    {
        switch (value)
        {
            case Vector3D v3d:
                result = v3d;
                return true;
            case Vector2D v2d:
                result = v2d.As3D();
                return true;
            case double v1d:
                result = new(v1d, 0, 0);
                return true;
            default:
                result = default;
                return false;
        }
    }
    
    public static bool TryConvertToSaturating<TOther>(Vector3D value, [MaybeNullWhen(false)] out TOther result) where TOther : INumberBase<TOther>
    {
        switch (TOther.Zero)
        {
            case Vector3D:
                result = (TOther)(object)value;
                return true;
            case Vector2D:
                result = (TOther)(object)value.As2D();
                return true;
            case double:
                result = (TOther)(object)value.X;
                return true;
            default:
                result = TOther.Zero;
                return false;
        }
    }
}