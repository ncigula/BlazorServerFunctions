# BlazorServerFunctions Source Generator - Test Plan

## Overview

This document outlines the complete test strategy for the BlazorServerFunctions source generator. The test suite is divided into three projects: Unit Tests, Integration Tests, and End-to-End Tests.

## Project Structure

```
BlazorServerFunctions.Generator.Tests/
├── BlazorServerFunctions.Generator.UnitTests/
│   ├── ClientProxyGeneratorTests.cs
│   ├── ServerEndpointGeneratorTests.cs
│   ├── ClientRegistrationGeneratorTests.cs
│   ├── ServerRegistrationGeneratorTests.cs
│   ├── Helpers/
│   │   ├── InterfaceInfoBuilder.cs (✅ Already created)
│   │   ├── MethodInfoBuilder.cs (✅ Already created)
│   │   ├── ParameterInfoBuilder.cs (✅ Already created)
│   │   └── TestDataFactory.cs (Create this - see Test Data Builders section)
│   └── Snapshots/
│       └── *.verified.cs files
├── BlazorServerFunctions.Generator.IntegrationTests/
│   ├── SourceGeneratorTests.cs
│   ├── ProjectTypeDetectionTests.cs
│   ├── ReferencedAssemblyTests.cs
│   ├── Helpers/
│   │   └── TestDataFactory.cs (Can reuse from UnitTests)
│   └── Snapshots/
│       └── *.verified.cs files
└── BlazorServerFunctions.Generator.E2ETests/
    ├── TestShared/
    │   └── ITestService.cs
    ├── TestServer/
    │   ├── Program.cs
    │   └── Services/TestService.cs
    └── ServerFunctionE2ETests.cs
```

---

## 1. Unit Tests

**Purpose:** Test individual generator methods with pre-built `InterfaceInfo` objects  
**Use Verify:** ✅ YES - for snapshot testing of generated code  
**Focus:** String output correctness for each generator

### Required NuGet Packages
```xml
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
<PackageReference Include="Verify.Xunit" Version="latest" />
```

---

### 1.1 ClientProxyGeneratorTests.cs

**Test Data Strategy:**
- Use `TestDataFactory.BasicGetInterface()` for tests 1, 22, 23
- Use `TestDataFactory.CrudInterface()` for test 2
- Use `Builder` for tests 3-14, 21 (varying specific properties)
- Use `Object Initializers` for tests 15-20, 24 (edge cases with specific values)

#### Test Methods:

1. **`Task Generate_BasicInterface_ProducesCorrectCode()`**
    - Single method with one parameter
    - Async Task<T> return type
    - GET HTTP method

2. **`Task Generate_MultipleMethodsInterface_ProducesCorrectCode()`**
    - Interface with 3-5 methods
    - Mix of HTTP methods

3. **`Task Generate_MethodWithNoParameters_ProducesCorrectCode()`**
    - GET request without parameters
    - Should not create request object

4. **`Task Generate_MethodWithMultipleParameters_ProducesCorrectCode()`**
    - POST request with 3+ parameters
    - Should create request object with PascalCase properties

5. **`Task Generate_PostMethod_ProducesCorrectCode()`**
    - POST with parameters → PostAsJsonAsync
    - POST without parameters → PostAsync with null

6. **`Task Generate_GetMethod_ProducesCorrectCode()`**
    - GET with parameters → query string
    - GET without parameters → simple GetAsync

7. **`Task Generate_PutMethod_ProducesCorrectCode()`**
    - PUT request with HttpRequestMessage
    - JsonContent.Create for body

8. **`Task Generate_DeleteMethod_ProducesCorrectCode()`**
    - DELETE request with HttpRequestMessage

9. **`Task Generate_PatchMethod_ProducesCorrectCode()`**
    - PATCH request with HttpRequestMessage

10. **`Task Generate_VoidReturnType_ProducesCorrectCode()`**
    - No return value handling
    - No ReadFromJsonAsync call

11. **`Task Generate_TaskReturnType_ProducesCorrectCode()`**
    - Non-generic Task return
    - Should return result correctly

12. **`Task Generate_ComplexReturnType_ProducesCorrectCode()`**
    - List<T>, Dictionary<K,V>
    - Nested generic types

13. **`Task Generate_CustomRoute_UsesCustomRouteName()`**
    - CustomRoute property set
    - Uses custom route instead of method name

14. **`Task Generate_DefaultRoute_UsesMethodName()`**
    - CustomRoute is null
    - Uses method name as route

