using System;
using Core.Adaptation.CalculationErrorStrategies;
using Core.Adaptation.DistributionStrategies;
using Core.Problems;

namespace Core.Adaptation.Adapters.Adapters2DMeshes;

public static class AdaptersFactory
{
    public static IAdapter GetAdapter(
        AdaptationType type, 
        IProblem problem,
        IDistributionStrategy distributionStrategy,
        ICalculationErrorStrategy calculationErrorStrategy)
    {
        return type switch
        {
            AdaptationType.HAdaptation => new HAdapter2DMeshes(problem, distributionStrategy, calculationErrorStrategy),
            AdaptationType.PAdaptation => new PAdapter2DMeshes(problem, distributionStrategy, calculationErrorStrategy),
            _ => throw new ArgumentException("Unknown type of adaptation.")
        };
    }
}