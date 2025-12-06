# Development Practices and Patterns

This document outlines best practices and patterns used in the Wanderpool.Common project.

## Configuration Management: The Options Pattern

### Problem with Hardcoding Configuration Values

Hardcoding configuration values directly in code creates several issues:

1. **Not environment-aware**: The same value is used across development, staging, and production
2. **Difficult to modify**: Requires code changes and recompilation to adjust settings
3. **Security risk**: Sensitive configuration (endpoints, credentials) in source control
4. **Testing challenges**: Hard to test different scenarios without multiple code paths
5. **Poor separation of concerns**: Configuration logic mixed with application logic

### Example of BAD Practice (Hardcoded Values)

```csharp
public static IServiceCollection AddMyService(this IServiceCollection services)
{
    services.AddHttpClient("MyClient")
        .ConfigureHttpClient(client =>
        {
            client.BaseAddress = new Uri("http://localhost:5000");  // ❌ Hardcoded
            client.Timeout = TimeSpan.FromSeconds(10);               // ❌ Hardcoded
        });

    return services;
}
```

### Example of GOOD Practice (Options Pattern)

#### Step 1: Create a Configuration Class

```csharp
namespace Wanderpool.Common.Infra.MyFeature;

/// <summary>
/// Configuration for MyFeature service.
/// Maps to appsettings.json "MyFeature" section.
/// </summary>
public class MyFeatureConfiguration
{
    /// <summary>
    /// Configuration section name in appsettings.json
    /// </summary>
    public const string Name = "MyFeature";

    /// <summary>
    /// Base address for the service endpoint.
    /// Example: "http://localhost:5000" (development), "https://api.prod.com" (production)
    /// </summary>
    public string BaseAddress { get; set; } = "http://localhost:5000";

    /// <summary>
    /// HTTP request timeout in seconds.
    /// </summary>
    public int TimeoutSeconds { get; set; } = 10;

    /// <summary>
    /// Whether the service is enabled.
    /// </summary>
    public bool Enabled { get; set; } = true;
}
```

#### Step 2: Create Extension Method Using IConfiguration

```csharp
public static class MyFeatureExtensions
{
    public static IServiceCollection AddWanderpoolMyFeature(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // Configure the options from appsettings.json
        services.Configure<MyFeatureConfiguration>(
            configuration.GetSection(MyFeatureConfiguration.Name));

        // Register your service
        services.AddHttpClient("MyFeature")
            .ConfigureHttpClient((provider, client) =>
            {
                var options = provider.GetRequiredService<IOptions<MyFeatureConfiguration>>().Value;

                if (options.Enabled)
                {
                    client.BaseAddress = new Uri(options.BaseAddress);
                    client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                }
            });

        return services;
    }
}
```

#### Step 3: Configure in appsettings.json

**appsettings.Development.json:**
```json
{
  "MyFeature": {
    "BaseAddress": "http://localhost:5000",
    "TimeoutSeconds": 30,
    "Enabled": true
  }
}
```

**appsettings.Production.json:**
```json
{
  "MyFeature": {
    "BaseAddress": "https://api.prod.com",
    "TimeoutSeconds": 10,
    "Enabled": true
  }
}
```

#### Step 4: Use in Your Service

```csharp
public class MyService
{
    private readonly IOptions<MyFeatureConfiguration> _options;

    public MyService(IOptions<MyFeatureConfiguration> options)
    {
        _options = options;
    }

    public void DoSomething()
    {
        var config = _options.Value;
        // Use config.BaseAddress, config.TimeoutSeconds, etc.
    }
}
```

### Key Benefits

✅ **Environment-specific**: Different configs for dev, staging, production
✅ **Secure**: Sensitive values in configuration files, not in code
✅ **Flexible**: Change configuration without recompilation
✅ **Testable**: Inject mock options for unit tests
✅ **Maintainable**: Clear separation between configuration and logic
✅ **Discoverable**: Central location for all configuration in appsettings.json

### Real Example: OpenTelemetry Tracing Configuration

See `TracingExtensions.cs` for a complete implementation example:

- **Configuration class**: `OpenTelemetryConfiguration.cs`
- **Extension methods**: `AddWanderpoolTracingWithConfiguration()` and `AddWanderpoolTracingWithExporters()`
- **Usage**: Configured from `appsettings.json` under "OpenTelemetry" section
- **Injection**: Services receive `IOptions<OpenTelemetryConfiguration>`

### Testing with the Options Pattern

```csharp
[Fact]
public void MyService_WhenConfigured_UsesDevelopmentSettings()
{
    // Arrange
    var options = Options.Create(new MyFeatureConfiguration
    {
        BaseAddress = "http://test:5000",
        TimeoutSeconds = 5
    });

    var service = new MyService(options);

    // Act & Assert
    service.DoSomething(); // Works with test configuration
}
```

## Guidelines

1. **Always use `IOptions<T>` for configuration** - Never hardcode environment-specific values
2. **Create a configuration class** - Even if small, it provides type safety and documentation
3. **Include a const string `Name`** - Use this for configuration section binding
4. **Provide sensible defaults** - Default values should be safe for development
5. **Document properties** - Explain what each setting does and expected values
6. **Test with mock options** - Use `Options.Create()` for unit tests

## Current Implementation Status

### Following Best Practices ✅
- `AddWanderpoolTracingWithConfiguration()` - Uses Options pattern
- `AddWanderpoolTracingWithExporters()` - Uses Options pattern with IConfiguration

### Candidates for Refactoring ⚠️
- `AddWanderpoolLogging()` - Multiple hardcoded log levels and templates
- `AddStandardResilience()` - Multiple hardcoded resilience parameters (timeouts, retries, etc.)
- `AddWanderpoolCorrelationId()` - Uses hardcoded string keys

---

For more information about ASP.NET Core Options Pattern:
- [Microsoft Docs: Options Pattern](https://docs.microsoft.com/en-us/dotnet/core/extensions/options)
- [Configuration in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration)
