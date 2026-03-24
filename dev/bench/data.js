window.BENCHMARK_DATA = {
  "lastUpdate": 1774352578960,
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
      },
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
        "date": 1774351477366,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 1)",
            "value": 19977981.613333333,
            "unit": "ns",
            "range": "± 335982.907247408"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 5)",
            "value": 21235532.993895,
            "unit": "ns",
            "range": "± 1190249.7933549334"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 20)",
            "value": 22024047.78125,
            "unit": "ns",
            "range": "± 195968.87613548312"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 50)",
            "value": 27250534.114285715,
            "unit": "ns",
            "range": "± 323596.3938120808"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 100)",
            "value": 31722011.779761903,
            "unit": "ns",
            "range": "± 1096939.6773178251"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 200)",
            "value": 40093859.884615384,
            "unit": "ns",
            "range": "± 294757.80315301195"
          }
        ]
      },
      {
        "commit": {
          "author": {
            "email": "nikola.cigula@vanado.hr",
            "name": "ncigula",
            "username": "ncigulaatvanado"
          },
          "committer": {
            "email": "nikola.cigula@vanado.hr",
            "name": "ncigula",
            "username": "ncigulaatvanado"
          },
          "distinct": true,
          "id": "ecdfa57f9d0e7bd9c5fff6bfdd0cc2c9c5588618",
          "message": "bump: update package version to 0.10.0",
          "timestamp": "2026-03-24T12:41:41+01:00",
          "tree_id": "99a862b9c5b6261b73f8d62df55c3424ec7b7252",
          "url": "https://github.com/ncigula/BlazorServerFunctions/commit/ecdfa57f9d0e7bd9c5fff6bfdd0cc2c9c5588618"
        },
        "date": 1774352578348,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "BlazorServerFunctions.Benchmarks.IncrementalGenerationBenchmarks.IncrementalRun",
            "value": 199478.72166090744,
            "unit": "ns",
            "range": "± 1103.6583065086422"
          }
        ]
      }
    ]
  }
}