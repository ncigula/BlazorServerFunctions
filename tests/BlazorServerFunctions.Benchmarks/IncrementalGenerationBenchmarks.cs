using BenchmarkDotNet.Attributes;
using BlazorServerFunctions.Generator.Generators;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace BlazorServerFunctions.Benchmarks;

/// <summary>
/// Measures incremental generation time — how long it takes to re-run the generator
/// after an unrelated edit (a new class with no BSF attributes is added).
/// <para>
/// A correctly implemented <see cref="IIncrementalGenerator"/> should produce near-zero
/// time here because the BSF interface detection pipeline step is memoized. If this
/// benchmark is unexpectedly slow it means the generator has a caching bug and is
/// re-processing everything on every change — hurting IDE responsiveness.
/// </para>
/// </summary>
[MemoryDiagnoser]
public class IncrementalGenerationBenchmarks
{
    private CSharpCompilation _editedCompilation = null!;
    private GeneratorDriver _warmedDriver = null!;

    /// <summary>
    /// Runs once before the benchmark iterations begin.
    /// Builds the base compilation, warms the generator caches with a first run,
    /// and prepares an "edited" compilation that adds an unrelated class.
    /// </summary>
    [GlobalSetup]
    public void Setup()
    {
        var baseCompilation = BenchmarkCompilationHelper.BuildServerCompilation(interfaceCount: 5);

        // Simulate a real-world unrelated edit: a new class with no BSF attributes.
        // The incremental generator should detect no BSF-relevant change and skip
        // re-parsing all BSF interfaces.
        var unrelatedTree = CSharpSyntaxTree.ParseText("public class UnrelatedEdit { public int Value { get; set; } }");
        _editedCompilation = baseCompilation.AddSyntaxTrees(unrelatedTree);

        // Warm the generator caches with the base compilation so the first benchmark
        // iteration is not a cold start.
        GeneratorDriver driver = CSharpGeneratorDriver.Create(new ServerFunctionCollectionGenerator());
        _warmedDriver = driver.RunGenerators(baseCompilation);
    }

    [Benchmark(Description = "Re-run after unrelated edit — should hit generator cache")]
    public GeneratorDriverRunResult IncrementalRun()
        => _warmedDriver.RunGenerators(_editedCompilation).GetRunResult();
}
