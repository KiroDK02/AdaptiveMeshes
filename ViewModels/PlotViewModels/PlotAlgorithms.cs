using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Core.FEM;
using ScottPlot;
using ViewModels.MaterialViewModels;

namespace ViewModels.PlotViewModels;

public static class PlotAlgorithms
{
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public static void DrawElements(
        this IPlotControl wpfPlot,
        IFiniteElementMesh mesh,
        IEnumerable<MaterialViewModel>? materials,
        bool drawMaterials = false,
        bool showDofs = false)
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

    public static void DrawDofs(this IPlotControl wpfPlot, IFiniteElementMesh mesh)
    {
        foreach (var element in mesh.Elements)
        {
            if (element.VertexNumbers.Length == 2)
                continue;

            var groupedDofsPositions = element
                .GetDofsWithPositions(mesh.Vertex)
                .GroupBy(x => x.position, (pos, items) =>
                    (position: pos,
                        label: string.Join(", ", items.Select(x => x.dof))));

            foreach (var (position, label) in groupedDofsPositions)
            {
                var text = wpfPlot.Plot.Add.Text(label, position.X, position.Y);
                
                text.LabelFontSize = 18;
                text.LabelFontColor = Colors.DarkBlue;
                text.LabelAlignment = Alignment.MiddleCenter;
            }
        }
    }
}