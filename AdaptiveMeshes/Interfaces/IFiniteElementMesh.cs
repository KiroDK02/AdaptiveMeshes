using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Interfaces
{
    public interface IFiniteElementMesh
    {
        IEnumerable<IFiniteElement> Elements { get; }
        Vector2D[] Vertex { get; }
        int NumberOfDOFs { get; set; }
    }
}
