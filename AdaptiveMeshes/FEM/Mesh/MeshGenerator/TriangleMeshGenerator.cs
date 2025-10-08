using AdaptiveMeshes.Adaptation.SplitStrategies.SplitStrategies2DMeshes;
using AdaptiveMeshes.FEM.Materials.GeometryMaterials;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.FiniteElements.FiniteElements1D;
using AdaptiveMeshes.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;
using AdaptiveMeshes.Problems;
using AdaptiveMeshes.Vectors;
using System.Xml.Linq;

namespace AdaptiveMeshes.FEM.Mesh.MeshGenerator
{
    public class TriangleMeshGenerator : IMeshGenerator
    {
        // Допустим в geometryMaterials пришли все геометрии материалов и число разбиений ребер
        // Дискретизацию делаем через степени двойки, остальное впадлу
        public TriangleMeshGenerator(IEnumerable<IGeometryMaterial> geometryMaterials, Vector2D[] vertices,
                                     int countIntervalsForAll, double ratioForAll)
        {
            _geometryMaterials = geometryMaterials;
            _vertices = vertices;
            _edgesData = [];

            FillFullEdgesData(countIntervalsForAll, ratioForAll);
        }
        
        public TriangleMeshGenerator(IEnumerable<IGeometryMaterial> geometryMaterials, Vector2D[] vertices,
                                     Dictionary<(int i, int j), EdgesData> edgesData)
        {
            _geometryMaterials = geometryMaterials;
            _vertices = vertices;
            _edgesData = edgesData;

            FillFullEdgesData();
        }

        private readonly IEnumerable<IGeometryMaterial> _geometryMaterials;
        private readonly Vector2D[] _vertices;
        private readonly Dictionary<(int i, int j), EdgesData> _edgesData;

        private readonly List<IFiniteElement> _elements = [];
        private readonly List<IGeometryMaterial> _newGeometryMaterials = [];

        public IFiniteElementMesh GenerateMesh()
        {
            SplitToTriangleGeometryMateriels();
            GetFiniteElements();
            return Triangulation();
        }

        private void SplitToTriangleGeometryMateriels()
        {
            foreach (var gM in _geometryMaterials)
            {
                if (gM is PolygonGeometryMaterial polygonGM)
                    _newGeometryMaterials.AddRange(polygonGM.SplitToTriangles(_edgesData));
                else if (gM is StraightGeometryMaterial straightGM)
                    _newGeometryMaterials.Add(straightGM);
            }
        }

        private IFiniteElementMesh Triangulation()
        {
            SmoothOutSplits();

            int countVertex = _vertices.Length;
            var verticesSplitedEdges = CalcVerticesEdges(ref countVertex);

            List<IFiniteElement> newElements = [];
            List<(Vector2D vert, int num)> newVertices = [];

            foreach (var element in _elements)
            {
                if (element.VertexNumber.Length != 2)
                {
                    var datas = element.SplitToElements2D(_edgesData, verticesSplitedEdges, ref countVertex);

                    newElements.AddRange(datas.NewElements);
                    newVertices.AddRange(datas.NewVertices);
                }
                else
                {
                    var elements = element.SplitToElements1D([.. verticesSplitedEdges[element.GlobalEdge(0)].Select(vertex => vertex.num)]);

                    newElements.AddRange(elements);
                }
            }

            Vector2D[] vertices = new Vector2D[countVertex];

            foreach ((Vector2D vert, int number) in newVertices)
                vertices[number] = vert;

            return new FiniteElementMesh(newElements, vertices);
        }

        private void GetFiniteElements()
        {
            foreach (var gM in _newGeometryMaterials)
            {
                if (gM is PolygonGeometryMaterial polygonGM)
                    _elements.Add(new TriangleFEQuadraticBaseWithNI(polygonGM.MaterialName, polygonGM.VertexNumbers));
                else if (gM is StraightGeometryMaterial straightGM)
                    _elements.Add(new SegmentFEQuadraticBaseWithNI(straightGM.MaterialName, straightGM.VertexNumbers));
            }
        }

        private void SmoothOutSplits()
        {
            bool stop = false;

            while (!stop)
            {
                stop = true;
                foreach (var element in _elements)
                {
                    if (element.VertexNumber.Length == 2)
                        continue;

                    int maxSplit = FindMaxSplitInElement(_edgesData, element);

                    for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                    {
                        var edge = element.GlobalEdge(edgei);

                        int differenceBtwSplits = maxSplit - _edgesData[edge].CountIntervals;
                        if (differenceBtwSplits > 1)
                        {
                            stop = false;
                            _edgesData[edge].CountIntervals = maxSplit - 1;
                        }
                    }
                }
            }
        }

        private static int FindMaxSplitInElement(IDictionary<(int i, int j), EdgesData> splits, IFiniteElement element)
        {
            int max = 0;

            for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
            {
                var edge = element.GlobalEdge(edgei);
                max = int.Max(max, splits[edge].CountIntervals);
            }

            return max;
        }

        private IDictionary<(int i, int j), (Vector2D vert, int num)[]> CalcVerticesEdges(ref int countVertices)
        {
            Dictionary<(int i, int j), (Vector2D vert, int num)[]> verticesEdges = [];

            foreach (var element in _elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                {
                    var edge = element.GlobalEdge(edgei);

                    if (verticesEdges.ContainsKey(edge))
                        continue;

                    Vector2D v0 = _vertices[edge.i];
                    Vector2D v1 = _vertices[edge.j];

                    int split = (int)Math.Pow(2, _edgesData[edge].CountIntervals);
                    double ratio = _edgesData[edge].Ratio;

                    var newVertices = new (Vector2D vert, int num)[split + 1];

                    if (ratio == 1)
                    {
                        Vector2D h = (v1 - v0) / split;

                        newVertices[0] = (v0, edge.i);
                        for (int k = 1; k < split; k++)
                        {
                            Vector2D newVertex = v0 + h * k;
                            newVertices[k] = (newVertex, countVertices++);
                        }
                        newVertices[split] = (v1, edge.j);

                        verticesEdges[edge] = newVertices;
                    }
                    else
                    {
                        double sumProgression = (Math.Pow(ratio, split) - 1.0) / (ratio - 1.0);
                        Vector2D h = (v1 - v0) / sumProgression;

                        newVertices[0] = (v0, edge.i);
                        for (int k = 1; k < split; k++)
                        {
                            Vector2D newVertex = v0 + h * (Math.Pow(ratio, k) - 1.0) / (ratio - 1.0);
                            newVertices[k] = (newVertex, countVertices++);
                        }
                        newVertices[split] = (v1, edge.j);
                    }

                    verticesEdges[edge] = newVertices;
                }
            }

            return verticesEdges;
        }

        private void FillFullEdgesData(int countIntervals = 0, double ratio = 1.0)
        {
            foreach (var gM in _geometryMaterials)
            {
                for (int edgei = 0; edgei < gM.NumberEdges; edgei++)
                {
                    var edge = gM.Edge(edgei);
                    if (edge.i > edge.j)
                        edge = (edge.j, edge.i);

                    if (!_edgesData.ContainsKey(edge))
                        _edgesData[edge] = new(countIntervals, ratio);
                }
            }
        }
    }
}
