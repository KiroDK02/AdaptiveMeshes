using AdaptiveMeshes.FEM;
using AdaptiveMeshes.Solution.Interfaces;
using AdaptiveMeshes.TimeMesh;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Solution
{
    public class SolutionStationaryProblem : ISolution
    {
        public SolutionStationaryProblem(IFiniteElementMesh mesh)
        {
            Mesh = mesh;
            _solutionVector = new double[mesh.NumberOfDOFs];
        }

        public IFiniteElementMesh Mesh { get; }

        private double[] _solutionVector;

        public ReadOnlySpan<double> SolutionVector
        {
            get => _solutionVector;
            set => _solutionVector = value.ToArray();
        }

        public Vector2D Flow(Vector2D point, IDictionary<string, IMaterial> materials)
        {
            foreach (var element in Mesh.Elements)
            {
                if (element.VertexNumbers.Length == 2)
                    continue;

                if (!element.IsPointOnElement(Mesh.Vertex, point)) 
                    continue;
                
                var lambda = materials[element.Material].Lambda;

                return -lambda(point) * element.GetGradientAtPoint(Mesh.Vertex, SolutionVector, point);
            }

            throw new ArgumentException("The point is outside mesh.");
        }

        public Vector2D Gradient(Vector2D point)
        {
            foreach (var element in Mesh.Elements)
            {
                if (element.VertexNumbers.Length == 2)
                    continue;

                if (element.IsPointOnElement(Mesh.Vertex, point))
                    return element.GetGradientAtPoint(Mesh.Vertex, SolutionVector, point);
            }

            throw new ArgumentException("The point is outside mesh.");
        }

        public double Value(Vector2D point)
        {
            foreach (var element in Mesh.Elements)
            {
                if (element.VertexNumbers.Length == 2)
                    continue;

                if (element.IsPointOnElement(Mesh.Vertex, point))
                    return element.GetValueAtPoint(Mesh.Vertex, SolutionVector, point);
            }

            throw new ArgumentException("The point is outside mesh.");
        }
    }
}