namespace AdaptiveMeshes.Matrices
{
    public interface IMatrix
    {
        int N { get; }
        
        void SetProfile(SortedSet<int>[] profile);
        void AddLocal(int[] dofs, double[,] matrix, double coeff = 1.0);
        void Symmetrize(int dof, double value, double[] rightPart);
        void MultToVector(ReadOnlySpan<double> to, double[] result);
        void Clear();
    }
}
