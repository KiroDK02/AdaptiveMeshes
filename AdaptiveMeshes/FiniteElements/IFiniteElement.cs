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

        /// <summary>
        /// Проводит дискретизацию двумерного элемента.
        /// Не поддерживается для элементов других размерностей.
        /// </summary>
        /// <param name="splits">Число разбиений ребер</param>
        /// <param name="verticesOfSplitedEdges">Вершины дискретезированных ребер всей сетки.</param>
        /// <param name="countVertex">Счетчик номеров для новых вершин.</param>
        /// <returns>Данные, являющееся результатом дискретизации текущего элемента.</returns>
        IDataForFragmentation SplitToElements2D(IDictionary<(int i, int j), int> splits,
                                                IDictionary<(int i, int j), (Vector2D vert, int num)[]> verticesOfSplitedEdges,
                                                ref int countVertex);
        /// <summary>
        /// Проводит дискретизацию одномерного элемента.
        /// Не поддерживается для элементов других размерностей.
        /// </summary>
        /// <param name="globalVerticesNums">Номера новых вершин одномерного элемента.</param>
        /// <returns>Набор новых одномерных элементов.</returns>
        IEnumerable<IFiniteElement> SplitToElements1D(int[] globalVerticesNums);

        /// <summary>
        /// Рассчитывет и возвращает внешнюю нормаль к ребру <c>edge</c>.
        /// </summary>
        /// <param name="VertexCoords">Глобальные координаты вершин сетки.</param>
        /// <param name="edgei">Номер ребра, к которому нужно вернуть нормаль.</param>
        /// <param name="normalize">Устанавливает, обязательно ли нужно вернуть единичную нормаль.</param>
        /// <returns>Вектор нормали к ребру.</returns>
        Vector2D GetOuterNormalToEdge(Vector2D[] VertexCoords, int edgei, bool normalize = false);
    }

    public interface IFiniteElementWithNumericalIntegration<T> : IFiniteElement
    {
        IMasterElement<T> MasterElement { get; }
    }
}
