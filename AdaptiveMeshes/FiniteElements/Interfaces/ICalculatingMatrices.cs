using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FiniteElements.Interfaces;

public interface ICalculatingMatrices
{
    double[,] BuildLocalMatrix(Vector2D[] vertexCoords, IFiniteElement.MatrixTypeEnum type,
        Func<Vector2D, double> coefficient);
    double[] BuildLocalRightPart(Vector2D[] vertexCoords, Func<Vector2D, double> f);
}