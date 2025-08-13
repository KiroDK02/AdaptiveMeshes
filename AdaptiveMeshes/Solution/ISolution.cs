using AdaptiveMeshes.FEM;
using AdaptiveMeshes.TimeMesh;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Solution
{
    public interface ISolution
    {
        double Time { get; set; }
        IFiniteElementMesh Mesh { get; }
        ITimeMesh TimeMesh { get; }
        ReadOnlySpan<double> SolutionVector { get; set; }
        
        void AddSolutionVector(double t, double[] solution);
        double Value(Vector2D point);
        Vector2D Gradient(Vector2D point);
        Vector2D Flow(Vector2D point, IDictionary<string, IMaterial> materials);
    }
}
