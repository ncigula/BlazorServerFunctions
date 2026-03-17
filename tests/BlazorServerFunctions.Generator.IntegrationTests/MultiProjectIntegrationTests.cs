using BlazorServerFunctions.Generator.IntegrationTests.Helpers;

namespace BlazorServerFunctions.Generator.IntegrationTests;

public class MultiProjectIntegrationTests
{
    // ── Scenario tests ────────────────────────────────────────────────────────

    [Fact]
    public void Scenario1_Server_References_Client()
    {
        var scenario = new ProjectBuilder()
            .AddClientProject(
                "MyApp.Client",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Client;

                [ServerFunctionCollection]
                public interface IWeatherService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetWeather();
                }
                """)
            .AddServerProject(
                "MyApp.Server",
                """
                namespace MyApp.Server;

                public class Program { }
                """,
                references: "MyApp.Client")
            .Build();

        scenario.Client.AssertNoDiagnostics();
        scenario.Client.AssertHasClientFiles("IWeatherService");
        scenario.Client.AssertCompilesSuccessfully();

        scenario.Server.AssertNoDiagnostics();
        scenario.Server.AssertHasServerFiles("IWeatherService");
        scenario.Server.AssertCompilesSuccessfully();
    }

    [Fact]
    public void Scenario2_Server_References_Client_Which_References_Shared()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject(
                "MyApp.Shared",
                """
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
            .AddClientProject(
                "MyApp.Client",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Client;

                [ServerFunctionCollection]
                public interface IWeatherService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetWeather();
                }
                """,
                references: "MyApp.Shared")
            .AddServerProject(
                "MyApp.Server",
                """
                namespace MyApp.Server;

                public class Program { }
                """,
                references: "MyApp.Client")
            .Build();

        var shared = scenario.GetProject("MyApp.Shared");
        shared.AssertNoDiagnostics();
        shared.AssertHasClientProxyFiles("IUserService");
        shared.AssertCompilesSuccessfully();

        scenario.Client.AssertNoDiagnostics();
        scenario.Client.AssertHasClientFiles("IWeatherService");
        scenario.Client.AssertCompilesSuccessfully();

        scenario.Server.AssertNoDiagnostics();
        scenario.Server.AssertHasServerFiles("IWeatherService", "IUserService");
        scenario.Server.AssertCompilesSuccessfully();
    }

    [Fact]
    public void Scenario3_Server_References_Shared_And_Client_Has_No_Refs()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject(
                "MyApp.Shared",
                """
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
            .AddClientProject(
                "MyApp.Client",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Client;

                [ServerFunctionCollection]
                public interface IWeatherService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetWeather();
                }
                """)
            .AddServerProject(
                "MyApp.Server",
                """
                namespace MyApp.Server;

                public class Program { }
                """,
                references: "MyApp.Shared")
            .Build();

        var shared = scenario.GetProject("MyApp.Shared");
        shared.AssertNoDiagnostics();
        shared.AssertHasClientProxyFiles("IUserService");
        shared.AssertCompilesSuccessfully();

        scenario.Client.AssertNoDiagnostics();
        scenario.Client.AssertHasClientFiles("IWeatherService");
        scenario.Client.AssertCompilesSuccessfully();

        scenario.Server.AssertNoDiagnostics();
        scenario.Server.AssertHasServerFiles("IUserService");
        scenario.Server.AssertCompilesSuccessfully();
        Assert.False(scenario.Server.HasGeneratedFile("IWeatherServiceServerExtensions"),
            "Server should not generate endpoints for unreferenced Client project");
    }

    [Fact]
    public void Scenario4_Server_References_Both_Client_And_Shared_Client_Also_References_Shared()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject("MyApp.Shared",
                """
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
            .AddClientProject("MyApp.Client",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Client;

                [ServerFunctionCollection]
                public interface IWeatherService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetWeather();
                }
                """,
                references: "MyApp.Shared")
            .AddServerProject("MyApp.Server",
                """
                namespace MyApp.Server;

                public class Program { }
                """,
                "MyApp.Client",
                "MyApp.Shared")
            .Build();

        var shared = scenario.GetProject("MyApp.Shared");
        shared.AssertNoDiagnostics();
        shared.AssertHasClientProxyFiles("IUserService");
        shared.AssertCompilesSuccessfully();

        scenario.Client.AssertNoDiagnostics();
        scenario.Client.AssertHasClientFiles("IWeatherService");
        scenario.Client.AssertCompilesSuccessfully();

        scenario.Server.AssertNoDiagnostics();
        scenario.Server.AssertHasServerFiles("IWeatherService", "IUserService");
        scenario.Server.AssertCompilesSuccessfully();

        var registrationFiles = scenario.Server.GeneratedFileNames
            .Where(f => f.Contains("ServerFunctionEndpointsRegistration"))
            .ToList();
        Assert.Single(registrationFiles);
    }

    [Fact]
    public void Multiple_Shared_Libraries_All_Discovered()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject(
                "MyApp.Shared.Users",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetUser(int id);
                }
                """)
            .AddSharedProject(
                "MyApp.Shared.Products",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                [ServerFunctionCollection]
                public interface IProductService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetProduct(int id);
                }
                """)
            .AddServerProject("MyApp.Server",
                """
                public class Program { }
                """,
                "MyApp.Shared.Users",
                "MyApp.Shared.Products")
            .Build();

        scenario.Server.AssertNoDiagnostics();
        scenario.Server.AssertHasServerFiles("IUserService", "IProductService");
        scenario.Server.AssertCompilesSuccessfully();
    }

    // ── File placement tests ──────────────────────────────────────────────────

    [Fact]
    public void Server_Files_Always_In_Server_Client_Files_In_Source_Project()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject(
                "MyApp.Shared",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                [ServerFunctionCollection]
                public interface ISharedService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetData();
                }
                """)
            .AddServerProject("MyApp.Server", "public class Program { }",
                references: "MyApp.Shared")
            .Build();

        var shared = scenario.GetProject("MyApp.Shared");

        Assert.True(shared.HasGeneratedFile("SharedServiceClient.g.cs"),
            "Client proxy should be in Shared project where interface is defined");
        Assert.False(shared.HasGeneratedFile("ServerFunctionClientsRegistration.g.cs"),
            "Client registration should NOT be in a source library — consuming Client/Server projects generate it");

        Assert.True(scenario.Server.HasGeneratedFile("ISharedServiceServerExtensions.g.cs"),
            "Server endpoints should be in Server project");
        Assert.True(scenario.Server.HasGeneratedFile("ServerFunctionEndpointsRegistration.g.cs"),
            "Server registration should be in Server project");

        Assert.False(shared.HasGeneratedFile("ServerExtensions"),
            "Shared project should NOT have server endpoints");

        shared.AssertNoDiagnostics();
        shared.AssertCompilesSuccessfully();
        scenario.Server.AssertNoDiagnostics();
        scenario.Server.AssertCompilesSuccessfully();
    }

    [Fact]
    public void ClientProxy_NotRegeneratedInConsumingProject()
    {
        // When Client references Shared, the client proxy for Shared's interface
        // must NOT be regenerated in Client — it already lives in Shared.
        // Regenerating causes CS0436 conflicts at compile time.
        var scenario = new ProjectBuilder()
            .AddSharedProject(
                "MyApp.Shared",
                """
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
            .AddClientProject(
                "MyApp.Client",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Client;

                [ServerFunctionCollection]
                public interface IWeatherService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetWeather();
                }
                """,
                references: "MyApp.Shared")
            .AddServerProject("MyApp.Server", "namespace MyApp.Server; public class Program { }",
                references: "MyApp.Client")
            .Build();

        var shared = scenario.GetProject("MyApp.Shared");

        Assert.True(shared.HasGeneratedFile("UserServiceClient.g.cs"),
            "Client proxy for IUserService must be in Shared (source project)");
        Assert.False(scenario.Client.HasGeneratedFile("UserServiceClient.g.cs"),
            "Client proxy for IUserService must NOT be regenerated in MyApp.Client");
        Assert.False(scenario.Server.HasGeneratedFile("UserServiceClient.g.cs"),
            "Client proxy for IUserService must NOT be regenerated in MyApp.Server");

        Assert.True(scenario.Client.HasGeneratedFile("WeatherServiceClient.g.cs"),
            "Client proxy for IWeatherService must be in Client (its source project)");

        scenario.Client.AssertNoDiagnostics();
        scenario.Client.AssertCompilesSuccessfully();
    }

    // ── Namespace content tests ───────────────────────────────────────────────

    [Fact]
    public void ClientProxy_IsInInterfaceNamespace()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject(
                "MyApp.Shared",
                """
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
            .AddServerProject("MyApp.Server", "namespace MyApp.Server; public class Program { }",
                references: "MyApp.Shared")
            .Build();

        var shared = scenario.GetProject("MyApp.Shared");

        // Proxy namespace must match the interface's namespace
        shared.AssertFileContains("UserServiceClient.g.cs", "namespace MyApp.Shared;");

        // Server project must not contain the client proxy at all
        Assert.False(scenario.Server.HasGeneratedFile("UserServiceClient.g.cs"),
            "Server project must not contain client proxy files");
    }

    [Fact]
    public void ServerExtensions_AreInServerProject_WithServerProjectNamespace()
    {
        // Server extension files are generated in the SERVER project (not in the shared project),
        // and use the server project's assembly name as their namespace.
        // The interface namespace is added as a using directive so referenced types are visible.
        var scenario = new ProjectBuilder()
            .AddSharedProject(
                "MyApp.Shared",
                """
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
            .AddServerProject("MyApp.Server", "namespace MyApp.Server; public class Program { }",
                references: "MyApp.Shared")
            .Build();

        var shared = scenario.GetProject("MyApp.Shared");

        // Server extensions are generated in the server project
        Assert.True(scenario.Server.HasGeneratedFile("IUserServiceServerExtensions.g.cs"),
            "Server extension file must be in the server project");
        Assert.False(shared.HasGeneratedFile("IUserServiceServerExtensions.g.cs"),
            "Server extension file must NOT be in the shared project");

        // Server extensions use the server project's assembly name as namespace
        scenario.Server.AssertFileContains("IUserServiceServerExtensions.g.cs", "namespace MyApp.Server;");
        // Interface namespace is added as a using so types are visible
        scenario.Server.AssertFileContains("IUserServiceServerExtensions.g.cs", "using MyApp.Shared;");
    }

    // ── Registration content tests ────────────────────────────────────────────

    [Fact]
    public void ServerRegistration_ContainsAllInterfaces_FromMultipleNamespaces()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject(
                "MyApp.Shared",
                """
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
            .AddClientProject(
                "MyApp.Client",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Client;

                [ServerFunctionCollection]
                public interface IWeatherService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetWeather();
                }
                """,
                references: "MyApp.Shared")
            .AddServerProject("MyApp.Server", "namespace MyApp.Server; public class Program { }",
                references: "MyApp.Client")
            .Build();

        var registration = scenario.Server.GetGeneratedFileContent("ServerFunctionEndpointsRegistration.g.cs");
        Assert.Contains("MapIUserServiceEndpoints", registration, StringComparison.Ordinal);
        Assert.Contains("MapIWeatherServiceEndpoints", registration, StringComparison.Ordinal);

        scenario.Server.AssertNoDiagnostics();
        scenario.Server.AssertCompilesSuccessfully();
    }

    [Fact]
    public void ClientRegistration_ContainsAllInterfaces_LocalAndReferenced()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject(
                "MyApp.Shared",
                """
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
            .AddClientProject(
                "MyApp.Client",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                namespace MyApp.Client;

                [ServerFunctionCollection]
                public interface IWeatherService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<string> GetWeather();
                }
                """,
                references: "MyApp.Shared")
            .AddServerProject("MyApp.Server", "namespace MyApp.Server; public class Program { }",
                references: "MyApp.Client")
            .Build();

        // The client project's registration must include BOTH its own interface
        // and the referenced one from Shared — it's the single DI registration point.
        var registration = scenario.Client.GetGeneratedFileContent("ServerFunctionClientsRegistration.g.cs");
        Assert.Contains("IWeatherService", registration, StringComparison.Ordinal);
        Assert.Contains("WeatherServiceClient", registration, StringComparison.Ordinal);
        Assert.Contains("IUserService", registration, StringComparison.Ordinal);
        Assert.Contains("UserServiceClient", registration, StringComparison.Ordinal);

        scenario.Client.AssertNoDiagnostics();
        scenario.Client.AssertCompilesSuccessfully();
    }

    // ── Compilation tests ─────────────────────────────────────────────────────

    [Fact]
    public void All_Generated_Code_Compiles_Successfully()
    {
        var scenario = new ProjectBuilder()
            .AddSharedProject(
                "MyApp.Shared",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                [ServerFunctionCollection]
                public interface IUserService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<User> GetUser(int id);

                    [ServerFunction(HttpMethod = "POST")]
                    Task CreateUser(string name);
                }

                public record User(int Id, string Name);
                """)
            .AddClientProject(
                "MyApp.Client",
                """
                using BlazorServerFunctions.Abstractions;
                using System.Threading.Tasks;

                [ServerFunctionCollection]
                public interface IWeatherService
                {
                    [ServerFunction(HttpMethod = "GET")]
                    Task<Weather[]> GetForecast();
                }

                public record Weather(string Date, int Temp);
                """,
                references: "MyApp.Shared")
            .AddServerProject("MyApp.Server", "public class Program { }",
                references: "MyApp.Client")
            .Build();

        var shared = scenario.GetProject("MyApp.Shared");
        shared.AssertNoDiagnostics();
        shared.AssertCompilesSuccessfully();

        scenario.Client.AssertNoDiagnostics();
        scenario.Client.AssertCompilesSuccessfully();

        scenario.Server.AssertNoDiagnostics();
        scenario.Server.AssertCompilesSuccessfully();
    }
}
