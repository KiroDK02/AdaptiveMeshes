using AdaptiveMeshes.FEM.Materials.GeometryMaterials;
using AdaptiveMeshes.Vectors;
using System.Drawing;

namespace AdaptiveMeshes.FEM.Mesh.MeshGenerator
{
    public static class AlgorithmsForMeshGeneration
    {
        public static bool IsEar(Vector2D[] vertices, IList<int> vertexNumbers, int i)
        {
            int i0 = (i - 1 + vertexNumbers.Count) % vertexNumbers.Count;
            int i1 = i;
            int i2 = (i + 1) % vertexNumbers.Count;

            var A = vertices[i0];
            var B = vertices[i1];
            var C = vertices[i2];

            if (!IsConvex(A, B, C))
                return false;

            for (int j = 0; j < vertexNumbers.Count; j++)
            {
                if (j == i0 || j == i1 || j == i2)
                    continue;

                if (IsPointInTriangle(A, B, C, vertices[vertexNumbers[j]]))
                    return false;
            }

            return true;
        }

        private static bool IsPointInTriangle(Vector2D A, Vector2D B, Vector2D C, Vector2D point)
        {
            double product1 = Cross(point, A, B);
            double product2 = Cross(point, B, C);
            double product3 = Cross(point, C, A);

            if (product1 <= 0 && product2 <= 0 && product3 <= 0)
                return true;
            else if (product1 >= 0 && product2 >= 0 && product3 >= 0)
                return true;
            else
                return false;
        }

        public static (int countIntervals, double ratio) FindDataForNewEdge(int iA, int iB, int iC,
                                                                             IDictionary<(int i, int j), EdgesData> edgesData)
        {
            var newEdge = iA < iC ? (iA, iC) : (iC, iA);
            int newEdgeCountIntervals = 0;
            double newEdgeRatio = 0.0;

            if (!edgesData.ContainsKey(newEdge))
            {
                var edge1 = iA < iB ? (iA, iB) : (iB, iA);
                var edge2 = iB < iC ? (iB, iC) : (iC, iB);

                if (edgesData.TryGetValue(edge1, out var data1) && edgesData.TryGetValue(edge2, out var data2))
                {
                    var (edge, data) = data1.CountIntervals < data2.CountIntervals ? (edge1, data1) : (edge2, data2);

                    newEdgeCountIntervals = data.CountIntervals;
                    newEdgeRatio = data.Ratio;

                    if (!SameDir(edge, newEdge))
                        newEdgeRatio = 1.0 / newEdgeRatio;

                    edgesData[newEdge] = new(newEdgeCountIntervals, newEdgeRatio);
                }
            }

            return (newEdgeCountIntervals, newEdgeRatio);
        }

        private static bool SameDir((int i, int j) edge1, (int i, int j) edge2)
            => edge1.i == edge2.i || edge1.j == edge2.j;

        private static double Cross(Vector2D A, Vector2D B, Vector2D C)
            => (A.X - C.X) * (B.Y - C.Y) - (B.X - C.X) * (A.Y - C.Y);

        private static bool IsConvex(Vector2D A, Vector2D B, Vector2D C)
            => (B.X - A.X) * (C.Y - B.Y) - (B.Y - A.Y) * (C.X - B.X) > 0;


        public static bool IsCCW(Vector2D[] vertices, int[] vertexNumbers)
        {
            Vector2D[] newVertices = new Vector2D[vertexNumbers.Length];

            for (int i = 0; i < vertexNumbers.Length; i++)
                newVertices[i] = vertices[vertexNumbers[i]];

            return CalcSignedSquare(newVertices) > 0;
        }

        private static double CalcSignedSquare(Vector2D[] vertices)
        {
            double signedSquare = 0;

            for (int i = 0; i < vertices.Length - 1; i++)
            {
                var pointCur = vertices[i];
                var pointNext = vertices[i + 1];

                signedSquare += (pointCur.X * pointNext.Y - pointNext.X * pointCur.Y);
            }

            return 0.5 * signedSquare;
        }
    }
}
