using BlazorServerFunctions.Generator.IntegrationTests.Helpers;

namespace BlazorServerFunctions.Generator.IntegrationTests;

public class MultiProjectIntegrationTests
{
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
        shared.AssertHasClientFiles("IUserService");
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
        shared.AssertHasClientFiles("IUserService");

        scenario.Client.AssertHasClientFiles("IWeatherService");

        scenario.Server.AssertHasServerFiles("IUserService");
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
        shared.AssertHasClientFiles("IUserService");

        scenario.Client.AssertHasClientFiles("IWeatherService");

        scenario.Server.AssertHasServerFiles("IWeatherService", "IUserService");

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

        scenario.Server.AssertHasServerFiles("IUserService", "IProductService");
    }

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
        Assert.True(shared.HasGeneratedFile("ServerFunctionClientsRegistration.g.cs"),
            "Client registration should be in Shared project where interface is defined");

        Assert.True(scenario.Server.HasGeneratedFile("ISharedServiceServerExtensions.g.cs"),
            "Server endpoints should be in Server project");
        Assert.True(scenario.Server.HasGeneratedFile("ServerFunctionEndpointsRegistration.g.cs"),
            "Server registration should be in Server project");

        Assert.False(shared.HasGeneratedFile("ServerExtensions"),
            "Shared project should NOT have server endpoints");
    }

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
