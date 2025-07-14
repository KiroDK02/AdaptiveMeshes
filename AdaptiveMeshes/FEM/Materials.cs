using AdaptiveMeshes.Interfaces;
using AdaptiveMeshes.Vectors;

namespace AdaptiveMeshes.FEM
{
    public class Material : IMaterial
    {
        public Material(bool isVolume, bool is1, bool is2, Func<Vector2D, double> lambda, Func<Vector2D, double> sigma, Func<Vector2D, double, double> ug, Func<Vector2D, double, double> thetta, Func<Vector2D, double, double> f)
        {
            IsVolume = isVolume;
            Is1 = is1;
            Is2 = is2;
            Lambda = lambda;
            Sigma = sigma;
            Ug = ug;
            Thetta = thetta;
            F = f;
        }

        public bool IsVolume { get; }
        public bool Is1 { get; }
        public bool Is2 { get; }

        public Func<Vector2D, double> Lambda { get; }
        public Func<Vector2D, double> Sigma { get; }
        public Func<Vector2D, double, double> F { get; }
        public Func<Vector2D, double, double> Ug { get; }
        public Func<Vector2D, double, double> Thetta { get; }
    }
}
