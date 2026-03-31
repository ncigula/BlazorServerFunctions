window.BENCHMARK_DATA = {
  "lastUpdate": 1774965277997,
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
        "date": 1774352658488,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 1)",
            "value": 22222099.285285283,
            "unit": "ns",
            "range": "± 748193.2701503612"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 5)",
            "value": 20702729.079391893,
            "unit": "ns",
            "range": "± 1035838.6380654132"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 20)",
            "value": 23872287.069444444,
            "unit": "ns",
            "range": "± 652356.6679971218"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 50)",
            "value": 24152113.2,
            "unit": "ns",
            "range": "± 274393.2677568845"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 100)",
            "value": 30449162.9125,
            "unit": "ns",
            "range": "± 608599.0625152619"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 200)",
            "value": 40304850.240963854,
            "unit": "ns",
            "range": "± 2092211.6647159525"
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
          "id": "9a5f900790b18a6e6454198d9ffa207a0ae31cc4",
          "message": "chore: remove stale tests/tests.md planning document\n\nPre-implementation planning doc superseded by the actual test suite,\nCLAUDE.md testing patterns section, and per-release CHANGELOG entries.\n\nCo-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-03-25T11:49:06+01:00",
          "tree_id": "9be74f6a2c81c2edb95a43f5386dc5f539838c33",
          "url": "https://github.com/ncigula/BlazorServerFunctions/commit/9a5f900790b18a6e6454198d9ffa207a0ae31cc4"
        },
        "date": 1774445291808,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "BlazorServerFunctions.Benchmarks.IncrementalGenerationBenchmarks.IncrementalRun",
            "value": 215793.97745768228,
            "unit": "ns",
            "range": "± 265.94222478736026"
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
          "id": "9a5f900790b18a6e6454198d9ffa207a0ae31cc4",
          "message": "chore: remove stale tests/tests.md planning document\n\nPre-implementation planning doc superseded by the actual test suite,\nCLAUDE.md testing patterns section, and per-release CHANGELOG entries.\n\nCo-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-03-25T11:49:06+01:00",
          "tree_id": "9be74f6a2c81c2edb95a43f5386dc5f539838c33",
          "url": "https://github.com/ncigula/BlazorServerFunctions/commit/9a5f900790b18a6e6454198d9ffa207a0ae31cc4"
        },
        "date": 1774445361755,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 1)",
            "value": 23447501.86923077,
            "unit": "ns",
            "range": "± 385242.8218365366"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 5)",
            "value": 20953395.21875,
            "unit": "ns",
            "range": "± 251286.58714166886"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 20)",
            "value": 25211506.285714284,
            "unit": "ns",
            "range": "± 356902.3638655937"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 50)",
            "value": 26173022.30952381,
            "unit": "ns",
            "range": "± 399383.959795759"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 100)",
            "value": 30683221.3125,
            "unit": "ns",
            "range": "± 1004282.3925660576"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 200)",
            "value": 39773636.26229508,
            "unit": "ns",
            "range": "± 1705664.9569743313"
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
          "id": "dabc6f4f871d0e82087f448bcebf00e3035b4e96",
          "message": "feat: add Result Mapper — library-agnostic result wrapper support (BSF029, BSF030)\n\nIntroduces IServerFunctionResultMapper<TResult, TValue> and ServerFunctionError in\nAbstractions, a new ResultMapper property on [ServerFunctionCollection], and full\ncode-generation support on both the server and client sides.\n\nGenerator changes:\n- Server endpoint: instantiates the mapper with new MapperType<T>(), calls IsSuccess /\n  GetValue / GetError, and emits Results.Ok(value) or Results.Problem(...) accordingly.\n  .Produces<TValue>() annotation uses the inner value type, not the wrapper.\n- Client proxy: on 2xx calls WrapValue; on 4xx/5xx parses the ProblemDetails response\n  body via System.Text.Json (no Microsoft.AspNetCore.Mvc reference needed in WASM) and\n  calls WrapFailure. CancellationToken is forwarded to all async HTTP operations.\n- void / Task return types and IAsyncEnumerable<T> streaming methods skip the mapper\n  silently.\n\nNew diagnostics:\n- BSF029 (error): ResultMapper set on a gRPC interface — REST-only feature.\n- BSF030 (warning): a method's return type is non-generic when ResultMapper is set;\n  the method falls back to Results.Ok(result) / direct deserialisation.\n\nSample additions:\n- Result<T> type, ResultMapper<T>, IResultDemoService in Sample.Shared\n- In-memory ResultDemoService (server) with 404 / 409 scenarios\n- Blazor Server and WASM demo pages; nav entries wired\n\nTests:\n- 11 new unit snapshot tests (ResultMapperGeneratorTests): single-arg client/server,\n  two-arg client/server, void skip, streaming skip, mixed, CT parameter, BSF029, BSF030\n- 8 new E2E tests (ResultDemoServiceClientTests): success path, NotFound (404),\n  Conflict (409), create/delete round-trips — mapper runs on both sides of the wire\n\nCo-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-03-30T19:01:17+02:00",
          "tree_id": "a67edce2abe988eecd40c36db215d9af4effb77a",
          "url": "https://github.com/ncigula/BlazorServerFunctions/commit/dabc6f4f871d0e82087f448bcebf00e3035b4e96"
        },
        "date": 1774890197354,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "BlazorServerFunctions.Benchmarks.IncrementalGenerationBenchmarks.IncrementalRun",
            "value": 209700.21883719307,
            "unit": "ns",
            "range": "± 467.64464855475893"
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
          "id": "dabc6f4f871d0e82087f448bcebf00e3035b4e96",
          "message": "feat: add Result Mapper — library-agnostic result wrapper support (BSF029, BSF030)\n\nIntroduces IServerFunctionResultMapper<TResult, TValue> and ServerFunctionError in\nAbstractions, a new ResultMapper property on [ServerFunctionCollection], and full\ncode-generation support on both the server and client sides.\n\nGenerator changes:\n- Server endpoint: instantiates the mapper with new MapperType<T>(), calls IsSuccess /\n  GetValue / GetError, and emits Results.Ok(value) or Results.Problem(...) accordingly.\n  .Produces<TValue>() annotation uses the inner value type, not the wrapper.\n- Client proxy: on 2xx calls WrapValue; on 4xx/5xx parses the ProblemDetails response\n  body via System.Text.Json (no Microsoft.AspNetCore.Mvc reference needed in WASM) and\n  calls WrapFailure. CancellationToken is forwarded to all async HTTP operations.\n- void / Task return types and IAsyncEnumerable<T> streaming methods skip the mapper\n  silently.\n\nNew diagnostics:\n- BSF029 (error): ResultMapper set on a gRPC interface — REST-only feature.\n- BSF030 (warning): a method's return type is non-generic when ResultMapper is set;\n  the method falls back to Results.Ok(result) / direct deserialisation.\n\nSample additions:\n- Result<T> type, ResultMapper<T>, IResultDemoService in Sample.Shared\n- In-memory ResultDemoService (server) with 404 / 409 scenarios\n- Blazor Server and WASM demo pages; nav entries wired\n\nTests:\n- 11 new unit snapshot tests (ResultMapperGeneratorTests): single-arg client/server,\n  two-arg client/server, void skip, streaming skip, mixed, CT parameter, BSF029, BSF030\n- 8 new E2E tests (ResultDemoServiceClientTests): success path, NotFound (404),\n  Conflict (409), create/delete round-trips — mapper runs on both sides of the wire\n\nCo-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-03-30T19:01:17+02:00",
          "tree_id": "a67edce2abe988eecd40c36db215d9af4effb77a",
          "url": "https://github.com/ncigula/BlazorServerFunctions/commit/dabc6f4f871d0e82087f448bcebf00e3035b4e96"
        },
        "date": 1774890271152,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 1)",
            "value": 21902259.173333332,
            "unit": "ns",
            "range": "± 825427.6868806246"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 5)",
            "value": 23927879.638461538,
            "unit": "ns",
            "range": "± 327243.6552117281"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 20)",
            "value": 24154763.14285714,
            "unit": "ns",
            "range": "± 572650.937531367"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 50)",
            "value": 28631136.769230776,
            "unit": "ns",
            "range": "± 984565.4797057253"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 100)",
            "value": 31103726.472972974,
            "unit": "ns",
            "range": "± 1039300.960745517"
          },
          {
            "name": "BlazorServerFunctions.Benchmarks.FullGenerationBenchmarks.FullGeneration(InterfaceCount: 200)",
            "value": 37121343.80120482,
            "unit": "ns",
            "range": "± 1977513.4553613802"
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
          "id": "b29fb116e2397d13ab058961c0078514f74c661d",
          "message": "feat: TypedResults on server endpoints — typed lambda return annotations, remove redundant .Produces/.ProducesProblem (§4)\n\n- Replace Results.Ok/Results.Problem with TypedResults.Ok/TypedResults.Problem in all generated endpoint handlers\n- Annotate lambda return types explicitly: async Task<Results<Ok<T>, ProblemHttpResult>> (...) =>\n  (or Ok<T> when GenerateProblemDetails = false; Results<Ok, ProblemHttpResult> for void)\n- Remove .Produces<T>(200) and .ProducesProblem(500) from fluent chains — now inferred by\n  ASP.NET Core OpenAPI from the typed lambda return annotation\n- Add using System.Threading.Tasks; and using Microsoft.AspNetCore.Http.HttpResults; to generated files\n- User-specified ProducesStatusCodes still emitted as explicit .Produces(statusCode) calls\n- 99 snapshot files updated across ServerGeneratorTests, ConfigurationTests,\n  GrpcServerGeneratorTests, and ResultMapperGeneratorTests\n\nCo-Authored-By: Claude Sonnet 4.6 <noreply@anthropic.com>",
          "timestamp": "2026-03-31T12:01:48+02:00",
          "tree_id": "aa8dfd50710ccdb36e4deec121ab0a1750d7a2f6",
          "url": "https://github.com/ncigula/BlazorServerFunctions/commit/b29fb116e2397d13ab058961c0078514f74c661d"
        },
        "date": 1774965277541,
        "tool": "benchmarkdotnet",
        "benches": [
          {
            "name": "BlazorServerFunctions.Benchmarks.IncrementalGenerationBenchmarks.IncrementalRun",
            "value": 205244.1594426082,
            "unit": "ns",
            "range": "± 639.8532787695191"
          }
        ]
      }
    ]
  }
}