using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.FiniteElements.Interfaces;
using AdaptiveMeshes.NumericalIntegration;
using AdaptiveMeshes.Solution.Interfaces;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Adaptation.CalculationErrorStrategies.CalculationErrorStrategies2DMeshes
{
    public class CesDifferenceAverageFlowOnEdge : CesAbstractDifferenceFlowOnEdge
    {
        public CesDifferenceAverageFlowOnEdge(IDictionary<string, IMaterial> materials) :
            base(materials)
        {
            _quadratureNodes = new([.. NumericalIntegrationMethods.GaussQuadrature1DOrder3()], 3);
        }

        private readonly QuadratureNodes<double> _quadratureNodes;

        protected override double GetFlowOnEdge(ISolution solution, IFiniteElement element, int edgei)
        {
            var (i, j) = element.GlobalEdge(edgei);
            var outerNormal = element.GetOuterNormalToEdge(solution.Mesh.Vertex, edgei, true);

            var x0 = solution.Mesh.Vertex[i].X;
            var y0 = solution.Mesh.Vertex[i].Y;
            var x1 = solution.Mesh.Vertex[j].X;
            var y1 = solution.Mesh.Vertex[j].Y;

            var lambda = Materials[element.Material].Lambda;

            var edgeFlow = NumericalIntegrationMethods.NumericalValueIntegralOnEdge(_quadratureNodes,
                    t =>
                    {
                        var point = new Vector2D(x0 * (1 - t) + x1 * t, y0 * (1 - t) + y1 * t);

                        return lambda(point) * outerNormal * element.GetGradientAtPoint(solution.Mesh.Vertex, solution.SolutionVector, point);
                    });

            return edgeFlow;
        }
    }
}
