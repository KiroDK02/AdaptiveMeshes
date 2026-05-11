using System;
using System.Collections.Generic;
using Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesAbsolute;
using Core.FEM;

namespace Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesRelative;

public class CesDifferenceProjectionRelativeNormFlowCenter : CesDifferenceProjectionFlowOnEdge
{
    public CesDifferenceProjectionRelativeNormFlowCenter(IDictionary<string, IMaterial> materials) 
        : base(materials) { }

    protected override double GetDifferenceFlowsOnEdge(double valueFlow1, double valueFlow2, (int i, int j) edge) =>
        Math.Abs(valueFlow1 + valueFlow2) / ValuesNormFlowAtCenter[edge];
}