using BenchmarkDotNet.Attributes;
using BlazorServerFunctions.Generator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BlazorServerFunctions.Benchmarks;

/// <summary>
/// Measures cold-start generation time — how long it takes to run the full pipeline
/// from a fresh <see cref="CSharpCompilation"/>, parameterized by interface count.
/// This shows whether generation time scales linearly with the number of interfaces.
/// </summary>
[MemoryDiagnoser]
public class FullGenerationBenchmarks
{
    /// <summary>
    /// Number of [ServerFunctionCollection] interfaces in the compilation.
    /// BenchmarkDotNet runs each benchmark once per value, producing one row per count.
    /// </summary>
    [Params(1, 5, 20, 50, 100, 200)]
    public int InterfaceCount { get; set; }

    [Benchmark(Description = "Full pipeline from fresh compilation")]
    public GeneratorDriverRunResult FullGeneration()
    {
        var compilation = BenchmarkCompilationHelper.BuildServerCompilation(InterfaceCount);
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ServerFunctionCollectionGenerator());
        driver = driver.RunGenerators(compilation);
        return driver.GetRunResult();
    }
}
