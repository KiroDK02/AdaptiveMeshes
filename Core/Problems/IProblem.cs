using Core.FEM;
using Core.Solution.Interfaces;

namespace Core.Problems;

public interface IProblem
{
    IDictionary<string, IMaterial> Materials { get; }
    ISolution Solution { get; set; }
    IFiniteElementMesh Mesh { get; }

    void Prepare();
    double? Solve();
}