using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.Interfaces
{
    public interface IMaterial
    {
        bool IsVolume { get; }
        bool Is1 { get; }
        bool Is2 { get; }

        Func<Vector2D, double> Lambda { get; }
        Func<Vector2D, double> Sigma { get; }
        Func<Vector2D, double, double> F { get; }

        Func<Vector2D, double, double> Ug { get; }
        Func<Vector2D, double, double> Thetta { get; }
    }
}
