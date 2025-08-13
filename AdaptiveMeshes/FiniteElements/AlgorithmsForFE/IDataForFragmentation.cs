using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FiniteElements.AlgorithmsForFE
{
    public interface IDataForFragmentation
    {
        IEnumerable<IFiniteElement> NewElements { get; }
        IEnumerable<(Vector2D vert, int num)> NewVertices { get; }
    }
}
