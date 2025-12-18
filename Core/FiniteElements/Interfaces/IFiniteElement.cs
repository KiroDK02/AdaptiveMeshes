using Core.Vectors;

namespace Core.FiniteElements.Interfaces;

public interface IFiniteElement : IGeometryElement
{
    enum MatrixTypeEnum
    {
        Stiffness,
        Mass
    }
    
    string Material { get; }
    int[] Dofs { get; }
    
    void SetVertexDof(int vertex, int n, int dof);
    void SetEdgeDof(int edge, int n, int dof);
    void SetElementDof(int n, int dof);

    int DofOnVertex(int vertex);
    int DofOnEdge(int edge);
    int DofOnElement();
    
    double GetValueAtPoint(Vector2D[] vertexCoords, ReadOnlySpan<double> weights, Vector2D point,
        bool isLocalPoint = false);
    Vector2D GetGradientAtPoint(Vector2D[] vertexCoords, ReadOnlySpan<double> weights, Vector2D point,
        bool isLocalPoint = false);
}