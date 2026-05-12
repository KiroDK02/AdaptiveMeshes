using Services.MeshLoaders;

namespace DataTransferObjects;

public class MeshDto
{
    public string MeshFilePath { get; init; } =  string.Empty;
    public MeshLoaderType LoaderType { get; init; }
}