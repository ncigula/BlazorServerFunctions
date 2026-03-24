# Benchmarks

Performance benchmarks for the BlazorServerFunctions source generator, measuring incremental
generator execution time and memory allocations using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## What Is Measured

| Benchmark | Description |
|---|---|
| `FullGenerationBenchmarks` | Cold-start pipeline time for 1 / 5 / 20 / 50 / 100 / 200 interfaces. Shows how generation time scales with interface count. |
| `IncrementalGenerationBenchmarks` | Re-run time after an unrelated edit. Should be near-zero — proves the generator's incremental caching is working. |

## Running Locally

Benchmarks **must** be run in Release mode — Debug builds are not representative.

```bash
# Full run — takes several minutes, produces JSON/HTML/CSV artifacts
dotnet run --configuration Release --project tests/BlazorServerFunctions.Benchmarks \
  --filter '*' --exporters json --artifacts ./benchmark-artifacts

# Quick smoke test — fast iterations, not statistically valid but useful for sanity checks
dotnet run --configuration Release --project tests/BlazorServerFunctions.Benchmarks \
  --filter '*' --job short

# Run a specific benchmark class
dotnet run --configuration Release --project tests/BlazorServerFunctions.Benchmarks \
  --filter '*FullGeneration*' --exporters json --artifacts ./benchmark-artifacts
```

Artifacts land in `./benchmark-artifacts/results/` (or `BenchmarkDotNet.Artifacts/results/` if
`--artifacts` is omitted).

## Interpreting the Output

```
| Method         | InterfaceCount | Mean     | Allocated |
|----------------|---------------|----------|-----------|
| FullGeneration | 1             |  ~20 ms  |           |
| FullGeneration | 5             |  ~22 ms  |           |
| FullGeneration | 20            |  ~26 ms  |           |
| FullGeneration | 50            |  ~30 ms  |           |
| FullGeneration | 100           |  ~35 ms  |           |
| FullGeneration | 200           |  ~40 ms  |           |
| IncrementalRun | -             | ~207 µs  |           |  ← near-zero = caching works
```

- **Mean** — average wall-clock time per call after JIT warmup
- **Allocated** — bytes allocated on the managed heap per call (lower = less GC pressure)
- **IncrementalRun near-zero** — confirms the generator's `IIncrementalGenerator` pipeline
  correctly memoizes results; if this is comparable to `FullGeneration` there is a caching bug

## Updating Baselines

After adding a feature or making a performance-sensitive change, update the committed baseline:

```bash
# Run benchmarks with JSON export
dotnet run --configuration Release --project tests/BlazorServerFunctions.Benchmarks \
  --filter '*' --exporters json --artifacts ./benchmark-artifacts

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
**https://ncigula.github.io/BlazorServerFunctions/dev/bench/**

The `gh-pages` branch is fully managed by the CI workflow — do not commit to it manually.
