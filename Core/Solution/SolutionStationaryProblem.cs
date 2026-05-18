using System;
using System.Collections.Generic;
using System.Linq;
using Core.FEM;
using Core.Solution.Interfaces;
using Core.Vectors;

namespace Core.Solution;

public class SolutionStationaryProblem : ISolution
{
    public IFiniteElementMesh Mesh { get; }

    private double[] _solutionVector;

    public ReadOnlySpan<double> SolutionVector
    {
        get => _solutionVector;
        set => _solutionVector = value.ToArray();
    }

    public SolutionStationaryProblem(IFiniteElementMesh mesh)
    {
        Mesh = mesh;
        _solutionVector = new double[mesh.NumberOfDOFs];
    }

    public Vector2D Flow(Vector2D point, IDictionary<string, IMaterial> materials)
    {
        foreach (var element in Mesh.Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;

            if (element.IsPointOnElement(Mesh.Vertex, point))
            {
                var lambda = materials[element.Material].Lambda;

                return -lambda(point) * element.GetGradientAtPoint(Mesh.Vertex, SolutionVector, point);
            }
        }

        throw new ArgumentException("The point is outside mesh.");
    }

    public Vector2D Gradient(Vector2D point)
    {
        foreach (var element in Mesh.Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;

            if (element.IsPointOnElement(Mesh.Vertex, point))
                return element.GetGradientAtPoint(Mesh.Vertex, SolutionVector, point);
        }

        throw new ArgumentException("The point is outside mesh.");
    }

    public double Value(Vector2D point)
    {
        foreach (var element in Mesh.Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;

            if (element.IsPointOnElement(Mesh.Vertex, point))
                return element.GetValueAtPoint(Mesh.Vertex, SolutionVector, point);
        }

        throw new ArgumentException("The point is outside mesh.");
    }

    public double CalcErrorFrom(Func<Vector2D, double> otherSolution)
    {
        var (leftLowerPoint, rightHigherPoint) = GetRectangle();
        var points = GetCross(
            BuildSplitting(leftLowerPoint.X, rightHigherPoint.X, 100),
            BuildSplitting(leftLowerPoint.Y, rightHigherPoint.Y, 100));

        var absError = 0.0;
        var otherSolutionSum = 0.0;
        foreach (var point in points)
        {
            try
            {
                if (!Mesh.TryFindElementWithPoint(point, out var element))
                    continue;

                var value = element!.GetValueAtPoint(Mesh.Vertex, SolutionVector, point);
                var valueOtherSolution = otherSolution(point);

                absError += (valueOtherSolution - value) * (valueOtherSolution - value);
                otherSolutionSum += valueOtherSolution * valueOtherSolution;
            }
            catch (ArgumentException) { }
        }

        return Math.Sqrt(absError / otherSolutionSum);
    }

    public double CalcErrorFrom(ISolution otherSolution)
    {
        throw new NotImplementedException();
    }

    private (Vector2D leftLowerPoint, Vector2D rightHigherPoint) GetRectangle()
    {
        var minX = Mesh.Vertex.Min(point => point.X);
        var minY = Mesh.Vertex.Min(point => point.Y);
        var maxX = Mesh.Vertex.Max(point => point.X);
        var maxY = Mesh.Vertex.Max(point => point.Y);

        return (new Vector2D(minX, minY), new Vector2D(maxX, maxY));
    }

    private static Vector2D[] GetCross(double[] x, double[] y)
    {
        var points = new Vector2D[x.Length * y.Length];

        for (int i = 0; i < y.Length; i++)
        {
            for (int j = 0; j < x.Length; j++)
                points[i * x.Length + j] = new(x[j], y[i]);
        }

        return points;
    }

    private double[] BuildSplitting(double start, double end, int countSegments)
    {
        var points = new double[countSegments + 1];
        var step = Math.Abs(end - start) / countSegments;

        points[0] = start;
        for (int i = 1; i < countSegments; i++)
            points[i] = start + step * i;
        points[countSegments] = end;

        return points;
    }
}