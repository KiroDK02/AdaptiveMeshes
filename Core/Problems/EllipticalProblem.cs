using Core.FEM;
using Core.FiniteElements.Interfaces;
using Core.Matrices;
using Core.SLAE;
using Core.SLAESolvers;
using Core.Solution;
using Core.Solution.Interfaces;

namespace Core.Problems;

public class EllipticalProblem : IProblem
{
    private PardisoSLAE? _slae;
    public IDictionary<string, IMaterial> Materials { get; }
    public ISolution Solution { get; set; }
    public IFiniteElementMesh Mesh { get; }

    public EllipticalProblem(IDictionary<string, IMaterial> materials, IFiniteElementMesh mesh)
    {
        Materials = materials;
        Mesh = mesh;
        Solution = new SolutionStationaryProblem(Mesh);
    }

    public void Prepare()
    {
        FEMAlgorithms.EnumerateMeshDofs(Mesh);
        _slae = new PardisoSLAE(
            new PardisoMatrix(
                FEMAlgorithms.BuildPortraitFirstStep(Mesh),
                Quasar.Native.PardisoMatrixType.SymmetricIndefinite
            )
        );
    }

    public double? Solve()
    {
        foreach (var element in Mesh.Elements)
        {
            var material = Materials[element.Material];

            if (material.IsVolume)
            {
                var calculatingMatrices = (ICalculatingMatrices)element;
                double[,] localMatrix = calculatingMatrices.BuildLocalMatrix(Mesh.Vertex,
                    IFiniteElement.MatrixTypeEnum.Stiffness,
                    material.Lambda);
                _slae?.Matrix.AddLocal(element.Dofs, localMatrix);

                localMatrix = calculatingMatrices.BuildLocalMatrix(Mesh.Vertex,
                    IFiniteElement.MatrixTypeEnum.Mass,
                    material.Sigma);
                _slae?.Matrix.AddLocal(element.Dofs, localMatrix);

                double[] localRightPart =
                    calculatingMatrices.BuildLocalRightPart(Mesh.Vertex, point => material.F(point, 0.0));
                _slae?.AddLocalRightPart(element.Dofs, localRightPart);
            }
            else if (material.Is2)
            {
                var calculatingMatrices = (ICalculatingMatricesForBoundaryConditions)element;
                double[] localRightPart = calculatingMatrices.BuildLocalRightPartSecondBc(
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
                var calculatingMatrices = (ICalculatingMatricesForBoundaryConditions)element;
                double[] localRightPart = calculatingMatrices.BuildLocalRightPartFirstBc(
                    Mesh.Vertex,
                    point => material.Ug(point, 0.0)
                );

                _slae?.AddFirstBoundaryConditions(element.Dofs, localRightPart);
            }
        }

        using (var _slaeSolver = new PardisoSLAESolver(_slae!))
        {
            _slaeSolver.Prepare();
            Solution.SolutionVector = _slaeSolver.Solve();
        }

        return _slae?.CalcDiscrepancy(Solution.SolutionVector);
    }
}