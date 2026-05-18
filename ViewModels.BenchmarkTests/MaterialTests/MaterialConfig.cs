using System;
using System.Threading.Tasks;
using Core.FEM;
using Core.Vectors;
using Services.ScriptCompilers.Interfaces;

namespace ViewModels.BenchmarkTests.MaterialTests;

public record MaterialConfig(bool IsVolume, bool Is1, bool Is2)
{
    private const string StationaryBody = "point.X * point.X + point.Y";
    private const string NonStationaryBody = "point.X * t + point.Y";

    public async Task<IMaterial> BuildMaterialParallelAsync(IScriptCompiler compiler)
    {
        Func<Vector2D, double> lambda = _ => 0;
        Func<Vector2D, double> sigma = _ => 0;
        Func<Vector2D, double, double> f = (_, _) => 0;
        Func<Vector2D, double, double> ug = (_, _) => 0;
        Func<Vector2D, double, double> thetta = (_, _) => 0;

        if (IsVolume)
        {
            var lambdaTask = compiler.CompileStationaryFunction(StationaryBody);
            var sigmaTask = compiler.CompileStationaryFunction(StationaryBody);
            var fTask = compiler.CompileNonStationaryFunction(NonStationaryBody);

            await Task.WhenAll(lambdaTask, sigmaTask, fTask);

            lambda = await lambdaTask;
            sigma = await sigmaTask;
            f = await fTask;
        }
        else if (Is1)
            ug = await compiler.CompileNonStationaryFunction(NonStationaryBody);
        else if (Is2)
            thetta = await compiler.CompileNonStationaryFunction(NonStationaryBody);

        return new Material(IsVolume, Is1, Is2, lambda, sigma, ug, thetta, f);
    }

    public async Task<IMaterial> BuildMaterialSequentialAsync(IScriptCompiler compiler)
    {
        Func<Vector2D, double> lambda = _ => 0;
        Func<Vector2D, double> sigma = _ => 0;
        Func<Vector2D, double, double> f = (_, _) => 0;
        Func<Vector2D, double, double> ug = (_, _) => 0;
        Func<Vector2D, double, double> thetta = (_, _) => 0;

        if (IsVolume)
        {
            lambda = await compiler.CompileStationaryFunction(StationaryBody);
            sigma = await compiler.CompileStationaryFunction(StationaryBody);
            f = await compiler.CompileNonStationaryFunction(NonStationaryBody);
        }
        else if (Is1)
            ug = await compiler.CompileNonStationaryFunction(NonStationaryBody);
        else if (Is2)
            thetta = await compiler.CompileNonStationaryFunction(NonStationaryBody);

        return new Material(IsVolume, Is1, Is2, lambda, sigma, ug, thetta, f);
    }
}