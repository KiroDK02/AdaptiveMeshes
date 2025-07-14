using AdaptiveMeshes.Interfaces;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FEM
{
    public class FiniteElementMesh : IFiniteElementMesh
    {
        public FiniteElementMesh(IEnumerable<IFiniteElement> elements, Vector2D[] vertex)
        {
            Elements = elements;
            Vertex = vertex;
        }

        public IEnumerable<IFiniteElement> Elements { get; }
        public Vector2D[] Vertex { get; }
        public int NumberOfDOFs { get; set; }
    }
}
