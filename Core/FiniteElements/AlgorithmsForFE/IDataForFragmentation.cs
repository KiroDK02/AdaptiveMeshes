using Core.FiniteElements.Interfaces;
using Core.Vectors;

namespace Core.FiniteElements.AlgorithmsForFE;

public interface IDataForFragmentation
{
    IEnumerable<IFiniteElement> NewElements { get; }
    IEnumerable<(Vector2D vert, int num)> NewVertices { get; }
}