using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FEM.Mesh
{
    public interface IFiniteElementMesh
    {
        IEnumerable<IFiniteElement> Elements { get; }
        Vector2D[] Vertex { get; }
        int NumberOfDOFs { get; set; }
    }
}
