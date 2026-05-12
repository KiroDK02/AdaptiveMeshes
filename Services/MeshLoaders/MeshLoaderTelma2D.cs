using Core.FEM;
using Services.MeshLoaders.Interfaces;
using System.Threading.Tasks;

namespace Services.MeshLoaders;

public class MeshLoaderTelma2D : IMeshLoader
{
    public Task<IFiniteElementMesh> LoadMeshAsync(string path)
    {
        throw new System.NotImplementedException();
    }

    public Task SaveMeshToFileAsync(IFiniteElementMesh mesh, string path)
    {
        throw new System.NotImplementedException();
    }
}
