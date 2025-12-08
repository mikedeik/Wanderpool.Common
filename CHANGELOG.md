# Changelog

All notable changes to the Wanderpool.Common library will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.0.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [1.0.0] - 2025-12-08

### Added

#### Wanderpool.Common.Contracts
- `OperationResult` and `OperationResult<T>` - Result pattern for service operations
- `OperationResultError` - Error details with code, message, and level
- `OperationResultErrorLevel` - Error severity levels (Info, Warning, Error)
- `ApiResponseEnvelope<T>` - Standardized API response wrapper
- `ApiResponseError` - API error details

#### Wanderpool.Common.Infra

**Exception Handling**
- `GlobalExceptionMiddleware` - Catches unhandled exceptions and returns consistent responses
- `WanderpoolException` - Base exception with error codes
- `ValidationException` - Validation errors with field-level details
- `NotFoundException` - Resource not found errors
- `ForbiddenException` - Access denied errors
- `ConflictException` - Resource conflict errors
- `BusinessRuleException` - Business rule violations

**Request Logging**
- `RequestLoggingMiddleware` - Structured request/response logging
- `RequestLoggingOptions` - Configurable logging options
- Configurable body logging with size limits
- Sensitive data redaction

**Correlation ID**
- `CorrelationIdMiddleware` - Automatic correlation ID propagation
- `CorrelationIdConfiguration` - Header name and format options
- `ICorrelationIdAccessor` - Scoped correlation ID access

**Health Checks**
- Liveness endpoint (`/health/live`)
- Readiness endpoint (`/health/ready`)
- `AddWanderpoolHealthChecks` extension method

**HTTP Resilience**
- `ResilienceOptions` - Timeout, retry, circuit breaker configuration
- `ConfigurableResiliencePipeline` - Named resilience pipelines
- Polly integration with configurable policies
- Token refresh handler support

**Telemetry**
- OpenTelemetry tracing integration
- OpenTelemetry metrics integration
- Configurable exporters (OTLP, Console, Jaeger, Zipkin)
- Custom HTTP client metrics

**Logging**
- Serilog integration
- Structured logging configuration
- Environment-aware log levels
- Log enrichment (machine name, process ID, thread ID)

**Validation**
- FluentValidation integration
- `WithValidation<T>` endpoint filter
- Automatic 400 responses for validation failures

**Unified Infrastructure**
- `AddWanderpoolInfrastructure` - Single method to register all services
- `UseWanderpoolInfrastructure` - Single method to configure middleware
- `WanderpoolOptions` - Feature flags for selective enablement
- Configuration validation

**Testing**
- `WanderpoolTestServerBuilder` - Fluent API for integration tests
- TestServer with full infrastructure support

### Configuration

All components support configuration via `IOptions<T>` pattern with sensible defaults.

Example `appsettings.json`:
```json
{
  "Wanderpool": {
    "ServiceName": "MyService",
    "ServiceVersion": "1.0.0"
  },
  "OpenTelemetry": {
    "Endpoint": "http://localhost:4317",
    "Enabled": true
  },
  "Resilience": {
    "Timeout": { "TimeoutSeconds": 10 },
    "Retry": { "MaxRetryAttempts": 3 }
  }
}
```

### Dependencies

- .NET 10.0
- FluentValidation 11.9.0
- Microsoft.Extensions.Http.Resilience 10.0.0
- OpenTelemetry 1.14.0
- Serilog.AspNetCore 8.0.0
