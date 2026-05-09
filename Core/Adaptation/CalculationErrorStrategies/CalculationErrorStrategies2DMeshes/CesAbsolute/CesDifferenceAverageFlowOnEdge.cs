using System;
using System.Collections.Generic;
using Core.FEM;
using Core.FiniteElements.AlgorithmsForFE;
using Core.FiniteElements.Interfaces;
using Core.NumericalIntegration;
using Core.Solution.Interfaces;
using Core.Vectors;

namespace Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesAbsolute;

public class CesDifferenceAverageFlowOnEdge : CesAbstractDifferenceFlowOnEdge
{
    private readonly QuadratureNodes<double> _quadratureNodes;

    public CesDifferenceAverageFlowOnEdge(IDictionary<string, IMaterial> materials) :
        base(materials)
    {
        _quadratureNodes = 
            new([.. NumericalIntegrationMethods.GaussQuadrature1DOrder3()], 3);
    }

    protected override double GetDifferenceFlowsOnEdge(double valueFlow1, double valueFlow2, (int i, int j) edge) =>
        Math.Abs(valueFlow1 + valueFlow2);

    protected override double GetFlowOnEdge(ISolution solution, IFiniteElement element, int edgei)
    {
        var (i, j) = element.GlobalEdge(edgei);
        var outerNormal = element.GetOuterNormalToEdge(solution.Mesh.Vertex, edgei, true);

        var x0 = solution.Mesh.Vertex[i].X;
        var y0 = solution.Mesh.Vertex[i].Y;
        var x1 = solution.Mesh.Vertex[j].X;
        var y1 = solution.Mesh.Vertex[j].Y;

        var lambda = Materials[element.Material].Lambda;

        var edgeFlow = NumericalIntegrationMethods.NumericalValueIntegralOnEdge(_quadratureNodes,
            t =>
            {
                Vector2D point = new(x0 * (1 - t) + x1 * t, y0 * (1 - t) + y1 * t);

                return lambda(point)
                       * outerNormal
                       * element.GetGradientAtPoint(solution.Mesh.Vertex, solution.SolutionVector, point);
            });

        return edgeFlow;
    }
}