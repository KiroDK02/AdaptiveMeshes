using AdaptiveMeshes.Interfaces;
using AdaptiveMeshes.MasterElements;
using AdaptiveMeshes.Vectors;
using static AdaptiveMeshes.FiniteElements.AlgorithmsForFragmentationTriangleElements;

namespace AdaptiveMeshes.FiniteElements
{
    public class TriangleFEQuadraticBaseWithNI : IFiniteElementWithNumericalIntegration<Vector2D>
    {
        public TriangleFEQuadraticBaseWithNI(string material, int[] vertexNumber)
        {
            Material = material;
            VertexNumber = vertexNumber;

            MasterElement = MasterElementTriangleBarycentricQuadraticBase.Instance;
        }
        public IMasterElement<Vector2D> MasterElement { get; }

        public string Material { get; }

        public int[] VertexNumber { get; } = new int[3];

        public int[] Dofs { get; } = new int[6];

        public int NumberOfEdges => 3;

        public double[,] BuildLocalMatrix(Vector2D[] VertexCoords, IFiniteElement.MatrixTypeEnum type, Func<Vector2D, double> Coeff)
        {
            double detD = DetD(VertexCoords);
            double[,] J = GetMatrixJacobi(VertexCoords);
            double[,] localMatrix = new double[Dofs.Length, Dofs.Length];

            var nodes = MasterElement.QuadratureNodes;

            double LocalCoef(Vector2D vert) => GetCoefAtLocalCoords(VertexCoords, Coeff, vert);

            switch (type)
            {
                case IFiniteElement.MatrixTypeEnum.Stiffness:
                    for (int i = 0; i < Dofs.Length; i++)
                    {
                        for (int j = 0; j < Dofs.Length; j++)
                        {
                            var values = MasterELementsAlgorithms.CalcGradMultGrad(nodes, MasterElement.ValuesBasicFuncsGradients, i, j, J);
                            double valueIntegral = 0.0;

                            for (int k = 0; k < nodes.Nodes.Length; k++)
                                valueIntegral += LocalCoef(nodes.Nodes[k].Node) * values[k];

                            localMatrix[i, j] = Math.Abs(detD) * valueIntegral;
                        }
                    }

                    return localMatrix;

                case IFiniteElement.MatrixTypeEnum.Mass:
                    for (int i = 0; i < Dofs.Length; i++)
                    {
                        for (int j = 0; j < Dofs.Length; j++)
                        {
                            var values = MasterElement.PsiProduct[(i, j)];
                            double valueIntegral = 0.0;

                            for (int k = 0; k < nodes.Nodes.Length; k++)
                                valueIntegral += LocalCoef(nodes.Nodes[k].Node) * values[k];

                            localMatrix[i, j] = Math.Abs(detD) * valueIntegral;
                        }
                    }

                    return localMatrix;

                default:
                    throw new ArgumentException("Invalid type of matrix.");
            }
        }

        public double[] BuildLocalRightPart(Vector2D[] VertexCoords, Func<Vector2D, double> F)
        {
            var nodes = MasterElement.QuadratureNodes;
            var values = MasterElement.ValuesBasicFuncs;

            double detD = DetD(VertexCoords);
            double[] localRightPart = new double[Dofs.Length];

            double LocalF(Vector2D vert) => GetCoefAtLocalCoords(VertexCoords, F, vert);

            for (int i = 0; i < Dofs.Length; i++)
            {
                double valueIntegral = 0.0;

                for (int k = 0; k < nodes.Nodes.Length; k++)
                    valueIntegral += nodes.Nodes[k].Weight * LocalF(nodes.Nodes[k].Node) * values[i, k];

                localRightPart[i] = Math.Abs(detD) * valueIntegral;
            }

            return localRightPart;
        }

        public double[] BuildLocalRightPartFirstBC(Vector2D[] VertexCoords, Func<Vector2D, double> Ug)
            => throw new NotSupportedException();

        public double[] BuildLocalRightPartSecondBC(Vector2D[] VertexCoords, Func<Vector2D, double> Thetta)
            => throw new NotSupportedException();

