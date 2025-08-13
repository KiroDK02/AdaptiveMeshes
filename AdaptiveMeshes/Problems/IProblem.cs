using AdaptiveMeshes.FEM;
using AdaptiveMeshes.Solution;

namespace AdaptiveMeshes.Problems
{
    public interface IProblem
    {
        IDictionary<string, IMaterial> Materials { get; }
        ISolution Solution { get; set; }

        void Prepare();
        double? Solve();
    }
}
