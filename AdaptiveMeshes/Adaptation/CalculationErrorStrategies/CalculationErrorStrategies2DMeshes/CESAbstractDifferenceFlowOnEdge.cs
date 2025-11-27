using AdaptiveMeshes.Adaptation.Adapters;
using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.Solution;

namespace AdaptiveMeshes.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes
{
    public abstract class CESAbstractDifferenceFlowOnEdge : ICalculationErrorStrategy
    {
        protected CESAbstractDifferenceFlowOnEdge(IDictionary<string, IMaterial> materials)
        {
            _materials = materials;
        }

        protected readonly IDictionary<string, IMaterial> _materials;

        public IDictionary<(int i, int j), double> ComputeError(ISolution solution)
        {
            var amountOccurencesOfEdges = AlgorithmsForAdaptation.CalcNumberOccurrencesOfEdgesInElems(solution.Mesh.Elements);

            Dictionary<(int i, int j), double> errors = [];

            foreach (var element in solution.Mesh.Elements)
            {
                if (element.VertexNumber.Length == 2)
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
