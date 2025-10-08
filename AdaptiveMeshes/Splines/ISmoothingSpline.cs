using AdaptiveMeshes.FEM.Mesh;

namespace AdaptiveMeshes.Splines
{
    public interface ISmoothingSpline<T> where T : struct
    {
        T[] Points { get; }
        ReadOnlySpan<double> Weights { get; }
        IFiniteElementMesh Mesh { get; }

        double Alpha { get; }
        double Betta { get; }

        void BuildSpline(double[] valuesFuncAtPoints);
        double Value(T point);
        T Gradient(T point);
    }
}
