# Wanderpool.Common Infrastructure - User Stories

## Project Context
A common infrastructure library for a travel aggregation platform (Wanderpool) that will power multiple microservices wrapping external APIs (hotels, flights, car rentals, etc.). All microservices use ASP.NET Core Minimal APIs targeting .NET 10.

## Existing Components
- `Wanderpool.Common.Infra`: Infrastructure library with resilience pipelines, base HTTP client, token refresh handler, caching handler
- `Wanderpool.Common.Contracts`: Shared contracts with `OperationResult<T>`, `ApiResponseEnvelope<T>`, error types

---

## Epic 1: Structured Logging Infrastructure

### US-1.1: Serilog Integration with Structured Logging
**As a** microservice developer
**I want** a pre-configured Serilog setup with structured logging
**So that** all services have consistent, queryable logs

**Acceptance Criteria:**
- Create `ServiceCollectionExtensions.AddWanderpoolLogging()` extension method
- Configure Serilog with JSON formatting for production
- Configure console sink with readable format for development
- Include default enrichers: MachineName, ProcessId, ThreadId, Environment
- Support configuration via `appsettings.json` under `Serilog` section
- Auto-enrich logs with `ServiceName` from configuration

**Technical Notes:**
- Add packages: `Serilog.AspNetCore`, `Serilog.Sinks.Console`, `Serilog.Enrichers.Environment`, `Serilog.Enrichers.Process`, `Serilog.Enrichers.Thread`
- Location: `src/Wanderpool.Common.Infra/Logging/LoggingExtensions.cs`

---

### US-1.2: HTTP Request/Response Logging
**As a** microservice developer
**I want** automatic HTTP request and response logging
**So that** I can trace API calls for debugging

**Acceptance Criteria:**
- Create middleware that logs incoming requests (method, path, query, headers)
- Log response status code and duration
- Redact sensitive headers (Authorization, X-Api-Key, etc.)
- Configurable request/response body logging (disabled by default)
- Integrate with correlation ID from OpenTelemetry

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Logging/RequestLoggingMiddleware.cs`
- Create `app.UseWanderpoolRequestLogging()` extension

---

### US-1.3: Outbound HTTP Client Logging
**As a** microservice developer
**I want** automatic logging for all outbound HTTP calls
**So that** I can trace external API interactions

**Acceptance Criteria:**
- Create `DelegatingHandler` that logs outbound requests
- Log: URL, method, status code, duration, retry attempts
- Integrate with existing `BaseHttpClient`
- Redact sensitive data in URLs (tokens, keys in query strings)
- Log at appropriate levels (Info for success, Warning for retries, Error for failures)

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Clients/HttpClientHandlers/LoggingHandler.cs`
- Should work with existing resilience pipeline

---

## Epic 2: Global Exception Handling

### US-2.1: Global Exception Handler Middleware
**As a** microservice developer
**I want** a global exception handler
**So that** unhandled exceptions return consistent API responses

**Acceptance Criteria:**
- Create middleware that catches all unhandled exceptions
- Map exceptions to appropriate HTTP status codes
- Return `ApiResponseEnvelope<T>` format for all errors
- Include `TraceId` from OpenTelemetry in error responses
- Log exceptions with full stack trace at Error level
- Hide internal error details in production (show generic message)

**Exception Mapping:**
| Exception Type | HTTP Status | Error Code |
|----------------|-------------|------------|
| `ValidationException` | 400 | VALIDATION_ERROR |
| `UnauthorizedAccessException` | 401 | UNAUTHORIZED |
| `ForbiddenException` | 403 | FORBIDDEN |
| `NotFoundException` | 404 | NOT_FOUND |
| `RemoteServiceException` | 502 | UPSTREAM_ERROR |
| `OperationCanceledException` | 499 | CLIENT_CLOSED |
| `Exception` (default) | 500 | INTERNAL_ERROR |

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Exceptions/GlobalExceptionMiddleware.cs`
- Create `app.UseWanderpoolExceptionHandling()` extension

---

### US-2.2: Domain Exception Types
**As a** microservice developer
**I want** standard domain exception types
**So that** I can throw meaningful exceptions that map to HTTP responses

**Acceptance Criteria:**
- Create base `WanderpoolException` with error code and message
- Create `ValidationException` with collection of validation errors
- Create `NotFoundException` with resource type and identifier
- Create `ForbiddenException` for authorization failures
- Create `ConflictException` for concurrent modification conflicts
- Create `BusinessRuleException` for domain rule violations

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Exceptions/` folder
- All exceptions should be serializable

