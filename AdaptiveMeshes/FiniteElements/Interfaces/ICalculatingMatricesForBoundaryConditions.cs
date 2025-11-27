using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FiniteElements.Interfaces;

public interface ICalculatingMatricesForBoundaryConditions
{
    double[] BuildLocalRightPartFirstBc(Vector2D[] vertexCoords, Func<Vector2D, double> ug);
    double[] BuildLocalRightPartSecondBc(Vector2D[] vertexCoords, Func<Vector2D, double> theta);
}