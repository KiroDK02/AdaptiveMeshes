using AdaptiveMeshes.FEM.Materials.GeometryMaterials;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveMeshes.FEM.Mesh.MeshGenerator
{
    public class Area(IEnumerable<IGeometryMaterial> geometryMaterials, Dictionary<(int i, int j), EdgesData> edgesData)
    {
        public IEnumerable<IGeometryMaterial> GeometryMaterials { get; } = geometryMaterials;
        public Dictionary<(int i, int j), EdgesData> EdgesData { get; } = edgesData;
    }
}
