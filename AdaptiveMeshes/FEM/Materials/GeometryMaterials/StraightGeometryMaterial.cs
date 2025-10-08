using AdaptiveMeshes.Vectors;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveMeshes.FEM.Materials.GeometryMaterials
{
    public class StraightGeometryMaterial : IGeometryMaterial
    {
        public StraightGeometryMaterial(Vector2D[] vertices, string materialName, int[] vertexNumbers)
        {
            Vertices = vertices;
            MaterialName = materialName;
            VertexNumbers = vertexNumbers;
        }

        public Vector2D[] Vertices { get; }

        public string MaterialName { get; }

        public int[] VertexNumbers { get; } = new int[2];

        public int NumberEdges => 1;

        public (int i, int j) Edge(int edgei)
        {
            if (edgei != 0)
                throw new ArgumentOutOfRangeException(nameof(edgei));

            return (VertexNumbers[0], VertexNumbers[1]);
        }
            

        public IList<IGeometryMaterial> SplitToTriangles(IDictionary<(int i, int j), EdgesData> edgesData)
        {
            throw new NotImplementedException();
        }
    }
}
