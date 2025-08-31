using AdaptiveMeshes.Solution;

namespace AdaptiveMeshes.Adaptation.CalculationErrorStrategies
{
    public interface ICalculationErrorStrategy
    {
        IDictionary<(int i, int j), double> ComputeError(ISolution solution);
    }
}
