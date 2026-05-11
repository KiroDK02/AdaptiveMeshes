using System;
using System.Collections.Generic;
using Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesAbsolute;
using Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesRelative;
using Core.FEM;
using static Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategyType;

namespace Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes;

public static class CalculationErrorStrategyFactory
{
    public static ICalculationErrorStrategy GetCalculationErrorStrategy(CalculationErrorStrategyType type, 
        IDictionary<string, IMaterial> materials)
    {
        return type switch
        {
            AverageFlowDifference => new CesDifferenceAverageFlowOnEdge(materials),
            
            ProjectionFlowDifference => new CesDifferenceProjectionFlowOnEdge(materials),
            
            AverageFlowDifferenceRelativeAverage => new CesDifferenceAverageRelativeAverage(materials),
            
            ProjectionFlowDifferenceRelativeProjection => 
                new CesDifferenceProjectionRelativeProjection(materials),
            
            AverageFlowDifferenceRelativeNormFlowAtCenter => 
                new CesDifferenceAverageRelativeNormFlowCenter(materials),
            
            ProjectionFlowDifferenceRelativeNormFlowAtCenter => 
                new CesDifferenceProjectionRelativeNormFlowCenter(materials),
            
            _ => throw new ArgumentException("Unknown calculation error strategy")
        };
    }
}