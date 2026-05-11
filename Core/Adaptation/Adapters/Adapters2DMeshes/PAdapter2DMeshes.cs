using System;
using System.Collections.Generic;
using System.Linq;
using Core.Adaptation.CalculationErrorStrategies;
using Core.Adaptation.DistributionStrategies;
using Core.FEM;
using Core.FiniteElements;
using Core.FiniteElements.AlgorithmsForFE;
using Core.FiniteElements.Interfaces;
using Core.Problems;

namespace Core.Adaptation.Adapters.Adapters2DMeshes;

public class PAdapter2DMeshes : IAdapter
{
    public IDistributionStrategy IncreasingOrderStrategy { get; }
    public ICalculationErrorStrategy CalculationErrorStrategy { get; }
    public IProblem Problem { get; }

    public PAdapter2DMeshes(
        IProblem problem,
        IDistributionStrategy increasingOrderStrategy,
        ICalculationErrorStrategy calculationErrorStrategy)
    {
        Problem = problem;
        IncreasingOrderStrategy = increasingOrderStrategy;
        CalculationErrorStrategy = calculationErrorStrategy;
    }

    public IFiniteElementMesh Adapt()
    {
        var errors = CalculationErrorStrategy.ComputeError(Problem.Solution);
        var orders = IncreasingOrderStrategy.GetDistribution(errors);

        var elements1D = FindElements1D();

        var newElementsOrders = new Dictionary<IFiniteElement, int>();
        
        foreach (var element in Problem.Mesh.Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;
            
            var maxOrder = Enumerable
                .Range(0, element.NumberOfEdges)
                .Select(element.GlobalEdge)
                .Max(edge => orders[edge]);

            var edgeElements1D = Enumerable
                .Range(0, element.NumberOfEdges)
                .Select(element.GlobalEdge)
                .Where(elements1D.ContainsKey)
                .Select(edge => elements1D[edge]);

            newElementsOrders[element] = maxOrder;
            foreach (var element1D in edgeElements1D)
                newElementsOrders[element1D] = maxOrder;
        }

        var newElements = new List<IFiniteElement>(newElementsOrders.Count);

        foreach (var (element, order) in newElementsOrders)
            newElements.Add(
                FiniteElementsFactory.CreateElement(element, element.VertexNumbers, Math.Min(element.Order + order, 3)));

        return new FiniteElementMesh(newElements, Problem.Mesh.Vertex);
    }

    private Dictionary<(int i, int j), IFiniteElement> FindElements1D()
        => Problem.Mesh.Elements
            .Where(element => element.VertexNumbers.Length == 2)
            .ToDictionary(e => e.GlobalEdge(0), e => e);
}