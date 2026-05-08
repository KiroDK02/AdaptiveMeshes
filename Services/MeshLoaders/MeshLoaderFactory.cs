using System;
using System.IO;
using System.Threading;
using Services.MeshLoaders.Interfaces;

namespace Services.MeshLoaders;

public class MeshLoaderFactory
{
    private static readonly Lazy<MeshLoaderFactory> LazyInstance =
        new(() => new MeshLoaderFactory(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    public static MeshLoaderFactory Instance => LazyInstance.Value;
    
    private readonly MeshLoaderTxt _meshLoaderTxt = new();
    
    private MeshLoaderFactory() { }
    
    public IMeshLoader CreateMeshLoader(string file) =>
        Path.GetExtension(file) switch
        {
            ".txt" => _meshLoaderTxt,
            _ => throw new ArgumentException("Unknown file format")
        };
}