15. **`Task Generate_DefaultParameterValue_String_FormatsCorrectly()`**
    - String default value → `"value"`

16. **`Task Generate_DefaultParameterValue_Bool_FormatsCorrectly()`**
    - Bool default value → lowercase `true`/`false`

17. **`Task Generate_DefaultParameterValue_Null_FormatsCorrectly()`**
    - Nullable type with null default

18. **`Task Generate_DefaultParameterValue_Numeric_FormatsCorrectly()`**
    - Int, double, decimal defaults

19. **`Task Generate_NullableReferenceType_HandlesCorrectly()`**
    - string? parameters
    - Nullable handling in query strings

20. **`Task Generate_NullableValueType_HandlesCorrectly()`**
    - int?, bool? parameters

21. **`Task Generate_CorrectNamespace_IsUsed()`**
    - Namespace from InterfaceInfo

22. **`Task Generate_CorrectClassName_RemovesIPrefix()`**
    - IUserService → UserServiceClient

23. **`Task Generate_CorrectBaseRoute_UsesRoutePrefix()`**
    - BaseRoute = "/api/functions/{RoutePrefix}"

24. **`Task Generate_QueryStringParameters_UsePascalCase()`**
    - Parameter names converted to PascalCase in query string

---

### 1.2 ServerEndpointGeneratorTests.cs

**Test Data Strategy:**
- Use `TestDataFactory.BasicGetInterface()` for tests 1, 17, 18
- Use `TestDataFactory.CrudInterface()` for test 2
- Use `Builder` for tests 3-16, 19, 20 (varying HTTP methods and parameters)

#### Test Methods:

1. **`Task Generate_BasicInterface_ProducesCorrectCode()`**
    - Single method endpoint
    - MapGet/MapPost/etc

2. **`Task Generate_MultipleMethodsInterface_ProducesCorrectCode()`**
    - Multiple endpoints in same group

3. **`Task Generate_MethodWithNoParameters_ProducesCorrectCode()`**
    - No [FromBody] parameter
    - Only service parameter in lambda

4. **`Task Generate_MethodWithParameters_ProducesCorrectCode()`**
    - [FromBody] request parameter
    - Request DTO usage

5. **`Task Generate_GetEndpoint_ProducesCorrectCode()`**
    - MapGet method

6. **`Task Generate_PostEndpoint_ProducesCorrectCode()`**
    - MapPost method

7. **`Task Generate_PutEndpoint_ProducesCorrectCode()`**
    - MapPut method

8. **`Task Generate_DeleteEndpoint_ProducesCorrectCode()`**
    - MapDelete method

9. **`Task Generate_PatchEndpoint_ProducesCorrectCode()`**
    - MapPatch method

10. **`Task Generate_VoidReturnType_ReturnsOk()`**
    - Returns Results.Ok() without parameter

11. **`Task Generate_NonVoidReturnType_ReturnsOkWithResult()`**
    - Returns Results.Ok(result)

12. **`Task Generate_RequestDto_OnlyForMethodsWithParameters()`**
    - No DTO for parameterless methods
    - DTO generated for methods with parameters

13. **`Task Generate_RequestDto_UsesPascalCaseProperties()`**
    - Parameter names converted to PascalCase

14. **`Task Generate_ServiceCall_PassesParametersCorrectly()`**
    - request.PropertyName passed to service method

15. **`Task Generate_EndpointName_IsUnique()`**
    - WithName("{InterfaceName}_{MethodName}")

16. **`Task Generate_CustomRoute_UsesCustomRouteName()`**
    - Custom route in MapGet/MapPost

17. **`Task Generate_RouteGroup_UsesRoutePrefix()`**
    - MapGroup("/api/functions/{RoutePrefix}")

18. **`Task Generate_CorrectNamespace_IsUsed()`**
    - Namespace from InterfaceInfo

19. **`Task Generate_ExtensionClassName_IsCorrect()`**
    - {InterfaceName}ServerExtensions

20. **`Task Generate_MapEndpointsMethod_ReturnsIEndpointRouteBuilder()`**
    - Fluent API support

---

### 1.3 ClientRegistrationGeneratorTests.cs

**Test Data Strategy:**
- Use `TestDataFactory.BasicGetInterface()` for tests 2, 4-7
- Use `Builder` to create lists of interfaces for tests 3, 8, 9
- Use `empty list` for test 1

#### Test Methods:

