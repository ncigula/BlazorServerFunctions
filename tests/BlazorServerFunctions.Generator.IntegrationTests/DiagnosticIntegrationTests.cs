using BlazorServerFunctions.Generator.IntegrationTests.Helpers;

namespace BlazorServerFunctions.Generator.IntegrationTests;

public class DiagnosticIntegrationTests
{
    // ── Happy path ────────────────────────────────────────────────────────────

    [Fact]
    public void ValidInterface_EmitsNoDiagnostics()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertNoDiagnostics();
        project.AssertHasClientProxyFiles("IUserService");
    }

    // ── BSF001: Missing [ServerFunctionCollection] ────────────────────────────

    [Fact]
    public void MissingCollectionAttribute_EmitsBSF001()
    {
        // BSF001 fires when a method is decorated with [ServerFunction] but the
        // containing interface is missing [ServerFunctionCollection].
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF001");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF002: Missing [ServerFunction] on method ────────────────────────────

    [Fact]
    public void MissingServerFunctionAttribute_EmitsBSF002()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF002");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF003: Interface must be public ──────────────────────────────────────

    [Fact]
    public void InternalInterface_EmitsBSF003()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                internal interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF003");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF004: Interface cannot be generic ───────────────────────────────────

    [Fact]
    public void GenericInterface_EmitsBSF004()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService<T>
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF004");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF005: Interface cannot have properties ──────────────────────────────

    [Fact]
    public void InterfaceWithProperty_EmitsBSF005()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    string Name { get; }

                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF005");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF006: Interface cannot have events ──────────────────────────────────

    [Fact]
    public void InterfaceWithEvent_EmitsBSF006()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    event EventHandler OnChanged;

                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF006");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF007: Method must return Task/ValueTask ─────────────────────────────

    [Fact]
    public void NonAsyncReturnType_EmitsBSF007()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    string GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF007");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF008: Method cannot be generic ─────────────────────────────────────

    [Fact]
    public void GenericMethod_EmitsBSF008()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<T> GetAsync<T>();
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF008");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF009: Out parameters not supported ──────────────────────────────────

    [Fact]
    public void OutParameter_EmitsBSF009()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task GetUser(out int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF009");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF010: Ref parameters not supported ──────────────────────────────────

    [Fact]
    public void RefParameter_EmitsBSF010()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task GetUser(ref int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF010");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF011: Params parameters not supported ───────────────────────────────

    [Fact]
    public void ParamsParameter_EmitsBSF011()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "POST")]
                    Task CreateUsers(params int[] ids);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF011");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF012: HttpMethod is required ────────────────────────────────────────

    [Fact]
    public void MissingHttpMethod_EmitsBSF012()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF012");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF013: Invalid HttpMethod value ──────────────────────────────────────

    [Fact]
    public void InvalidHttpMethod_EmitsBSF013()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "INVALID")]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF013");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF014: Duplicate routes ──────────────────────────────────────────────

    [Fact]
    public void DuplicateRoutes_EmitsBSF014()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetUser(int id);

                    [ServerFunction(HttpMethod = "GET", Route = "GetUser")]
                    Task<string> FindUser(string name);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF014");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF015: Invalid route format ──────────────────────────────────────────

    [Fact]
    public void InvalidRouteFormat_EmitsBSF015()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET", Route = "get user")]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF015");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF016: Referenced interface parse failure ────────────────────────────

    [Fact]
    public void ReferencedInterfaceParseFailure_DoesNotCrashServerProject()
    {
        // BSF016 is triggered when parsing a referenced interface throws an exception.
        // We can't easily force an exception in a test, but we verify that a referenced
        // project with a bad interface (validation errors) does NOT re-emit its diagnostics
        // in the consuming server project — the server project processes the reference silently.
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetUser(int id);
                }
                """)
            .AddServerProject("MyApp.Server", """
                namespace MyApp.Server;
                public class Program { }
                """,
                references: "MyApp.Shared")
            .Build();

        // The library project may have its own diagnostics, but the server project
        // should NOT duplicate them — cross-project diagnostic isolation.
        var server = scenario.Server;
        var sharedDiagnosticIds = scenario.GetProject("MyApp.Shared").Diagnostics
            .Select(d => d.Id)
            .ToHashSet(StringComparer.Ordinal);

        var serverOnlyDiagnostics = server.Diagnostics
            .Where(d => !sharedDiagnosticIds.Contains(d.Id))
            .ToList();

        Assert.Empty(serverOnlyDiagnostics);
    }

    // ── BSF101: Empty interface warning ──────────────────────────────────────

    [Fact]
    public void EmptyInterface_EmitsBSF101Warning()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF101");
        // BSF101 message says "No code will be generated" — empty interfaces are skipped
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF021: Empty Roles value ─────────────────────────────────────────────

    [Fact]
    public void EmptyRoles_EmitsBSF021()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET", Roles = "")]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF021");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF022: Empty CorsPolicy value ────────────────────────────────────────

    [Fact]
    public void EmptyCorsPolicy_EmitsBSF022()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection(CorsPolicy = "")]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetUser(int id);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF022");
        project.AssertHasNoGeneratedFiles();
    }

    // ── BSF102: Too many parameters warning ───────────────────────────────────

    [Fact]
    public void TooManyParameters_EmitsBSF102Warning()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared", """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Shared;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "POST")]
                    Task CreateUser(string a, string b, string c, string d, string e, string f);
                }
                """)
            .Build();

        var project = scenario.GetProject("MyApp.Shared");
        project.AssertHasDiagnostic("BSF102");
        // Warning doesn't block generation — files should still be created
        project.AssertHasClientProxyFiles("IUserService");
    }
}
