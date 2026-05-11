using System;
using System.Collections.Generic;
using Core.FEM;
using Core.FiniteElements.AlgorithmsForFE;
using Core.FiniteElements.Interfaces;
using Core.Solution.Interfaces;

namespace Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesAbsolute;

public class CesDifferenceProjectionFlowOnEdge : CesAbstractDifferenceFlowOnEdge
{
    public CesDifferenceProjectionFlowOnEdge(IDictionary<string, IMaterial> materials)
        : base(materials) { }


    protected override double GetDifferenceFlowsOnEdge(double valueFlow1, double valueFlow2, (int i, int j) edge) =>
        Math.Abs(valueFlow1 + valueFlow2);
    
    protected override double GetFlowOnEdge(ISolution solution, IFiniteElement element, int edgei)
    {
        var outerNormal = element.GetOuterNormalToEdge(solution.Mesh.Vertex, edgei, normalize: true);
        var elementCenter = element.GetElementCenter(solution.Mesh.Vertex);
        var lambda = Materials[element.Material].Lambda;

        return lambda(elementCenter)
               * element.GetGradientAtPoint(solution.Mesh.Vertex, solution.SolutionVector, elementCenter)
               * outerNormal;
    }
}