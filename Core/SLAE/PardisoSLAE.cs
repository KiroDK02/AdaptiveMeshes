using Core.Matrices;

namespace Core.SLAE;

public class PardisoSLAE : ISLAE
{
    public IMatrix Matrix { get; }
    public double[] RightPart { get; }

    public PardisoSLAE(IMatrix matrix)
    {
        Matrix = matrix;
        RightPart = new double[Matrix.N];
    }

    public void AddFirstBoundaryConditions(int[] dofs, double[] lrp)
    {
        for (int i = 0; i < dofs.Length; i++)
        {
            var val = lrp[i];
            RightPart.ThreadSafeSet(dofs[i], val);
            Matrix.Symmetrize(dofs[i], val, RightPart);
        }
    }

    public void AddLocalRightPart(int[] dofs, double[] lrp)
    {
        for (int i = 0; i < dofs.Length; i++)
            RightPart.ThreadSafeAdd(dofs[i], lrp[i]);
    }

    public double CalcDiscrepancy(ReadOnlySpan<double> solution)
    {
        var Ax = new double[Matrix.N];
        Matrix.MultVect(solution, Ax);

        var discrepancy = 0.0;
        var normRightPart = 0.0;

        for (int i = 0; i < Matrix.N; i++)
        {
            discrepancy += (Ax[i] - RightPart[i]) * (Ax[i] - RightPart[i]);
            normRightPart += RightPart[i] * RightPart[i];
        }

        return Math.Sqrt(discrepancy / normRightPart);
    }

    public void Clear()
    {
        Matrix.Clear();
        ClearRightPart();
    }

    public void ClearRightPart()
    {
        Array.Clear(RightPart);
    }
}