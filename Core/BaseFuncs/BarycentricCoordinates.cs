using System;
using Core.Vectors;

namespace Core.BaseFuncs;

public static class BarycentricCoordinates
{
    public static Func<Vector2D, double> L1 => (point) => 1 - point.X - point.Y;
    public static Func<Vector2D, double> L2 => point => point.X;
    public static Func<Vector2D, double> L3 => point => point.Y;
}