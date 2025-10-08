namespace AdaptiveMeshes.FEM.Materials.GeometryMaterials
{
    public class EdgesData(int countIntervals, double ratio)
    {
        public int CountIntervals { get; set; } = countIntervals;
        public double Ratio { get; } = ratio;
    }
}
