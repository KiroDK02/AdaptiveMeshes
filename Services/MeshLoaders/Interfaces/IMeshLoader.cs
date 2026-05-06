using System.Threading.Tasks;
using Core.FEM;

namespace Services.MeshLoaders.Interfaces;

public interface IMeshLoader
{
    Task<IFiniteElementMesh> LoadMeshAsync(string path);
    Task SaveMeshToFileAsync(IFiniteElementMesh mesh, string path);
}