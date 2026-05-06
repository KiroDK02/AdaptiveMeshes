using System;
using Core.Vectors;
using static Core.BaseFuncs.BarycentricCoordinates;

namespace Core.BaseFuncs;

public static class TriangleBarycentricQuadraticBase
{
    public static readonly Func<Vector2D, double>[] BasesFuncs =
    [
        point => L1(point) * (2 * L1(point) - 1),
        point => L2(point) * (2 * L2(point) - 1),
        point => L3(point) * (2 * L3(point) - 1),
        point => 4 * L1(point) * L2(point),
        point => 4 * L2(point) * L3(point),
        point => 4 * L1(point) * L3(point)
    ];

    public static readonly Func<Vector2D, double>[,] GradientBasesFuncs =
    {
        {
            point => -(2 * L1(point) - 1) - 2 * L1(point),
            point => -(2 * L1(point) - 1) - 2 * L1(point)
        },

        {
            point => (2 * L2(point) - 1) + 2 * L2(point),
            point => 0
        },

        {
            point => 0,
            point => (2 * L3(point) - 1) + 2 * L3(point)
        },

        {
            point => 4 * (-L2(point) + L1(point)),
            point => -4 * L2(point)
        },

        {
            point => 4 * L3(point),
            point => 4 * L2(point)
        },

        {
            point => -4 * L3(point),
            point => 4 * (-L3(point) + L1(point))
        }
    };
}