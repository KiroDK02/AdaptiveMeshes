using AdaptiveMeshes.Solution;

namespace AdaptiveMeshes.Adaptation.CalculationErrorStrategies
{
    public interface IStrategyOfCalculationError
    {
        IDictionary<(int i, int j), double> ComputeError(ISolution solution);
    }
}
