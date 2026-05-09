namespace DataTransferObjects;

/// <summary>
/// Данные значений решения в точках для сериализации/десериализации проблемы в\из json-файл(а)
/// </summary>
public class PointDto
{
    public double X { get; init; }
    public double Y { get; init; }
    public double? Value { get; init; }
}