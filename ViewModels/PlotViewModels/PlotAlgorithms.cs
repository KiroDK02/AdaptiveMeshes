using System.Windows.Media.Animation;
using Core.FEM;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using ScottPlot;
using ScottPlot.WPF;

namespace ViewModels.PlotViewModels;

public static class PlotAlgorithms
{
    public static void DrawElements(this WpfPlot wpfPlot, IFiniteElementMesh mesh, Color? fillColor = null)
    {
        foreach (var element in mesh.Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;

            var vertices = element.VertexNumbers
                .Select(number => mesh.Vertex[number])
                .ToArray();

            var polygon = wpfPlot.Plot.Add.Polygon(
                vertices.Select(vert => vert.X),
                vertices.Select(vert => vert.Y));

            polygon.LineColor = Colors.Black;
            polygon.LineWidth = 1;

            polygon.FillColor = fillColor ?? Colors.Transparent.WithAlpha(1f);
        }
    }
}