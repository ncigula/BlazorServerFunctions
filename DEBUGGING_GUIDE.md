# Debugging the Source Generator

To debug the `ServerFunctionCollectionGenerator` line-by-line, you can use one of the following methods:

### 1. Using `Debugger.Launch()` (Recommended for quick checks)
Add the following line at the beginning of the `Initialize` or `Execute` method in `ServerFunctionCollectionGenerator.cs`:

```csharp
public void Initialize(IncrementalGeneratorInitializationContext context)
{
    if (!System.Diagnostics.Debugger.IsAttached)
    {
        System.Diagnostics.Debugger.Launch();
    }
    // ... rest of the code
}
```

When you build the project or run a test that uses the generator, a dialog will appear asking you to attach a debugger. Select your current IDE instance.

### 2. Debugging via Unit Tests (Best for development)
Since you now have unit tests in the `BlazorServerFunctions.UnitTests` project, you can simply:
1. Set a breakpoint inside the generator code (e.g., in the `Execute` method).
2. Right-click the test in the Test Explorer (Rider or Visual Studio).
3. Select **Debug**.
The debugger will stop at your breakpoint just like normal code.

### 3. Roslyn Component Debugging (Rider/Visual Studio)
You can also configure a "Roslyn Component" run configuration:
- **Visual Studio**: In the Generator project properties, go to the "Debug" tab and select "Roslyn Component".
- **Rider**: Create a new "Roslyn Component" run configuration and point it to the project that consumes the generator (e.g., the Sample project).

## Potential Issues Identified
While creating the tests, I noticed a few things that might be causing your exception or silence:
1. **Attribute Name Mismatch**: The generator looks for `ServerFunctionMethodAttribute` on methods, but the abstractions project only contains `ServerFunctionAttribute`.
2. **Boolean Parsing**: `bool.Parse` is used on build properties. If a property is present but contains an invalid value (e.g., empty string or "1"), it will throw a `FormatException`.
3. **Empty Strings**: In `ServerEndpointGenerator`, `httpMethod[0]` might throw an `IndexOutOfRangeException` if the HTTP method string is empty.