        public int DOFOnVertex(int vertex) => 1;

        public int DOFOnEdge(int edge) => 1;

        public int DOFOnElement() => 0;

        public (int i, int j) Edge(int edge)
            => edge switch
            {
                0 => (0, 1),
                1 => (1, 2),
                2 => (2, 0),
                _ => throw new Exception("Invalid number of edge.")
            };

        public Vector2D GetGradientAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false)
        {
            double[,] J = GetMatrixJacobi(VertexCoords);

            Vector2D gradAtLocalCoords = isLocalPoint ? GetGradientAtLocalPoint(weights, point) : GetGradientAtLocalPoint(weights, GetLocalPoint(VertexCoords, point));

            double xComp = gradAtLocalCoords.X * J[0, 0] + gradAtLocalCoords.Y * J[0, 1];
            double yComp = gradAtLocalCoords.X * J[1, 0] + gradAtLocalCoords.Y * J[1, 1];

            return new(xComp, yComp);
        }

        public Vector2D GetGradientAtLocalPoint(ReadOnlySpan<double> weights, Vector2D localPoint)
        {
            var gradBasesFuncs = BaseFuncs.TriangleBarycentricQuadraticBase.GradientBasesFuncs;

            double valueXComp = 0.0;
            double valueYComp = 0.0;

            for (int i = 0; i < Dofs.Length; i++)
            {
                valueXComp += weights[Dofs[i]] * gradBasesFuncs[i, 0](localPoint);
                valueYComp += weights[Dofs[i]] * gradBasesFuncs[i, 1](localPoint);
            }

            return new(valueXComp, valueYComp);
        }

