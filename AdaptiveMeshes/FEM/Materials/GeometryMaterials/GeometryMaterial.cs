using AdaptiveMeshes.FEM.Mesh.MeshGenerator;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FEM.Materials.GeometryMaterials
{
    public abstract class GeometryMaterial : IGeometryMaterial
    {
        public GeometryMaterial(Vector2D[] vertices, string materialName, int[] vertexNumbers)
        {
            Vertices = vertices;
            MaterialName = materialName;
            VertexNumbers = vertexNumbers;
            
            if (!AlgorithmsForMeshGeneration.IsCCW(vertices, vertexNumbers))
                Array.Reverse(VertexNumbers);
        }

        public Vector2D[] Vertices { get; }
        public string MaterialName { get; }

        public int[] VertexNumbers { get; }

        public int NumberEdges => VertexNumbers.Length;

        public (int i, int j) Edge(int edgei)
        {
            if (edgei < 0 || edgei > NumberEdges)
                throw new ArgumentOutOfRangeException(nameof(edgei));

            (int i, int j) edge = (VertexNumbers[edgei], VertexNumbers[(edgei + 1) % VertexNumbers.Length]);

            return edge;
        }

        public abstract IList<IGeometryMaterial> SplitToTriangles(IDictionary<(int i, int j), EdgesData> edgesData);
    }
}
