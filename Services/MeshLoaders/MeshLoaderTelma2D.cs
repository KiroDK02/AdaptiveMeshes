using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Core.FEM;
using Services.MeshLoaders.Interfaces;
using System.Threading.Tasks;
using Core.FiniteElements.FiniteElements1D;
using Core.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;
using Core.FiniteElements.Interfaces;
using Core.Vectors;

namespace Services.MeshLoaders;

public class MeshLoaderTelma2D : IMeshLoader
{
    private IDictionary<int, string> _elementMaterialsNames = new Dictionary<int, string>();
    private IDictionary<int, string> _boundMaterialNames = new Dictionary<int, string>();
    
    public async Task<IFiniteElementMesh> LoadMeshAsync(string path)
    {
        var allLines = await File.ReadAllLinesAsync(path);

        var countVertices = int.Parse(allLines[1]);
        var countElements = int.Parse(allLines[countVertices + 2]);
        var countElementMaterials = int.Parse(allLines[countVertices + countElements + 3]);
        var countBoundMaterials = int.Parse(allLines[countVertices + countElements + countElementMaterials + 4]);
        
        var vertices = GetVertices(allLines.AsSpan(2, countVertices), countVertices);
        
        _elementMaterialsNames = GetMaterialsNames(
            allLines.AsSpan(countVertices + countElements + 4, countElementMaterials), 
            countElementMaterials);
        _boundMaterialNames = GetMaterialsNames(
            allLines.AsSpan(countVertices + countElements + countElementMaterials + 5, countBoundMaterials),
            countBoundMaterials);

        var elements = GetElements(allLines.AsSpan(countVertices + 3, countElements), countElements);

        return new FiniteElementMesh(elements, vertices);
    }

    public async Task SaveMeshToFileAsync(IFiniteElementMesh mesh, string path)
        => throw new NotSupportedException();

    private static Vector2D[] GetVertices(ReadOnlySpan<string> verticesLines, int verticesCount)
    {
        var vertices = new Vector2D[verticesCount];
        var index = 0;

        foreach (var line in verticesLines)
            if (Vector3D.TryParse(line, out var vertex))
                vertices[index++] = vertex.As2D();

        return vertices;
    }

    private IEnumerable<IFiniteElement> GetElements(ReadOnlySpan<string> elementsLines, int elementsCount)
    {
        var elements = new List<IFiniteElement>(elementsCount);
        
        foreach (var line in elementsLines)
            elements.Add(GetElement(line.Split(' ', StringSplitOptions.RemoveEmptyEntries)));
        
        return elements;
    }

    private static IDictionary<int, string> GetMaterialsNames(ReadOnlySpan<string> materialsLines, int 
        materialsCount)
    {
        var elementMaterialsNames = new Dictionary<int, string>(materialsCount);

        foreach (var materialsLine in materialsLines)
        {
            var idString = materialsLine
                .TakeWhile(c => c != ' ')
                .ToArray();
            
            var idLength = idString.Length;
            var materialName = materialsLine.Skip(idLength).SkipWhile(c => c == ' ');

            if (int.TryParse(string.Join("", idString), out var materialId))
                elementMaterialsNames[materialId] = string.Join("", materialName);
        }

        return elementMaterialsNames;
    }

    private IFiniteElement GetElement(string[] element)
    {
        return element[0] switch
        {
            "Triangle" => 
                new TriangleFiniteElementHierarchical(
                _elementMaterialsNames[int.Parse(element[3])],
                [int.Parse(element[5]), int.Parse(element[6]), int.Parse(element[7])], 
                int.Parse(element[1])),
            
            "Segment" => new SegmentFiniteElementHierarchical(
                _boundMaterialNames[int.Parse(element[3])],
                [int.Parse(element[5]), int.Parse(element[6])],
                int.Parse(element[1])),
            
            _ => throw new ArgumentException("Unknown element type.")
        };
    }
}