---

### US-2.3: Problem Details Support
**As a** microservice developer
**I want** RFC 7807 Problem Details support
**So that** errors follow industry standards

**Acceptance Criteria:**
- Optionally return errors as `ProblemDetails` instead of `ApiResponseEnvelope`
- Include `type` URI pointing to error documentation
- Include `instance` with request path
- Include `traceId` as extension property
- Configurable via `AddWanderpoolExceptionHandling(options => options.UseProblemDetails = true)`

**Technical Notes:**
- Use `Microsoft.AspNetCore.Mvc.ProblemDetails`
- Location: `src/Wanderpool.Common.Infra/Exceptions/ProblemDetailsMapper.cs`

---

## Epic 3: OpenTelemetry Integration

### US-3.1: OpenTelemetry Tracing Setup
**As a** microservice developer
**I want** distributed tracing configured out of the box
**So that** I can trace requests across services

**Acceptance Criteria:**
- Create `ServiceCollectionExtensions.AddWanderpoolTracing()` extension
- Configure ASP.NET Core instrumentation for incoming requests
- Configure HttpClient instrumentation for outgoing requests
- Support OTLP exporter configuration via `appsettings.json`
- Support Jaeger exporter for local development
- Auto-set service name and version from assembly info
- Propagate W3C trace context headers

**Technical Notes:**
- Add packages: `OpenTelemetry.Instrumentation.AspNetCore`, `OpenTelemetry.Instrumentation.Http`, `OpenTelemetry.Exporter.OpenTelemetryProtocol`
- Location: `src/Wanderpool.Common.Infra/Telemetry/TracingExtensions.cs`

---

### US-3.2: OpenTelemetry Metrics Setup
**As a** microservice developer
**I want** standardized metrics collection
**So that** I can monitor service health and performance

**Acceptance Criteria:**
- Create `ServiceCollectionExtensions.AddWanderpoolMetrics()` extension
- Configure ASP.NET Core metrics (request duration, count, etc.)
- Configure HttpClient metrics (outbound request stats)
- Configure runtime metrics (GC, thread pool, etc.)
- Support OTLP exporter configuration
- Support Prometheus exporter with `/metrics` endpoint

**Custom Metrics to Include:**
- `wanderpool_http_client_requests_total` (counter by client name, status)
- `wanderpool_http_client_request_duration_seconds` (histogram)
- `wanderpool_circuit_breaker_state` (gauge: 0=closed, 1=open, 2=half-open)

**Technical Notes:**
- Add packages: `OpenTelemetry.Instrumentation.Runtime`, `OpenTelemetry.Exporter.Prometheus.AspNetCore`
- Location: `src/Wanderpool.Common.Infra/Telemetry/MetricsExtensions.cs`

---

### US-3.3: Custom Activity Sources for Business Operations
**As a** microservice developer
**I want** easy creation of custom spans for business operations
**So that** I can trace domain-specific workflows

**Acceptance Criteria:**
- Create `IActivityScope` abstraction for creating spans
- Support adding tags/attributes to spans
- Support recording exceptions on spans
- Support nested spans with proper parent-child relationships
- Provide `[Traced]` attribute for automatic method tracing (optional)

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Telemetry/ActivityScope.cs`
- Use `System.Diagnostics.ActivitySource`

---

### US-3.4: Correlation ID Propagation
**As a** microservice developer
**I want** automatic correlation ID handling
**So that** I can trace requests across all logs and services

**Acceptance Criteria:**
- Extract `X-Correlation-Id` header from incoming requests (or generate new GUID)
- Add correlation ID to all log entries via Serilog enricher
- Propagate correlation ID to outbound HTTP requests
- Include correlation ID in all API responses
- Make correlation ID available via `ICorrelationContext` service

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Telemetry/CorrelationIdMiddleware.cs`
- Location: `src/Wanderpool.Common.Infra/Telemetry/ICorrelationContext.cs`