1. **`Task Generate_EmptyInterfaceList_ReturnsEmptyString()`**
    - No interfaces → empty string

2. **`Task Generate_SingleInterface_ProducesCorrectCode()`**
    - One AddHttpClient call

3. **`Task Generate_MultipleInterfaces_ProducesCorrectCode()`**
    - Multiple AddHttpClient calls

4. **`Task Generate_CorrectInterfaceName_IsUsed()`**
    - AddHttpClient<IInterface, InterfaceClient>

5. **`Task Generate_CorrectClientClassName_IsUsed()`**
    - Removes 'I' prefix and adds 'Client' suffix

6. **`Task Generate_ReturnsIServiceCollection_ForFluentApi()`**
    - Method returns IServiceCollection

7. **`Task Generate_CorrectNamespace_IsUsed()`**
    - Uses namespace from first interface

8. **`Task Generate_StaticClassName_IsCorrect()`**
    - ServerFunctionClientsRegistration

9. **`Task Generate_MethodName_IsCorrect()`**
    - AddServerFunctionClients

---

### 1.4 ServerRegistrationGeneratorTests.cs

**Test Data Strategy:**
- Use `TestDataFactory.BasicGetInterface()` for tests 2, 4-6
- Use `Builder` to create lists of interfaces for tests 3, 7, 8
- Use `empty list` for test 1

#### Test Methods:

1. **`Task Generate_EmptyInterfaceList_ReturnsEmptyString()`**
    - No interfaces → empty string

2. **`Task Generate_SingleInterface_ProducesCorrectCode()`**
    - One Map{InterfaceName}Endpoints call

3. **`Task Generate_MultipleInterfaces_ProducesCorrectCode()`**
    - Multiple Map calls

4. **`Task Generate_CorrectInterfaceName_IsUsed()`**
    - endpoints.Map{InterfaceName}Endpoints()

5. **`Task Generate_ReturnsIEndpointRouteBuilder_ForFluentApi()`**
    - Method returns IEndpointRouteBuilder

6. **`Task Generate_CorrectNamespace_IsUsed()`**
    - Uses namespace from first interface

7. **`Task Generate_StaticClassName_IsCorrect()`**
    - ServerFunctionEndpointsRegistration

8. **`Task Generate_MethodName_IsCorrect()`**
    - MapServerFunctionEndpoints

---

## 2. Integration Tests

**Purpose:** Test full source generator pipeline using `CSharpGeneratorDriver`  
**Use Verify:** ✅ YES - for both generated code and diagnostics  
**Focus:** Code generation correctness and compilation success

**Test Data Strategy:**
- Can reuse `TestDataFactory` from Unit Tests for consistency
- Source code strings should be defined as constants or in a separate `TestSources.cs` file
- Use realistic interface definitions that reflect actual usage patterns

### Required NuGet Packages
```xml
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
<PackageReference Include="Verify.Xunit" Version="latest" />
<PackageReference Include="Microsoft.CodeAnalysis.CSharp" Version="4.8.0" />
<PackageReference Include="Microsoft.CodeAnalysis.CSharp.Workspaces" Version="4.8.0" />
```

---

### 2.1 SourceGeneratorTests.cs

#### Test Methods:

1. **`Task Generator_BasicInterface_GeneratesClientAndServer()`**
    - Simple interface with 1-2 methods
    - Verify all generated files

2. **`Task Generator_BasicInterface_CompilesWithoutErrors()`**
    - Verify no compilation errors
    - Verify no warnings

3. **`Task Generator_MultipleInterfaces_GeneratesAllFiles()`**
    - 2-3 interfaces in same namespace
    - Verify client + server for each

4. **`Task Generator_MultipleNamespaces_GeneratesCorrectly()`**
    - Interfaces in different namespaces
    - Verify namespace isolation

5. **`Task Generator_InterfaceWithoutAttribute_IsIgnored()`**
    - No [ServerFunctionCollection]
    - Should generate nothing

6. **`Task Generator_EmptyInterface_GeneratesCorrectly()`**
    - Interface with no methods
    - Should still generate structure

7. **`Task Generator_ComplexReturnTypes_GeneratesCorrectly()`**
    - List<T>, Dictionary<K,V>, tuples
    - Verify type handling

8. **`Task Generator_ComplexParameterTypes_GeneratesCorrectly()`**
    - Custom classes as parameters
    - Verify serialization

9. **`Task Generator_AllHttpMethods_GenerateCorrectly()`**
    - GET, POST, PUT, DELETE, PATCH
    - Verify each method type

