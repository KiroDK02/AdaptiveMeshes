using System;
using Core.SLAE;

namespace Core.SLAESolvers;

public interface ISLAESolver : IDisposable
{
    ISLAE SLAE { get; }
    double[] Solve();
}