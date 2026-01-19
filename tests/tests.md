# BlazorServerFunctions Generator Test Suite Reference

This document outlines all the tests that should be implemented for the BlazorServerFunctions source generator.

---

## 1. Generator Contract Tests (Unit Tests)

These are your primary tests that validate the generator produces correct code for various scenarios.

### 1.1 Project Type Tests

```csharp
[Theory]
[InlineData(ProjectType.Server)]
[InlineData(ProjectType.Client)]
[InlineData(ProjectType.Library)]
public Task Generates_Correct_Files_For_ProjectType(ProjectType projectType)
{
    // Tests that each project type generates the appropriate files:
    // Server: 2 files (endpoints + endpoint registration)
    // Client: 2 files (proxies + client registration)
    // Library: 2 files (proxies + client registration)
}
```

### 1.2 Interface Scenarios

```csharp
[Fact]
public Task Generates_Single_Interface_With_Single_Method()
{
    // Basic scenario: one interface, one method
}

[Fact]
public Task Generates_Single_Interface_With_Multiple_Methods()
{
    // One interface with multiple methods
}

[Fact]
public Task Generates_Multiple_Interfaces_In_Same_File()
{
    // Multiple interfaces in the same source file
}

[Fact]
public Task Generates_Multiple_Interfaces_In_Different_Namespaces()
{
    // Interfaces in different namespaces
}

[Fact]
public Task Generates_Empty_Interface()
{
    // Interface with no methods (edge case)
}

[Fact]
public Task Generates_Interface_With_Custom_RoutePrefix()
{
    // [ServerFunctionCollection(RoutePrefix = "custom")]
}

[Fact]
public Task Generates_Interface_With_RequireAuthorization()
{
    // [ServerFunctionCollection(RequireAuthorization = true)]
}
```

### 1.3 HTTP Method Tests

```csharp
[Fact]
public Task Generates_POST_Method()
{
    // Default HTTP method
}

[Fact]
public Task Generates_GET_Method()
{
    // [ServerFunction(HttpMethod = "GET")]
}

[Fact]
public Task Generates_PUT_Method()
{
    // [ServerFunction(HttpMethod = "PUT")]
}

[Fact]
public Task Generates_DELETE_Method()
{
    // [ServerFunction(HttpMethod = "DELETE")]
}

[Fact]
public Task Generates_PATCH_Method()
{
    // [ServerFunction(HttpMethod = "PATCH")]
}

[Fact]
public Task Generates_Multiple_Methods_With_Different_HTTP_Verbs()
{
    // Mix of GET, POST, PUT, DELETE in same interface
}
```

### 1.4 Parameter Tests

```csharp
[Fact]
public Task Generates_Method_With_No_Parameters()
{
    // Task<string> GetData()
}

[Fact]
public Task Generates_Method_With_Single_Parameter()
{
    // Task<string> GetData(int id)
}

[Fact]
public Task Generates_Method_With_Multiple_Parameters()
{
    // Task<string> GetData(int id, string name, bool isActive)
}

[Fact]
public Task Generates_Method_With_Optional_Parameters()
{
    // Task<string> GetData(int id, string name = "default")
}

[Fact]
public Task Generates_Method_With_All_Optional_Parameters()
{
    // Task<string> GetData(int id = 0, string name = "default")
}

[Fact]
public Task Generates_Method_With_Complex_Type_Parameters()
{
    // Task<User> CreateUser(UserRequest request)
}

[Fact]
public Task Generates_Method_With_Nullable_Parameters()
{
    // Task<string> GetData(int? id, string? name)
}

[Fact]
public Task Generates_GET_Method_With_Parameters()
{
    // GET methods should use query strings
    // Task<string> GetData(int id, string name)
}
```

### 1.5 Return Type Tests

