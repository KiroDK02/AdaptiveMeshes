using System.Linq;
using Core.FiniteElements.Interfaces;
using Core.Vectors;

namespace Core.FiniteElements.AlgorithmsForFE;

public static class FiniteElementsExtensions
{
    extension(IFiniteElement element)
    {
        public (int i, int j) GlobalEdge(int edge)
        {
            var targetEdge = element.Edge(edge);
            targetEdge = (element.VertexNumbers[targetEdge.i], element.VertexNumbers[targetEdge.j]);

            return targetEdge.i > targetEdge.j ? (targetEdge.j, targetEdge.i) : targetEdge;
        }

        public Vector2D GetElementCenter(Vector2D[] vertexCoords)
        {
            var sumVertices = Vector2D.Zero;

            foreach (var vertex in element.VertexNumbers.Select(number => vertexCoords[number]))
                sumVertices += vertex;
        
            return sumVertices / element.VertexNumbers.Length;
        }
    }
}