using System.Collections.Generic;
using Core.Problems;

namespace DataTransferObjects;

/// <summary>
/// Данные проблемы для сериализации/десериализации в/из json-файл(а)
/// </summary>
public class ProblemDto
{
    public string ProblemName { get; init; } = string.Empty;
    public ProblemType SelectedProblemType { get; init; }
    
    public string MeshFilePath { get; init; } = string.Empty;
    public List<MaterialDto> Materials { get; init; } = [];
    
    public List<PointDto> Points { get; init; } = [];
    
    public required AdaptationDto AdaptationDto { get; init; }
}