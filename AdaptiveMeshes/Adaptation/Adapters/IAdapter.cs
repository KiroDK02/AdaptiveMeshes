using AdaptiveMeshes.Adaptation.CalculationErrorStrategies;
using AdaptiveMeshes.Adaptation.SplitStrategies;
using AdaptiveMeshes.FEM;
using AdaptiveMeshes.Solution;

namespace AdaptiveMeshes.Adaptation.Adapters
{
    public interface IAdapter
    {
        IFiniteElementMesh Mesh { get; }
        ISolution Solution { get; }
        ISplitStrategy SplitStrategy { get; }
        IStrategyOfCalculationError CalculationErrorStrategy { get; }

        IFiniteElementMesh Adapt();
    }
}