10. **`Task Generator_CustomRoutes_AreUsedCorrectly()`**
    - Methods with custom route attributes
    - Verify route names

11. **`Task Generator_DefaultParameterValues_AreHandledCorrectly()`**
    - Methods with default parameters
    - Verify generation

12. **`Task Generator_NullableTypes_AreHandledCorrectly()`**
    - string?, int?, reference types
    - Verify nullable handling

13. **`Task Generator_GenericTypes_AreHandledCorrectly()`**
    - Generic methods or return types
    - Verify type parameters

14. **`Task Generator_VoidMethods_GenerateCorrectly()`**
    - Methods returning void or Task
    - Verify return handling

15. **`Task Generator_AsyncMethods_GenerateCorrectly()`**
    - Task<T> return types
    - Verify async/await

---

### 2.2 ProjectTypeDetectionTests.cs

#### Test Methods:

1. **`Task ServerProject_GeneratesEndpointsAndClients()`**
    - Has IEndpointRouteBuilder reference
    - Generates 4 files: Client, Server, ClientReg, ServerReg

2. **`Task ClientProject_GeneratesClientsOnly()`**
    - Has WebAssemblyHostBuilder reference
    - Generates 2 files: Client, ClientReg

3. **`Task LibraryProject_GeneratesClientsOnly()`**
    - No special references
    - Generates 2 files: Client, ClientReg

4. **`Task ServerProject_IncludesServerRegistration()`**
    - Verify ServerFunctionEndpointsRegistration exists

5. **`Task ClientProject_DoesNotIncludeServerRegistration()`**
    - Verify no server files generated

6. **`Task LibraryProject_DoesNotIncludeServerRegistration()`**
    - Verify no server files generated

---

### 2.3 ReferencedAssemblyTests.cs

#### Test Methods:

1. **`Task ReferencedInterface_GeneratesEndpoints()`**
    - Interface in referenced assembly
    - Server project generates endpoints for it

2. **`Task ReferencedInterface_DoesNotGenerateClients()`**
    - Client should not be generated for referenced interface

3. **`Task MultipleReferencedInterfaces_GenerateEndpoints()`**
    - Multiple interfaces from dependencies
    - All endpoints generated

4. **`Task SystemAssemblies_AreSkipped()`**
    - System.* assemblies ignored
    - Microsoft.* assemblies ignored

5. **`Task LocalAndReferencedInterfaces_BothWork()`**
    - Combination scenario
    - Verify no duplicates

6. **`Task ClientProject_DoesNotSearchReferencedAssemblies()`**
    - Client projects skip referenced assembly scan
    - Performance optimization

7. **`Task LibraryProject_DoesNotSearchReferencedAssemblies()`**
    - Library projects skip referenced assembly scan

---

### 2.4 CompilationTests.cs

#### Test Methods:

1. **`Task GeneratedClientCode_Compiles()`**
    - Create compilation with generated client
    - Assert no errors

2. **`Task GeneratedServerCode_Compiles()`**
    - Create compilation with generated server
    - Assert no errors

3. **`Task GeneratedRegistrationCode_Compiles()`**
    - Both registration classes compile

4. **`Task CompleteGeneration_ProducesValidCSharp()`**
    - All generated code together
    - Full compilation succeeds

5. **`Task GeneratedCode_HasNoWarnings()`**
    - Verify clean compilation
    - No nullable warnings, etc.

---

## 3. End-to-End Tests

**Purpose:** Test runtime behavior in actual Blazor applications  
**Use Verify:** ⚠️ Minimal - focus on runtime assertions  
**Focus:** HTTP calls, DI, actual functionality

### Required NuGet Packages
```xml
<PackageReference Include="xunit" Version="2.6.0" />
<PackageReference Include="xunit.runner.visualstudio" Version="2.5.0" />
<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="8.0.0" />
```

---

### Test Project Structure

**TestShared/ITestService.cs:**
```csharp
using BlazorServerFunctions.Abstractions;

namespace TestShared;

[ServerFunctionCollection]
public interface ITestService
{
    Task<string> GetMessageAsync();
    Task<User> GetUserAsync(int id);
    Task<User> CreateUserAsync(string name, string email);
    Task DeleteUserAsync(int id);
    Task<int> AddNumbersAsync(int a, int b);
    Task UpdateUserAsync(int id, string name);
}

public record User(int Id, string Name, string Email);
```

