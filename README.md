# Wanderpool.Common

A comprehensive .NET infrastructure library providing production-ready middleware, telemetry, resilience patterns, and standardized API response handling.

## Features

- **Exception Handling** - Global exception middleware with consistent error responses
- **Request Logging** - Structured request/response logging with Serilog
- **Correlation ID** - Automatic correlation ID propagation for distributed tracing
- **Health Checks** - Liveness and readiness endpoints
- **HTTP Resilience** - Retry, circuit breaker, and timeout policies via Polly
- **OpenTelemetry** - Distributed tracing and metrics
- **Validation** - FluentValidation integration for endpoints
- **Result Pattern** - Consistent `OperationResult<T>` and `ApiResponseEnvelope<T>` types

## Quick Start

### 1. Install the Package

```bash
dotnet add package Wanderpool.Common.Infra
dotnet add package Wanderpool.Common.Contracts
```

### 2. Configure Services

```csharp
var builder = WebApplication.CreateBuilder(args);

// Add Wanderpool infrastructure
builder.Services.AddWanderpoolInfrastructure(options =>
{
    options.ServiceName = "MyService";
    options.ServiceVersion = "1.0.0";
    options.EnableTracing = true;
    options.EnableMetrics = true;
    options.EnableHealthChecks = true;
    options.EnableCorrelationId = true;
});

var app = builder.Build();

// Add Wanderpool middleware
app.UseWanderpoolInfrastructure();

app.Run();
```

### 3. Use the Result Pattern

**In Services (return `OperationResult<T>`):**

```csharp
public class UserService : IUserService
{
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
}
```

**In Endpoints (return `ApiResponseEnvelope<T>`):**

```csharp
app.MapGet("/api/users/{id}", async (int id, IUserService userService, HttpContext context) =>
{
    var result = await userService.GetUserByIdAsync(id);
    return result.ToResult(context);
});
```

## Configuration

### WanderpoolOptions

| Property | Type | Default | Description |
|----------|------|---------|-------------|
| `ServiceName` | string | Required | Service name for telemetry |
| `ServiceVersion` | string | Required | Service version |
| `EnableLogging` | bool | true | Enable structured logging |
| `EnableTracing` | bool | true | Enable OpenTelemetry tracing |
| `EnableMetrics` | bool | true | Enable OpenTelemetry metrics |
| `EnableHealthChecks` | bool | true | Enable health check endpoints |
| `EnableCorrelationId` | bool | true | Enable correlation ID middleware |
| `EnableExceptionHandling` | bool | true | Enable global exception handling |
| `EnableRequestLogging` | bool | true | Enable request/response logging |

### appsettings.json Example

```json
{
  "Wanderpool": {
    "ServiceName": "MyService",
    "ServiceVersion": "1.0.0"
  },
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317",
    "Enabled": true,
    "SamplingProbability": 1.0
  },
  "Resilience": {
    "Timeout": { "TimeoutSeconds": 10 },
    "Retry": { "MaxRetryAttempts": 3, "InitialDelayMilliseconds": 300 },
    "CircuitBreaker": { "BreakDurationSeconds": 20, "FailureRatio": 0.25 }
  },
  "Logging": {
    "MinimumLevel": "Information"
  },
  "CorrelationId": {
    "HeaderName": "X-Correlation-Id",
    "IncludeInResponseHeader": true
  }
}
```

## Feature Details

### Exception Handling

The global exception middleware catches all unhandled exceptions and returns consistent `ApiResponseEnvelope` responses:

| Exception Type | HTTP Status | Error Code |
|----------------|-------------|------------|
| `ValidationException` | 400 | VALIDATION_ERROR |
| `UnauthorizedAccessException` | 401 | UNAUTHORIZED |
| `ForbiddenException` | 403 | FORBIDDEN |
| `NotFoundException` | 404 | NOT_FOUND |
| `ConflictException` | 409 | CONFLICT |
| `OperationCanceledException` | 499 | CLIENT_CLOSED |
| `RemoteServiceException` | 502 | UPSTREAM_ERROR |
| Other exceptions | 500 | INTERNAL_ERROR |

### HTTP Client with Resilience

```csharp
services.AddWanderpoolHttpClient<IMyApiClient>(options =>
{
    options.BaseAddress = "https://api.example.com";
    options.ResiliencePipelineName = "default";
    options.TokenProviderType = typeof(MyTokenProvider); // Optional
});
```

### Health Checks

Endpoints are automatically mapped:
- `GET /health/live` - Liveness probe
- `GET /health/ready` - Readiness probe

### Validation Filter

```csharp
app.MapPost("/api/users", CreateUser)
    .WithValidation<CreateUserRequest>();

public class CreateUserRequestValidator : AbstractValidator<CreateUserRequest>
{
    public CreateUserRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.Name).NotEmpty().MaximumLength(100);
    }
}
```

## Testing

Use `WanderpoolTestServerBuilder` for integration tests:

```csharp
[Fact]
public async Task GetUser_ReturnsSuccess()
{
    using var server = new WanderpoolTestServerBuilder()
        .WithServiceName("TestService")
        .ConfigureServices(services =>
        {
            services.AddSingleton<IUserService>(new MockUserService());
        })
        .WithEndpoint("/api/users/1", async context =>
        {
            await context.Response.WriteAsJsonAsync(new { Id = 1, Name = "Test" });
        })
        .Build();

    var client = server.CreateClient();
    var response = await client.GetAsync("/api/users/1");

    Assert.True(response.IsSuccessStatusCode);
}
```

## API Response Format

All API responses use the `ApiResponseEnvelope<T>` format:

**Success Response:**
```json
{
  "isSuccess": true,
  "data": { "id": 1, "name": "John Doe" },
  "error": null,
  "traceId": "00-abc123-def456-01"
}
```

**Error Response:**
```json
{
  "isSuccess": false,
  "data": null,
  "error": {
    "code": "NOT_FOUND",
    "message": "User with ID 999 was not found",
    "level": "Warning"
  },
  "traceId": "00-abc123-def456-01"
}
```

## Troubleshooting

### Health checks return 503

Ensure all required dependencies are registered and healthy. Check the `/health/ready` endpoint for details.

### Correlation ID not propagating

Verify the correlation ID middleware is registered:
```csharp
app.UseWanderpoolCorrelationId();
```

### Traces not appearing in collector

1. Verify the OpenTelemetry endpoint is correct
2. Check `EnableTracing` is set to `true`
3. Ensure the collector is running and accessible

### Validation errors not returning 400

Ensure the validation filter is applied to the endpoint:
```csharp
.WithValidation<MyRequest>()
```

## License

MIT License - see LICENSE file for details.