```csharp
[Fact]
public Task Generates_Method_With_Void_Return()
{
    // Task DoSomething()
}

[Fact]
public Task Generates_Method_With_Primitive_Return()
{
    // Task<int>, Task<string>, Task<bool>
}

[Fact]
public Task Generates_Method_With_Complex_Return()
{
    // Task<UserDto>
}

[Fact]
public Task Generates_Method_With_Collection_Return()
{
    // Task<List<User>>, Task<User[]>
}

[Fact]
public Task Generates_Method_With_Nullable_Return()
{
    // Task<User?>
}

[Fact]
public Task Generates_Async_Method()
{
    // Task<string> GetDataAsync()
}

[Fact]
public Task Generates_Non_Async_Method()
{
    // string GetData() - should this be supported?
}
```

### 1.6 Route Configuration Tests

```csharp
[Fact]
public Task Generates_Method_With_Custom_Route()
{
    // [ServerFunction(Route = "custom-endpoint")]
}

[Fact]
public Task Generates_Method_With_Default_Route()
{
    // Uses method name as route
}

[Fact]
public Task Generates_Methods_With_Mixed_Route_Configurations()
{
    // Some with custom routes, some without
}
```

### 1.7 Authorization Tests

```csharp
[Fact]
public Task Generates_Method_With_RequireAuthorization()
{
    // [ServerFunction(RequireAuthorization = true)]
}

[Fact]
public Task Generates_Interface_Level_Authorization()
{
    // [ServerFunctionCollection(RequireAuthorization = true)]
}

[Fact]
public Task Generates_Mixed_Authorization_Requirements()
{
    // Interface requires auth, some methods override
}
```

### 1.8 Edge Cases & Error Scenarios

```csharp
[Fact]
public Task Handles_Interface_Without_Attribute()
{
    // Should not generate anything
}

[Fact]
public Task Handles_Special_Characters_In_Method_Names()
{
    // Method names with underscores, etc.
}

[Fact]
public Task Handles_Reserved_Keywords_In_Parameter_Names()
{
    // Parameters named "string", "class", etc.
}

[Fact]
public Task Handles_Generic_Types_In_Parameters()
{
    // Task<T> GetData<T>() - if supported
}

[Fact]
public Task Handles_Very_Long_Method_Names()
{
    // Edge case for route generation
}

[Fact]
public Task Handles_Interface_In_Global_Namespace()
{
    // No namespace declared
}

[Fact]
public Task Handles_Nested_Interface()
{
    // Interface inside a class
}
```

### 1.9 Real-World Scenarios

```csharp
[Fact]
public Task Generates_CRUD_Interface()
{
    // Complete CRUD operations: Create, Read, Update, Delete
}

[Fact]
public Task Generates_Weather_Service_Interface()
{
    // Your sample Weather service
}

[Fact]
public Task Generates_User_Management_Interface()
{
    // Realistic user management service
}

[Fact]
public Task Generates_File_Upload_Interface()
{
    // POST with file/stream parameters
}
```

---

## 2. Integration Tests

These tests validate cross-project scenarios and compilation.

### 2.1 Multi-Project Tests

```csharp
[Fact]
public void Server_Project_Generates_Endpoints_From_Local_Interfaces()
{
    // Server project with local interface generates endpoints
}

[Fact]
public void Server_Project_Generates_Endpoints_From_Referenced_Shared_Library()
{
    // Server project references Shared library
    // Should generate endpoints for Shared interfaces
}

[Fact]
public void Client_Project_Generates_Proxies_From_Local_Interfaces()
{
    // Client project generates proxies for local interfaces
}

[Fact]
public void Client_Project_Uses_Proxies_From_Referenced_Shared_Library()
{
    // Client references Shared library
    // Should use proxies from Shared, not regenerate
}

[Fact]
public void Shared_Library_Generates_Only_Client_Proxies()
{
    // Library project should NOT generate server endpoints
}

[Fact]
public void Multiple_Shared_Libraries_Are_All_Discovered()
{
    // Server references Shared.Users + Shared.Products
    // Should generate endpoints for both
}

[Fact]
public void Transitive_Dependencies_Are_Discovered()
{
    // Server → Shared.A → Shared.B
    // Should discover interfaces in Shared.B
}

[Fact]
public void System_Assemblies_Are_Skipped()
{
    // Should not scan System.* or Microsoft.* assemblies
}
```

