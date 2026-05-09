using System.Collections.Generic;
using Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesAbsolute;
using Core.FEM;

namespace Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesRelative;

public class CesDifferenceAverageRelativeNormFlowCenter : CesDifferenceAverageFlowOnEdge
{
    public CesDifferenceAverageRelativeNormFlowCenter(IDictionary<string, IMaterial> materials) 
        : base(materials) { }

    protected override double GetDifferenceFlowsOnEdge(double valueFlow1, double valueFlow2, (int i, int j) edge) =>
        (valueFlow1 + valueFlow2) / ValuesNormFlowAtCenter[edge];
}