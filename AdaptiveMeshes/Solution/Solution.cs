using AdaptiveMeshes.Interfaces;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Solution
{
    public class SolutionStationaryProblem : ISolution
    {
        public SolutionStationaryProblem(IFiniteElementMesh mesh)
        {
            Mesh = mesh;
            solutionVector = new double[mesh.NumberOfDOFs];
        }

        public double Time
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public IFiniteElementMesh Mesh { get; }
        public ITimeMesh TimeMesh => throw new NotSupportedException();

        double[] solutionVector = [];
        public ReadOnlySpan<double> SolutionVector
        {
            get => solutionVector;
            set => solutionVector = value.ToArray();
        }

        public void AddSolutionVector(double t, double[] solution)
            => throw new NotSupportedException();

        public Vector2D Flow(Vector2D point, IDictionary<string, IMaterial> materials)
        {
            foreach (var element in Mesh.Elements)
            {
                if (element.VertexNumber.Length == 2)
                    continue;

                if (element.IsPointOnElement(Mesh.Vertex, point))
                {
                    var lambda = materials[element.Material].Lambda;

                    return -lambda(point) * element.GetGradientAtPoint(Mesh.Vertex, SolutionVector, point);
                }
            }

            throw new ArgumentException("The point is outside mesh.");
        }

        public Vector2D Gradient(Vector2D point)
        {
            foreach (var element in Mesh.Elements)
            {
                if (element.VertexNumber.Length == 2)
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
                if (element.VertexNumber.Length == 2)
                    continue;

                if (element.IsPointOnElement(Mesh.Vertex, point))
                    return element.GetValueAtPoint(Mesh.Vertex, SolutionVector, point);
            }

            throw new ArgumentException("The point is outside mesh.");
        }
    }
}
