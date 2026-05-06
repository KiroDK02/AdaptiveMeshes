using System;
using System.Collections.Generic;
using Core.TimeMesh;
using Core.FEM;
using Core.Vectors;

namespace Core.Solution.Interfaces;

public interface ISolution
{
    IFiniteElementMesh Mesh { get; }
    ReadOnlySpan<double> SolutionVector { get; set; }

    double Value(Vector2D point);
    Vector2D Gradient(Vector2D point);
    Vector2D Flow(Vector2D point, IDictionary<string, IMaterial> materials);

    double CalcErrorFrom(Func<Vector2D, double> otherSolution);
    double CalcErrorFrom(ISolution otherSolution);
}