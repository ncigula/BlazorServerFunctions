using Microsoft.AspNetCore.Mvc.Testing;

namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// xUnit collection that shares a single <see cref="WebApplicationFactory{TEntryPoint}"/>
/// across all server-path test classes (those that call endpoints directly without going
/// through the generated client proxies).
/// </summary>
[CollectionDefinition("Server")]
public sealed class ServerCollection : ICollectionFixture<WebApplicationFactory<Program>>;
