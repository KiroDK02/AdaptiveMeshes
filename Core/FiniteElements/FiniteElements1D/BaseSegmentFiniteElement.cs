using System;
using System.Collections.Generic;
using Core.FiniteElements.AlgorithmsForFE;
using Core.FiniteElements.Interfaces;
using Core.MasterElements;
using Core.Vectors;

namespace Core.FiniteElements.FiniteElements1D;

public abstract class BaseSegmentFiniteElement : IFiniteElementWithNumericalIntegration<double>, ISplittableElement
{
    private const double Epsilon = 1e-12;
    
    public abstract IMasterElement<double> MasterElement { get; }
    public abstract int[] Dofs { get; }
    public abstract IFiniteElement.BasicFunctionsTypeEnum FunctionsType { get; }
    public abstract int Order { get; }
    public abstract IDictionary<(int i, int j), int> EdgesDofs { get; }
    
    public string Material { get; }
    public int[] VertexNumbers { get; }
    public int NumberOfEdges => 1;

    protected BaseSegmentFiniteElement(string material, int[] vertexNumbers)
    {
        Material = material;
        VertexNumbers = vertexNumbers;
    }

    public abstract int DofOnVertex(int vertex);

    public int DofOnEdge(int edge) => EdgesDofs[this.GlobalEdge(edge)];
    
    public abstract void SetVertexDof(int vertex, int n, int dof);
    
    public abstract void SetEdgeDof(int edge, int n, int dof);
    
    protected abstract double[] CalcLocalF(Vector2D[] vertexCoords, Func<Vector2D, double> F);
    
    public int DofOnElement() => 0;
    
    public void SetElementDof(int n, int dof)
        => throw new NotSupportedException();
    
    public (int i, int j) Edge(int edge) =>
        edge switch
        {
            0 => (0, 1),
            _ => throw new ArgumentException("Invalid number of edge.")
        };
    
    public bool IsPointOnElement(Vector2D[] vertexCoords, Vector2D point)
    {
        var begin = vertexCoords[VertexNumbers[0]];
        var end = vertexCoords[VertexNumbers[1]];

        var firstPart = begin.Distance(point);
        var secondPart = end.Distance(point);
        var segmentLength = begin.Distance(end);

        return Math.Abs(firstPart + secondPart - segmentLength) / segmentLength < Epsilon;
    }

    public Vector2D GetOuterNormalToEdge(Vector2D[] vertexCoords, int edgei, bool normalize = false)
        => throw new NotSupportedException();
    
    public double GetValueAtPoint(Vector2D[] vertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false)
    {
        throw new NotImplementedException();
    }
    
    public Vector2D GetGradientAtPoint(Vector2D[] vertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false)
    {
        throw new NotImplementedException();
    }
    
    public IDataForFragmentation SplitToElements2D(IDictionary<(int i, int j), int> splits, IDictionary<(int i, int j), (Vector2D vert, int num)[]> verticesOfSplitedEdges, ref int countVertex)
        => throw new NotSupportedException();
    public IEnumerable<IFiniteElement> SplitToElements1D(int[] globalVerticesNums)
    {
        List<IFiniteElement> elems = [];

        for (int i = 0; i < globalVerticesNums.Length - 1; i++)
        {
            int[] globalNums = [globalVerticesNums[i], globalVerticesNums[i + 1]];

            elems.Add(new SegmentFiniteElementQuadraticLagrange(Material, globalNums));
        }

        return elems;
    }
    
    protected double GetCoefAtLocalCoords(Vector2D[] vertexCoords, Func<Vector2D, double> coeff, double t)
    {
        var x0 = vertexCoords[VertexNumbers[0]].X;
        var x1 = vertexCoords[VertexNumbers[1]].X;
        var y0 = vertexCoords[VertexNumbers[0]].Y;
        var y1 = vertexCoords[VertexNumbers[1]].Y;

        return coeff(new(x0 * (1 - t) + x1 * t, y0 * (1 - t) + y1 * t));
    }
}