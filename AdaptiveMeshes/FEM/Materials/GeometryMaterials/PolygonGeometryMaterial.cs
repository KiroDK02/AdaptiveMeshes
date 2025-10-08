using AdaptiveMeshes.FEM.Mesh.MeshGenerator;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.Vectors;


namespace AdaptiveMeshes.FEM.Materials.GeometryMaterials
{
    public class PolygonGeometryMaterial : GeometryMaterial
    {
        public PolygonGeometryMaterial(Vector2D[] vertices, string materialName, int[] vertexNumbers) 
            : base(vertices, materialName, vertexNumbers) { }


        public override IList<IGeometryMaterial> SplitToTriangles(IDictionary<(int i, int j), EdgesData> edgesData)
        {
            List<IGeometryMaterial> triangles = [];
            List<int> vertices = [.. VertexNumbers];

            // Не факт, что так сработает, мб последний нужно после цикла все-таки добавлять.
            while (vertices.Count >= 3)
            {
                bool earFound = false;

                for (int i = 0; i < vertices.Count; i++)
                {
                    if (AlgorithmsForMeshGeneration.IsEar(Vertices, vertices, i))
                    {
                        int i0 = (i - 1 + vertices.Count) % vertices.Count;
                        int i1 = i;
                        int i2 = (i + 1) % vertices.Count;

                        int iA = vertices[i0];
                        int iB = vertices[i1];
                        int iC = vertices[i2];

                        AlgorithmsForMeshGeneration.FindDataForNewEdge(iA, iB, iC, edgesData);

                        triangles.Add(new PolygonGeometryMaterial(Vertices, MaterialName, [iA, iB, iC]));

                        vertices.RemoveAt(i);
                        earFound = true;
                        break;
                    }
                }

                if (!earFound)
                    throw new InvalidOperationException("Couldn't find the ear.");
            }

            return triangles;
        }
    }
}
