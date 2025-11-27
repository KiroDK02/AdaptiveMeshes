using AdaptiveMeshes.FEM;
using AdaptiveMeshes.TimeMesh;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Solution.Interfaces;

public interface ISolution
{
    IFiniteElementMesh Mesh { get; }
    ReadOnlySpan<double> SolutionVector { get; set; }

    double Value(Vector2D point);
    Vector2D Gradient(Vector2D point);
    Vector2D Flow(Vector2D point, IDictionary<string, IMaterial> materials);
}