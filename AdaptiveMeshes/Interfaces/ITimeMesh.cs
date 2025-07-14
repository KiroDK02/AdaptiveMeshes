using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AdaptiveMeshes.Interfaces
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
