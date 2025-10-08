using AdaptiveMeshes.BaseFuncs;
using AdaptiveMeshes.FEM.Materials.GeometryMaterials;
using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveMeshes.FiniteElements.FiniteElements1D
{
    public class SegmentFECubicHermitianBase : IFiniteElement
    {
        public SegmentFECubicHermitianBase(string material, int[] vertexNumber)
        {
            Material = material;
            VertexNumber = vertexNumber;
        }

        public string Material { get; }

        public int[] VertexNumber { get; } = new int[2];

        public int[] Dofs { get; } = new int[4];

        public int NumberOfEdges => 1;

        public double[,] BuildLocalMatrix(Vector2D[] VertexCoords, IFiniteElement.MatrixTypeEnum type, Func<Vector2D, double> Coeff)
        {
            double[,] localMatrix = new double[4, 4];

            double x0 = VertexCoords[VertexNumber[0]].X;
            double x1 = VertexCoords[VertexNumber[1]].X;
            double h = x1 - x0;
            double averageCoef = (Coeff(VertexCoords[VertexNumber[0]]) + Coeff(VertexCoords[VertexNumber[1]])) / 2.0;

            return type switch
            {
                IFiniteElement.MatrixTypeEnum.Stiffness => GetStiffnessMatrix(h, averageCoef),

                IFiniteElement.MatrixTypeEnum.RegularizationSecondDerivatives => GetSecondRegulazationMatrix(h, averageCoef),

                _ => throw new ArgumentException("Invalid type of matrix.")
            };
        }

        private double[,] GetStiffnessMatrix(double h, double averageLambda)
        {
            return new[,] { {  averageLambda * 6.0 / (5.0 * h),  averageLambda * 1.0 / 10.0,     -averageLambda * 6.0 / (5.0 * h),  averageLambda * 1.0 / 10.0     },
                            {  averageLambda * 1.0 / 10.0,       averageLambda * 2.0 * h / 15.0, -averageLambda * 1.0 / 10.0,      -averageLambda * h / 30.0       },
                            { -averageLambda * 6.0 / (5.0 * h), -averageLambda * 1.0 / 10.0,      averageLambda * 6.0 / (5.0 * h), -averageLambda * 1.0 / 10.0     },
                            {  averageLambda * 1.0 / 10.0,      -averageLambda * h / 30.0,       -averageLambda * 1.0 / 10.0,       averageLambda * 2.0 * h / 15.0 } };
        }

        private double[,] GetSecondRegulazationMatrix(double h, double averageLambda)
        {
            return new[,] {{ averageLambda * 12.0 / (h * h * h), averageLambda * 6.0 / (h * h), -averageLambda * 12.0 / (h * h * h), averageLambda * 6.0 / (h * h)},
                           { averageLambda * 6.0 / (h * h),      averageLambda * 4.0 / h,       -averageLambda * 6.0 / (h * h),      averageLambda * 2.0 / h      },
                           {-averageLambda * 12.0 / (h * h * h),-averageLambda * 6.0 / (h * h),  averageLambda * 12.0 / (h * h * h),-averageLambda * 6.0 / (h * h)},
                           { averageLambda * 6.0 / (h * h),      averageLambda * 2.0 / h,       -averageLambda * 6.0 / (h * h),      averageLambda * 4.0 / h     } };
        }

        public double[] BuildLocalRightPart(Vector2D[] VertexCoords, Func<Vector2D, double> F)
            => throw new NotSupportedException();

        public double[] BuildLocalRightPartFirstBC(Vector2D[] VertexCoords, Func<Vector2D, double> Ug)
        {
            throw new NotImplementedException();
        }

        public double[] BuildLocalRightPartSecondBC(Vector2D[] VertexCoords, Func<Vector2D, double> Thetta)
        {
            throw new NotImplementedException();
        }

        public int DOFOnEdge(int edge) => 0;

        public int DOFOnElement() => 0;

        public int DOFOnVertex(int vertex) => 2;

        public (int i, int j) Edge(int edge)
            => edge == 0 ? (0, 1) : throw new ArgumentException("Invalid number of edge.");

        public Vector2D GetGradientAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false)
        {
            var localGradientBaseFunc = CubicHermitianBase1D.GradientBaseFuncs;

            double x0 = VertexCoords[VertexNumber[0]].X;
            double x1 = VertexCoords[VertexNumber[1]].X;
            double h = x1 - x0;

            double ksi = (point.X - x0) / h;
            double value = 0;

            for (int i = 0; i < 4; i++)
            {
                double psii = i % 2 == 0 ? localGradientBaseFunc[i](ksi) / h : localGradientBaseFunc[i](ksi);

                value += weights[Dofs[i]] * psii;
            }

            return new(value, 0);

        }

        public Vector2D GetOuterNormalToEdge(Vector2D[] VertexCoords, int edgei, bool normalize = false)
            => throw new NotSupportedException();

        public double GetValueAtPoint(Vector2D[] VertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false)
        {
            var localBaseFuncs = CubicHermitianBase1D.BasesFuncs;

            double x0 = VertexCoords[VertexNumber[0]].X;
            double x1 = VertexCoords[VertexNumber[1]].X;
            double h = x1 - x0;

            double ksi = (point.X - x0) / h;
            double value = 0;

            for (int i = 0; i < 4; i++)
            {
                double psii = i % 2 == 0 ? localBaseFuncs[i](ksi) : h * localBaseFuncs[i](ksi);
                value += weights[Dofs[i]] * psii;
            }

            return value;
        }

        public bool IsPointOnElement(Vector2D[] VertexCoords, Vector2D point)
        {
            double x0 = VertexCoords[VertexNumber[0]].X;
            double x1 = VertexCoords[VertexNumber[1]].X;
            double x = point.X;

            return x0 <= x && x <= x1;
        }

        public void SetEdgeDOF(int edge, int n, int dof)
            => throw new NotSupportedException();

        public void SetElementDOF(int n, int dof)
            => throw new NotSupportedException();

        public void SetVertexDOF(int vertex, int n, int dof)
        {
            switch (vertex)
            {
                case 0:
                {
                    Dofs[n] = dof;

                    break;
                }

                case 1:
                {
                    Dofs[n + 2] = dof;

                    break;
                }
            }
        }

        public IEnumerable<IFiniteElement> SplitToElements1D(int[] globalVerticesNums)
            => throw new NotSupportedException();

        public IDataForFragmentation SplitToElements2D(IDictionary<(int i, int j), int> splits, IDictionary<(int i, int j), (Vector2D vert, int num)[]> verticesOfSplitedEdges, ref int countVertex)
            => throw new NotSupportedException();

        public IDataForFragmentation SplitToElements2D(IDictionary<(int i, int j), EdgesData> splits, IDictionary<(int i, int j), (Vector2D vert, int num)[]> verticesOfSplitedEdges, ref int countVertex)
        {
            throw new NotImplementedException();
        }
    }
}
