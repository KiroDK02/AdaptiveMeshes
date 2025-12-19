using DataTransfers;

namespace Services.ProblemFactories.Interfaces;

public interface IProblemLoader
{
    Task<ProblemDto> LoadProblemFromFile(string file);
    Task SaveProblemToFile(string file, ProblemDto problem);
}