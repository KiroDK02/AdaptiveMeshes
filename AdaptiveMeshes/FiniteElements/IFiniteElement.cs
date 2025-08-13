using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.MasterElements;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FiniteElements
{
    public interface IFiniteElement
    {
        enum MatrixTypeEnum { Stiffness, Mass }
        string Material { get; }
        int[] VertexNumber { get; }
        int[] Dofs { get; }
        int NumberOfEdges { get; }

        (int i, int j) Edge(int edge);

        void SetVertexDOF(int vertex, int n, int dof);
        void SetEdgeDOF(int edge, int n, int dof);
        void SetElementDOF(int n, int dof);

        int DOFOnVertex(int vertex);
        int DOFOnEdge(int edge);
        int DOFOnElement();

        double[,] BuildLocalMatrix(Vector2D[] VertexCoords, MatrixTypeEnum type, Func<Vector2D, double> Coeff);
        double[] BuildLocalRightPart(Vector2D[] VertexCoords, Func<Vector2D, double> F);

        double[] BuildLocalRightPartFirstBC(Vector2D[] VertexCoords, Func<Vector2D, double> Ug);
        double[] BuildLocalRightPartSecondBC(Vector2D[] VertexCoords, Func<Vector2D, double> Thetta);

        bool IsPointOnElement(Vector2D[] VertexCoords, Vector2D point);
        double GetValueAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false);
        Vector2D GetGradientAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false);

        IDataForFragmentation SplitToElements2D(IDictionary<(int i, int j), int> splits,
                                                IDictionary<(int i, int j), (Vector2D vert, int num)[]> verticesOfSplitedEdges,
                                                ref int countVertex);
        IEnumerable<IFiniteElement> SplitToElements1D(int[] globalVerticesNums);
    }

    public interface IFiniteElementWithNumericalIntegration<T> : IFiniteElement
    {
        IMasterElement<T> MasterElement { get; }
    }
}