        public double GetValueAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false)
        {
            if (isLocalPoint)
                return GetValueAtLocalPoint(weights, point);

            Vector2D localPoint = GetLocalPoint(VertexCoords, point);

            return GetValueAtLocalPoint(weights, localPoint);
        }

        public double GetValueAtLocalPoint(ReadOnlySpan<double> weights, Vector2D localPoint)
        {
            var basicFuncs = BaseFuncs.TriangleBarycentricQuadraticBase.BasesFuncs;

            double value = 0.0;

            for (int i = 0; i < Dofs.Length; i++)
                value += weights[Dofs[i]] * basicFuncs[i](localPoint);

            return value;
        }

        public bool IsPointOnElement(Vector2D[] VertexCoords, Vector2D point)
        {
            double x1 = VertexCoords[VertexNumber[0]].X, x2 = VertexCoords[VertexNumber[1]].X, x3 = VertexCoords[VertexNumber[2]].X;
            double y1 = VertexCoords[VertexNumber[0]].Y, y2 = VertexCoords[VertexNumber[1]].Y, y3 = VertexCoords[VertexNumber[2]].Y;
            double x0 = point.X, y0 = point.Y;

            double product1 = (x1 - x0) * (y2 - y1) - (x2 - x1) * (y1 - y0);
            double product2 = (x2 - x0) * (y3 - y2) - (x3 - x2) * (y2 - y0);
            double product3 = (x3 - x0) * (y1 - y3) - (x1 - x3) * (y3 - y0);

            if (product1 <= 0 && product2 <= 0 && product3 <= 0)
                return true;
            else if (product1 >= 0 && product2 >= 0 && product3 >= 0)
                return true;
            else
                return false;
        }

        public void SetEdgeDOF(int edge, int n, int dof)
        {
            switch (edge)
            {
                case 0:
                    Dofs[3] = dof;
                    break;

                case 1:
                    Dofs[4] = dof;
                    break;

                case 2:
                    Dofs[5] = dof;
                    break;

                default:
                    throw new Exception("Invalid number of edge");
            }
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

                case 2:
                    Dofs[2] = dof;
                    break;

                default:
                    throw new Exception("Invalid number of vertex");
            }
        }

        public IDataForFragmentation SplitToElements2D(IDictionary<(int i, int j), int> splits,
                                                       IDictionary<(int i, int j), (Vector2D vert, int num)[]> verticesOfSplitedEdges,
                                                       ref int countVertex)
        {
            (var edge1, var edge2, var edge3) = DefineOrderEdges(this);

            int split1 = (int)Math.Pow(2, splits[edge1]);
            int split2 = (int)Math.Pow(2, splits[edge2]);
            int split3 = (int)Math.Pow(2, splits[edge3]);

            var verticesOfEdge1 = verticesOfSplitedEdges[edge1];
            var verticesOfEdge2 = verticesOfSplitedEdges[edge2];
            var verticesOfEdge3 = verticesOfSplitedEdges[edge3];

            int minSplit = int.Min(split1, int.Min(split2, split3));

            var listVerticesFromCurElement = FindAllVerticesOfSplittedTriangle(split1, split2, split3,
                                                                               verticesOfEdge1, verticesOfEdge2, verticesOfEdge3,
                                                                               ref countVertex);

            var newElementsFromCurElement = SplitToTriangles(this, [.. listVerticesFromCurElement.Select(vertex => vertex.num)], minSplit);

            if (split1 / minSplit != 1)
                DoubleElemsOnEdge(split1 / minSplit, verticesOfEdge1, newElementsFromCurElement, listVerticesFromCurElement);
            if (split2 / minSplit != 1)
                DoubleElemsOnEdge(split2 / minSplit, verticesOfEdge2, newElementsFromCurElement, listVerticesFromCurElement);
            if (split3 / minSplit != 1)
                DoubleElemsOnEdge(split3 / minSplit, verticesOfEdge3, newElementsFromCurElement, listVerticesFromCurElement);

            return new DataForTriangleFragmentation(newElementsFromCurElement, listVerticesFromCurElement);
        }

        public IEnumerable<IFiniteElement> SplitToElements1D(int[] globalVerticesNums)
        {
            throw new NotSupportedException();
        }

        private double GetCoefAtLocalCoords(Vector2D[] VertexCoords, Func<Vector2D, double> coeff, Vector2D point)
        {
            Vector2D point1 = VertexCoords[VertexNumber[0]];
            Vector2D point2 = VertexCoords[VertexNumber[1]];
            Vector2D point3 = VertexCoords[VertexNumber[2]];

            Vector2D localPoint = new((point2.X - point1.X) * point.X + (point3.X - point1.X) * point.Y + point1.X,
                                      (point2.Y - point1.Y) * point.X + (point3.Y - point1.Y) * point.Y + point1.Y);

            return coeff(localPoint);
        }

        private Vector2D GetLocalPoint(Vector2D[] VertexCoords, Vector2D point)
        {
            Vector2D point1 = VertexCoords[VertexNumber[0]];
            Vector2D point2 = VertexCoords[VertexNumber[1]];
            Vector2D point3 = VertexCoords[VertexNumber[2]];

            double detD = DetD(VertexCoords);

            return new(((point3.X * point1.Y - point1.X * point3.Y) + (point3.Y - point1.Y) * point.X + (point1.X - point3.X) * point.Y) / detD,
                       ((point1.X * point2.Y - point2.X * point1.Y) + (point1.Y - point2.Y) * point.X + (point2.X - point1.X) * point.Y) / detD);
        }

        private double[,] GetMatrixJacobi(Vector2D[] VertexCoords)
        {
            Vector2D point1 = VertexCoords[VertexNumber[0]];
            Vector2D point2 = VertexCoords[VertexNumber[1]];
            Vector2D point3 = VertexCoords[VertexNumber[2]];

            double detD = DetD(VertexCoords);

            double[,] J = { { (point3.Y - point1.Y) / detD, (point1.Y - point2.Y) / detD },
                            { (point1.X - point3.X) / detD, (point2.X - point1.X) / detD } };

            return J;
        }

        private double DetD(Vector2D[] VertexCoords)
        {
            Vector2D point1 = VertexCoords[VertexNumber[0]];
            Vector2D point2 = VertexCoords[VertexNumber[1]];
            Vector2D point3 = VertexCoords[VertexNumber[2]];

            return (point2.X - point1.X) * (point3.Y - point1.Y) -
                   (point3.X - point1.X) * (point2.Y - point1.Y);
        }
    }
}
