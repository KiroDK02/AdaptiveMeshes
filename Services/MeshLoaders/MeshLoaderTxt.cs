using Core.FEM;
using Core.FiniteElements.FiniteElements1D;
using Core.FiniteElements.FiniteElements2D.FiniteElements2DTriangles;
using Core.FiniteElements.Interfaces;
using Core.Vectors;
using Services.MeshLoaders.Interfaces;

namespace Services.MeshLoaders;

public class MeshLoaderTxt : IMeshLoader
{
    public async Task<IFiniteElementMesh> LoadMeshAsync(string path)
    {
        using var reader = new StreamReader(path);
        
        var allLines = (await reader.ReadToEndAsync()).Split(['\n', '\r'], StringSplitOptions.RemoveEmptyEntries);
        var verticesCount = int.Parse(allLines[0]);
        var elementsCount = int.Parse(allLines[verticesCount + 1]);
        
        var vertices = LoadVerticesAsync(
            verticesCount, 
            allLines.AsSpan(1, verticesCount));
        
        var elements = LoadElementsAsync(
            elementsCount, 
            allLines.AsSpan(verticesCount + 2, elementsCount));
        
        return new FiniteElementMesh(elements, vertices);
    }

    public async Task SaveMeshToFileAsync(IFiniteElementMesh mesh, string path)
    {
        await using var writer = new StreamWriter(path);

        await SaveToFileAsync(mesh.Vertex, writer);
        await SaveToFileAsync(mesh.Elements, writer);
    }

    private static Vector2D[] LoadVerticesAsync(int verticesCount, ReadOnlySpan<string> inputVertices)
    {
        var vertices = new Vector2D[verticesCount];

        var i = 0;
        foreach (var vertex in inputVertices)
        {
            if (!Vector2D.TryParse(vertex, out vertices[i++]))
                throw new ArgumentException("Invalid vertex format.");
        }
        
        return vertices;
    }

    public static IEnumerable<IFiniteElement> LoadElementsAsync(int elementsCount, ReadOnlySpan<string> inputElements)
    {
        var elements = new IFiniteElement[elementsCount];

        var i = 0;
        foreach (var element in inputElements)
            elements[i++] = GetElement(element.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        
        return elements;
    }

    private static async Task SaveToFileAsync<T>(IEnumerable<T> collection, StreamWriter writer)
    {
        var itemsCount = 0;

        var collectionString = collection
            .Select(item =>
            {
                itemsCount++;
                return item?.ToString();
            });
        
        await writer.WriteAsync(string.Join('\n', itemsCount, collectionString));
    }
    
    private static IFiniteElement GetElement(string[] element)
        => element[0] switch
        {
            "TriangleLagrange2" => new TriangleFEQuadraticBaseWithNI(
                string.Join(' ', element[4..]),
                [int.Parse(element[1]), int.Parse(element[2]), int.Parse(element[3])]),
                
            "SegmentLagrange2" => new SegmentFEQuadraticBaseWithNI(
                string.Join(' ', element[3..]),
                [int.Parse(element[1]), int.Parse(element[2])]),
                
            _ => throw new ArgumentException("Invalid type of element.")
        };
}