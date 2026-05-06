using System.Collections.Generic;
using Core.FiniteElements.Interfaces;
using Core.Vectors;

namespace Core.FEM;

public interface IFiniteElementMesh
{
    IDictionary<(int i, int j), List<IFiniteElement>> EdgesToElements { get; }
    IEnumerable<IFiniteElement> Elements { get; }
    Vector2D[] Vertex { get; }
    int NumberOfDOFs { get; set; }
    
    bool TryFindElementWithPoint(Vector2D point, out IFiniteElement? result);
}