using System;
using System.Collections.Generic;
using Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesAbsolute;
using Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes.CesRelative;
using Core.FEM;
using static Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategy;

namespace Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes;

public static class CalculationErrorStrategyFactory
{
    public static ICalculationErrorStrategy GetCalculationErrorStrategy(CalculationErrorStrategy type, 
        IDictionary<string, IMaterial> materials)
    {
        return type switch
        {
            StrategyAverageFlowDifference => new CesDifferenceAverageFlowOnEdge(materials),
            
            StrategyProjectionFlowDifference => new CesDifferenceProjectionFlowOnEdge(materials),
            
            StrategyAverageFlowDifferenceRelativeAverage => new CesDifferenceAverageRelativeAverage(materials),
            
            StrategyProjectionFlowDifferenceRelativeProjection => 
                new CesDifferenceProjectionRelativeProjection(materials),
            
            StrategyAverageFlowDifferenceRelativeNormFlowAtCenter => 
                new CesDifferenceAverageRelativeNormFlowCenter(materials),
            
            StrategyProjectionFlowDifferenceRelativeNormFlowAtCenter => 
                new CesDifferenceProjectionRelativeNormFlowCenter(materials),
            
            _ => throw new ArgumentException("Unknown calculation error strategy")
        };
    }
}