**TestServer/Program.cs:**
```csharp
var builder = WebApplication.CreateBuilder(args);
builder.Services.AddScoped<ITestService, TestService>();

var app = builder.Build();
app.MapServerFunctionEndpoints(); // Auto-generated

app.Run();
```

---

### 3.1 ServerFunctionE2ETests.cs

#### Test Methods:

1. **`async Task GetRequest_WithParameter_ReturnsCorrectResult()`**
    - Call GetUserAsync(1)
    - Assert user returned with correct data

2. **`async Task GetRequest_WithoutParameters_ReturnsResult()`**
    - Call GetMessageAsync()
    - Assert message returned

3. **`async Task GetRequest_WithMultipleParameters_BuildsQueryStringCorrectly()`**
    - Call AddNumbersAsync(5, 10)
    - Assert result is 15
    - Verify query string format

4. **`async Task PostRequest_WithMultipleParameters_CreatesResource()`**
    - Call CreateUserAsync("John", "john@test.com")
    - Assert user created correctly

5. **`async Task PostRequest_WithNoParameters_CallsEndpoint()`**
    - Method with no parameters
    - Verify POST with null body

6. **`async Task PutRequest_UpdatesResource()`**
    - Call UpdateUserAsync(1, "Updated Name")
    - Verify update works

7. **`async Task DeleteRequest_CallsEndpointSuccessfully()`**
    - Call DeleteUserAsync(1)
    - Should not throw

8. **`async Task VoidMethod_CompletesSuccessfully()`**
    - Call void method
    - Assert no exception

9. **`async Task ComplexReturnType_DeserializesCorrectly()`**
    - Method returning List<T> or Dictionary<K,V>
    - Verify deserialization

10. **`async Task NullableParameter_HandledCorrectly()`**
    - Pass null for nullable parameter
    - Verify server receives null

11. **`async Task ErrorResponse_ThrowsHttpRequestException()`**
    - Call with invalid parameter (e.g., -1)
    - Assert throws HttpRequestException

12. **`async Task ServerError_ThrowsHttpRequestException()`**
    - Trigger 500 error on server
    - Assert EnsureSuccessStatusCode throws

13. **`async Task CustomRoute_IsUsedCorrectly()`**
    - Method with custom route attribute
    - Verify correct endpoint called

14. **`async Task DefaultParameterValue_IsUsedWhenNotProvided()`**
    - Call method without optional parameter
    - Verify default value used

---

### 3.2 DependencyInjectionTests.cs

#### Test Methods:

1. **`void ClientRegistration_AddsHttpClientCorrectly()`**
    - Call AddServerFunctionClients()
    - Resolve ITestService
    - Assert type is ITestServiceClient

2. **`void ServerRegistration_MapsEndpointsCorrectly()`**
    - Call MapServerFunctionEndpoints()
    - Verify endpoints registered

3. **`void ClientService_CanBeResolvedFromDI()`**
    - Register services
    - Resolve ITestService
    - Assert not null

4. **`void ServerService_CanBeResolvedFromDI()`**
    - Register service implementation
    - Resolve from endpoint
    - Assert not null

5. **`void HttpClientFactory_IsUsedCorrectly()`**
    - Verify HttpClient is injected
    - Verify factory pattern

6. **`void MultipleInterfaces_AllRegisteredCorrectly()`**
    - Multiple service interfaces
    - All can be resolved

---

### 3.3 RouteTests.cs

#### Test Methods:

1. **`async Task DefaultRoute_UsesMethodName()`**
    - Verify /api/functions/{prefix}/{methodName}

2. **`async Task CustomRoute_IsRespected()`**
    - Method with custom route
    - Verify custom route used

3. **`async Task RoutePrefix_IsAppliedCorrectly()`**
    - Interface with specific route prefix
    - Verify all methods use prefix

4. **`async Task MultipleInterfaces_HaveSeparateRoutes()`**
    - Different interfaces
    - Verify no route conflicts

---

## Test Execution Order

### Phase 1: Foundation (Start Here)
1. Unit Tests - ClientProxyGeneratorTests (basic scenarios)
2. Unit Tests - ServerEndpointGeneratorTests (basic scenarios)
3. Integration Tests - Basic interface generation

### Phase 2: Core Functionality
4. Unit Tests - Complete all generator tests
5. Integration Tests - Project type detection
6. Integration Tests - Compilation tests
7. E2E Tests - Basic GET/POST requests

