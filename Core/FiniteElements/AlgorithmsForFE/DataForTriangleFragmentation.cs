using System.Collections.Generic;
using Core.FiniteElements.Interfaces;
using Core.Vectors;

namespace Core.FiniteElements.AlgorithmsForFE;

public class DataForTriangleFragmentation : IDataForFragmentation
{
    public DataForTriangleFragmentation(IEnumerable<IFiniteElement> newElements,
        IEnumerable<(Vector2D vert, int num)> newVertices)
    {
        NewElements = newElements;
        NewVertices = newVertices;
    }

    public IEnumerable<IFiniteElement> NewElements { get; }

    public IEnumerable<(Vector2D vert, int num)> NewVertices { get; }
}