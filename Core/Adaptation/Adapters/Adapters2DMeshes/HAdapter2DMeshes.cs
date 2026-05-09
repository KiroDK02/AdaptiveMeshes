using System.Collections.Generic;
using System.Linq;
using Core.FiniteElements.AlgorithmsForFE;
using Core.Adaptation.CalculationErrorStrategies;
using Core.Adaptation.DistributionStrategies;
using Core.FEM;
using Core.FiniteElements.Interfaces;
using Core.Problems;
using Core.Vectors;

namespace Core.Adaptation.Adapters.Adapters2DMeshes;

/// <summary>
/// В этом классе по сути надо будет реализовать один метод - Adapt
/// Может быть сделать сразу этот класс с заделкой на циклическую адаптацию,
/// просто добавив количество повторов, адаптация будет происходить через
/// стратегии расчета ошибок и разбиения, в которых будет реализовано все нужное.
/// </summary>
public class HAdapter2DMeshes : IAdapter
{
    public IProblem Problem { get; }

    /// <value>
    /// Свойство <c>SplitStrategy</c> является стратегией разбиения - шкала + методы для расчета разбиений
    /// </value>
    public IDistributionStrategy SplitStrategy { get; }

    /// <value>
    /// Свойство <c>CalculationErrorStrategy</c> является стратегией расчета на ребрах локальных ошибок решения - скачков потока + метод(ы) для их расчета
    /// </value>
    public ICalculationErrorStrategy CalculationErrorStrategy { get; }

    public HAdapter2DMeshes(IProblem problem, IDistributionStrategy splitStrategy,
        ICalculationErrorStrategy calculationErrorStrategy)
    {
        Problem = problem;
        SplitStrategy = splitStrategy;
        CalculationErrorStrategy = calculationErrorStrategy;
    }

    /// <summary>
    /// Выполняет адаптацию по заданным стратегиям дробления <c>SplitStrategy</c> и расчета ошибки <c>CalculationErrorStrategy</c>.
    /// Пока что не циклическая, позже мб сделаю с циклической тут же.
    /// </summary>
    /// <returns>Сетка после адаптации.</returns>
    public IFiniteElementMesh Adapt()
    {
        var errors = CalculationErrorStrategy.ComputeError(Problem.Solution);
        var splits = SplitStrategy.GetDistribution(errors);

        var countVertices = Problem.Mesh.Vertex.Length;
        var verticesSplittedEdges = SplitStrategy.CalcVerticesEdges(splits, ref countVertices);

        List<IFiniteElement> newElements = [];
        List<(Vector2D vert, int num)> newVertices = [];

        foreach (var element in Problem.Mesh.Elements)
        {
            var splittableElement = (ISplittableElement)element;
            if (element.VertexNumbers.Length != 2)
            {
                var data = splittableElement.SplitToElements2D(splits, verticesSplittedEdges, ref countVertices);

                newElements.AddRange(data.NewElements);
                newVertices.AddRange(data.NewVertices);
            }
            else
            {
                var elements = splittableElement.SplitToElements1D(
                [
                    .. verticesSplittedEdges[element.GlobalEdge(0)]
                        .Select(vertex => vertex.num)
                ]);

                newElements.AddRange(elements);
            }
        }

        var vertices = new Vector2D[countVertices];

        foreach (var (vert, number) in newVertices)
            vertices[number] = vert;

        return new FiniteElementMesh(newElements, vertices);
    }
}