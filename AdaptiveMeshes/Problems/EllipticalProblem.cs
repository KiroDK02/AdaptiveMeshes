using AdaptiveMeshes.FEM;
using AdaptiveMeshes.Interfaces;
using AdaptiveMeshes.Matrices;
using AdaptiveMeshes.SLAE;

namespace AdaptiveMeshes.Problems
{
    public class EllipticalProblem : IProblem
    {
        public EllipticalProblem(IDictionary<string, IMaterial> materials, IFiniteElementMesh mesh)
        {
            Materials = materials;
            Mesh = mesh;
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

        public double? Solve(ISolution solution)
        {
            throw new NotImplementedException();
        }
    }
}
