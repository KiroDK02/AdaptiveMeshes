using System.Diagnostics.CodeAnalysis;
using Core.FEM;
using ScottPlot;
using ScottPlot.WPF;
using ViewModels.MaterialViewModels;

namespace ViewModels.PlotViewModels;

public static class PlotAlgorithms
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static void DrawElements(
        this WpfPlot wpfPlot,
        IFiniteElementMesh mesh,
        IEnumerable<MaterialViewModel>? materials,
        bool drawMaterials = false)
    {
        var materialColors = materials?
            .ToDictionary(mat => mat.Name, mat => mat.SelectedColor);

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

            if (materialColors != null
                && drawMaterials
                && materialColors.TryGetValue(element.Material, out var color))
                polygon.FillColor = color;
            else
                polygon.FillColor = Colors.Transparent.WithAlpha(0f);
        }

        if (!drawMaterials
            || materialColors == null)
            return;

        foreach (var element in mesh.Elements)
        {
            if (element.VertexNumbers.Length != 2)
                continue;

            var vertices = element.VertexNumbers
                .Select(number => mesh.Vertex[number])
                .ToArray();

            var line = wpfPlot.Plot.Add.Line(vertices[0].X, vertices[0].Y, vertices[1].X, vertices[1].Y);

            if (materialColors.TryGetValue(element.Material, out var color))
            {
                line.LineColor = color;
                line.LineWidth = 1.5f;
            }
            else
                line.Color = Colors.Black;
        }
    }
}