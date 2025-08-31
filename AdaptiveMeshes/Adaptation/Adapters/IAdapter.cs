using AdaptiveMeshes.Adaptation.CalculationErrorStrategies;
using AdaptiveMeshes.Adaptation.SplitStrategies;
using AdaptiveMeshes.FEM;
using AdaptiveMeshes.Problems;
using AdaptiveMeshes.Solution;

namespace AdaptiveMeshes.Adaptation.Adapters
{
    public interface IAdapter
    {
        IProblem Problem { get; }
        ISplitStrategy SplitStrategy { get; }
        ICalculationErrorStrategy CalculationErrorStrategy { get; }

        IFiniteElementMesh Adapt();
    }
}
