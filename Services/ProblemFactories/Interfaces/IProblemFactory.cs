using Core.FEM;
using Core.Problems;

namespace Services.ProblemFactories.Interfaces;

public interface IProblemFactory
{
    IProblem CreateProblem(
        ProblemType problemType,
        IDictionary<string, IMaterial> materials,
        IFiniteElementMesh mesh);
}