---

## Epic 4: Resilience Policies Enhancement

### US-4.1: Named Resilience Pipelines
**As a** microservice developer
**I want** pre-configured resilience pipelines for different scenarios
**So that** I can apply appropriate policies per external service

**Acceptance Criteria:**
- Enhance existing `ResiliencePipelines` with named configurations
- Create `AddAggressiveRetryPipeline()` - more retries, longer timeouts (for critical services)
- Create `AddFastFailPipeline()` - fewer retries, short timeouts (for non-critical services)
- Create `AddIdempotentPipeline()` - safe for retrying POST/PUT operations
- Create `AddReadOnlyPipeline()` - for GET-only endpoints with caching

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Policies/ResiliencePipelines.cs` (extend existing)
- All pipelines should emit metrics for circuit breaker state changes

---

### US-4.2: Configurable Resilience Options
**As a** microservice developer
**I want** to configure resilience settings via appsettings.json
**So that** I can tune policies without code changes

**Acceptance Criteria:**
- Create `ResilienceOptions` configuration class
- Support per-client configuration by name
- Configurable: MaxRetryAttempts, RetryDelay, TimeoutSeconds, CircuitBreakerThreshold
- Validate configuration on startup
- Provide sensible defaults

**Configuration Example:**
```json
{
  "Resilience": {
    "Default": {
      "MaxRetryAttempts": 3,
      "RetryDelayMs": 300,
      "TimeoutSeconds": 10,
      "CircuitBreaker": {
        "FailureRatio": 0.25,
        "MinimumThroughput": 20,
        "BreakDurationSeconds": 30
      }
    },
    "Clients": {
      "HotelApi": {
        "MaxRetryAttempts": 5,
        "TimeoutSeconds": 30
      }
    }
  }
}
```

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Policies/ResilienceOptions.cs`
- Use `IOptions<ResilienceOptions>` pattern

---

### US-4.3: Resilience Events Logging
**As a** microservice developer
**I want** automatic logging of resilience events
**So that** I can monitor retry and circuit breaker activity

**Acceptance Criteria:**
- Log when retry occurs (include attempt number, delay, exception)
- Log when circuit breaker opens (include failure stats)
- Log when circuit breaker transitions to half-open
- Log when circuit breaker closes
- Log when timeout occurs
- All logs should include client name and request URL

**Technical Notes:**
- Use `ResilienceContext` to pass logging context
- Integrate with existing Serilog setup

---

## Epic 5: Minimal API Infrastructure

### US-5.1: Standard Endpoint Response Helpers
**As a** microservice developer
**I want** helper methods for consistent API responses
**So that** all endpoints return standardized formats

**Acceptance Criteria:**
- Create `Results.ApiSuccess<T>(data)` returning `ApiResponseEnvelope<T>`
- Create `Results.ApiError(error)` returning `ApiResponseEnvelope`
- Create `Results.FromOperationResult<T>(result)` mapping `OperationResult<T>` to HTTP response
- Auto-include TraceId in all responses
- Support pagination wrapper: `Results.ApiPaged<T>(items, page, pageSize, total)`

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Api/ResultExtensions.cs`
- Should integrate with existing `ApiResponseEnvelope<T>` and `OperationResult<T>`

---

### US-5.2: Endpoint Filter for Validation
**As a** microservice developer
**I want** automatic request validation
**So that** invalid requests are rejected before hitting business logic

**Acceptance Criteria:**
- Create `AddValidation<TRequest>()` endpoint filter
- Integrate with FluentValidation validators
- Return 400 Bad Request with validation errors in `ApiResponseEnvelope` format
- Support automatic validator discovery via DI
- Log validation failures at Warning level

**Technical Notes:**
- Add package: `FluentValidation.DependencyInjectionExtensions`
- Location: `src/Wanderpool.Common.Infra/Api/ValidationFilter.cs`

---

### US-5.3: Endpoint Group Extensions
**As a** microservice developer
**I want** extension methods for common endpoint configurations
**So that** I can reduce boilerplate in route definitions

**Acceptance Criteria:**
- Create `group.WithWanderpoolDefaults()` applying standard configurations
- Include: OpenAPI metadata, authorization, rate limiting tags
- Create `endpoint.RequiresApiKey()` filter
- Create `endpoint.WithCaching(duration)` filter for response caching headers

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Api/EndpointExtensions.cs`