### Phase 3: Advanced Features
8. Integration Tests - Referenced assemblies
9. E2E Tests - All HTTP methods
10. E2E Tests - DI tests
11. All edge cases and error scenarios

---

## Verify Configuration

### ModuleInitializer.cs (for both Unit and Integration tests)

```csharp
using System.Runtime.CompilerServices;
using VerifyTests;

public static class ModuleInitializer
{
    [ModuleInitializer]
    public static void Init()
    {
        VerifySourceGenerators.Initialize();
        
        // Customize Verify settings
        VerifierSettings.DontScrubDateTimes();
        
        // Use directory for snapshots
        UseProjectRelativeDirectory("Snapshots");
    }
}
```

---

## Test Data Builders - Strategy & Usage

### Overview

Use a **hybrid approach**: combine fluent builders for reusable scenarios with object initializers for one-off edge cases.

### Project Structure

```
BlazorServerFunctions.Generator.UnitTests/
└── Helpers/
    ├── InterfaceInfoBuilder.cs    (✅ Already created)
    ├── MethodInfoBuilder.cs        (✅ Already created)
    ├── ParameterInfoBuilder.cs     (✅ Already created)
    └── TestDataFactory.cs          (⚠️ Create this)
```

---

### When to Use Builders vs Object Initializers

| Scenario | Use This | Why |
|----------|----------|-----|
| Common CRUD interface | **TestDataFactory** | Reusable across tests |
| Testing different route prefixes | **Builder** | Easy to vary one property |
| Testing default parameter edge case | **Object Initializer** | One-off, clear intent |
| Building 5 similar tests | **Builder** | DRY principle |
| Testing weird type combinations | **Object Initializer** | Not reusable |
| Setup for integration tests | **TestDataFactory** | Consistency |
| Complex nested structures | **Builder** | More readable |

---

### TestDataFactory.cs - Create This Helper

This is your central factory for common test scenarios:

```csharp
namespace BlazorServerFunctions.Generator.UnitTests.Helpers;

public static class TestDataFactory
{
    // ========================================
    // COMMON INTERFACES
    // ========================================
    
    public static InterfaceInfo BasicGetInterface() =>
        new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(Method.BasicGet())
            .Build();

    public static InterfaceInfo BasicPostInterface() =>
        new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethod(Method.BasicPost())
            .Build();

    public static InterfaceInfo CrudInterface() =>
        new InterfaceInfoBuilder()
            .WithName("IUserService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("users")
            .WithMethods(
                Method.GetList(),
                Method.GetById(),
                Method.Create(),
                Method.Update(),
                Method.Delete())
            .Build();

    public static InterfaceInfo EmptyInterface() =>
        new InterfaceInfoBuilder()
            .WithName("IEmptyService")
            .WithNamespace("MyApp.Services")
            .WithRoutePrefix("empty")
            .Build();

    // ========================================
    // COMMON METHODS
    // ========================================
    
    public static class Method
    {
        public static MethodInfo BasicGet() =>
            new MethodInfoBuilder()
                .WithName("GetUserAsync")
                .Returning("User")
                .UsingHttp("GET")
                .IsAsyncMethod()
                .WithParameter(Param.Id())
                .Build();

        public static MethodInfo GetList() =>
            new MethodInfoBuilder()
                .WithName("GetUsersAsync")
                .Returning("List<User>")
                .UsingHttp("GET")
                .IsAsyncMethod()
                .Build();

        public static MethodInfo GetById() =>
            new MethodInfoBuilder()
                .WithName("GetUserAsync")
                .Returning("User")
                .UsingHttp("GET")
                .IsAsyncMethod()
                .WithParameter(Param.Id())
                .Build();

        public static MethodInfo BasicPost() =>
            new MethodInfoBuilder()
                .WithName("CreateUserAsync")
                .Returning("User")
                .UsingHttp("POST")
                .IsAsyncMethod()
                .WithParameters(Param.Name(), Param.Email())
                .Build();

        public static MethodInfo Create() =>
            new MethodInfoBuilder()
                .WithName("CreateUserAsync")
                .Returning("User")
                .UsingHttp("POST")
                .IsAsyncMethod()
                .WithParameters(Param.Name(), Param.Email())
                .Build();

        public static MethodInfo Update() =>
            new MethodInfoBuilder()
                .WithName("UpdateUserAsync")
                .Returning("User")
                .UsingHttp("PUT")
                .IsAsyncMethod()
                .WithParameters(Param.Id(), Param.Name(), Param.Email())
                .Build();

        public static MethodInfo Delete() =>
            new MethodInfoBuilder()
                .WithName("DeleteUserAsync")
                .Returning("void")
                .UsingHttp("DELETE")
                .IsAsyncMethod()
                .WithParameter(Param.Id())
                .Build();

        public static MethodInfo VoidMethod() =>
            new MethodInfoBuilder()
                .WithName("DoSomethingAsync")
                .Returning("void")
                .UsingHttp("POST")
                .IsAsyncMethod()
                .Build();

        public static MethodInfo WithNoParameters() =>
            new MethodInfoBuilder()
                .WithName("GetMessageAsync")
                .Returning("string")
                .UsingHttp("GET")
                .IsAsyncMethod()
                .Build();
    }

    // ========================================
    // COMMON PARAMETERS
    // ========================================
    
    public static class Param
    {
        public static ParameterInfo Id() =>
            new ParameterInfoBuilder()
                .WithName("id")
                .WithType("int")
                .Build();

        public static ParameterInfo Name() =>
            new ParameterInfoBuilder()
                .WithName("name")
                .WithType("string")
                .Build();

        public static ParameterInfo Email() =>
            new ParameterInfoBuilder()
                .WithName("email")
                .WithType("string")
                .Build();

        public static ParameterInfo OptionalString() =>
            new ParameterInfoBuilder()
                .WithName("optional")
                .WithType("string?")
                .Build();

        public static ParameterInfo WithDefault(string name, string type, object defaultValue) =>
            new ParameterInfoBuilder()
                .WithName(name)
                .WithType(type)
                .WithDefault(defaultValue?.ToString() ?? "null")
                .Build();
    }
}
```

