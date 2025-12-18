using Core.FEM;
using Core.Problems;

namespace Services.ProblemFactories.Interfaces;

public interface IProblemFactory
{
    public enum ProblemType
    {
        EllipticalProblem
    }
    
    IProblem CreateProblem(
        ProblemType problemType,
        IDictionary<string, IMaterial> materials,
        IFiniteElementMesh mesh);
}