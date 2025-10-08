namespace AdaptiveMeshes.BaseFuncs
{
    public static class CubicHermitianBase1D
    {
        public static readonly Func<double, double>[] BasesFuncs =
        {
            x => 1.0 - 3.0 * x * x + 2.0 * x * x * x,
            x => x - 2.0 * x * x + x * x * x,
            x => 3.0 * x * x - 2.0 * x * x * x,
            x => -x * x + x * x * x
        };

        public static readonly Func<double, double>[] GradientBaseFuncs =
        {
            x => -6.0 * x + 6.0 * x * x,
            x => 1.0 - 4.0 * x + 3.0 * x * x,
            x => 6.0 * x - 6.0 * x * x,
            x => -2.0 * x + 3.0 * x * x
        };
    }
}
