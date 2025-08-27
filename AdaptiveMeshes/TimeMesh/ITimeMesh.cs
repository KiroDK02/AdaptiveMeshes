namespace AdaptiveMeshes.TimeMesh
{
    public interface ITimeMesh
    {
        double this[int i] { get; }

        int Size();
        double[] Weights(int i);
        void ChangeWeights(double[] weights);
        bool IsChangeStep(int i);
        void DoubleMesh();
    }
}