---

### Usage Examples in Tests

#### ✅ Example 1: Use TestDataFactory for Common Scenarios

```csharp
public class ClientProxyGeneratorTests
{
    [Fact]
    public Task Generate_BasicInterface_ProducesCorrectCode()
    {
        // Use factory - this is a common scenario
        var interfaceInfo = TestDataFactory.BasicGetInterface();
        
        var generated = ClientProxyGenerator.Generate(interfaceInfo);
        return Verify(generated);
    }

    [Fact]
    public Task Generate_CrudInterface_ProducesCorrectCode()
    {
        // Use factory - common CRUD pattern
        var interfaceInfo = TestDataFactory.CrudInterface();
        
        var generated = ClientProxyGenerator.Generate(interfaceInfo);
        return Verify(generated);
    }
}
```

#### ✅ Example 2: Use Builders for Variations

```csharp
[Fact]
public Task Generate_CustomRoutePrefix_UsesCorrectRoute()
{
    // Use builder to customize one property
    var interfaceInfo = new InterfaceInfoBuilder()
        .WithName("ICustomService")
        .WithNamespace("Custom.Namespace")
        .WithRoutePrefix("my-custom-prefix") // ← The variation
        .WithMethod(TestDataFactory.Method.GetList())
        .Build();
    
    var generated = ClientProxyGenerator.Generate(interfaceInfo);
    return Verify(generated);
}

[Fact]
public Task Generate_MultipleHttpMethods_ProducesCorrectCode()
{
    // Use builder to compose custom combinations
    var interfaceInfo = new InterfaceInfoBuilder()
        .WithName("ITestService")
        .WithNamespace("Test")
        .WithRoutePrefix("test")
        .WithMethods(
            TestDataFactory.Method.GetList(),
            TestDataFactory.Method.Create(),
            TestDataFactory.Method.Delete())
        .Build();
    
    var generated = ClientProxyGenerator.Generate(interfaceInfo);
    return Verify(generated);
}
```

#### ✅ Example 3: Use Object Initializers for Edge Cases

