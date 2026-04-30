using Core.FiniteElements.Interfaces;
using Core.MasterElements;
using Core.Vectors;

namespace Core.FiniteElements.FiniteElements1D;

public class SegmentFiniteElementQuadraticLagrange : BaseSegmentFiniteElement, ICalculatingMatricesForBoundaryConditions
{
    private readonly string _toStringObject;
    
    public override IMasterElement<double> MasterElement { get; }
    public override int[] Dofs { get; } = new int[3];
    public override IFiniteElement.BasicFunctionsTypeEnum FunctionsType => 
        IFiniteElement.BasicFunctionsTypeEnum.Lagrange;

    public override int Order => 2;

    public SegmentFiniteElementQuadraticLagrange(string material, int[] vertexNumbers) 
        : base(material, vertexNumbers)
    {
        MasterElement = MasterElementBarycentricQuadraticBaseStraight.Instance;
        _toStringObject = $"SegmentLagrange2 {VertexNumbers[0]} {VertexNumbers[1]} {Material}";
    }
    
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
            var valueIntegral = 0.0;

            for (int k = 0; k < nodes.Nodes.Length; k++)
                valueIntegral += nodes.Nodes[k].Weight * LocalTheta(nodes.Nodes[k].Node) * values[i, k];

            localRightPart[i] = valueIntegral * lengthBound;
        }

        return localRightPart;

        double LocalTheta(double t) => GetCoefAtLocalCoords(vertexCoords, theta, t);
    }

    public override int DofOnVertex(int vertex) => 1;
    
    public override int DofOnEdge(int edge) => 1;
    
    public override void SetEdgeDof(int edge, int n, int dof)
    {
        if (edge == 0)
            Dofs[2] = dof;
        else
            throw new Exception("Invalid number of edge.");
    }
    
    public override void SetVertexDof(int vertex, int n, int dof)
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
    
    public override string ToString() => _toStringObject;
}