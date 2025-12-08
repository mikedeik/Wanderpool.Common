# Development Practices and Patterns
This document outlines best practices and patterns used in the Wanderpool.Common project.

## Commands and contants used
```gdscript
dotnet build ProjectName
```
```gdscript
dotnet test ProjectName
```

.NET 10 SDK is located in C:\Users\miked\.dotnet\sdk

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

#### OpenTelemetry Tracing (Telemetry/TracingExtensions.cs)
- **Configuration class**: `OpenTelemetryConfiguration` with const Name = "OpenTelemetry"
- **Extension methods**:
  - `AddWanderpoolTracingWithConfiguration()` - Uses IConfiguration pattern
  - `AddWanderpoolTracingWithExporters()` - Environment-aware with IConfiguration

#### HTTP Resilience (Policies/ResiliencePipelines.cs)
- **Configuration class**: `ResilienceConfiguration` with nested config classes for:
  - Timeout policies (per-request timeout)
  - Retry policies (exponential backoff + jitter)
  - Circuit breaker (failure ratio, sampling, minimum throughput)
  - Hedging strategies (improved P99 latency)
- **Extension methods**:
  - `AddStandardResilience()` - Original hardcoded version (for backward compatibility)
  - `AddStandardResilienceWithConfiguration()` - Uses IOptions pattern

#### Structured Logging (Logging/LoggingExtensions.cs)
- **Configuration class**: `LoggingConfiguration` with nested classes for:
  - Log level overrides (Microsoft.*,  Microsoft.Hosting.Lifetime)
  - Output templates (Development vs Production formatting)
  - Enrichment options (machine name, process ID, thread ID, environment name)
- **Extension methods**:
  - `AddWanderpoolLogging()` - Original hardcoded version (for backward compatibility)
  - `AddWanderpoolLoggingWithConfiguration()` - Uses IConfiguration pattern

#### Correlation ID Tracking (Telemetry/CorrelationIdExtensions.cs)
- **Configuration class**: `CorrelationIdConfiguration` with options for:
  - HTTP header name for reading correlation ID
  - HttpContext.Items key name for storing correlation ID
  - GUID format for generated correlation IDs
  - Response header inclusion
- **Extension methods**:
  - `AddWanderpoolCorrelationId()` - Original hardcoded version (for backward compatibility)
  - `AddWanderpoolCorrelationIdWithConfiguration()` - Uses IConfiguration pattern

### Implementation Pattern Summary

All new configuration-aware methods follow this pattern:

1. **Configuration Class** with:
   - `public const string Name` for appsettings.json section binding
   - Sensible defaults for each property
   - Full documentation of each setting

2. **Extension Methods** with:
   - Original method for backward compatibility (uses default configuration)
   - New `*WithConfiguration()` method accepting `IConfiguration`
   - Private `*Internal()` method with shared implementation

3. **Example appsettings.json**:
```json
{
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317",
    "Enabled": true,
    "SamplingProbability": 1.0
  },
  "Resilience": {
    "Timeout": { "TimeoutSeconds": 10 },
    "Retry": { "MaxRetryAttempts": 3, "InitialDelayMilliseconds": 300 },
    "CircuitBreaker": { "BreakDurationSeconds": 20, "FailureRatio": 0.25 },
    "Hedging": { "DelayMilliseconds": 200, "MaxHedgedAttempts": 2 }
  },
  "Logging": {
    "MinimumLevel": "Information",
    "LogLevelOverrides": {
      "Microsoft": "Warning",
      "MicrosoftHostingLifetime": "Information"
    }
  },
  "CorrelationId": {
    "HeaderName": "X-Correlation-Id",
    "ContextItemKey": "CorrelationId",
    "IncludeInResponseHeader": true,
    "GuidFormat": "D"
  }
}
```

---

## Return Type Patterns

### Overview

The Wanderpool project uses two distinct return types for different layers of the application:

| Layer | Return Type | Purpose |
|-------|-------------|---------|
| API Endpoints | `ApiResponseEnvelope<T>` | HTTP response wrapper with standardized format |
| Services, Handlers, Utilities | `OperationResult<T>` | Internal operation result with success/failure state |

### OperationResult<T> - For Internal Operations

Use `OperationResult<T>` for all internal operations including:
- Service methods
- Command handlers
- Query handlers
- Domain operations
- Utility methods

**Location:** `Wanderpool.Common.Contracts.Operations.OperationResult`

```csharp
// Definition
public record OperationResult(bool IsSuccess, OperationResultError? Error);
public record OperationResult<T>(T? Value, bool IsSuccess, OperationResultError? Error) : OperationResult(IsSuccess, Error);
```

#### Creating Success Results

```csharp
// Success without data
return OperationResult.Success();

// Success with data
return OperationResult.Success(user);

// Success with data and warning
return OperationResult.Success(user, new OperationResultError("DATA_STALE", "Data may be stale", OperationResultErrorLevel.Warning));

// Implicit conversion from value
public async Task<OperationResult<User>> GetUserAsync(int id)
{
    var user = await _repository.GetByIdAsync(id);
    return user; // Implicit conversion to OperationResult<User>
}
```

#### Creating Failure Results

```csharp
// Failure with error
return OperationResult.Fail(new OperationResultError("NOT_FOUND", "User not found"));

// Failure with typed result
return OperationResult.Fail<User>(new OperationResultError("VALIDATION_ERROR", "Invalid email format"));

// Implicit conversion from error
public async Task<OperationResult<User>> GetUserAsync(int id)
{
    if (id <= 0)
        return new OperationResultError("INVALID_ID", "ID must be positive"); // Implicit conversion

    // ...
}
```