```csharp
[Fact]
public Task Generate_DefaultParameterValue_String_FormatsCorrectly()
{
    // One-off edge case - object initializer is clearer
    var interfaceInfo = new InterfaceInfo
    {
        Name = "ITestService",
        Namespace = "Test",
        RoutePrefix = "test",
        Methods = new List<MethodInfo>
        {
            new()
            {
                Name = "TestMethod",
                ReturnType = "string",
                HttpMethod = "GET",
                AsyncType = AsyncType.Task,
                Parameters = new List<ParameterInfo>
                {
                    new() 
                    { 
                        Name = "value", 
                        Type = "string", 
                        HasDefaultValue = true, 
                        DefaultValue = "\"hello\"" // ← Focus is here
                    }
                }
            }
        }
    };
    
    var generated = ClientProxyGenerator.Generate(interfaceInfo);
    return Verify(generated);
}

[Fact]
public Task Generate_ComplexReturnType_ProducesCorrectCode()
{
    // Edge case with weird types
    var interfaceInfo = new InterfaceInfo
    {
        Name = "IWeirdService",
        Namespace = "Edge.Cases",
        RoutePrefix = "weird",
        Methods = new List<MethodInfo>
        {
            new()
            {
                Name = "GetComplexDataAsync",
                ReturnType = "Dictionary<string, List<int?>>", // ← Complex type
                HttpMethod = "GET",
                AsyncType = AsyncType.Task,
                Parameters = new List<ParameterInfo>()
            }
        }
    };
    
    var generated = ClientProxyGenerator.Generate(interfaceInfo);
    return Verify(generated);
}
```

---

### Builder Enhancement - Add Static Factories (Optional)

You can optionally enhance your builders with static factory methods:

```csharp
// In MethodInfoBuilder.cs
internal sealed class MethodInfoBuilder
{
    // ... existing code ...
    
    // Static factory methods for common patterns
    public static MethodInfoBuilder Get(string name, string returnType) =>
        new MethodInfoBuilder()
            .WithName(name)
            .Returning(returnType)
            .UsingHttp("GET")
            .IsAsyncMethod();
    
    public static MethodInfoBuilder Post(string name, string returnType) =>
        new MethodInfoBuilder()
            .WithName(name)
            .Returning(returnType)
            .UsingHttp("POST")
            .IsAsyncMethod();
    
    public static MethodInfoBuilder Put(string name, string returnType) =>
        new MethodInfoBuilder()
            .WithName(name)
            .Returning(returnType)
            .UsingHttp("PUT")
            .IsAsyncMethod();
    
    public static MethodInfoBuilder Delete(string name) =>
        new MethodInfoBuilder()
            .WithName(name)
            .Returning("void")
            .UsingHttp("DELETE")
            .IsAsyncMethod();
}

// Usage becomes even cleaner:
var method = MethodInfoBuilder.Get("GetUserAsync", "User")
    .WithParameter(TestDataFactory.Param.Id())
    .Build();
```

---

### Decision Tree: Which Approach to Use?

```
Is the test data reusable across multiple tests?
├─ YES → Use TestDataFactory
│
└─ NO → Is it a variation of common data?
    ├─ YES → Use Builder with TestDataFactory helpers
    │
    └─ NO → Is it testing a specific edge case?
        ├─ YES → Use Object Initializer
        │
        └─ NO → Is the structure complex/nested?
            ├─ YES → Use Builder
            └─ NO → Use Object Initializer
```

---

### Summary: Where to Use Each Approach

**Use TestDataFactory when:**
- ✅ Testing basic/common scenarios (GET, POST, CRUD)
- ✅ Need consistency across multiple tests
- ✅ Setting up integration test data

**Use Builders when:**
- ✅ Need to customize one property (e.g., different route prefix)
- ✅ Composing methods from factory (mix and match)
- ✅ Building complex nested structures
- ✅ Creating variations of common patterns

**Use Object Initializers when:**
- ✅ One-off edge cases
- ✅ Testing specific property combinations
- ✅ Simple structures with few properties
- ✅ The focus is on a particular field/value

---

## Success Criteria

- ✅ All unit tests pass (100+ tests)
- ✅ All integration tests pass (30+ tests)
- ✅ All E2E tests pass (20+ tests)
- ✅ Code coverage > 90% for generator code
- ✅ All Verify snapshots are reviewed and approved
- ✅ Generated code compiles without warnings
- ✅ E2E tests run in < 30 seconds
- ✅ CI/CD pipeline runs all tests successfully

---

## CI/CD Considerations

1. **Unit Tests**: Run on every commit (fast)
2. **Integration Tests**: Run on every PR (medium)
3. **E2E Tests**: Run on every PR + before release (slower)

## Maintenance

- Review Verify snapshots on every generator change
- Update test data when adding new features
- Keep E2E test projects up to date with latest Blazor version
- Run full test suite before each release

---

## Total Test Count

- **Unit Tests**: ~100 tests
- **Integration Tests**: ~30 tests
- **E2E Tests**: ~20 tests
- **Total**: ~150 tests

This comprehensive test suite ensures the source generator is robust, maintainable, and catches issues early in development.