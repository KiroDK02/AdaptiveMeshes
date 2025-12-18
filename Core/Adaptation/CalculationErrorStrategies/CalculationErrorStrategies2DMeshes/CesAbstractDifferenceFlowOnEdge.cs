using Core.FiniteElements.AlgorithmsForFE;
using Core.Adaptation.Adapters;
using Core.FEM;
using Core.FiniteElements.Interfaces;
using Core.Solution.Interfaces;

namespace Core.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes;

public abstract class CesAbstractDifferenceFlowOnEdge : ICalculationErrorStrategy
{
    protected readonly IDictionary<string, IMaterial> Materials;

    protected CesAbstractDifferenceFlowOnEdge(IDictionary<string, IMaterial> materials)
    {
        Materials = materials;
    }

    public IDictionary<(int i, int j), double> ComputeError(ISolution solution)
    {
        var amountOccurencesOfEdges =
            AlgorithmsForAdaptation.CalcNumberOccurrencesOfEdgesInElems(solution.Mesh.Elements);

        Dictionary<(int i, int j), double> errors = [];

        foreach (var element in solution.Mesh.Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;

            for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
            {
                var edge = element.GlobalEdge(edgei);
                var edgeFlow = GetFlowOnEdge(solution, element, edgei);

                if (amountOccurencesOfEdges[edge] == 1)
                {
                    errors[edge] = 0.0;
                    continue;
                }

                errors[edge] = errors.TryGetValue(edge, out double flow) ? Math.Abs(flow + edgeFlow) : edgeFlow;
            }
        }

        return errors;
    }

    protected abstract double GetFlowOnEdge(ISolution solution, IFiniteElement element, int edgei);
}