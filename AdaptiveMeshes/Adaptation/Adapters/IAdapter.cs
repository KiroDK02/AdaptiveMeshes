using AdaptiveMeshes.Adaptation.StrategiesOfCalculationError;
using AdaptiveMeshes.Adaptation.StrategiesOfSplit;
using AdaptiveMeshes.FEM;
using AdaptiveMeshes.Solution;

namespace AdaptiveMeshes.Adaptation.Adapters
{
    public interface IAdapter
    {
        IFiniteElementMesh Mesh { get; }
        ISolution Solution { get; }
        IStrategyOfSplit StrategyOfSplit { get; }
        IStrategyOfCalculationError StrategyOfCalculationError { get; }

        IFiniteElementMesh Adapt();
    }
}
