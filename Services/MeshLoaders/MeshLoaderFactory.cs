using Services.MeshLoaders.Interfaces;

namespace Services.MeshLoaders;

public class MeshLoaderFactory
{
    public static MeshLoaderFactory Instance
    {
        get
        {
            field ??= new();
            return field;
        }
    }
    
    private readonly MeshLoaderTxt _meshLoaderTxt = new();
    
    private MeshLoaderFactory() { }
    
    public IMeshLoader CreateMeshLoader(string file) =>
        Path.GetExtension(file) switch
        {
            ".txt" => _meshLoaderTxt,
            _ => throw new ArgumentException("Unknown file format")
        };
}