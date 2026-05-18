using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BenchmarkDotNet.Attributes;
using Core.FEM;
using Services.ScriptCompilers;
using Services.ScriptCompilers.Interfaces;

namespace ViewModels.BenchmarkTests.MaterialTests;

[MemoryDiagnoser]
[SimpleJob(warmupCount: 1, iterationCount: 5)]
public class MaterialCompilationBenchmark
{
    private IScriptCompiler _compiler = null!;
    private List<MaterialConfig> _materials = null!;

    [Params(3, 6, 10)]
    public int MaterialCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _compiler = new CSharpScriptingCompiler();

        _materials = Enumerable.Range(0, MaterialCount)
            .Select(i => (i % 3) switch
            {
                0 => new MaterialConfig(IsVolume: true,  Is1: false, Is2: false),
                1 => new MaterialConfig(IsVolume: false, Is1: true,  Is2: false),
                _ => new MaterialConfig(IsVolume: false, Is1: false, Is2: true),
            })
            .ToList();
    }

    [Benchmark(Baseline = true, Description = "Sequential (foreach + await)")]
    public async Task<Dictionary<string, IMaterial>> Sequential()
    {
        var materials = new Dictionary<string, IMaterial>();

        foreach (var (name, config) in _materials.Select((c, i) => ($"mat_{i}", c)))
            materials[name] = await config.BuildMaterialSequentialAsync(_compiler);

        return materials;
    }

    [Benchmark(Description = "Parallel (WhenAll + ToDictionary)")]
    public async Task<Dictionary<string, IMaterial>> Parallel()
    {
        var tasks = _materials
            .Select((config, i) => (Name: $"mat_{i}", Task: config.BuildMaterialParallelAsync(_compiler)))
            .ToList();

        await Task.WhenAll(tasks.Select(t => t.Task));

        return tasks.ToDictionary(t => t.Name, t => t.Task.Result);
    }
}