using Core.Adaptation.CalculationErrorStrategies;
using Core.Adaptation.SplitStrategies;
using Core.FEM;
using Core.Problems;

namespace Core.Adaptation.Adapters;

public interface IAdapter
{
    IProblem Problem { get; }
    ISplitStrategy SplitStrategy { get; }
    ICalculationErrorStrategy CalculationErrorStrategy { get; }

    IFiniteElementMesh Adapt();
}