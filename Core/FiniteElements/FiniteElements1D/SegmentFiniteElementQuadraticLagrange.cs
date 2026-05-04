using System;
using System.Collections.Generic;
using Core.FiniteElements.Interfaces;
using Core.MasterElements;
using Core.Vectors;

namespace Core.FiniteElements.FiniteElements1D;

public class SegmentFiniteElementQuadraticLagrange : BaseSegmentFiniteElement, ICalculatingMatricesForBoundaryConditions
{
    private const int EdgeNumOffset = 2;
    
    public override IMasterElement<double> MasterElement { get; }
    public override int[] Dofs { get; } = new int[3];
    public override IFiniteElement.BasicFunctionsTypeEnum FunctionsType => 
        IFiniteElement.BasicFunctionsTypeEnum.Lagrange;
    public override int Order => 2;
    public override IDictionary<(int i, int j), int> EdgesDofs { get; }

    public SegmentFiniteElementQuadraticLagrange(string material, int[] vertexNumbers) 
        : base(material, vertexNumbers)
    {
        MasterElement = MasterElementBarycentricQuadraticBaseStraight.Instance;
        
        EdgesDofs = new Dictionary<(int i, int j), int>();
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

    public override void SetEdgeDof(int edge, int n, int dof) => Dofs[EdgeNumOffset] = dof;

    public override void SetVertexDof(int vertex, int n, int dof) => Dofs[vertex] = dof;
    
    public override string ToString() => $"SegmentLagrange {Order} {VertexNumbers[0]} {VertexNumbers[1]} {Material}";

    protected override double[] CalcLocalF(Vector2D[] vertexCoords, Func<Vector2D, double> F)
    {
        var localF = new double[Dofs.Length];

        localF[0] = F(vertexCoords[VertexNumbers[0]]);
        localF[1] = F(vertexCoords[VertexNumbers[1]]);
        localF[2] = F((vertexCoords[VertexNumbers[1]] + vertexCoords[VertexNumbers[0]]) / 2d);

        return localF;
    }
}