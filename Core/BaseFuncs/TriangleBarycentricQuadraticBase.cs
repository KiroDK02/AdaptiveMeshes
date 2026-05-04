using System;
using Core.Vectors;

namespace Core.BaseFuncs;

public static class TriangleBarycentricQuadraticBase
{
    public static readonly Func<Vector2D, double>[] BasesFuncs =
    {
        point => (1 - point.X - point.Y) * (2 * (1 - point.X - point.Y) - 1),
        point => point.X * (2 * point.X - 1),
        point => point.Y * (2 * point.Y - 1),
        point => 4 * (1 - point.X - point.Y) * point.X,
        point => 4 * point.X * point.Y,
        point => 4 * (1 - point.X - point.Y) * point.Y
    };

    public static readonly Func<Vector2D, double>[,] GradientBasesFuncs =
    {
        {
            point => -(2 * (1 - point.X - point.Y) - 1) - 2 * (1 - point.X - point.Y),
            point => -(2 * (1 - point.X - point.Y) - 1) - 2 * (1 - point.X - point.Y)
        },

        {
            point => (2 * point.X - 1) + 2 * point.X,
            point => 0
        },

        {
            point => 0,
            point => (2 * point.Y - 1) + 2 * point.Y
        },

        {
            point => 4 * (1 - 2 * point.X - point.Y),
            point => -4 * point.X
        },

        {
            point => 4 * point.Y,
            point => 4 * point.X
        },

        {
            point => -4 * point.Y,
            point => 4 * (1 - point.X - 2 * point.Y)
        }
    };
}