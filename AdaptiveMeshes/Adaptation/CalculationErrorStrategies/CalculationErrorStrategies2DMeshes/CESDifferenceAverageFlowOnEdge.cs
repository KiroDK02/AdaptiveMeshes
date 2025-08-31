using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.NumericalIntegration;
using AdaptiveMeshes.Solution;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes
{
    public class CESDifferenceAverageFlowOnEdge : CESAbstractDifferenceFlowOnEdge
    {
        public CESDifferenceAverageFlowOnEdge(IDictionary<string, IMaterial> materials) :
            base(materials)
        {
            _quadratureNodes = new([.. NumericalIntegrationMethods.GaussQuadrature1DOrder3()], 3);
        }

        readonly QuadratureNodes<double> _quadratureNodes;

        protected override double GetFlowOnEdge(ISolution solution, IFiniteElement element, int edgei)
        {
            (int i, int j) = element.GlobalEdge(edgei);
            Vector2D outerNormal = element.GetOuterNormalToEdge(solution.Mesh.Vertex, edgei, true);

            double x0 = solution.Mesh.Vertex[i].X;
            double y0 = solution.Mesh.Vertex[i].Y;
            double x1 = solution.Mesh.Vertex[j].X;
            double y1 = solution.Mesh.Vertex[j].Y;

            var lambda = _materials[element.Material].Lambda;

            double edgeFlow = NumericalIntegrationMethods.NumericalValueIntegralOnEdge(_quadratureNodes,
                    t =>
                    {
                        Vector2D point = new(x0 * (1 - t) + x1 * t, y0 * (1 - t) + y1 * t);

                        return lambda(point) * outerNormal * element.GetGradientAtPoint(solution.Mesh.Vertex, solution.SolutionVector, point);
                    });

            return edgeFlow;
        }
    }
}
