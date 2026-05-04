using System.Collections.Generic;
using Core.FiniteElements.Interfaces;
using Core.Vectors;

namespace Core.FEM;

public class FiniteElementMesh : IFiniteElementMesh
{
    public IEnumerable<IFiniteElement> Elements { get; }
    public Vector2D[] Vertex { get; }
    public int NumberOfDOFs { get; set; }
    
    public IDictionary<(int i, int j), List<IFiniteElement>> EdgesToElements { get; } = 
        new Dictionary<(int i, int j), List<IFiniteElement>>();

    public FiniteElementMesh(IEnumerable<IFiniteElement> elements, Vector2D[] vertex)
    {
        Elements = elements;
        Vertex = vertex;
    }

    public bool TryFindElementWithPoint(Vector2D point, out IFiniteElement? result)
    {
        foreach (var element in Elements)
            if (element.VertexNumbers.Length != 2
                && element.IsPointOnElement(Vertex, point))
            {
                result = element;
                return true;
            }

        result = null;
        return false;
    }
}