using AdaptiveMeshes.Solution;

namespace AdaptiveMeshes.Adaptation
{
    public interface IStrategyOfCalculationError
    {
        IDictionary<(int i, int j), double> ComputeError(ISolution solution);
    }
}
