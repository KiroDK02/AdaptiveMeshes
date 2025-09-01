using AdaptiveMeshes.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FiniteElements.AlgorithmsForFE
{
    public static class AlgorithmsForFragmentationTriangleElements
    {
        public static IList<IFiniteElement> SplitToTriangles(IFiniteElement element, int[] globalVerticesNums, int countLayers)
        {
            List<IFiniteElement> newElements = [];

            // Идейно треугольник разбит на слои (сначала равномерно),
            // где каждый слой разбивается на равномерные треугольники.
            for (int layer = 0; layer < countLayers; layer++)
            {
                int amountElementsOnLayer = 2 * (countLayers - layer) - 1;

                for (int elemi = 0; elemi < amountElementsOnLayer; elemi++)
                {
                    int amountPassedVertices = (2 * (countLayers + 1) - (layer - 1)) * layer / 2;
                    int amountPassedVerticesForNextLayer = (2 * (countLayers + 1) - layer) * (layer + 1) / 2;

                    (int localNumV1, int localNumV2, int localNumV3) =
                        elemi % 2 == 0
                     ? (elemi / 2 + amountPassedVertices, elemi / 2 + amountPassedVertices + 1, elemi / 2 + amountPassedVerticesForNextLayer)
                     : ((elemi + 1) / 2 + amountPassedVertices, (elemi + 1) / 2 + amountPassedVerticesForNextLayer, (elemi + 1) / 2 + amountPassedVerticesForNextLayer - 1);

                    int[] globalNumbers = [globalVerticesNums[localNumV1], globalVerticesNums[localNumV2], globalVerticesNums[localNumV3]];

                    newElements.Add(new TriangleFEQuadraticBaseWithNI(element.Material, globalNumbers));
                }
            }

            return newElements;
        }

        public static IList<(Vector2D vert, int num)> FindAllVerticesOfSplittedTriangle(int split1, int split2, int split3,
                                                                                        (Vector2D vert, int num)[] verticesEdge1,
                                                                                        (Vector2D vert, int num)[] verticesEdge2,
                                                                                        (Vector2D vert, int num)[] verticesEdge3,
                                                                                        ref int countVertex)
        {
            int minSplit = int.Min(split1, int.Min(split2, split3));
            int countLayer = minSplit;
            int countVertices = (minSplit + 2) * (countLayer + 1) / 2;

            int step1 = split1 / minSplit;
            int step2 = split2 / minSplit;
            int step3 = split3 / minSplit;

            var verticesOfSplittedTriangle = new (Vector2D vert, int num)[countVertices];
            
            for (int i = 0, step = 0; i < minSplit + 1; i++, step += step1)
            {
                verticesOfSplittedTriangle[i] = verticesEdge1[step];
            }

            int k2 = step2;
            int k3 = step3;

            Vector2D h = (verticesEdge1[split1].vert - verticesEdge1[0].vert) / minSplit;

            for (int layer = 1, numVert = minSplit; layer < countLayer; layer++, numVert--)
            {
                // (minSplit + 1 + (minSplit + 1 - (layer - 1))) * layer / 2
                int countPassedVertices = (2 * (minSplit + 1) - (layer - 1)) * layer / 2;
                verticesOfSplittedTriangle[countPassedVertices] = verticesEdge2[k2];

                for (int vi = 1; vi < numVert - 1; vi++)
                {
                    int localNum = countPassedVertices + vi;
                    Vector2D newVertex = verticesEdge2[k2].vert + h * vi;

                    verticesOfSplittedTriangle[localNum] = (newVertex, countVertex++);
                }

                verticesOfSplittedTriangle[(2 * (minSplit + 1) - layer) * (layer + 1) / 2 - 1] = verticesEdge3[k3];

                k2 += step2;
                k3 += step3;
            }

            verticesOfSplittedTriangle[^1] = verticesEdge2[^1];

            return verticesOfSplittedTriangle;
        }

        public static void DoubleElemsOnEdge(int step,
                                             (Vector2D vert, int num)[] verticesEdge,
                                             IList<IFiniteElement> listElemsFromCurElem,
                                             IList<(Vector2D vert, int num)> listVerticesCurElems)
        {
            for (int k = 1; k < verticesEdge.Length; k += step)
            {
                listVerticesCurElems.Add(verticesEdge[k]);

                for (int elemi = 0; elemi < listElemsFromCurElem.Count; elemi++)
                {
                    for (int edgei = 0; edgei < 3; edgei++)
                    {
                        var edge = listElemsFromCurElem[elemi].GlobalEdge(edgei);

                        if (verticesEdge[k - 1].num == edge.i && verticesEdge[k + 1].num == edge.j ||
                            verticesEdge[k - 1].num == edge.j && verticesEdge[k + 1].num == edge.i)
                        {
                            int thirdVertex = edgei switch
                            {
                                0 => listElemsFromCurElem[elemi].VertexNumber[2],
                                1 => listElemsFromCurElem[elemi].VertexNumber[0],
                                2 => listElemsFromCurElem[elemi].VertexNumber[1],
                                _ => throw new ArgumentException("Invalid number of edge.")
                            };

                            var elem1 = new TriangleFEQuadraticBaseWithNI(listElemsFromCurElem[elemi].Material, [edge.i, verticesEdge[k].num, thirdVertex]);
                            var elem2 = new TriangleFEQuadraticBaseWithNI(listElemsFromCurElem[elemi].Material, [verticesEdge[k].num, edge.j, thirdVertex]);

                            listElemsFromCurElem[elemi] = elem1;
                            listElemsFromCurElem.Add(elem2);
                        }
                    }
                }
            }
        }

        public static ((int i, int j), (int i, int j), (int i, int j)) DefineOrderEdges(IFiniteElement element)
        {
            (int i, int j) edgeMain = (0, 0);
            (int i, int j) edgeFirst = (0, 0);
            (int i, int j) edgeSecond = (0, 0);

            var edge1 = element.GlobalEdge(0);
            var edge2 = element.GlobalEdge(1);
            var edge3 = element.GlobalEdge(2);

            int sumNum1 = edge1.i + edge1.j;
            int sumNum2 = edge2.i + edge2.j;
            int sumNum3 = edge3.i + edge3.j;

            int min = int.Min(sumNum1, int.Min(sumNum2, sumNum3));

            if (min == sumNum1)
            {
                edgeMain = edge1;
                (edgeFirst, edgeSecond) = edge2.i < edge3.i
                                        ? (edge2, edge3)
                                        : (edge3, edge2);
            }

            if (min == sumNum2)
            {
                edgeMain = edge2;
                (edgeFirst, edgeSecond) = edge1.i < edge3.i
                                        ? (edge1, edge3)
                                        : (edge3, edge1);
            }

            if (min == sumNum3)
            {
                edgeMain = edge3;
                (edgeFirst, edgeSecond) = edge2.i < edge1.i
                                        ? (edge2, edge1)
                                        : (edge1, edge2);
            }

            return (edgeMain, edgeFirst, edgeSecond);
        }
    }
}
