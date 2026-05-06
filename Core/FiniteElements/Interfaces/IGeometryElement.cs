using Core.Vectors;

namespace Core.FiniteElements.Interfaces;

public interface IGeometryElement
{
    int[] VertexNumbers { get; }
    int NumberOfEdges { get; }

    (int i, int j) Edge(int edge);
    bool IsPointOnElement(Vector2D[] vertexCoords, Vector2D point);
    Vector2D GetOuterNormalToEdge(Vector2D[] vertexCoords, int edgei, bool normalize = false);
}