using System;
using System.Threading.Tasks;
using Core.Vectors;
using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using Services.ScriptCompilers.Interfaces;

namespace Services.ScriptCompilers;

public class CSharpScriptingCompiler : IScriptCompiler
{
    private readonly ScriptOptions _options =
        ScriptOptions.Default
            .AddReferences(typeof(Vector2D).Assembly)
            .AddImports("System", "Core.Vectors");

    public async Task<Func<Vector2D, double>> CompileStationaryFunction(string functionBody)
    {
        var code = WrapStationaryFunction(functionBody);

        return await Task.Run(() =>
            CSharpScript.EvaluateAsync<Func<Vector2D, double>>(code, _options)); 
    }

    public async Task<Func<Vector2D, double, double>> CompileNonStationaryFunction(string functionBody)
    {
        var code = WrapNonStationaryFunction(functionBody);

        return await Task.Run(() =>
            CSharpScript.EvaluateAsync<Func<Vector2D, double, double>>(code, _options));
    }

    private static string WrapStationaryFunction(string functionBody) =>
        $"new Func<Core.Vectors.Vector2D, double>(point => {{{CheckFunctionBody(functionBody)};}})";

    private static string WrapNonStationaryFunction(string functionBody) =>
        $"new Func<Core.Vectors.Vector2D, double, double>((point, t) => {{{CheckFunctionBody(functionBody)};}})";
    
    private static string CheckFunctionBody(string functionBody)
    {
        functionBody = functionBody.Trim();

        if (!functionBody.StartsWith("return") && !functionBody.EndsWith(';'))
            functionBody = $"return {functionBody};";

        return functionBody;
    }
}