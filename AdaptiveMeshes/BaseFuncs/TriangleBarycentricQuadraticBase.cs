using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.BaseFuncs
{
    public static class TriangleBarycentricQuadraticBase
    {
        public static readonly Func<Vector2D, double>[] BasesFuncs =
        {
            (Vector2D vert) => (1 - vert.X - vert.Y) * (2 * (1 - vert.X - vert.Y) - 1),
            (Vector2D vert) => vert.X * (2 * vert.X - 1),
            (Vector2D vert) => vert.Y * (2 * vert.Y - 1),
            (Vector2D vert) => 4 * (1 - vert.X - vert.Y) * vert.X,
            (Vector2D vert) => 4 * vert.X * vert.Y,
            (Vector2D vert) => 4 * (1 - vert.X - vert.Y) * vert.Y
        };

        public static readonly Func<Vector2D, double>[,] GradientBasesFuncs =
        {
            {
                (Vector2D vert) => -(2 * (1 - vert.X - vert.Y) - 1) - 2 * (1 - vert.X - vert.Y),
                (Vector2D vert) => -(2 * (1 - vert.X - vert.Y) - 1) - 2 * (1 - vert.X - vert.Y)
            },

            {
                (Vector2D vert) => (2 * vert.X - 1) + 2 * vert.X,
                (Vector2D vert) => 0
            },

            {
                (Vector2D vert) => 0,
                (Vector2D vert) => (2 * vert.Y - 1) + 2 * vert.Y
            },

            {
                (Vector2D vert) => 4 * (1 - 2 * vert.X - vert.Y),
                (Vector2D vert) => -4 * vert.X
            },

            {
                (Vector2D vert) => 4 * vert.Y,
                (Vector2D vert) => 4 * vert.X
            },

            {
                (Vector2D vert) => -4 * vert.Y,
                (Vector2D vert) => 4 * (1 - vert.X - 2 * vert.Y)
            }
        };
    }
}
