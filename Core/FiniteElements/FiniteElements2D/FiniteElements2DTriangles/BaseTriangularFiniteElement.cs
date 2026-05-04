using System;
using System.Collections.Generic;
using System.Linq;
using Core.FiniteElements.AlgorithmsForFE;
using Core.FiniteElements.Interfaces;
using Core.MasterElements;
using Core.Vectors;
using static Core.FiniteElements.AlgorithmsForFE.AlgorithmsForFragmentationTriangleElements;

namespace Core.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;

public abstract class BaseTriangularFiniteElement : IFiniteElementWithNumericalIntegration<Vector2D>, ISplittableElement
{
    public abstract IMasterElement<Vector2D> MasterElement { get; }
    public abstract int[] Dofs { get; }
    public abstract IDictionary<(int i, int j), int> EdgesDofs { get; }
    
    public abstract IFiniteElement.BasicFunctionsTypeEnum FunctionsType { get; }
    public abstract int Order { get; }
    
    public int[] VertexNumbers { get; }
    public int NumberOfEdges => 3;
    public string Material { get; }

    protected BaseTriangularFiniteElement(string material, int[] vertexNumbers)
    {
        Material = material;
        VertexNumbers = vertexNumbers
            .Order()
            .ToArray();
    }

    public abstract void SetVertexDof(int vertex, int n, int dof);

    public abstract void SetEdgeDof(int edge, int n, int dof);

    public abstract void SetElementDof(int n, int dof);

    public abstract int DofOnVertex(int vertex);

    public int DofOnEdge(int edge) => EdgesDofs[this.GlobalEdge(edge)];

    public abstract int DofOnElement();

    protected abstract Vector2D GetGradientAtLocalPoint(ReadOnlySpan<double> weights, Vector2D localPoint);

    protected abstract double GetValueAtLocalPoint(ReadOnlySpan<double> weights, Vector2D localPoint);

    public (int i, int j) Edge(int edge) =>
        edge switch
        {
            0 => (0, 1),
            1 => (1, 2),
            2 => (2, 0),
            _ => throw new Exception("Invalid number of edge.")
        };
    
    public bool IsPointOnElement(Vector2D[] vertexCoords, Vector2D point)
    {
        var x1 = vertexCoords[VertexNumbers[0]].X;
        var x2 = vertexCoords[VertexNumbers[1]].X;
        var x3 = vertexCoords[VertexNumbers[2]].X;
        
        var y1 = vertexCoords[VertexNumbers[0]].Y;
        var y2 = vertexCoords[VertexNumbers[1]].Y;
        var y3 = vertexCoords[VertexNumbers[2]].Y;
        
        var x0 = point.X;
        var y0 = point.Y;

        var product1 = (x1 - x0) * (y2 - y1) - (x2 - x1) * (y1 - y0);
        var product2 = (x2 - x0) * (y3 - y2) - (x3 - x2) * (y2 - y0);
        var product3 = (x3 - x0) * (y1 - y3) - (x1 - x3) * (y3 - y0);

        return (product1 <= 0 && product2 <= 0 && product3 <= 0)
               || (product1 >= 0 && product2 >= 0 && product3 >= 0);
    }
    
    public Vector2D GetOuterNormalToEdge(Vector2D[] vertexCoords, int edgei, bool normalize = false)
    {
        var edge = Edge(edgei);

        if (!TryGetThirdVertex(edge.i, edge.j, out var vertex3))
            throw new ArgumentException("Incorrect edgei or edgej.");

        edge = (VertexNumbers[edge.i], VertexNumbers[edge.j]);
        vertex3 = VertexNumbers[vertex3];

        var x0 = vertexCoords[edge.i].X;
        var y0 = vertexCoords[edge.i].Y;
        var x1 = vertexCoords[edge.j].X;
        var y1 = vertexCoords[edge.j].Y;
        var x2 = vertexCoords[vertex3].X;
        var y2 = vertexCoords[vertex3].Y;

        var outerNormal = new Vector2D(y1 - y0, -(x1 - x0));
        var tempVector = new Vector2D(x2 - x0, y2 - y0);

        if (tempVector * outerNormal > 0)
            outerNormal = -outerNormal;

        return normalize ? outerNormal.Normalize() : outerNormal;
    }
    
    
    public double GetValueAtPoint(Vector2D[] vertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false)
    {
        if (isLocalPoint)
            return GetValueAtLocalPoint(weights, point);

        var localPoint = GetLocalPoint(vertexCoords, point);

        return GetValueAtLocalPoint(weights, localPoint);
    }
    
    public Vector2D GetGradientAtPoint(Vector2D[] vertexCoords, ReadOnlySpan<double> weights, Vector2D point, bool isLocalPoint = false)
    {
        var matrixJacobi = GetMatrixJacobi(vertexCoords);

        var gradAtLocalCoords = isLocalPoint
            ? GetGradientAtLocalPoint(weights, point)
            : GetGradientAtLocalPoint(weights, GetLocalPoint(vertexCoords, point));

        double xComp = gradAtLocalCoords.X * matrixJacobi[0, 0] 
                       + gradAtLocalCoords.Y * matrixJacobi[0, 1];
        double yComp = gradAtLocalCoords.X * matrixJacobi[1, 0] 
                       + gradAtLocalCoords.Y * matrixJacobi[1, 1];

        return new(xComp, yComp);
    }
    
