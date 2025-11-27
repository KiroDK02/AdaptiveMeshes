using AdaptiveMeshes.Solution.Interfaces;

namespace AdaptiveMeshes.Adaptation.CalculationErrorStrategies;

public interface ICalculationErrorStrategy
{
    enum CalculationErrorStrategyEnum
    {
        StrategyBasedOnAverageFlowJumps
    }

    IDictionary<(int i, int j), double> ComputeError(ISolution solution);
}