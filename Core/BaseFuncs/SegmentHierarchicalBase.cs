using System;

namespace Core.BaseFuncs;

public static class SegmentHierarchicalBase
{
    public static readonly Func<double, double>[] BasesFuncs =
    {
        point => 1 - point,
        point => point,
        point => point * (point - 1), // мб без 2
        point => point * (point - 1) * (2 * point - 1) // мб без 2/3
    };
}