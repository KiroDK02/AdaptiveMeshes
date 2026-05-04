using System;
using Core.Vectors;
using static Core.BaseFuncs.BarycentricCoordinates;

namespace Core.BaseFuncs;

public static class TriangleHierarchicalBase
{
    public static readonly Func<Vector2D, double>[] BasesFuncs =
    [
        // Вершины
        point => L1(point),
        point => L2(point),
        point => L3(point),
        
        // Ребра (возможно первые 3 нужно домножить на -1
        point => -L1(point) * L2(point),
        point => -L1(point) * L3(point),
        point => -L2(point) * L3(point),
        point => L1(point) * L2(point) * (L1(point) -  L2(point)),
        point =>  L1(point) * L3(point) * (L1(point) - L3(point)),
        point =>  L2(point) * L3(point) * (L2(point) - L3(point)),
        
        // Центр
        point => L1(point) * L2(point) * L3(point)
    ];

    public static readonly Func<Vector2D, double>[,] GradientBasesFuncs =
    {
        {
            point => -1,
            point => -1
        },
        
        {
            point => 1,
            point => 0
        },
        
        {
            point => 0,
            point => 1
        },
        
        {
            point => L2(point) + L1(point),
            point => L2(point)
        },
        
        {
            point => L3(point),
            point => L3(point) + L1(point)
        },
        
        {
            point => -L3(point),
            point => -L2(point)
        },
        
        {
            point => (L1(point) - L2(point)) * (L1(point) - L2(point)) - 2 * L1(point) * L2(point),
            point => -L2(point) * (2 * L1(point) - L2(point))
        },
        
        {
            point => -L3(point) * (2 * L1(point) - L3(point)),
            point => (L1(point) - L3(point)) * (L1(point) - L3(point)) - 2 * L1(point) * L3(point)
        },
        
        {
            point => L3(point) * (2 * L2(point) - L3(point)),
            point => L2(point) * (L2(point) - 2 * L3(point))
        },
        
        {
            point => L3(point) * (L1(point) - L2(point)),
            point => L2(point) * (L1(point) - L3( point))
        }
    };
}