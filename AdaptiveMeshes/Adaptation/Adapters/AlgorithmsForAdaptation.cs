using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.Interfaces;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Adaptation.Adapters
{
    public static class AlgorithmsForAdaptation
    {
        public static IDictionary<(int i, int j), int> CalcNumberOccurrencesOfEdgesInElems(IEnumerable<IGeometryElement> elements)
        {
            var numberOccurrencesOfEdges = new Dictionary<(int i, int j), int>();

            foreach (var element in elements)
            {
                if (element.VertexNumbers.Length == 2)
                    continue;

                for (int i = 0; i < element.NumberOfEdges; ++i)
                {
                    var edge = element.Edge(i);
                    edge = (element.VertexNumbers[edge.i], element.VertexNumbers[edge.j]);
                    if (edge.i > edge.j)
                        edge = (edge.j, edge.i);

                    if (numberOccurrencesOfEdges.TryGetValue(edge, out var count))
                        numberOccurrencesOfEdges[edge] = ++count;
                    else
                        numberOccurrencesOfEdges.Add(edge, 1);
                }
            }

            return numberOccurrencesOfEdges;
        }
    }
}
