namespace AdaptiveMeshes.FiniteElements.AlgorithmsForFE
{
    public static class FiniteElementsExtensions
    {
        public static (int i, int j) GlobalEdge(this IFiniteElement element, int edge)
        {
            var targetEdge = element.Edge(edge);
            targetEdge = (element.VertexNumber[targetEdge.i], element.VertexNumber[targetEdge.j]);

            return targetEdge.i > targetEdge.j ? (targetEdge.j, targetEdge.i) : targetEdge;
        }
    }
}
