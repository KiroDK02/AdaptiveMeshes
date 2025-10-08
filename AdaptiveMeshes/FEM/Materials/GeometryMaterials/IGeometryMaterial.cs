using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FEM.Materials.GeometryMaterials
{
    public interface IGeometryMaterial
    {
        Vector2D[] Vertices { get; }
        string MaterialName { get; }
        int[] VertexNumbers { get; }
        int NumberEdges { get; }

        (int i, int j) Edge(int edgei);

        IList<IGeometryMaterial> SplitToTriangles(IDictionary<(int i, int j), EdgesData> edgesData);

    }
}
