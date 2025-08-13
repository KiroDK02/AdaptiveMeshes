using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.Matrices;
using AdaptiveMeshes.SLAE;
using AdaptiveMeshes.SLAESolvers;
using AdaptiveMeshes.Solution;

namespace AdaptiveMeshes.Problems
{
    public class EllipticalProblem : IProblem
    {
        public EllipticalProblem(IDictionary<string, IMaterial> materials, IFiniteElementMesh mesh)
        {
            Materials = materials;
            Mesh = mesh;
            Solution = new SolutionStationaryProblem(Mesh);
        }

        public IDictionary<string, IMaterial> Materials { get; }
        public ISolution Solution { get; set; }
        public IFiniteElementMesh Mesh { get; }
        PardisoSLAE? SLAE { get; set; }

        public void Prepare()
        {
            FEMAlgorithms.EnumerateMeshDofs(Mesh);
            SLAE = new PardisoSLAE(new PardisoMatrix(FEMAlgorithms.BuildPortraitFirstStep(Mesh), Quasar.Native.PardisoMatrixType.SymmetricIndefinite));
        }

        public double? Solve()
        {
            foreach (var element in Mesh.Elements)
            {
                IMaterial material = Materials[element.Material];

                if (material.IsVolume)
                {
                    double[,] localMatrix = element.BuildLocalMatrix(Mesh.Vertex,
                                                                     IFiniteElement.MatrixTypeEnum.Stiffness,
                                                                     material.Lambda);
                    SLAE?.Matrix.AddLocal(element.Dofs, localMatrix);

                    localMatrix = element.BuildLocalMatrix(Mesh.Vertex,
                                                           IFiniteElement.MatrixTypeEnum.Mass,
                                                           material.Sigma);
                    SLAE?.Matrix.AddLocal(element.Dofs, localMatrix);

                    double[] localRightPart = element.BuildLocalRightPart(Mesh.Vertex, point => material.F(point, 0.0));
                    SLAE?.AddLocalRightPart(element.Dofs, localRightPart);
                }
                else if (material.Is2)
                {
                    double[] localRightPart = element.BuildLocalRightPartSecondBC(Mesh.Vertex, point => material.Thetta(point, 0.0));
                    SLAE?.AddLocalRightPart(element.Dofs, localRightPart);
                }
            }

            foreach (var element in Mesh.Elements)
            {
                var material = Materials[element.Material];

                if (material.Is1)
                {
                    double[] localRightPart = element.BuildLocalRightPartFirstBC(Mesh.Vertex, point => material.Ug(point, 0.0));
                    SLAE?.AddFirstBoundaryConditions(element.Dofs, localRightPart);
                }
            }

            using (var SLAESolver = new PardisoSLAESolver(SLAE!))
            {
                SLAESolver.Prepare();
                Solution.SolutionVector = SLAESolver.Solve();
            }

            return SLAE?.CalcDiscrepancy([.. Solution.SolutionVector]);
        }
    }
}
