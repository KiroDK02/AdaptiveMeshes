using System;
using Core.Matrices;

namespace Core.SLAE;

public interface ISLAE
{
    IMatrix Matrix { get; }
    void AddLocalRightPart(int[] dofs, double[] lrp);
    void AddFirstBoundaryConditions(int[] dofs, double[] lrp);
    double CalcDiscrepancy(ReadOnlySpan<double> solution);
    void Clear();
    void ClearRightPart();
    double[] RightPart { get; }
}