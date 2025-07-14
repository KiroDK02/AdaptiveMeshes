using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.Interfaces;

namespace AdaptiveMeshes.FEM
{
    public static class FEMAlgorithms
    {
        public static void EnumerateMeshDofs(IFiniteElementMesh mesh)
        {
            int dof = 0;
            
            EnumerateVerticesDofs(mesh, ref dof);
            EnumerateEdgesDofs(mesh, ref dof);
            EnumerateVolumeDofs(mesh, ref dof);

            mesh.NumberOfDOFs = dof;
        }

        public static void EnumerateVerticesDofs(IFiniteElementMesh mesh, ref int dof)
        {
            int[] vertexPortrait = BuildVertexPortrait(mesh);

            for (int i = 0; i < vertexPortrait.Length; i++)
                vertexPortrait[i] = dof += vertexPortrait[i];

            foreach (var element in mesh.Elements)
            {
                for (int vertexi = 0; vertexi < element.VertexNumber.Length; vertexi++)
                {
                    int dofOnVertex = element.DOFOnVertex(vertexi);
                    int startDof = vertexPortrait[element.VertexNumber[vertexi]] - dofOnVertex;

                    for (int n = 0; n < dofOnVertex; n++)
                        element.SetVertexDOF(vertexi, n, startDof + n);
                }
            }
        }

        public static void EnumerateEdgesDofs(IFiniteElementMesh mesh, ref int dof)
        {
            var edgesPortrait = BuildEdgePortrait(mesh);
            
            int tempDof = dof;
            edgesPortrait = edgesPortrait.ToDictionary(edges => edges.Key, edges => tempDof += edges.Value);
            dof = tempDof;

            foreach (var element in mesh.Elements)
            {
                for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                {
                    var edge = element.GlobalEdge(edgei);
                    int dofOnEdge = element.DOFOnEdge(edgei);
                    int startDof = edgesPortrait[edge] - dofOnEdge;

                    for (int j = 0; j < dofOnEdge; j++)
                        element.SetEdgeDOF(edgei, j, startDof + j);
                }
            }
        }

        public static void EnumerateVolumeDofs(IFiniteElementMesh mesh, ref int dof)
        {
            foreach (var element in mesh.Elements)
                for (int i = 0; i < element.DOFOnElement(); i++)
                    element.SetElementDOF(i, dof++);
        }

        public static SortedSet<int>[] BuildPortraitFirstStep(IFiniteElementMesh mesh)
        {
            var portraitFirstStep = new SortedSet<int>[mesh.NumberOfDOFs];

            for (int i = 0; i < mesh.NumberOfDOFs; i++)
                portraitFirstStep[i] = [];

            foreach (var element in mesh.Elements)
            {
                for (int i = 0; i < element.Dofs.Length; i++)
                    for (int j = 0; j < element.Dofs.Length; j++)
                        portraitFirstStep[element.Dofs[i]].Add(element.Dofs[j]);
            }

            return portraitFirstStep;
        }

        public static int[] BuildVertexPortrait(IFiniteElementMesh mesh)
        {
            int[] vertexDofs = new int[mesh.Vertex.Length];

            foreach (var element in mesh.Elements)
            {
                for (int vertexi = 0; vertexi < element.VertexNumber.Length; vertexi++)
                {
                    int vertexDof = element.DOFOnVertex(vertexi);
                    vertexDofs[element.VertexNumber[vertexi]] = vertexDof;
                }
            }

            return vertexDofs;
        }

        public static Dictionary<(int i, int j), int> BuildEdgePortrait(IFiniteElementMesh mesh)
        {
            var edgePortrait = new Dictionary<(int i, int j), int>();

            foreach (var element in mesh.Elements)
            {
                for (int edgei = 0; edgei < element.NumberOfEdges; edgei++)
                {
                    var edge = element.GlobalEdge(edgei);
                    int dofOnEdge = element.DOFOnEdge(edgei);

                    if (!edgePortrait.TryGetValue(edge, out int curDof) || curDof > dofOnEdge)
                        edgePortrait[edge] = dofOnEdge;
                }
            }

            return edgePortrait;
        }
    }
}