    public IDataForFragmentation SplitToElements2D(IDictionary<(int i, int j), int> splits,
        IDictionary<(int i, int j), (Vector2D vert, int num)[]> verticesOfSplitedEdges,
        ref int countVertex)
    {
        var (edge1, edge2, edge3) = DefineOrderEdges(this);

        var split1 = (int)Math.Pow(2, splits[edge1]);
        var split2 = (int)Math.Pow(2, splits[edge2]);
        var split3 = (int)Math.Pow(2, splits[edge3]);

        var verticesOfEdge1 = verticesOfSplitedEdges[edge1];
        var verticesOfEdge2 = verticesOfSplitedEdges[edge2];
        var verticesOfEdge3 = verticesOfSplitedEdges[edge3];

        var minSplit = Math.Min(split1, Math.Min(split2, split3));

        var listVerticesFromCurElement = new List<(Vector2D vert, int num)>(FindAllVerticesOfSplittedTriangle(split1,
            split2, split3,
            verticesOfEdge1, verticesOfEdge2, verticesOfEdge3,
            ref countVertex));

        var newElementsFromCurElement =
            SplitToTriangles(this, [.. listVerticesFromCurElement.Select(vertex => vertex.num)], minSplit);

        if (split1 / minSplit != 1)
            DoubleElemsOnEdge(split1 / minSplit, verticesOfEdge1, newElementsFromCurElement,
                listVerticesFromCurElement);
        if (split2 / minSplit != 1)
            DoubleElemsOnEdge(split2 / minSplit, verticesOfEdge2, newElementsFromCurElement,
                listVerticesFromCurElement);
        if (split3 / minSplit != 1)
            DoubleElemsOnEdge(split3 / minSplit, verticesOfEdge3, newElementsFromCurElement,
                listVerticesFromCurElement);

        return new DataForTriangleFragmentation(newElementsFromCurElement, listVerticesFromCurElement);
    }

    public IEnumerable<IFiniteElement> SplitToElements1D(int[] globalVerticesNums) => throw new NotSupportedException();
    
    protected Vector2D GetLocalPoint(Vector2D[] vertexCoords, Vector2D point)
    {
        var point1 = vertexCoords[VertexNumbers[0]];
        var point2 = vertexCoords[VertexNumbers[1]];
        var point3 = vertexCoords[VertexNumbers[2]];

        var detD = DetD(vertexCoords);

        return new(
            (point3.X * point1.Y
             - point1.X * point3.Y
             + (point3.Y - point1.Y) * point.X
             + (point1.X - point3.X) * point.Y)
            / detD,
            (point1.X * point2.Y
             - point2.X * point1.Y
             + (point1.Y - point2.Y) * point.X
             + (point2.X - point1.X) * point.Y)
            / detD);
    }

    protected double[,] GetMatrixJacobi(Vector2D[] vertexCoords)
    {
        var point1 = vertexCoords[VertexNumbers[0]];
        var point2 = vertexCoords[VertexNumbers[1]];
        var point3 = vertexCoords[VertexNumbers[2]];

        var detD = DetD(vertexCoords);

        double[,] J =
        {
            { (point3.Y - point1.Y) / detD, (point1.Y - point2.Y) / detD },
            { (point1.X - point3.X) / detD, (point2.X - point1.X) / detD }
        };

        return J;
    }

    protected double DetD(Vector2D[] vertexCoords)
    {
        var point1 = vertexCoords[VertexNumbers[0]];
        var point2 = vertexCoords[VertexNumbers[1]];
        var point3 = vertexCoords[VertexNumbers[2]];

        return (point2.X - point1.X) * (point3.Y - point1.Y) - (point3.X - point1.X) * (point2.Y - point1.Y);
    }
    
    protected static bool TryGetThirdVertex(int vertex1, int vertex2, out int vertex3)
    {
        vertex3 = Enumerable.Range(0, 3)
            .Where(i => i != vertex1 && i != vertex2)
            .FirstOrDefault(-1);

        return vertex3 != -1;
    }
    
    protected double GetCoefAtLocalCoords(Vector2D[] vertexCoords, Func<Vector2D, double> coeff, Vector2D point)
    {
        var point1 = vertexCoords[VertexNumbers[0]];
        var point2 = vertexCoords[VertexNumbers[1]];
        var point3 = vertexCoords[VertexNumbers[2]];

        var localPoint = new Vector2D((point2.X - point1.X) * point.X + (point3.X - point1.X) * point.Y + point1.X,
            (point2.Y - point1.Y) * point.X + (point3.Y - point1.Y) * point.Y + point1.Y);

        return coeff(localPoint);
    }
}