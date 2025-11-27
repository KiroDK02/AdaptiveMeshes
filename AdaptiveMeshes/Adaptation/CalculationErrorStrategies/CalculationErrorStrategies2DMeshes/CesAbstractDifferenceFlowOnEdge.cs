using AdaptiveMeshes.Adaptation.Adapters;
using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.FiniteElements.Interfaces;
using AdaptiveMeshes.Solution.Interfaces;

namespace AdaptiveMeshes.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes
{
    public abstract class CesAbstractDifferenceFlowOnEdge : ICalculationErrorStrategy
    {
        protected CesAbstractDifferenceFlowOnEdge(IDictionary<string, IMaterial> materials)
        {
            Materials = materials;
        }

        protected readonly IDictionary<string, IMaterial> Materials;

        public IDictionary<(int i, int j), double> ComputeError(ISolution solution)
        {
            var amountOccurencesOfEdges = AlgorithmsForAdaptation.CalcNumberOccurrencesOfEdgesInElems(solution.Mesh.Elements);

            Dictionary<(int i, int j), double> errors = [];

            foreach (var element in solution.Mesh.Elements)
            {
                if (element.VertexNumbers.Length == 2)
                    continue;

                for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                {
                    var edge = element.GlobalEdge(edgei);
                    double edgeFlow = GetFlowOnEdge(solution, element, edgei);

                    if (amountOccurencesOfEdges[edge] == 1)
                    {
                        errors[edge] = 0.0;
                        continue;
                    }

                    errors[edge] = errors.TryGetValue(edge, out double flow) ?
                                   Math.Abs(flow + edgeFlow) :
                                   edgeFlow;
                }
            }

            return errors;
        }

        protected abstract double GetFlowOnEdge(ISolution solution, IFiniteElement element, int edgei);
    }
}
