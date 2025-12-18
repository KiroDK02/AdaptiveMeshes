using Core.TimeMesh;

namespace Core.Solution.Interfaces;

public interface ISolutionNonStationaryProblem : ISolution
{
    ITimeMesh TimeMesh { get; }
    double Time { get; set; }
    
    void AddSolutionVector(double t, double[] solution);
}