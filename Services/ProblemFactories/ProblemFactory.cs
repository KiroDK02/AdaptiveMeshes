using Core.FEM;
using Core.Problems;
using Services.ProblemFactories.Interfaces;

namespace Services.ProblemFactories;

public class ProblemFactory : IProblemFactory
{
    public IProblem CreateProblem(
        IProblemFactory.ProblemType problemType,
        IDictionary<string, IMaterial> materials,
        IFiniteElementMesh mesh)
    {
        return problemType switch
        {
            IProblemFactory.ProblemType.EllipticalProblem => new EllipticalProblem(materials, mesh),
            
            _ => throw new ArgumentException("Unknown problem type.")
        };
    }
}