#### Example Service Implementation

```csharp
public class UserService : IUserService
{
    private readonly IUserRepository _repository;

    public async Task<OperationResult<User>> GetUserByIdAsync(int userId)
    {
        if (userId <= 0)
            return OperationResult.Fail<User>(
                new OperationResultError("INVALID_ID", "User ID must be positive"));

        var user = await _repository.GetByIdAsync(userId);

        if (user == null)
            return OperationResult.Fail<User>(
                new OperationResultError("NOT_FOUND", $"User with ID {userId} was not found"));

        return OperationResult.Success(user);
    }

    public async Task<OperationResult<User>> CreateUserAsync(CreateUserCommand command)
    {
        // Validation
        if (string.IsNullOrWhiteSpace(command.Email))
            return OperationResult.Fail<User>(
                new OperationResultError("VALIDATION_ERROR", "Email is required"));

        // Check for duplicates
        var existing = await _repository.GetByEmailAsync(command.Email);
        if (existing != null)
            return OperationResult.Fail<User>(
                new OperationResultError("CONFLICT", "User with this email already exists"));

        // Create user
        var user = new User { Email = command.Email, Name = command.Name };
        await _repository.AddAsync(user);

        return OperationResult.Success(user);
    }
}
```

### ApiResponseEnvelope<T> - For API Endpoints

Use `ApiResponseEnvelope<T>` for all API endpoint responses. This provides a consistent format for HTTP clients.

**Location:** `Wanderpool.Common.Contracts.ApiResponse.ApiResponseEnvelope`

```csharp
// Definition
public class ApiResponseEnvelope<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public ApiResponseError? Error { get; set; }
    public string? TraceId { get; set; }
}
```

#### Converting OperationResult to ApiResponseEnvelope

Use the `ToResult()` extension method to convert `OperationResult<T>` to `IResult` in endpoints:

```csharp
app.MapGet("/api/users/{id}", async (int id, IUserService userService, HttpContext context) =>
{
    var result = await userService.GetUserByIdAsync(id);
    return result.ToResult(context);
});
```

#### Using ApiResponse Helper Methods

For direct responses without going through a service:

```csharp
// Success responses
app.MapGet("/api/health", (HttpContext context) =>
    ApiResponse.Ok("Healthy", context));

app.MapPost("/api/users", async (CreateUserRequest request, HttpContext context) =>
{
    // ... create user
    return ApiResponse.Created(user, $"/api/users/{user.Id}", context);
});

// Error responses
app.MapGet("/api/users/{id}", async (int id, HttpContext context) =>
{
    if (id <= 0)
        return ApiResponse.BadRequest("INVALID_ID", "ID must be positive", context);

    // ...
});
```

### Standard Error Codes

Use consistent error codes across the application:

| Error Code | HTTP Status | Description |
|------------|-------------|-------------|
| `VALIDATION_ERROR` | 400 | Request validation failed |
| `UNAUTHORIZED` | 401 | Authentication required |
| `FORBIDDEN` | 403 | Access denied |
| `NOT_FOUND` | 404 | Resource not found |
| `CONFLICT` | 409 | Resource conflict (duplicate, concurrent modification) |
| `BUSINESS_RULE_VIOLATION` | 422 | Business rule violated |
| `INTERNAL_ERROR` | 500 | Unexpected server error |
| `UPSTREAM_ERROR` | 502 | External service failure |

### Complete Endpoint Example

```csharp
public static class UserEndpoints
{
    public static void MapUserEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/users").WithTags("Users");

        group.MapGet("/{id}", GetUserById);
        group.MapPost("/", CreateUser).WithValidation<CreateUserRequest>();
        group.MapPut("/{id}", UpdateUser).WithValidation<UpdateUserRequest>();
        group.MapDelete("/{id}", DeleteUser);
    }

    private static async Task<IResult> GetUserById(
        int id,
        IUserService userService,
        HttpContext context)
    {
        var result = await userService.GetUserByIdAsync(id);
        return result.ToResult(context);
    }

    private static async Task<IResult> CreateUser(
        CreateUserRequest request,
        IUserService userService,
        HttpContext context)
    {
        var result = await userService.CreateUserAsync(new CreateUserCommand
        {
            Email = request.Email,
            Name = request.Name
        });

        if (!result.IsSuccess)
            return result.ToResult(context);

        return ApiResponse.Created(result.Value, $"/api/users/{result.Value!.Id}", context);
    }

    private static async Task<IResult> UpdateUser(
        int id,
        UpdateUserRequest request,
        IUserService userService,
        HttpContext context)
    {
        var result = await userService.UpdateUserAsync(id, new UpdateUserCommand
        {
            Name = request.Name
        });
        return result.ToResult(context);
    }

    private static async Task<IResult> DeleteUser(
        int id,
        IUserService userService,
        HttpContext context)
    {
        var result = await userService.DeleteUserAsync(id);
        return result.ToResult(context);
    }
}
```

### Why Not ProblemDetails?

While RFC 7807 ProblemDetails is an industry standard, we chose `ApiResponseEnvelope<T>` for consistency:

1. **Unified format**: Both success and error responses use the same wrapper
2. **Simpler client handling**: Clients always deserialize to the same type
3. **Richer metadata**: Includes `TraceId` and custom error levels
4. **Existing infrastructure**: Already integrated with `OperationResult<T>` throughout the codebase

---

For more information about ASP.NET Core Options Pattern:
- [Microsoft Docs: Options Pattern](https://docs.microsoft.com/en-us/dotnet/core/extensions/options)
- [Configuration in .NET](https://docs.microsoft.com/en-us/dotnet/core/extensions/configuration)