### 2.2 Compilation Tests

```csharp
[Fact]
public void Generated_Server_Code_Compiles_Without_Errors()
{
    // Take generated server code and try to compile it
    // Should have no compilation errors
}

[Fact]
public void Generated_Client_Code_Compiles_Without_Errors()
{
    // Take generated client code and try to compile it
    // Should have no compilation errors
}

[Fact]
public void Generated_Registration_Code_Compiles_Without_Errors()
{
    // Both server and client registration classes compile
}

[Fact]
public void Generated_Code_With_Complex_Types_Compiles()
{
    // Generated code using custom DTOs compiles
}

[Fact]
public void Multiple_Generated_Files_Compile_Together()
{
    // All generated files compile in the same project
}

[Fact]
public void Generated_Code_Has_No_Compiler_Warnings()
{
    // Check for warnings like unused variables, etc.
}
```

### 2.3 Namespace & Assembly Tests

```csharp
[Fact]
public void Interfaces_In_Different_Namespaces_Generate_Correctly()
{
    // Namespace.A.IService1 and Namespace.B.IService2
}

[Fact]
public void Generated_Code_Uses_Correct_Namespaces()
{
    // Generated code in same namespace as interface
}

[Fact]
public void Registration_Class_Uses_First_Interface_Namespace()
{
    // When multiple namespaces, uses first one
}

[Fact]
public void Referenced_Assembly_Names_Are_Filtered_Correctly()
{
    // Only scans user assemblies, not system assemblies
}
```

### 2.4 Incremental Generation Tests

```csharp
[Fact]
public void Adding_New_Interface_Generates_Additional_Files()
{
    // Add a new interface, should generate new files
}

[Fact]
public void Removing_Interface_Removes_Generated_Files()
{
    // Remove interface, should not generate files for it
}

[Fact]
public void Modifying_Interface_Regenerates_Files()
{
    // Change method signature, should regenerate
}

[Fact]
public void Unchanged_Interface_Does_Not_Trigger_Regeneration()
{
    // Incremental generator optimization
}
```

### 2.5 Dependency Tests

```csharp
[Fact]
public void Generated_Code_References_Required_Packages()
{
    // System.Net.Http.Json, Microsoft.AspNetCore.Mvc, etc.
}

[Fact]
public void Client_Code_Can_Be_Used_Without_Server_Dependencies()
{
    // Client proxies don't require ASP.NET Core
}

[Fact]
public void Server_Code_Requires_ASP_NET_Core_Dependencies()
{
    // Endpoints require proper packages
}
```

---

## 3. End-to-End Tests (Optional but Recommended)

These tests validate actual runtime behavior with real HTTP calls.

### 3.1 Basic Communication Tests

```csharp
[Fact]
public async Task Generated_Client_Can_Call_Generated_Server_Endpoint()
{
    // Basic POST/GET request works
}

[Fact]
public async Task Client_Sends_Request_Body_Correctly()
{
    // POST with body parameters
}

[Fact]
public async Task Client_Sends_Query_Parameters_Correctly()
{
    // GET with query string
}

[Fact]
public async Task Client_Receives_Response_Body_Correctly()
{
    // Response deserialization works
}

[Fact]
public async Task Server_Returns_Correct_Status_Codes()
{
    // 200 OK, 400 Bad Request, etc.
}
```

### 3.2 Serialization Tests

```csharp
[Fact]
public async Task Complex_Types_Serialize_Correctly()
{
    // DTOs with nested objects
}

[Fact]
public async Task Collections_Serialize_Correctly()
{
    // Arrays, Lists, etc.
}

[Fact]
public async Task Nullable_Types_Serialize_Correctly()
{
    // Nullable reference types
}

[Fact]
public async Task DateTime_Serializes_Correctly()
{
    // Dates, times, timezones
}

[Fact]
public async Task Enums_Serialize_Correctly()
{
    // Enum types
}
```

### 3.3 Error Handling Tests

