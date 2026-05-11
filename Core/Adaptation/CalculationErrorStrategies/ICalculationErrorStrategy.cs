using System.Collections.Generic;
using System.ComponentModel;
using Core.Solution.Interfaces;

namespace Core.Adaptation.CalculationErrorStrategies;

public interface ICalculationErrorStrategy
{
    IDictionary<(int i, int j), double> ComputeError(ISolution solution);
}

public enum CalculationErrorStrategyType
{
    [Description("Разность средних потоков")]
    AverageFlowDifference,
    [Description("Разность проекций потоков")]
    ProjectionFlowDifference,
    [Description("Разность средних потоков / Max(|средних|)")]
    AverageFlowDifferenceRelativeAverage,
    [Description("Разность проекций потоков / Max(|проекций|)")]
    ProjectionFlowDifferenceRelativeProjection,
    [Description("Разность средних потоков / Max(норм потоков в центрах)")]
    AverageFlowDifferenceRelativeNormFlowAtCenter,
    [Description("Разность проекций потоков / Max(норм потоков в центрах)")]
    ProjectionFlowDifferenceRelativeNormFlowAtCenter
}