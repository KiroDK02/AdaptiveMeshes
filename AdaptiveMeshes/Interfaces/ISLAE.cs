namespace AdaptiveMeshes.Interfaces
{
    public interface ISLAE
    {
        IMatrix Matrix { get; }
        void AddLocalRightPart(int[] dofs, double[] lrp);
        void AddFirstBoundaryConditions(int[] dofs, double[] lrp);
        double CalcDiscrepancy(double[] solution);
        void Clear();
        void ClearRightPart();
        double[] RightPart { get; }
    }
}
