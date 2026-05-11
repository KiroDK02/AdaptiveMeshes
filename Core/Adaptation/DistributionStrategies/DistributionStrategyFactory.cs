using System;
using Core.Adaptation.Adapters;
using Core.Adaptation.DistributionStrategies.IncreasingOrderStrategies2DMeshes;
using Core.Adaptation.DistributionStrategies.SplitStrategies2DMeshes;
using Core.Problems;

namespace Core.Adaptation.DistributionStrategies;

public static class DistributionStrategyFactory
{
    public static IDistributionStrategy GetDistributionStrategy(
        AdaptationType type,
        IProblem problem)
    {
        return type switch
        {
            AdaptationType.HAdaptation => 
                new SplitStrategy2DMeshes(problem.Mesh.Elements, problem.Mesh.Vertex),
            AdaptationType.PAdaptation => 
                new IncreasingOrderStrategy2DMeshes(problem.Mesh.Elements, problem.Mesh.Vertex),

            _ => throw new ArgumentException("Unknown distribution strategy")
        };
    }

    public static IDistributionStrategy GetDistributionStrategy(
        AdaptationType type,
        IProblem problem,
        double[] distanceFromMinParts,
        int[] distributionScale)
    {
        return type switch
        {
            AdaptationType.HAdaptation =>
                new SplitStrategy2DMeshes(
                    distanceFromMinParts, 
                    distributionScale, 
                    problem.Mesh.Elements, 
                    problem.Mesh.Vertex),
            
            AdaptationType.PAdaptation =>
                new IncreasingOrderStrategy2DMeshes(
                    distanceFromMinParts,
                    distributionScale,
                    problem.Mesh.Elements,
                    problem.Mesh.Vertex),

            _ => throw new ArgumentException("Unknown distribution strategy")
        };
    }
}