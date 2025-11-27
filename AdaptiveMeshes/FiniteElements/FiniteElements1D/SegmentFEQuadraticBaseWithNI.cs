using AdaptiveMeshes.FiniteElements.AlgorithmsForFE;
using AdaptiveMeshes.FiniteElements.Interfaces;
using AdaptiveMeshes.MasterElements;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FiniteElements.FiniteElements1D;

public class SegmentFEQuadraticBaseWithNI :
    IFiniteElementWithNumericalIntegration<double>, ICalculatingMatricesForBoundaryConditions, ISplittableElement
{
    public SegmentFEQuadraticBaseWithNI(string material, int[] vertexNumbers)
    {
        Material = material;
        VertexNumbers = vertexNumbers;

        MasterElement = MasterElementBarycentricQuadraticBaseStraight.Instance;
    }

    public IMasterElement<double> MasterElement { get; }

    public string Material { get; }

    public int[] VertexNumbers { get; }

    public int[] Dofs { get; } = new int[3];

    public int NumberOfEdges => 1;

    public double[] BuildLocalRightPartFirstBc(Vector2D[] vertexCoords, Func<Vector2D, double> ug)
        => CalcLocalF(vertexCoords, ug);

    public double[] BuildLocalRightPartSecondBc(Vector2D[] vertexCoords, Func<Vector2D, double> theta)
    {
        var point1 = vertexCoords[VertexNumbers[0]];
        var point2 = vertexCoords[VertexNumbers[1]];

        var lengthBound = Math.Sqrt((point1.X - point2.X) * (point1.X - point2.X)
                                    + (point1.Y - point2.Y) * (point1.Y - point2.Y));

        var nodes = MasterElement.QuadratureNodes;
        var values = MasterElement.ValuesBasicFuncs;
        var localRightPart = new double[Dofs.Length];

        for (int i = 0; i < Dofs.Length; i++)
        {
            double valueIntegral = 0.0;

            for (int k = 0; k < nodes.Nodes.Length; k++)
                valueIntegral += nodes.Nodes[k].Weight * LocalTheta(nodes.Nodes[k].Node) * values[i, k];

            localRightPart[i] = valueIntegral * lengthBound;
        }

        return localRightPart;

        double LocalTheta(double t) => GetCoefAtLocalCoords(vertexCoords, theta, t);
    }

    public int DOFOnVertex(int vertex) => 1;
    public int DOFOnEdge(int edge) => 1;
    public int DOFOnElement() => 0;

    public (int i, int j) Edge(int edge)
        => edge switch
        {
            0 => (0, 1),
            _ => throw new Exception("Invalid number of edge.")
        };

    public Vector2D GetGradientAtPoint(Vector2D[] vertexCoords, ReadOnlySpan<double> weights, Vector2D point,
        bool isLocalPoint = false)
        => throw new NotSupportedException();

    public double GetValueAtPoint(Vector2D[] vertexCoords, ReadOnlySpan<double> weights, Vector2D point,
        bool isLocalPoint = false)
        => throw new NotSupportedException();

    public bool IsPointOnElement(Vector2D[] vertexCoords, Vector2D point) => throw new NotSupportedException();

    public void SetEdgeDof(int edge, int n, int dof)
    {
        if (edge == 0)
            Dofs[2] = dof;
        else
            throw new Exception("Invalid number of edge.");
    }

    public void SetElementDof(int n, int dof) => throw new NotSupportedException();

    public void SetVertexDof(int vertex, int n, int dof)
    {
        switch (vertex)
        {
            case 0:
                Dofs[0] = dof;
                break;

            case 1:
                Dofs[1] = dof;
                break;

            default:
                throw new Exception("Invalid number of vertex.");
        }
    }

    public IDataForFragmentation SplitToElements2D(
        IDictionary<(int i, int j), int> splits,
        IDictionary<(int i, int j), (Vector2D vert, int num)[]> verticesOfSplitedEdges,
        ref int countVertex)
        => throw new NotSupportedException();

    public IEnumerable<IFiniteElement> SplitToElements1D(int[] globalVerticesNums)
    {
        // Передать за globalVerticeNums для одномерных элементов что-то типа этого 
        // [.. verticesOfSplitiedEdges[elem.GlobalEdge(0)].Select(vertex => vertex.num)]
        List<IFiniteElement> elems = [];

        for (int i = 0; i < globalVerticesNums.Length - 1; i++)
        {
            int[] globalNums = [globalVerticesNums[i], globalVerticesNums[i + 1]];

            elems.Add(new SegmentFEQuadraticBaseWithNI(Material, globalNums));
        }

        return elems;
    }

    public Vector2D GetOuterNormalToEdge(Vector2D[] vertexCoords, int edgei, bool normalize = false)
        => throw new NotSupportedException();

    private double GetCoefAtLocalCoords(Vector2D[] vertexCoords, Func<Vector2D, double> coeff, double t)
    {
        var x0 = vertexCoords[VertexNumbers[0]].X;
        var x1 = vertexCoords[VertexNumbers[1]].X;
        var y0 = vertexCoords[VertexNumbers[0]].Y;
        var y1 = vertexCoords[VertexNumbers[1]].Y;

        return coeff(new(x0 * (1 - t) + x1 * t, y0 * (1 - t) + y1 * t));
    }

    private double[] CalcLocalF(Vector2D[] vertexCoords, Func<Vector2D, double> F)
    {
        var localF = new double[Dofs.Length];

        localF[0] = F(vertexCoords[VertexNumbers[0]]);
        localF[1] = F(vertexCoords[VertexNumbers[1]]);
        localF[2] = F((vertexCoords[VertexNumbers[1]] + vertexCoords[VertexNumbers[0]]) / 2d);

        return localF;
    }
}