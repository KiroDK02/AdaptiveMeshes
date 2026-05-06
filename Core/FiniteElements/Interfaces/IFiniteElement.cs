using System;
using System.Collections;
using System.Collections.Generic;
using Core.Vectors;

namespace Core.FiniteElements.Interfaces;

public interface IFiniteElement : IGeometryElement
{
    enum BasicFunctionsTypeEnum
    {
        Lagrange,
        Hierarchical
    }
    
    enum MatrixTypeEnum
    {
        Stiffness,
        Mass
    }
    
    BasicFunctionsTypeEnum FunctionsType { get; }
    int Order { get; }
    
    string Material { get; }
    int[] Dofs { get; }
    IDictionary<(int i, int j), int> EdgesDofs { get; }
    
    void SetVertexDof(int vertex, int n, int dof);
    void SetEdgeDof(int edge, int n, int dof);
    void SetElementDof(int n, int dof);

    int DofOnVertex(int vertex);
    int DofOnEdge(int edge);
    int DofOnElement();
    
    IEnumerable<(Vector2D position, int dof)> GetDofsWithPositions(Vector2D[] vertexCoords);
    
    double GetValueAtPoint(Vector2D[] vertexCoords, ReadOnlySpan<double> weights, Vector2D point,
        bool isLocalPoint = false);
    Vector2D GetGradientAtPoint(Vector2D[] vertexCoords, ReadOnlySpan<double> weights, Vector2D point,
        bool isLocalPoint = false);
}