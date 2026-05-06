using System;
using System.Threading.Tasks;
using Core.Vectors;

namespace Services.ScriptCompilers.Interfaces;

public interface IScriptCompiler
{
    Task<Func<Vector2D, double>> CompileStationaryFunction(string functionBody);
    Task<Func<Vector2D, double, double>> CompileNonStationaryFunction(string functionBody);
}