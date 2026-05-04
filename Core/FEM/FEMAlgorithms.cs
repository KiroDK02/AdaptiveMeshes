using System;
using System.Collections.Generic;
using System.Linq;
using Core.FiniteElements.AlgorithmsForFE;
using Core.FiniteElements.Interfaces;

namespace Core.FEM;

public static class FEMAlgorithms
{
    public static void EnumerateMeshDofs(IFiniteElementMesh mesh)
    {
        var dof = 0;

        PrepareForEnumerateMesh(mesh);

        EnumerateVerticesDofs(mesh, ref dof);
        EnumerateEdgesDofs(mesh, ref dof);
        EnumerateVolumeDofs(mesh, ref dof);

        mesh.NumberOfDOFs = dof;
    }

    public static void EnumerateVerticesDofs(IFiniteElementMesh mesh, ref int dof)
    {
        var vertexPortrait = BuildVertexPortrait(mesh);

        for (int i = 0; i < vertexPortrait.Length; i++)
            vertexPortrait[i] = dof += vertexPortrait[i];

        foreach (var element in mesh.Elements)
        {
            for (int vertexi = 0; vertexi < element.VertexNumbers.Length; vertexi++)
            {
                var dofOnVertex = element.DofOnVertex(vertexi);
                var startDof = vertexPortrait[element.VertexNumbers[vertexi]] - dofOnVertex;

                for (int n = 0; n < dofOnVertex; n++)
                    element.SetVertexDof(vertexi, n, startDof + n);
            }
        }
    }

    public static void EnumerateEdgesDofs(IFiniteElementMesh mesh, ref int dof)
    {
        var edgesPortrait = BuildEdgePortrait(mesh);

        var tempDof = dof;
        edgesPortrait = edgesPortrait.ToDictionary(edges => edges.Key, edges => tempDof += edges.Value);
        dof = tempDof;

        foreach (var element in mesh.Elements)
        {
            for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
            {
                var edge = element.GlobalEdge(edgei);
                var dofOnEdge = element.DofOnEdge(edgei);
                var startDof = edgesPortrait[edge] - dofOnEdge;

                for (int j = 0; j < dofOnEdge; j++)
                    element.SetEdgeDof(edgei, j, startDof + j);
            }
        }
    }

    public static void EnumerateVolumeDofs(IFiniteElementMesh mesh, ref int dof)
    {
        foreach (var element in mesh.Elements)
            for (int i = 0; i < element.DofOnElement(); i++)
                element.SetElementDof(i, dof++);
    }

    public static SortedSet<int>[] BuildPortraitFirstStep(IFiniteElementMesh mesh)
    {
        var portraitFirstStep = new SortedSet<int>[mesh.NumberOfDOFs];

        for (int i = 0; i < mesh.NumberOfDOFs; i++)
            portraitFirstStep[i] = [];

        foreach (var element in mesh.Elements)
        {
            foreach (var dofi in element.Dofs.Where(dof => dof >= 0))
            foreach (var dofj in element.Dofs.Where(dof => dof >= 0))
                portraitFirstStep[dofi]
                    .Add(dofj);
        }

        return portraitFirstStep;
    }

    private static int[] BuildVertexPortrait(IFiniteElementMesh mesh)
    {
        var vertexDofs = new int[mesh.Vertex.Length];

        foreach (var element in mesh.Elements)
        {
            for (int vertexi = 0; vertexi < element.VertexNumbers.Length; vertexi++)
            {
                var vertexDof = element.DofOnVertex(vertexi);
                vertexDofs[element.VertexNumbers[vertexi]] = vertexDof;
            }
        }

        return vertexDofs;
    }

    private static Dictionary<(int i, int j), int> BuildEdgePortrait(IFiniteElementMesh mesh)
    {
        var edgePortrait = new Dictionary<(int i, int j), int>();

        foreach (var element in mesh.Elements)
        {
            for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
            {
                var edge = element.GlobalEdge(edgei);
                var dofOnEdge = element.DofOnEdge(edgei);

                if (!edgePortrait.TryGetValue(edge, out int curDof)
                    || curDof > dofOnEdge)
                    edgePortrait[edge] = dofOnEdge;
            }
        }

        return edgePortrait;
    }

    private static void PrepareForEnumerateMesh(IFiniteElementMesh mesh)
    {
        foreach (var element in mesh.Elements)
        {
            for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
            {
                var edge = element.GlobalEdge(edgei);

                if (mesh.EdgesToElements.TryGetValue(edge, out var elements))
                    elements.Add(element);
                else
                    mesh.EdgesToElements[edge] = [element];
            }
        }

        foreach (var (edge, elements) in mesh.EdgesToElements)
        {
            if (elements.Count == 1)
            {
                elements[0].EdgesDofs[edge] = Math.Max(0, elements[0].Order - 1);
                continue;
            }

            var dof = Math.Max(elements.Min(elem => elem.Order) - 1, 0);

            foreach (var element in elements)
                element.EdgesDofs[edge] = dof;
        }
    }
}