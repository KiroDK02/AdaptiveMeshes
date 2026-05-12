using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Threading;
using Services.MeshLoaders.Interfaces;

namespace Services.MeshLoaders;

public class MeshLoaderFactory
{
    private static readonly Lazy<MeshLoaderFactory> LazyInstance =
        new(() => new MeshLoaderFactory(), LazyThreadSafetyMode.ExecutionAndPublication);
    
    public static MeshLoaderFactory Instance => LazyInstance.Value;
    
    private readonly MeshLoaderKiro2D _meshLoaderKiro2D = new();
    private readonly MeshLoaderTelma2D _meshLoaderTelma2D = new();
    
    private MeshLoaderFactory() { }
    
    public IMeshLoader CreateMeshLoader(string file) =>
        Path.GetExtension(file) switch
        {
            ".txt" => _meshLoaderKiro2D,
            _ => throw new ArgumentException("Unknown file format")
        };

    public IMeshLoader CreateMeshLoader(MeshLoaderType type) =>
        type switch
    {
        MeshLoaderType.Kiro2D => _meshLoaderKiro2D,
        MeshLoaderType.Telma2D => _meshLoaderTelma2D,
        _ => throw new ArgumentException("Unknown file format")
    };
}

public enum MeshLoaderType
{
    Kiro2D,
    Telma2D
}