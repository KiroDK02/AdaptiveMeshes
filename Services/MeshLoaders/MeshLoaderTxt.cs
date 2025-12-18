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
        
        var vertices = await LoadVerticesAsync(reader);
        var elements = await LoadElementsAsync(reader);
        
        return new FiniteElementMesh(elements, vertices);
    }

    public async Task SaveMeshToFileAsync(IFiniteElementMesh mesh, string path)
    {
        await using var writer = new StreamWriter(path);

        await SaveToFileAsync(mesh.Vertex, writer);
        await SaveToFileAsync(mesh.Elements, writer);
    }

    private static async Task<Vector2D[]> LoadVerticesAsync(StreamReader reader)
    {
        var verticesCount = int.Parse((await reader.ReadLineAsync())!);
        var vertices = new Vector2D[verticesCount];

        for (int i = 0; i < verticesCount; i++)
        {
            if (!Vector2D.TryParse((await reader.ReadLineAsync())!, out vertices[i]))
                throw new ArgumentException("Invalid vertex format.");
        }
        
        return vertices;
    }

    private static async Task<IFiniteElement[]> LoadElementsAsync(StreamReader reader)
    {
        var elementsCount = int.Parse((await reader.ReadLineAsync())!);
        var elements = new IFiniteElement[elementsCount];

        for (int i = 0; i < elementsCount; i++)
        {
            var input = (await reader.ReadLineAsync())!
                .Split(' ', StringSplitOptions.RemoveEmptyEntries);

            elements[i] = GetElement(input);
        }

        return elements;
    }

    private static async Task SaveToFileAsync<T>(IEnumerable<T> collection, StreamWriter writer)
    {
        var collectionArray = collection.ToArray();
        await writer.WriteLineAsync(collectionArray.Length.ToString());

        foreach (var element in collectionArray)
            await writer.WriteLineAsync(element?.ToString());
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