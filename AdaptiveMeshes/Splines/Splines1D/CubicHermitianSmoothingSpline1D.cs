using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FEM.Mesh;
using AdaptiveMeshes.Matrices;
using AdaptiveMeshes.SLAE;
using AdaptiveMeshes.SLAESolvers;

namespace AdaptiveMeshes.Splines.Splines1D
{
    public class CubicHermitianSmoothingSpline1D : ISmoothingSpline<double>
    {
        public CubicHermitianSmoothingSpline1D(double[] points, IFiniteElementMesh mesh, double alpha = 0.0, double betta = 0.0)
            : this(points, mesh, [], alpha, betta)
        {
            W = new double[points.Length];

            Array.Fill(W, 1.0);
        }
        public CubicHermitianSmoothingSpline1D(double[] points, IFiniteElementMesh mesh, double[] w, double alpha = 0.0, double betta = 0.0)
        {
            Points = points;
            Mesh = mesh;
            Alpha = alpha;
            Betta = betta;
            W = w;

            Prepare();
        }

        public double[] W { get; }
        public double[] Points { get; }

        double[] weights = [];
        public ReadOnlySpan<double> Weights
        {
            get => weights;
            set => weights = value.ToArray();
        }

        public IFiniteElementMesh Mesh { get; }

        public double Alpha { get; set; }

        public double Betta { get; set; }

        private ISLAE? SLAE { get; set; }

        public void BuildSpline(double[] valuesFuncAtPoints)
        {
            foreach (var element in Mesh.Elements)
            {
                double[,] localMatrix = element.BuildLocalMatrix(Mesh.Vertex, FiniteElements.IFiniteElement.MatrixTypeEnum.Stiffness, p => Alpha);
                SLAE?.Matrix.AddLocal(element.Dofs, localMatrix);

                localMatrix = element.BuildLocalMatrix(Mesh.Vertex, FiniteElements.IFiniteElement.MatrixTypeEnum.RegularizationSecondDerivatives, p => Betta);
                SLAE?.Matrix.AddLocal(element.Dofs, localMatrix);
            }

            var localBaseFuncs = BaseFuncs.CubicHermitianBase1D.BasesFuncs;

            for (int j = 0; j < Points.Length; j++)
            {
                foreach (var element in Mesh.Elements)
                {
                    if (element.IsPointOnElement(Mesh.Vertex, new(Points[j], 0)))
                    {
                        double[,] localMatrix = new double[4, 4];
                        double[] localRightPart = new double[4];

                        double x0 = Mesh.Vertex[element.VertexNumber[0]].X;
                        double x1 = Mesh.Vertex[element.VertexNumber[1]].X;
                        double h = x1 - x0;
                        double ksi = (Points[j] - x0) / h;

                        for (int v = 0; v < 4; v++)
                        {
                            double psiv = v % 2 == 0 ? localBaseFuncs[v](ksi) : h * localBaseFuncs[v](ksi);

                            for (int u = 0; u < 4; u++)
                            {
                                double psiu = u % 2 == 0 ? localBaseFuncs[u](ksi) : h * localBaseFuncs[u](ksi);

                                localMatrix[v, u] = W[j] * psiv * psiu;
                            }

                            localRightPart[v] = W[j] * psiv * valuesFuncAtPoints[j];
                        }

                        SLAE?.Matrix.AddLocal(element.Dofs, localMatrix);
                        SLAE?.AddLocalRightPart(element.Dofs, localRightPart);

                        break;
                    }
                }
            }

            using (var SLAESolver = new PardisoSLAESolver(SLAE!))
            {
                SLAESolver.Prepare();
                Weights = SLAESolver.Solve();
            }
        }

        private void Prepare()
        {
            FEMAlgorithms.EnumerateMeshDofs(Mesh);
            SLAE = new PardisoSLAE(new PardisoMatrix(FEMAlgorithms.BuildPortraitFirstStep(Mesh), Quasar.Native.PardisoMatrixType.SymmetricIndefinite));
        }

        public double Gradient(double point)
        {
            foreach (var element in Mesh.Elements)
            {
                if (element.IsPointOnElement(Mesh.Vertex, new(point, 0.0)))
                    return element.GetGradientAtPoint(Mesh.Vertex, Weights, new(point, 0.0)).X;
            }

            throw new ArgumentException("Point doesn't belong to any element.");
        }

        public double Value(double point)
        {
            foreach (var element in Mesh.Elements)
            {
                if (element.IsPointOnElement(Mesh.Vertex, new(point, 0.0)))
                    return element.GetValueAtPoint(Mesh.Vertex, Weights, new(point, 0.0));
            }

            throw new ArgumentException("Point doesn't belong to any element.");
        }
    }
}
