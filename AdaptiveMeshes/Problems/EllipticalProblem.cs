using AdaptiveMeshes.FEM;
using AdaptiveMeshes.FiniteElements.Interfaces;
using AdaptiveMeshes.Matrices;
using AdaptiveMeshes.SLAE;
using AdaptiveMeshes.SLAESolvers;
using AdaptiveMeshes.Solution;
using AdaptiveMeshes.Solution.Interfaces;

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

        private PardisoSLAE? _slae;
        
        public IDictionary<string, IMaterial> Materials { get; }
        public ISolution Solution { get; }
        public IFiniteElementMesh Mesh { get; }

        public void Prepare()
        {
            FEMAlgorithms.EnumerateMeshDofs(Mesh);
            _slae = new PardisoSLAE(new PardisoMatrix(FEMAlgorithms.BuildPortraitFirstStep(Mesh), Quasar.Native.PardisoMatrixType.SymmetricIndefinite));
        }

        public double? Solve()
        {
            foreach (var element in Mesh.Elements)
            {
                var material = Materials[element.Material];

                if (material.IsVolume)
                {
                    var calculatingMatricesElement = (ICalculatingMatrices)element;
                    var localMatrix = calculatingMatricesElement.BuildLocalMatrix(Mesh.Vertex,
                                                                     IFiniteElement.MatrixTypeEnum.Stiffness,
                                                                     material.Lambda);
                    _slae?.Matrix.AddLocal(element.Dofs, localMatrix);

                    localMatrix = calculatingMatricesElement.BuildLocalMatrix(Mesh.Vertex,
                                                           IFiniteElement.MatrixTypeEnum.Mass,
                                                           material.Sigma);
                    _slae?.Matrix.AddLocal(element.Dofs, localMatrix);

                    var localRightPart = calculatingMatricesElement.BuildLocalRightPart(Mesh.Vertex, point => material.F(point, 0.0));
                    _slae?.AddLocalRightPart(element.Dofs, localRightPart);
                }
                else if (material.Is2)
                {
                    var calculatingMatricesElement = (ICalculatingMatricesForBoundaryConditions)element;
                    var localRightPart = calculatingMatricesElement.BuildLocalRightPartSecondBc(
                        Mesh.Vertex, 
                        point => material.Thetta(point, 0.0)
                        );
                    
                    _slae?.AddLocalRightPart(element.Dofs, localRightPart);
                }
            }

            foreach (var element in Mesh.Elements)
            {
                var material = Materials[element.Material];

                if (material.Is1)
                {
                    var calculatingMatricesElement = (ICalculatingMatricesForBoundaryConditions)element;
                    var localRightPart = calculatingMatricesElement.BuildLocalRightPartFirstBc(
                        Mesh.Vertex,
                        point => material.Ug(point, 0.0)
                        );
                    
                    _slae?.AddFirstBoundaryConditions(element.Dofs, localRightPart);
                }
            }

            using (var slaeSolver = new PardisoSLAESolver(_slae!))
            {
                slaeSolver.Prepare();
                Solution.SolutionVector = slaeSolver.Solve();
            }

            return _slae?.CalcDiscrepancy([.. Solution.SolutionVector]);
        }
    }
}
