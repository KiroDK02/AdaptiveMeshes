using System;
using System.Collections.Generic;
using System.Linq;
using Core.Solution.Interfaces;
using Core.Vectors;

namespace ViewModels.PlotViewModels;

public class IsolinesAlgorithms
{
    public record IsolineSegment(Vector2D From, Vector2D To, double Level);

    public static List<IsolineSegment> ComputeIsolines(
        ISolution solution,
        int levelCount = 10,
        double eps = 1e-12)
    {
        var mesh = solution.Mesh;
        var weights = solution.SolutionVector;
        var vertices = mesh.Vertex;

        var values = new double[vertices.Length];
        for (int i = 0; i < vertices.Length; i++)
            values[i] = weights[i];

        var minValue = values.Min();
        var maxValue = values.Max();

        var levels = Enumerable
            .Range(1, levelCount)
            .Select(i => minValue + i * (maxValue - minValue) / (levelCount + 1))
            .ToArray();

        var segments = new List<IsolineSegment>();
        
        foreach (var element in mesh.Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;

            var v = element.VertexNumbers;

            var (p0, p1, p2) = (vertices[v[0]], vertices[v[1]], vertices[v[2]]);
            var (f0, f1, f2) = (values[v[0]], values[v[1]], values[v[2]]);

            foreach (var level in levels)
            {
                var points = new List<Vector2D>();
                
                TryAddIntersection(points, p0, p1, f0, f1, level, eps);
                TryAddIntersection(points, p1, p2, f1, f2, level, eps);
                TryAddIntersection(points, p2, p0, f2, f0, level, eps);
                
                if (points.Count == 2)
                    segments.Add(new(points[0], points[1], level));
            }
        }

        return segments;
    }

    private static void TryAddIntersection(
        List<Vector2D> points,
        Vector2D p0, Vector2D p1,
        double f0, double f1,
        double level,
        double eps)
    {
        if (Math.Abs(f0 - level) < eps)
        {
            if (!points.Contains(p0))
                points.Add(p0);
            
            return;
        }

        if ((f0 - level) * (f1 - level) < 0)
        {
            var t = (level - f0) / (f1 - f0);
            
            points.Add(new Vector2D(
                p0.X + t * (p1.X - p0.X),
                p0.Y + t * (p1.Y - p0.Y)));
        }
    }
}