---

### US-5.4: Health Check Endpoints
**As a** microservice developer
**I want** standardized health check endpoints
**So that** orchestrators can monitor service health

**Acceptance Criteria:**
- Create `AddWanderpoolHealthChecks()` service extension
- Create `MapWanderpoolHealthChecks()` endpoint extension
- Configure `/health/live` for liveness probe (always returns 200 if running)
- Configure `/health/ready` for readiness probe (checks dependencies)
- Include checks for: HTTP clients (ping), database (if configured), Redis (if configured)
- Return detailed health info in JSON format for authenticated requests

**Technical Notes:**
- Add package: `AspNetCore.HealthChecks.Uris`
- Location: `src/Wanderpool.Common.Infra/HealthChecks/HealthCheckExtensions.cs`

---

## Epic 6: Service Registration

### US-6.1: Unified Service Registration
**As a** microservice developer
**I want** a single extension method to register all infrastructure
**So that** service setup is simple and consistent

**Acceptance Criteria:**
- Create `builder.AddWanderpoolInfrastructure(options)` extension
- Registers: Logging, Telemetry (tracing + metrics), Exception handling, Health checks
- Support options to enable/disable individual components
- Support configuration via `appsettings.json` under `Wanderpool` section
- Validate configuration and throw helpful errors on misconfiguration

**Configuration Example:**
```json
{
  "Wanderpool": {
    "ServiceName": "HotelService",
    "Logging": {
      "EnableRequestLogging": true,
      "EnableHttpClientLogging": true
    },
    "Telemetry": {
      "EnableTracing": true,
      "EnableMetrics": true,
      "OtlpEndpoint": "http://localhost:4317"
    },
    "ExceptionHandling": {
      "ShowDetailedErrors": false,
      "UseProblemDetails": false
    }
  }
}
```

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/WanderpoolInfrastructureExtensions.cs`

---

### US-6.2: Unified Middleware Pipeline
**As a** microservice developer
**I want** a single extension method to configure all middleware
**So that** the middleware order is correct and consistent

**Acceptance Criteria:**
- Create `app.UseWanderpoolInfrastructure()` extension
- Configures middleware in correct order:
  1. Correlation ID middleware
  2. Exception handling middleware
  3. Request logging middleware
  4. (other app middleware)
  5. Health check endpoints
- Support options to customize behavior

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/WanderpoolInfrastructureExtensions.cs`

---

### US-6.3: HTTP Client Factory Integration
**As a** microservice developer
**I want** easy registration of typed HTTP clients with all infrastructure
**So that** external API clients are properly configured

**Acceptance Criteria:**
- Create `AddWanderpoolHttpClient<TClient, TImplementation>(name, baseUrl)` extension
- Automatically applies: Resilience pipeline, Logging handler, Token refresh handler (if configured)
- Support configuration via `appsettings.json`
- Support custom handler pipeline per client

**Usage Example:**
```csharp
builder.Services.AddWanderpoolHttpClient<IHotelApiClient, HotelApiClient>(
    "HotelApi",
    options => {
        options.BaseAddress = "https://api.hotels.com";
        options.ResiliencePipeline = "aggressive";
        options.TokenProvider = typeof(HotelApiTokenProvider);
    });
```

**Technical Notes:**
- Location: `src/Wanderpool.Common.Infra/Clients/HttpClientExtensions.cs`
- Integrate with existing `BaseHttpClient`, handlers, and resilience pipelines

---

## Implementation Order Recommendation

