using AdaptiveMeshes.FEM.Materials.GeometryMaterials;

namespace AdaptiveMeshes.FEM.Mesh.MeshGenerator
{
    public interface IMeshGenerator
    {
        IFiniteElementMesh GenerateMesh();
    }
}
