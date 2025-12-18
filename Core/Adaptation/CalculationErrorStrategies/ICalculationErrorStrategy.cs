using Core.Solution.Interfaces;

namespace Core.Adaptation.CalculationErrorStrategies;

public interface ICalculationErrorStrategy
{
    enum CalculationErrorStrategyEnum
    {
        StrategyBasedOnAverageFlowJumps
    }

    IDictionary<(int i, int j), double> ComputeError(ISolution solution);
}