```csharp
[Fact]
public async Task Client_Handles_404_Not_Found()
{
    // Endpoint doesn't exist
}

[Fact]
public async Task Client_Handles_500_Server_Error()
{
    // Server throws exception
}

[Fact]
public async Task Client_Handles_Network_Timeout()
{
    // Request times out
}

[Fact]
public async Task Client_Handles_Deserialization_Errors()
{
    // Invalid response format
}

[Fact]
public async Task Server_Returns_Validation_Errors()
{
    // Model validation fails
}
```

### 3.4 Authorization Tests

```csharp
[Fact]
public async Task Unauthorized_Request_Returns_401()
{
    // Endpoint requires auth, no token provided
}

[Fact]
public async Task Authorized_Request_Succeeds()
{
    // Valid token, request succeeds
}

[Fact]
public async Task Invalid_Token_Returns_401()
{
    // Expired or invalid token
}

[Fact]
public async Task Role_Based_Authorization_Works()
{
    // User doesn't have required role
}
```

### 3.5 HTTP Method Tests

```csharp
[Fact]
public async Task POST_Endpoint_Works()
{
    // POST request succeeds
}

[Fact]
public async Task GET_Endpoint_Works()
{
    // GET request succeeds
}

[Fact]
public async Task PUT_Endpoint_Works()
{
    // PUT request succeeds
}

[Fact]
public async Task DELETE_Endpoint_Works()
{
    // DELETE request succeeds
}

[Fact]
public async Task PATCH_Endpoint_Works()
{
    // PATCH request succeeds
}
```

### 3.6 Performance Tests

```csharp
[Fact]
public async Task Handles_Concurrent_Requests()
{
    // Multiple simultaneous requests
}

[Fact]
public async Task Handles_Large_Payloads()
{
    // Large request/response bodies
}

[Fact]
public async Task Response_Time_Is_Acceptable()
{
    // Performance benchmarking
}
```

---

## Test Organization

### Folder Structure

```
tests/
├── BlazorServerFunctions.Generator.Tests/
│   ├── GeneratorContractTests/
│   │   ├── ProjectTypeTests.cs
│   │   ├── InterfaceScenarioTests.cs
│   │   ├── HttpMethodTests.cs
│   │   ├── ParameterTests.cs
│   │   ├── ReturnTypeTests.cs
│   │   ├── RouteConfigurationTests.cs
│   │   ├── AuthorizationTests.cs
│   │   ├── EdgeCaseTests.cs
│   │   └── RealWorldScenarioTests.cs
│   │
│   ├── IntegrationTests/
│   │   ├── MultiProjectTests.cs
│   │   ├── CompilationTests.cs
│   │   ├── NamespaceAssemblyTests.cs
│   │   ├── IncrementalGenerationTests.cs
│   │   └── DependencyTests.cs
│   │
│   ├── Helpers/
│   │   ├── GeneratorTestHelper.cs
│   │   └── ProjectTypeTestData.cs
│   │
│   └── _snapshots/
│       └── [Verify snapshot files]
│
└── BlazorServerFunctions.EndToEnd.Tests/
    ├── BasicCommunicationTests.cs
    ├── SerializationTests.cs
    ├── ErrorHandlingTests.cs
    ├── AuthorizationTests.cs
    ├── HttpMethodTests.cs
    └── PerformanceTests.cs
```

---

## Test Naming Conventions

- **Contract Tests**: `Generates_[Scenario]_[ExpectedResult]`
- **Integration Tests**: `[Component]_[Action]_[ExpectedResult]`
- **E2E Tests**: `[Action]_[ExpectedBehavior]`

---

## Snapshot Testing Guidelines

When using Verify for snapshot testing:

1. Use `.UseParameters()` for theory tests
2. Use `.UseDirectory("Snapshots/[Category]")` to organize snapshots
3. Review snapshot diffs carefully in PR reviews
4. Commit snapshot files to version control
5. Use `[UsesVerify]` attribute on test classes

---

## Coverage Goals

- **Contract Tests**: 100% of generation scenarios
- **Integration Tests**: Key multi-project scenarios
- **E2E Tests**: Critical user workflows

Focus on contract tests first, then add integration and E2E tests as needed.