namespace AdaptiveMeshes.Interfaces
{
    public interface IProblem
    {
        IDictionary<string, IMaterial> Materials { get; }
        ISolution Solution { get; set; }

        void Prepare();
        double? Solve();
    }
}
