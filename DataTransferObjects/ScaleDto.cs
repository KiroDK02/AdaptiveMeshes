using System.Collections.Generic;

namespace DataTransferObjects;

public class ScaleDto
{
    public string Name { get; init; } = string.Empty;

    public List<double> DistanceFromMinParts { get; init; } = [];
    public List<int> ScaleDistribution { get; init; } = [];
}