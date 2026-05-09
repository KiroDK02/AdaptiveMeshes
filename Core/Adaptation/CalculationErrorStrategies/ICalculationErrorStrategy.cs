using System.Collections.Generic;
using Core.Solution.Interfaces;

namespace Core.Adaptation.CalculationErrorStrategies;

public interface ICalculationErrorStrategy
{
    IDictionary<(int i, int j), double> ComputeError(ISolution solution);
}

public enum CalculationErrorStrategy
{
    StrategyAverageFlowDifference,
    StrategyProjectionFlowDifference,
    StrategyAverageFlowDifferenceRelativeAverage,
    StrategyProjectionFlowDifferenceRelativeProjection,
    StrategyAverageFlowDifferenceRelativeNormFlowAtCenter,
    StrategyProjectionFlowDifferenceRelativeNormFlowAtCenter
}