**Phase 1 - Core Infrastructure:**
1. US-2.2: Domain Exception Types
2. US-2.1: Global Exception Handler Middleware
3. US-1.1: Serilog Integration
4. US-3.4: Correlation ID Propagation

**Phase 2 - Telemetry:**
5. US-3.1: OpenTelemetry Tracing Setup
6. US-3.2: OpenTelemetry Metrics Setup
7. US-3.3: Custom Activity Sources

**Phase 3 - Enhanced Logging:**
8. US-1.2: HTTP Request/Response Logging
9. US-1.3: Outbound HTTP Client Logging
10. US-4.3: Resilience Events Logging

**Phase 4 - Resilience Enhancement:**
11. US-4.2: Configurable Resilience Options
12. US-4.1: Named Resilience Pipelines

**Phase 5 - API Infrastructure:**
13. US-5.1: Standard Endpoint Response Helpers
14. US-5.2: Endpoint Filter for Validation
15. US-5.4: Health Check Endpoints
16. US-5.3: Endpoint Group Extensions

**Phase 6 - Integration:**
17. US-6.3: HTTP Client Factory Integration
18. US-6.1: Unified Service Registration
19. US-6.2: Unified Middleware Pipeline
20. US-2.3: Problem Details Support (optional)

---

## NuGet Packages Required

```xml
<!-- Logging -->
<PackageReference Include="Serilog.AspNetCore" Version="8.*" />
<PackageReference Include="Serilog.Sinks.Console" Version="5.*" />
<PackageReference Include="Serilog.Enrichers.Environment" Version="2.*" />
<PackageReference Include="Serilog.Enrichers.Process" Version="2.*" />
<PackageReference Include="Serilog.Enrichers.Thread" Version="3.*" />

<!-- OpenTelemetry -->
<PackageReference Include="OpenTelemetry.Instrumentation.AspNetCore" Version="1.*" />
<PackageReference Include="OpenTelemetry.Instrumentation.Http" Version="1.*" />
<PackageReference Include="OpenTelemetry.Instrumentation.Runtime" Version="1.*" />
<PackageReference Include="OpenTelemetry.Exporter.OpenTelemetryProtocol" Version="1.*" />
<PackageReference Include="OpenTelemetry.Exporter.Prometheus.AspNetCore" Version="1.*" />

<!-- Validation -->
<PackageReference Include="FluentValidation.DependencyInjectionExtensions" Version="11.*" />

<!-- Health Checks -->
<PackageReference Include="AspNetCore.HealthChecks.Uris" Version="8.*" />
```

---

## File Structure (Proposed)

```
src/Wanderpool.Common.Infra/
├── Api/
│   ├── EndpointExtensions.cs
│   ├── ResultExtensions.cs
│   └── ValidationFilter.cs
├── Clients/
│   ├── BaseHttpClient.cs (existing)
│   ├── HttpClientExtensions.cs
│   ├── Exceptions/
│   │   └── RemoteServiceException.cs (existing)
│   └── HttpClientHandlers/
│       ├── CachingHandler.cs (existing)
│       ├── LoggingHandler.cs
│       ├── TokenRefreshHandler.cs (existing)
│       └── ITokenProvider.cs (existing)
├── Exceptions/
│   ├── GlobalExceptionMiddleware.cs
│   ├── ProblemDetailsMapper.cs
│   ├── WanderpoolException.cs
│   ├── ValidationException.cs
│   ├── NotFoundException.cs
│   ├── ForbiddenException.cs
│   ├── ConflictException.cs
│   └── BusinessRuleException.cs
├── HealthChecks/
│   └── HealthCheckExtensions.cs
├── Logging/
│   ├── LoggingExtensions.cs
│   └── RequestLoggingMiddleware.cs
├── Policies/
│   ├── ResiliencePipelines.cs (existing - extend)
│   └── ResilienceOptions.cs
├── Telemetry/
│   ├── TracingExtensions.cs
│   ├── MetricsExtensions.cs
│   ├── ActivityScope.cs
│   ├── CorrelationIdMiddleware.cs
│   └── ICorrelationContext.cs
└── WanderpoolInfrastructureExtensions.cs
```
