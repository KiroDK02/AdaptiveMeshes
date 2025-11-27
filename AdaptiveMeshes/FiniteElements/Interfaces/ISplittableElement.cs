using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FiniteElements.Interfaces;

public interface ISplittableElement
{
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
}