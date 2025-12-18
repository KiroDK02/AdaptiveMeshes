using Core.FiniteElements.Interfaces;
using Core.Vectors;

namespace Core.FEM;

public class FiniteElementMesh : IFiniteElementMesh
{
    public IEnumerable<IFiniteElement> Elements { get; }
    public Vector2D[] Vertex { get; }
    public int NumberOfDOFs { get; set; }

    public FiniteElementMesh(IEnumerable<IFiniteElement> elements, Vector2D[] vertex)
    {
        Elements = elements;
        Vertex = vertex;
    }
}