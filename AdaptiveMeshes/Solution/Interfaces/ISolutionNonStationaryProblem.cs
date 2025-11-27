using AdaptiveMeshes.TimeMesh;

namespace AdaptiveMeshes.Solution.Interfaces;

public interface ISolutionNonStationaryProblem : ISolution
{
    ITimeMesh TimeMesh { get; }
    double Time { get; set; }
    
    void AddSolutionVector(double t, double[] solution);
}