window.BENCHMARK_DATA = {
  "lastUpdate": 1774351415542,
  "repoUrl": "https://github.com/ncigula/BlazorServerFunctions",
  "entries": {
    "Benchmark": [
      {
        "commit": {
          "author": {
            "name": "ncigula",
            "username": "ncigulaatvanado",
            "email": "nikola.cigula@vanado.hr"
          },
          "committer": {
            "name": "ncigula",
            "username": "ncigulaatvanado",
            "email": "nikola.cigula@vanado.hr"
          },
          "id": "da3246bbc0b9399211be98540d1c2e9f75ef2cfc",
          "message": "ci: add workflows for release, benchmarks, and enhanced CI triggers\n\n- Added `.github/workflows/release.yml` to automate NuGet packaging and publishing on new releases.\n- Updated `benchmarks.yml` for matrix-based benchmarks and improved result storage/reporting.\n- Enhanced `ci.yml` with path filters and environmental consistency across workflows.",
          "timestamp": "2026-03-24T11:14:31Z",
          "url": "https://github.com/ncigula/BlazorServerFunctions/commit/da3246bbc0b9399211be98540d1c2e9f75ef2cfc"
        },
        "date": 1774351414940,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "BlazorServerFunctions.Benchmarks.IncrementalGenerationBenchmarks.IncrementalRun",
            "value": 206902.61521809894,
            "unit": "ns",
            "range": "± 542.3758621829209"
          }
        ]
      }
    ]
  }
}