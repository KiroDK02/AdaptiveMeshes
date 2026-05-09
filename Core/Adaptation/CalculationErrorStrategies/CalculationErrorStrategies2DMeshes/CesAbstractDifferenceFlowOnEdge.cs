using System;
using System.Collections.Generic;
using Core.FiniteElements.AlgorithmsForFE;
using Core.Adaptation.Adapters;
using Core.FEM;
using Core.FiniteElements.Interfaces;
using Core.Solution.Interfaces;

namespace Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes;

public abstract class CesAbstractDifferenceFlowOnEdge : ICalculationErrorStrategy
{
    protected readonly Dictionary<(int i, int j), double> ValuesNormFlowAtCenter = new();
    
    protected readonly IDictionary<string, IMaterial> Materials;

    protected CesAbstractDifferenceFlowOnEdge(IDictionary<string, IMaterial> materials)
    {
        Materials = materials;
    }

    public IDictionary<(int i, int j), double> ComputeError(ISolution solution)
    {
        var amountOccurencesOfEdges =
            AlgorithmsForAdaptation.CalcNumberOccurrencesOfEdgesInElems(solution.Mesh.Elements);

        Dictionary<(int i, int j), double> errors = [];

        foreach (var element in solution.Mesh.Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;

            var lambda = Materials[element.Material].Lambda;
            var center = element.GetElementCenter(solution.Mesh.Vertex);
            
            var normFlowAtCenter =
                (lambda(center) * element.GetGradientAtPoint(solution.Mesh.Vertex, solution.SolutionVector, center))
                .Norm;
            
            for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
            {
                var edge = element.GlobalEdge(edgei);

                if (amountOccurencesOfEdges[edge] == 1)
                {
                    errors[edge] = 0.0;
                    continue;
                }
                
                ValuesNormFlowAtCenter[edge] = 
                    ValuesNormFlowAtCenter.TryGetValue(edge, out var valueNorm)
                    ? Math.Max(normFlowAtCenter, valueNorm)
                    : normFlowAtCenter;
                
                var edgeFlow = GetFlowOnEdge(solution, element, edgei);

                errors[edge] = 
                    errors.TryGetValue(edge, out double flow) 
                        ? GetDifferenceFlowsOnEdge(flow, edgeFlow, edge) 
                        : edgeFlow;
            }
        }

        return errors;
    }

    protected abstract double GetDifferenceFlowsOnEdge(double valueFlow1, double valueFlow2, (int i, int j) edge);
    protected abstract double GetFlowOnEdge(ISolution solution, IFiniteElement element, int edgei);
}