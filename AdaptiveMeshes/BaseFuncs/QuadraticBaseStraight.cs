namespace AdaptiveMeshes.BaseFuncs
{
    public static class QuadraticBaseStraight
    {
        public static readonly Func<double, double>[] BasesFuncs =
        {
            x => 2 * (x - 1 / 2d) * (x - 1),
            x => 2 * x * (x - 1 / 2d),
            x => -4 * x * (x - 1)
        };
    }
}
