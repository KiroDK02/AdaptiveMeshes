using Core.FEM;

namespace DataTransferObjects;

/// <summary>
/// Данные материала для сериализации/десериализации проблемы в\из json-файл(а)
/// </summary>
public class MaterialDto
{
    public string Name { get; init; } = string.Empty;
    public uint ARGB { get; init; }
    public MaterialType MaterialType { get; init; }
    
    public string LambdaBody { get; init; } = "0";
    public string SigmaBody { get; init; } = "0";
    public string FBody { get; init; } = "0";
    public string UgBody { get; init; } = "0";
    public string ThettaBody { get; init; } = "0";
}