using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.MasterElements;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FiniteElements.FiniteElements1D
{
    public class SegmentFEQuadraticBaseWithNI : IFiniteElementWithNumericalIntegration<double>
    {
        public SegmentFEQuadraticBaseWithNI(string material, int[] vertexNumber)
        {
            Material = material;
            VertexNumber = vertexNumber;

            MasterElement = MasterElementBarycentricQuadraticBaseStraight.Instance;
        }
        public IMasterElement<double> MasterElement { get; }

        public string Material { get; }

        public int[] VertexNumber { get; } = new int[2];

        public int[] Dofs { get; } = new int[3];

        public int NumberOfEdges => 1;

        public double[,] BuildLocalMatrix(Vector2D[] VertexCoords, IFiniteElement.MatrixTypeEnum type, Func<Vector2D, double> Coeff)
            => throw new NotSupportedException();

        public double[] BuildLocalRightPart(Vector2D[] VertexCoords, Func<Vector2D, double> F)
            => throw new NotSupportedException();

        public double[] BuildLocalRightPartFirstBC(Vector2D[] VertexCoords, Func<Vector2D, double> Ug)
            => CalcLocalF(VertexCoords, Ug);

        public double[] BuildLocalRightPartSecondBC(Vector2D[] VertexCoords, Func<Vector2D, double> Thetta)
        {
            Vector2D point1 = VertexCoords[VertexNumber[0]];
            Vector2D point2 = VertexCoords[VertexNumber[1]];

            double lengthBound = Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X) + (point1.Y - point2.Y) * (point1.Y - point2.Y));

            var nodes = MasterElement.QuadratureNodes;
            double[,] values = MasterElement.ValuesBasicFuncs;
            double[] localRightPart = new double[Dofs.Length];

            double LocalThetta(double t) => GetCoefAtLocalCoords(VertexCoords, Thetta, t);

            for (int i = 0; i < Dofs.Length; i++)
            {
                double valueIntegral = 0.0;

                for (int k = 0; k < nodes.Nodes.Length; k++)
                    valueIntegral += nodes.Nodes[k].Weight * LocalThetta(nodes.Nodes[k].Node) * values[i, k];

                localRightPart[i] = valueIntegral * lengthBound;
            }

            return localRightPart;
        }

        public int DOFOnVertex(int vertex) => 1;

        public int DOFOnEdge(int edge) => 1;

        public int DOFOnElement() => 0;

        public (int i, int j) Edge(int edge)
            => edge switch
            {
                0 => (0, 1),
                _ => throw new Exception("Invalid number of edge.")
            };

        public Vector2D GetGradientAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false)
            => throw new NotSupportedException();

        public double GetValueAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false)
            => throw new NotSupportedException();

        public bool IsPointOnElement(Vector2D[] VertexCoords, Vector2D point)
            => throw new NotSupportedException();

        public void SetEdgeDOF(int edge, int n, int dof)
        {
            if (edge == 0)
                Dofs[2] = dof;
            else
                throw new Exception("Invalid number of edge.");
        }

        public void SetElementDOF(int n, int dof)
            => throw new NotSupportedException();

        public void SetVertexDOF(int vertex, int n, int dof)
        {
            switch (vertex)
            {
                case 0:
                    Dofs[0] = dof;
                    break;

                case 1:
                    Dofs[1] = dof;
                    break;

                default:
                    throw new Exception("Invalid number of vertex.");
            }
        }

        public IDataForFragmentation SplitToElements2D(IDictionary<(int i, int j), int> splits, IDictionary<(int i, int j), (Vector2D vert, int num)[]> verticesOfSplitedEdges, ref int countVertex)
        {
            throw new NotSupportedException();
        }

        public IEnumerable<IFiniteElement> SplitToElements1D(int[] globalVerticesNums)
        {
            // Передать за globalVerticeNums для одномерных элементов что-то типа этого 
            // [.. verticesOfSplitiedEdges[elem.GlobalEdge(0)].Select(vertex => vertex.num)]
            List<IFiniteElement> elems = [];

            for (int i = 0; i < globalVerticesNums.Length - 1; i++)
            {
                int[] globalNums = [globalVerticesNums[i], globalVerticesNums[i + 1]];

                elems.Add(new SegmentFEQuadraticBaseWithNI(Material, globalNums));
            }

            return elems;
        }

        public Vector2D GetOuterNormalToEdge(Vector2D[] VertexCoords, int edgei, bool normalize = false)
            => throw new NotSupportedException();

        private double GetCoefAtLocalCoords(Vector2D[] VertexCoords, Func<Vector2D, double> coeff, double t)
        {
            double x0 = VertexCoords[VertexNumber[0]].X;
            double x1 = VertexCoords[VertexNumber[1]].X;
            double y0 = VertexCoords[VertexNumber[0]].Y;
            double y1 = VertexCoords[VertexNumber[1]].Y;

            return coeff(new(x0 * (1 - t) + x1 * t, y0 * (1 - t) + y1 * t));
        }

        private double[] CalcLocalF(Vector2D[] VertexCoords, Func<Vector2D, double> F)
        {
            double[] localF = new double[Dofs.Length];

            localF[0] = F(VertexCoords[VertexNumber[0]]);
            localF[1] = F(VertexCoords[VertexNumber[1]]);
            localF[2] = F((VertexCoords[VertexNumber[1]] + VertexCoords[VertexNumber[0]]) / 2d);

            return localF;
        }
    }
}
