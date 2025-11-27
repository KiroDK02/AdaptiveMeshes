using AdaptiveMeshes.FEM;
using AdaptiveMeshes.Solution;
using AdaptiveMeshes.Solution.Interfaces;

namespace AdaptiveMeshes.Problems
{
    public interface IProblem
    {
        IDictionary<string, IMaterial> Materials { get; }
        ISolution Solution { get; }
        IFiniteElementMesh Mesh { get; }

        void Prepare();
        double? Solve();
    }
}
