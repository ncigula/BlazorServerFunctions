# Benchmarks

Performance benchmarks for the BlazorServerFunctions source generator, measuring incremental
generator execution time and memory allocations using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## What Is Measured

| Benchmark | Description |
|---|---|
| `FullGenerationBenchmarks` | Cold-start pipeline time for 1 / 5 / 20 interfaces. Shows how generation time scales with interface count. |
| `IncrementalGenerationBenchmarks` | Re-run time after an unrelated edit. Should be near-zero — proves the generator's incremental caching is working. |

## Running Locally

Benchmarks **must** be run in Release mode — Debug builds are not representative.

```bash
# Full run — takes several minutes, produces JSON/HTML/CSV artifacts
dotnet run -c Release --project tests/BlazorServerFunctions.Benchmarks \
  -- --exporters json --artifacts ./benchmark-artifacts

# Quick smoke test — fast iterations, not statistically valid but useful for sanity checks
dotnet run -c Release --project tests/BlazorServerFunctions.Benchmarks \
  -- --job short --filter '*'

# Run a specific benchmark class
dotnet run -c Release --project tests/BlazorServerFunctions.Benchmarks \
  -- --filter '*FullGeneration*'
```

Artifacts land in `./benchmark-artifacts/results/` (or `BenchmarkDotNet.Artifacts/results/` if
`--artifacts` is omitted).

## Interpreting the Output

```
| Method         | InterfaceCount | Mean     | Allocated |
|----------------|---------------|----------|-----------|
| FullGeneration | 1             |  12.3 ms |   2.1 MB  |
| FullGeneration | 5             |  48.7 ms |   8.6 MB  |
| FullGeneration | 20            | 196.2 ms |  34.4 MB  |
| IncrementalRun | ?             |   0.9 ms | 120.0 KB  |  ← near-zero = caching works
```

- **Mean** — average wall-clock time per call after JIT warmup
- **Allocated** — bytes allocated on the managed heap per call (lower = less GC pressure)
- **IncrementalRun near-zero** — confirms the generator's `IIncrementalGenerator` pipeline
  correctly memoizes results; if this is comparable to `FullGeneration` there is a caching bug

## Updating Baselines

After adding a feature or making a performance-sensitive change, update the committed baseline:

```bash
# Run benchmarks with JSON export
dotnet run -c Release --project tests/BlazorServerFunctions.Benchmarks \
  -- --exporters json --artifacts ./benchmark-artifacts

# Copy the JSON files to the baselines folder
cp benchmark-artifacts/results/*.json benchmarks/baselines/

# Commit alongside the code change
git add benchmarks/baselines/
git commit -m "perf: update benchmark baseline after §X.Y"
```

Baselines are stored in `benchmarks/baselines/`. The CI workflow (`benchmarks.yml`) also
stores historical results in the `gh-pages` branch automatically on every push to `master`.

## CI Integration

The `.github/workflows/benchmarks.yml` workflow runs on every push to `master` and stores
results via [`github-action-benchmark`](https://github.com/benchmark-action/github-action-benchmark).

Historical trend charts are published to:
`https://<owner>.github.io/<repo>/dev/bench/`

To enable the charts, go to **Settings → Pages** in your GitHub repository and set the source
to the `gh-pages` branch.
