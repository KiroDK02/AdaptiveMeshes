using AdaptiveMeshes.Interfaces;

namespace AdaptiveMeshes.SLAE
{
    public class PardisoSLAE : ISLAE
    {
        public PardisoSLAE(IMatrix matrix)
        {
            Matrix = matrix;
            RightPart = new double[Matrix.N];
        }

        public IMatrix Matrix { get; }
        public double[] RightPart { get; }

        public void AddFirstBoundaryConditions(int[] dofs, double[] lrp)
        {
            for (int i = 0; i < dofs.Length; i++)
            {
                double val = lrp[i];
                RightPart.ThreadSafeSet(dofs[i], val);
                Matrix.Symmetrize(dofs[i], val, RightPart);
            }
        }

        public void AddLocalRightPart(int[] dofs, double[] lrp)
        {
            for (int i = 0; i < dofs.Length; i++)
                RightPart.ThreadSafeAdd(dofs[i], lrp[i]);
        }

        public double CalcDiscrepancy(double[] solution)
        {
            double[] Ax = new double[Matrix.N];
            Matrix.MultVect(solution, Ax);

            double discrepancy = 0.0;
            double normRightPart = 0.0;

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
}
