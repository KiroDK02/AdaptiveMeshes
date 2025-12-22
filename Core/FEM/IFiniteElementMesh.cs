using Core.FiniteElements.Interfaces;
using Core.Vectors;

namespace Core.FEM;

public interface IFiniteElementMesh
{
    IEnumerable<IFiniteElement> Elements { get; }
    Vector2D[] Vertex { get; }
    int NumberOfDOFs { get; set; }
    
    bool TryFindElementWithPoint(Vector2D point, out IFiniteElement? result);
}