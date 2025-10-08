using AdaptiveMeshes.FEM.Materials;
using AdaptiveMeshes.FEM.Mesh;
using AdaptiveMeshes.FiniteElements;
using AdaptiveMeshes.FiniteElements.FiniteElements1D;
using AdaptiveMeshes.Solution;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Splines
{
    public static class AlgorithmsForSplines
    {
        public static IFiniteElementMesh GetMesh(double x0, double x1, int countX)
        {
            Vector2D[] vertices = new Vector2D[countX];
            List<IFiniteElement> elements = new(vertices.Length - 1);
            double hx = (x1 - x0) / (countX - 1);

            vertices[0] = new(x0, 0);
            for (int i = 1; i < vertices.Length; i++)
            {
                vertices[i] = new(x0 + i * hx, 0);
                elements.Add(new SegmentFECubicHermitianBase("", [i - 1, i]));
            }
            vertices[^1] = new(x1, 0);

            return new FiniteElementMesh(elements, vertices);
        }

        public static double[] GetPointsForCalculating(double x0, double x1, int countX)
        {
            double hx = (x1 - x0) / (countX - 1);
            double[] x = new double[countX];

            x[0] = x0;
            x[^1] = x1;

            for (int i = 1; i < countX - 1; i++)
                x[i] = x0 + i * hx;

            return x;
        }

        public static double[] ValuesAtPoints(ISolution sol, double[] x, double yCoord)
        {
            double[] values = new double[x.Length];

            for (int i = 0; i < x.Length; i++)
            {
                Vector2D point = new(x[i], yCoord);

                values[i] = sol.Value(point);
            }

            return values;
        }

        public static Vector2D[] GradientsAtPoints(ISolution sol, double[] x, double yCoord)
        {
            Vector2D[] values = new Vector2D[x.Length];

            for (int i = 0; i < x.Length; i++)
            {
                Vector2D point = new(x[i], yCoord);

                values[i] = sol.Gradient(point);
            }

            return values;
        }

        public static Vector2D[] FlowsAtPoints<TResult>(ISolution sol, double[] x, double yCoord, IDictionary<string, IMaterial> materials)
        {
            Vector2D[] values = new Vector2D[x.Length];

            for (int i = 0; i < x.Length; i++)
            {
                Vector2D point = new(x[i], yCoord);

                values[i] = sol.Flow(point, materials);
            }

            return values;
        }

        public static double CalcApproximateL2NormDifference(Func<double, double> funcReal, Func<double, double> funcNum, double[] points)
        {
            double norm = 0;
            double normDifference = 0;

            foreach (double point in points)
            {
                double valReal = funcReal(point);
                double valNum = funcNum(point);

                normDifference += (valReal - valNum) * (valReal - valNum);
                norm += valReal * valReal;
            }

            return Math.Sqrt(normDifference / norm);
        }
    }
}
