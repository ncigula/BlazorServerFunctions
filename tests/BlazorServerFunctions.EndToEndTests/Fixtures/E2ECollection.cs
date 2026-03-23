namespace BlazorServerFunctions.EndToEndTests;

/// <summary>
/// xUnit collection that shares a single <see cref="E2EFixture"/> across all client-path
/// test classes. Without this, each class that uses <c>IClassFixture&lt;E2EFixture&gt;</c>
/// would spin up its own <see cref="Microsoft.AspNetCore.Mvc.Testing.WebApplicationFactory{TEntryPoint}"/>,
/// resulting in N full server startups for N test classes.
/// <para>
/// All classes in this collection run sequentially (xUnit default for collections),
/// which is fine because E2E tests are I/O-bound and the server startup cost dominates.
/// </para>
/// </summary>
[CollectionDefinition("E2E")]
public sealed class E2ECollection : ICollectionFixture<E2EFixture>;
