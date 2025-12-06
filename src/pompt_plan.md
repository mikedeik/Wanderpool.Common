```markdown
# Wanderpool.Common Infrastructure - Implementation Plan

## Project Overview

Implementation plan for Wanderpool.Common infrastructure library following Test-Driven Development (TDD) principles. Each step is independently testable and deliverable.

---

## PHASE 1: CORE INFRASTRUCTURE

### STEP-001: Domain Exception Base Class

**Status:** PENDING  
**User Story:** US-2.2  
**Dependencies:** None  
**Estimated Effort:** 2 hours

#### Objective

Create the base `WanderpoolException` class with error code support and serialization.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Exceptions/WanderpoolExceptionTests.cs`

   - Write test: Constructor sets ErrorCode and Message correctly
   - Write test: Constructor throws ArgumentNullException for null errorCode
   - Write test: Exception is serializable (round-trip test)
   - Write test: Exception preserves inner exception
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Exceptions/WanderpoolException.cs`

   - Implement minimal code to pass all tests
   - Add `[Serializable]` attribute
   - Implement serialization constructors
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add XML documentation
   - Ensure consistent error message formatting
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >95% coverage
- [ ] Exception is properly serializable
- [ ] ErrorCode property is immutable and required

#### Deliverable

- `WanderpoolException.cs` implementation
- `WanderpoolExceptionTests.cs` test suite (min 5 tests)

---

### STEP-002: ValidationException Implementation

**Status:** PENDING  
**User Story:** US-2.2  
**Dependencies:** STEP-001  
**Estimated Effort:** 2 hours

#### Objective

Create `ValidationException` that stores structured validation errors.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Exceptions/ValidationExceptionTests.cs`

   - Write test: Constructor stores validation errors dictionary
   - Write test: Constructor throws ArgumentException for empty errors
   - Write test: ErrorCode defaults to "VALIDATION_ERROR"
   - Write test: Exception is serializable with errors preserved
   - Write test: Errors property is immutable
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Exceptions/ValidationException.cs`

   - Inherit from WanderpoolException
   - Store errors as `Dictionary<string, string[]>`
   - Implement serialization
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add helper method to format error messages
   - Ensure errors dictionary is defensive copy
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >95% coverage
- [ ] Validation errors properly stored and accessible
- [ ] Serialization preserves all error details

#### Deliverable

- `ValidationException.cs` implementation
- `ValidationExceptionTests.cs` test suite (min 5 tests)

---

### STEP-003: Remaining Domain Exceptions

**Status:** PENDING  
**User Story:** US-2.2  
**Dependencies:** STEP-001  
**Estimated Effort:** 3 hours

#### Objective

Create `NotFoundException`, `ForbiddenException`, `ConflictException`, and `BusinessRuleException`.

#### TDD Instructions

1. **RED**: Create test files for each exception type

   - `NotFoundExceptionTests.cs`: Test ResourceType and ResourceId properties
   - `ForbiddenExceptionTests.cs`: Test basic exception behavior
   - `ConflictExceptionTests.cs`: Test with resource identifier
   - `BusinessRuleExceptionTests.cs`: Test with rule name
   - Each should test: constructor, serialization, error code
   - Run tests → ALL FAIL

2. **GREEN**: Implement all four exception classes

   - Each inherits from WanderpoolException
   - Each has appropriate error code
   - `NotFoundException` includes ResourceType and ResourceId
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract common patterns
   - Improve error message formatting
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All 4 exception types implemented
- [ ] Each has >90% test coverage
- [ ] All exceptions properly serializable
- [ ] Error messages are clear and actionable

#### Deliverable

- 4 exception class implementations
- 4 test suite files (min 4 tests each)

---

### STEP-004: Global Exception Handler Middleware - Core

**Status:** PENDING  
**User Story:** US-2.1  
**Dependencies:** STEP-001, STEP-002, STEP-003  
**Estimated Effort:** 4 hours

#### Objective

Create middleware that catches exceptions and returns ApiResponseEnvelope format.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Exceptions/GlobalExceptionMiddlewareTests.cs`

   - Write test: ValidationException returns 400 with error details
   - Write test: NotFoundException returns 404
   - Write test: UnauthorizedAccessException returns 401
   - Write test: Generic Exception returns 500
   - Write test: Response includes TraceId
   - Write test: Exception is logged with full stack trace
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Exceptions/GlobalExceptionMiddleware.cs`

   - Implement RequestDelegate wrapper
   - Implement try-catch with exception mapping
   - Map exceptions to HTTP status codes
   - Return ApiResponseEnvelope format
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract exception mapping to separate method
   - Improve logging structure
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >90% coverage
- [ ] All exception types correctly mapped
- [ ] TraceId included in responses
- [ ] Exceptions logged with context

#### Deliverable

- `GlobalExceptionMiddleware.cs` implementation
- `GlobalExceptionMiddlewareTests.cs` (min 6 tests)

---

### STEP-005: Global Exception Handler - Production Mode

**Status:** PENDING  
**User Story:** US-2.1  
**Dependencies:** STEP-004  
**Estimated Effort:** 2 hours

#### Objective

Add production mode that hides internal error details.

#### TDD Instructions

1. **RED**: Extend `GlobalExceptionMiddlewareTests.cs`

   - Write test: Production mode returns generic message for 500 errors
   - Write test: Production mode still returns specific message for 4xx errors
   - Write test: Development mode shows full error details
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `GlobalExceptionMiddleware.cs`

   - Add isProduction constructor parameter
   - Conditionally hide internal details for 500 errors
   - Run tests → ALL PASS

3. **REFACTOR**
   - Make environment detection automatic via IHostEnvironment
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Production mode hides sensitive details
- [ ] 4xx errors still show specific messages
- [ ] Environment properly detected

#### Deliverable

- Updated `GlobalExceptionMiddleware.cs`
- 3 additional tests

---

### STEP-006: Exception Handler Extension Methods

**Status:** PENDING  
**User Story:** US-2.1  
**Dependencies:** STEP-005  
**Estimated Effort:** 1 hour

#### Objective

Create fluent extension methods for easy middleware registration.

#### TDD Instructions

1. **RED**: Create `ExceptionHandlingExtensionsTests.cs`

   - Write test: UseWanderpoolExceptionHandling registers middleware
   - Write test: Extension method is chainable
   - Run tests → ALL FAIL

2. **GREEN**: Create `ExceptionHandlingExtensions.cs`

   - Implement UseWanderpoolExceptionHandling extension
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add XML documentation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] Extension method works correctly
- [ ] Method is chainable
- [ ] Well documented

#### Deliverable

- `ExceptionHandlingExtensions.cs`
- Extension tests (min 2 tests)

---

### STEP-007: Serilog Configuration Infrastructure

**Status:** PENDING  
**User Story:** US-1.1  
**Dependencies:** None  
**Estimated Effort:** 3 hours

#### Objective

Create service collection extension for Serilog with structured logging.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Logging/LoggingExtensionsTests.cs`

   - Write test: AddWanderpoolLogging configures Serilog
   - Write test: Logger includes MachineName enricher
   - Write test: Logger includes ProcessId enricher
   - Write test: Logger includes ThreadId enricher
   - Write test: Logger includes ServiceName from config
   - Write test: Development uses console sink with readable format
   - Write test: Production uses JSON formatting
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Logging/LoggingExtensions.cs`

   - Implement AddWanderpoolLogging extension
   - Configure all required enrichers
   - Set up environment-specific sinks
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract sink configuration to separate methods
   - Add configuration validation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >85% coverage
- [ ] All enrichers properly configured
- [ ] Environment-specific formatting works
- [ ] ServiceName enricher applied

#### Deliverable

- `LoggingExtensions.cs` implementation
- `LoggingExtensionsTests.cs` (min 7 tests)
- Required NuGet packages added to csproj

---

### STEP-008: Correlation ID Context Service

**Status:** PENDING  
**User Story:** US-3.4  
**Dependencies:** None  
**Estimated Effort:** 2 hours

#### Objective

Create ICorrelationContext service for accessing correlation IDs throughout request lifecycle.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Telemetry/CorrelationContextTests.cs`

   - Write test: CorrelationContext stores correlation ID
   - Write test: CorrelationContext registered as scoped service
   - Write test: AddCorrelationContext extension registers service
   - Run tests → ALL FAIL

2. **GREEN**: Create implementations

   - Create `ICorrelationContext.cs` interface
   - Create `CorrelationContext.cs` implementation
   - Create registration extension
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add XML documentation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Service is scoped (not singleton)
- [ ] Interface is clean and simple

#### Deliverable

- `ICorrelationContext.cs` and implementation
- `CorrelationContextTests.cs` (min 3 tests)

---

### STEP-009: Correlation ID Middleware

**Status:** PENDING  
**User Story:** US-3.4  
**Dependencies:** STEP-008  
**Estimated Effort:** 3 hours

#### Objective

Create middleware that extracts/generates correlation IDs and makes them available.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Telemetry/CorrelationIdMiddlewareTests.cs`

   - Write test: Existing correlation ID is used
   - Write test: New correlation ID is generated if missing
   - Write test: Correlation ID added to response header
   - Write test: Correlation ID available via ICorrelationContext
   - Write test: Generated ID is valid GUID format
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Telemetry/CorrelationIdMiddleware.cs`

   - Extract or generate correlation ID
   - Set in response header
   - Populate ICorrelationContext
   - Run tests → ALL PASS

3. **REFACTOR**
   - Make header name configurable
   - Add validation for correlation ID format
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >90% coverage
- [ ] Correlation ID properly propagated
- [ ] ICorrelationContext properly populated
- [ ] Response header always set

#### Deliverable

- `CorrelationIdMiddleware.cs` implementation
- `CorrelationIdMiddlewareTests.cs` (min 5 tests)

---

### STEP-010: Correlation ID Serilog Enricher

**Status:** PENDING  
**User Story:** US-3.4  
**Dependencies:** STEP-007, STEP-009  
**Estimated Effort:** 2 hours

#### Objective

Create Serilog enricher that adds correlation ID to all log entries.

#### TDD Instructions

1. **RED**: Create `CorrelationIdEnricherTests.cs`

   - Write test: Enricher adds CorrelationId property to log event
   - Write test: Enricher handles null/empty correlation ID
   - Write test: Enricher integrates with ICorrelationContext
   - Run tests → ALL FAIL

2. **GREEN**: Create `CorrelationIdEnricher.cs`

   - Implement ILogEventEnricher
   - Read from ICorrelationContext
   - Add property to log event
   - Run tests → ALL PASS

3. **REFACTOR**
   - Optimize for performance
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Enricher properly integrated with Serilog
- [ ] No performance impact

#### Deliverable

- `CorrelationIdEnricher.cs` implementation
- `CorrelationIdEnricherTests.cs` (min 3 tests)
- Update LoggingExtensions to use enricher

---

## PHASE 2: TELEMETRY

### STEP-011: OpenTelemetry Tracing - Basic Setup

**Status:** PENDING  
**User Story:** US-3.1  
**Dependencies:** None  
**Estimated Effort:** 4 hours

#### Objective

Configure OpenTelemetry with ASP.NET Core and HttpClient instrumentation.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Telemetry/TracingExtensionsTests.cs`

   - Write test: AddWanderpoolTracing registers TracerProvider
   - Write test: ASP.NET Core instrumentation is configured
   - Write test: HttpClient instrumentation is configured
   - Write test: Service name is set from configuration
   - Write test: Service version is set from assembly
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Telemetry/TracingExtensions.cs`

   - Implement AddWanderpoolTracing extension
   - Configure ASP.NET Core instrumentation
   - Configure HttpClient instrumentation
   - Set resource attributes
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract resource builder configuration
   - Add configuration validation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >80% coverage
- [ ] TracerProvider registered correctly
- [ ] Both instrumentations active
- [ ] Service metadata properly set

#### Deliverable

- `TracingExtensions.cs` implementation
- `TracingExtensionsTests.cs` (min 5 tests)
- Required OpenTelemetry packages added

---

### STEP-012: OpenTelemetry Tracing - Exporters

**Status:** PENDING  
**User Story:** US-3.1  
**Dependencies:** STEP-011  
**Estimated Effort:** 3 hours

#### Objective

Add OTLP and Jaeger exporters with environment-based configuration.

#### TDD Instructions

1. **RED**: Extend `TracingExtensionsTests.cs`

   - Write test: OTLP exporter configured when endpoint provided
   - Write test: Jaeger exporter configured in Development
   - Write test: Console exporter configured in Development
   - Write test: No debug exporters in Production
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `TracingExtensions.cs`

   - Add OTLP exporter configuration
   - Add Jaeger exporter for dev
   - Add console exporter for dev
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract exporter configuration methods
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] OTLP endpoint configurable
- [ ] Environment-specific exporters work
- [ ] W3C trace context propagation verified

#### Deliverable

- Updated `TracingExtensions.cs`
- 4 additional tests
- Exporter packages added

---

### STEP-013: OpenTelemetry Tracing - Enrichment

**Status:** PENDING  
**User Story:** US-3.1  
**Dependencies:** STEP-012  
**Estimated Effort:** 2 hours

#### Objective

Add request/response enrichment to traces.

#### TDD Instructions

1. **RED**: Extend `TracingExtensionsTests.cs`

   - Write test: HTTP request enrichment adds client IP
   - Write test: HTTP response enrichment adds status code
   - Write test: Exceptions are recorded on spans
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `TracingExtensions.cs`

   - Configure ASP.NET Core enrichment callbacks
   - Configure HttpClient enrichment callbacks
   - Enable exception recording
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract enrichment logic to separate methods
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Traces include relevant metadata
- [ ] Exceptions captured on spans

#### Deliverable

- Updated `TracingExtensions.cs`
- 3 additional tests

---

### STEP-014: OpenTelemetry Metrics - Basic Setup

**Status:** PENDING  
**User Story:** US-3.2  
**Dependencies:** None  
**Estimated Effort:** 4 hours

#### Objective

Configure OpenTelemetry metrics with standard instrumentations.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Telemetry/MetricsExtensionsTests.cs`

   - Write test: AddWanderpoolMetrics registers MeterProvider
   - Write test: ASP.NET Core metrics configured
   - Write test: HttpClient metrics configured
   - Write test: Runtime metrics configured
   - Write test: Service name set correctly
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Telemetry/MetricsExtensions.cs`

   - Implement AddWanderpoolMetrics extension
   - Configure all standard instrumentations
   - Set resource attributes
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract configuration methods
   - Add validation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >80% coverage
- [ ] MeterProvider registered
- [ ] All instrumentations active
- [ ] Metrics are collectible

#### Deliverable

- `MetricsExtensions.cs` implementation
- `MetricsExtensionsTests.cs` (min 5 tests)
- Required metric packages added

---

### STEP-015: Custom HTTP Client Metrics

**Status:** PENDING  
**User Story:** US-3.2  
**Dependencies:** STEP-014  
**Estimated Effort:** 3 hours

#### Objective

Implement custom metrics for HTTP client requests.

#### TDD Instructions

1. **RED**: Create `HttpClientMetricsTests.cs`

   - Write test: wanderpool_http_client_requests_total counter increments
   - Write test: Counter includes client name tag
   - Write test: Counter includes status code tag
   - Write test: wanderpool_http_client_request_duration_seconds records duration
   - Write test: Histogram includes method tag
   - Run tests → ALL FAIL

2. **GREEN**: Create metric instruments in `MetricsExtensions.cs`

   - Create Counter for requests total
   - Create Histogram for request duration
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract metric definitions
   - Optimize tag allocation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Metrics properly tagged
- [ ] Low performance overhead

#### Deliverable

- Updated `MetricsExtensions.cs` with metrics
- `HttpClientMetricsTests.cs` (min 5 tests)

---

### STEP-016: HTTP Client Metrics Handler

**Status:** PENDING  
**User Story:** US-3.2  
**Dependencies:** STEP-015  
**Estimated Effort:** 3 hours

#### Objective

Create DelegatingHandler that records HTTP client metrics.

#### TDD Instructions

1. **RED**: Create `HttpClientMetricsHandlerTests.cs`

   - Write test: Handler records request count on success
   - Write test: Handler records request count on failure
   - Write test: Handler records duration accurately
   - Write test: Handler includes client name in tags
   - Write test: Handler handles exceptions without breaking
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Clients/HttpClientHandlers/HttpClientMetricsHandler.cs`

   - Implement DelegatingHandler
   - Record metrics on request completion
   - Handle exceptions
   - Run tests → ALL PASS

3. **REFACTOR**
   - Optimize tag creation
   - Ensure no metric leaks
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >90% coverage
- [ ] Metrics recorded accurately
- [ ] No performance degradation
- [ ] Exception handling correct

#### Deliverable

- `HttpClientMetricsHandler.cs` implementation
- `HttpClientMetricsHandlerTests.cs` (min 5 tests)

---

### STEP-017: Circuit Breaker State Metrics

**Status:** PENDING  
**User Story:** US-3.2  
**Dependencies:** STEP-014  
**Estimated Effort:** 4 hours

#### Objective

Implement observable gauge for circuit breaker state.

#### TDD Instructions

1. **RED**: Create `CircuitBreakerMetricsTests.cs`

   - Write test: Gauge reports 0 when circuit is closed
   - Write test: Gauge reports 1 when circuit is open
   - Write test: Gauge reports 2 when circuit is half-open
   - Write test: Gauge includes circuit name tag
   - Run tests → ALL FAIL

2. **GREEN**: Update `MetricsExtensions.cs`

   - Create ObservableGauge for circuit breaker state
   - Implement state collection logic
   - Integrate with Polly circuit breakers
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract state tracking
   - Optimize collection
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] State accurately reported
- [ ] Works with existing ResiliencePipelines
- [ ] Tagged by circuit name

#### Deliverable

- Updated `MetricsExtensions.cs`
- Circuit breaker state tracking
- `CircuitBreakerMetricsTests.cs` (min 4 tests)

---

### STEP-018: Prometheus Exporter Endpoint

**Status:** PENDING  
**User Story:** US-3.2  
**Dependencies:** STEP-014  
**Estimated Effort:** 2 hours

#### Objective

Configure Prometheus exporter and expose /metrics endpoint.

#### TDD Instructions

1. **RED**: Create `PrometheusEndpointTests.cs`

   - Write test: MapWanderpoolMetrics exposes endpoint
   - Write test: /metrics returns prometheus format
   - Write test: Endpoint includes custom metrics
   - Run tests → ALL FAIL

2. **GREEN**: Update `MetricsExtensions.cs`

   - Add Prometheus exporter
   - Create MapWanderpoolMetrics extension
   - Run tests → ALL PASS

3. **REFACTOR**
   - Configure endpoint path if needed
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] /metrics endpoint accessible
- [ ] Prometheus format correct
- [ ] All metrics exported

#### Deliverable

- Updated `MetricsExtensions.cs`
- Endpoint mapping extension
- `PrometheusEndpointTests.cs` (min 3 tests)

---

### STEP-019: Custom Activity Sources Infrastructure

**Status:** PENDING  
**User Story:** US-3.3  
**Dependencies:** STEP-011  
**Estimated Effort:** 3 hours

#### Objective

Create IActivityScope abstraction for custom business operation tracing.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Telemetry/ActivityScopeTests.cs`

   - Write test: CreateScope creates new activity
   - Write test: AddTag adds tag to current activity
   - Write test: RecordException records exception on span
   - Write test: Nested scopes create parent-child relationship
   - Write test: Dispose completes activity
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Telemetry/IActivityScope.cs` and implementation

   - Define IActivityScope interface
   - Implement ActivityScope class
   - Use System.Diagnostics.ActivitySource
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add fluent API for tags
   - Optimize activity creation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >85% coverage
- [ ] Interface is intuitive
- [ ] Nested spans work correctly
- [ ] Exception recording works

#### Deliverable

- `IActivityScope.cs` interface
- `ActivityScope.cs` implementation
- `ActivityScopeTests.cs` (min 5 tests)

---

### STEP-020: Activity Source Registration

**Status:** PENDING  
**User Story:** US-3.3  
**Dependencies:** STEP-019  
**Estimated Effort:** 2 hours

#### Objective

Register custom ActivitySource with OpenTelemetry.

#### TDD Instructions

1. **RED**: Extend `TracingExtensionsTests.cs`

   - Write test: Custom activity source is registered
   - Write test: Activities from custom source are traced
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `TracingExtensions.cs`

   - Register "Wanderpool.Common" activity source
   - Add to OpenTelemetry configuration
   - Run tests → ALL PASS

3. **REFACTOR**
   - Make source name configurable
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] Tests pass
- [ ] Custom activities are traced
- [ ] Works with distributed tracing

#### Deliverable

- Updated `TracingExtensions.cs`
- 2 additional tests

---

## PHASE 3: ENHANCED LOGGING

### STEP-021: Request Logging Options

**Status:** PENDING  
**User Story:** US-1.2  
**Dependencies:** None  
**Estimated Effort:** 1 hour

#### Objective

Create configuration class for request logging behavior.

#### TDD Instructions

1. **RED**: Create `RequestLoggingOptionsTests.cs`

   - Write test: Default options have body logging disabled
   - Write test: Sensitive headers list is populated
   - Write test: Options are mutable
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Logging/RequestLoggingOptions.cs`

   - Define configuration properties
   - Set sensible defaults
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add XML documentation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Defaults are secure (no body logging)
- [ ] Well documented

#### Deliverable

- `RequestLoggingOptions.cs`
- `RequestLoggingOptionsTests.cs` (min 3 tests)

---

### STEP-022: Request Logging Middleware - Core

**Status:** PENDING  
**User Story:** US-1.2  
**Dependencies:** STEP-007, STEP-009, STEP-021  
**Estimated Effort:** 4 hours

#### Objective

Create middleware that logs incoming HTTP requests and responses.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Logging/RequestLoggingMiddlewareTests.cs`

   - Write test: Logs request method and path
   - Write test: Logs response status and duration
   - Write test: Includes correlation ID in logs
   - Write test: Uses appropriate log levels (200→Info, 400→Warning, 500→Error)
   - Write test: Logs query string parameters
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Logging/RequestLoggingMiddleware.cs`

   - Implement request logging
   - Implement response logging with timing
   - Set log levels based on status
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract log message formatting
   - Optimize stream handling
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >85% coverage
- [ ] Request/response details logged
- [ ] Correlation ID included
- [ ] Performance impact minimal

#### Deliverable

- `RequestLoggingMiddleware.cs` implementation
- `RequestLoggingMiddlewareTests.cs` (min 5 tests)

---

### STEP-023: Request Logging - Header Redaction

**Status:** PENDING  
**User Story:** US-1.2  
**Dependencies:** STEP-022  
**Estimated Effort:** 2 hours

#### Objective

Implement sensitive header redaction in request logs.

#### TDD Instructions

1. **RED**: Extend `RequestLoggingMiddlewareTests.cs`

   - Write test: Authorization header is redacted
   - Write test: X-Api-Key header is redacted
   - Write test: Cookie header is redacted
   - Write test: Non-sensitive headers are logged
   - Write test: Redaction list is configurable
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `RequestLoggingMiddleware.cs`

   - Implement header redaction logic
   - Use SensitiveHeaders list from options
   - Run tests → ALL PASS

3. **REFACTOR**
   - Optimize header checking (case-insensitive)
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Sensitive headers properly redacted
- [ ] Configurable redaction list
- [ ] Case-insensitive matching

#### Deliverable

- Updated `RequestLoggingMiddleware.cs`
- 5 additional tests

---

### STEP-024: Request Logging - Body Logging

**Status:** PENDING  
**User Story:** US-1.2  
**Dependencies:** STEP-023  
**Estimated Effort:** 3 hours

#### Objective

Add optional request/response body logging with proper stream handling.

#### TDD Instructions

1. **RED**: Extend `RequestLoggingMiddlewareTests.cs`

   - Write test: Request body logged when enabled
   - Write test: Request body NOT logged when disabled (default)
   - Write test: Response body logged when enabled
   - Write test: Stream is properly reset after reading
   - Write test: Large bodies are truncated
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `RequestLoggingMiddleware.cs`

   - Enable request buffering
   - Read and log body if configured
   - Properly reset stream position
   - Add truncation for large bodies
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract stream reading to helper method
   - Add max body size configuration
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Body logging configurable
- [ ] Streams properly handled
- [ ] No data loss or corruption
- [ ] Large bodies handled gracefully

#### Deliverable

- Updated `RequestLoggingMiddleware.cs`
- 5 additional tests
- Body size limit configuration

---

### STEP-025: Request Logging Extension Method

**Status:** PENDING  
**User Story:** US-1.2  
**Dependencies:** STEP-024  
**Estimated Effort:** 1 hour

#### Objective

Create fluent extension method for middleware registration.

#### TDD Instructions

1. **RED**: Create `RequestLoggingExtensionsTests.cs`

   - Write test: UseWanderpoolRequestLogging registers middleware
   - Write test: Options can be configured via lambda
   - Write test: Extension is chainable
   - Run tests → ALL FAIL

2. **GREEN**: Create `RequestLoggingExtensions.cs`

   - Implement UseWanderpoolRequestLogging
   - Accept optional configuration action
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add XML documentation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Configuration lambda works
- [ ] Method is chainable

#### Deliverable

- `RequestLoggingExtensions.cs`
- `RequestLoggingExtensionsTests.cs` (min 3 tests)

---

### STEP-026: Outbound HTTP Logging Handler - Core

**Status:** PENDING  
**User Story:** US-1.3  
**Dependencies:** STEP-007  
**Estimated Effort:** 4 hours

#### Objective

Create DelegatingHandler that logs outbound HTTP calls.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Clients/LoggingHandlerTests.cs`

   - Write test: Logs request URL and method
   - Write test: Logs response status and duration
   - Write test: Uses Info level for 2xx responses
   - Write test: Uses Warning level for 4xx responses
   - Write test: Uses Error level for 5xx responses
   - Write test: Logs exceptions with details
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Clients/HttpClientHandlers/LoggingHandler.cs`

   - Implement DelegatingHandler
   - Log before and after request
   - Use stopwatch for duration
   - Handle exceptions
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract log message formatting
   - Optimize stopwatch usage
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >90% coverage
- [ ] Request/response logged correctly
- [ ] Appropriate log levels used
- [ ] Exceptions properly logged
- [ ] Duration accurately measured

#### Deliverable

- `LoggingHandler.cs` implementation
- `LoggingHandlerTests.cs` (min 6 tests)

---

### STEP-027: Outbound HTTP Logging - URL Redaction

**Status:** PENDING  
**User Story:** US-1.3  
**Dependencies:** STEP-026  
**Estimated Effort:** 2 hours

#### Objective

Implement sensitive data redaction in URLs (tokens, keys in query strings).

#### TDD Instructions

1. **RED**: Extend `LoggingHandlerTests.cs`

   - Write test: Query param "token" is redacted
   - Write test: Query param "key" is redacted
   - Write test: Query param "password" is redacted
   - Write test: Query param "api-key" is redacted
   - Write test: Non-sensitive params are NOT redacted
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `LoggingHandler.cs`

   - Implement URL redaction with regex
   - Replace sensitive param values with [REDACTED]
   - Run tests → ALL PASS

3. **REFACTOR**
   - Optimize regex compilation
   - Make sensitive param list configurable
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Sensitive query params redacted
- [ ] URL structure preserved
- [ ] Minimal performance impact

#### Deliverable

- Updated `LoggingHandler.cs`
- 5 additional tests
- Configurable sensitive param list

---

### STEP-028: Outbound HTTP Logging - Client Name

**Status:** PENDING  
**User Story:** US-1.3  
**Dependencies:** STEP-027  
**Estimated Effort:** 2 hours

#### Objective

Include HTTP client name in logs for better traceability.

#### TDD Instructions

1. **RED**: Extend `LoggingHandlerTests.cs`

   - Write test: Client name included in logs when available
   - Write test: Falls back to "Unknown" when name not set
   - Write test: Client name extracted from request options
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `LoggingHandler.cs`

   - Extract client name from HttpRequestMessage.Options
   - Include in all log messages
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract client name resolution to helper
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Client name properly extracted
- [ ] Graceful fallback for missing name

#### Deliverable

- Updated `LoggingHandler.cs`
- 3 additional tests

---

### STEP-029: Outbound HTTP Logging - Retry Detection

**Status:** PENDING  
**User Story:** US-1.3  
**Dependencies:** STEP-028  
**Estimated Effort:** 3 hours

#### Objective

Detect and log retry attempts with Warning level.

#### TDD Instructions

1. **RED**: Create `LoggingHandlerRetryTests.cs`

   - Write test: First attempt logged as Info
   - Write test: Retry attempts logged as Warning
   - Write test: Retry count included in log message
   - Write test: Integration with Polly retry policy
   - Run tests → ALL FAIL

2. **GREEN**: Update `LoggingHandler.cs`

   - Detect retry context from Polly
   - Adjust log level for retries
   - Include attempt number
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract retry detection logic
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Retries properly detected
- [ ] Log level escalated for retries
- [ ] Works with existing ResiliencePipelines

#### Deliverable

- Updated `LoggingHandler.cs`
- `LoggingHandlerRetryTests.cs` (min 4 tests)

---

## PHASE 4: RESILIENCE ENHANCEMENT

### STEP-030: Resilience Options Configuration Class

**Status:** PENDING  
**User Story:** US-4.2  
**Dependencies:** None  
**Estimated Effort:** 2 hours

#### Objective

Create configuration classes for resilience pipeline options.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Policies/ResilienceOptionsTests.cs`

   - Write test: Default options have reasonable values
   - Write test: Timeout options are configurable
   - Write test: Retry options are configurable
   - Write test: Circuit breaker options are configurable
   - Write test: Hedging options are configurable
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Policies/ResilienceOptions.cs`

   - Define configuration classes
   - Set default values
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add XML documentation
   - Add validation attributes
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Sensible defaults set
- [ ] All policies configurable
- [ ] Well documented

#### Deliverable

- `ResilienceOptions.cs` with nested classes
- `ResilienceOptionsTests.cs` (min 5 tests)

---

### STEP-031: Configurable Resilience Pipeline Builder

**Status:** PENDING  
**User Story:** US-4.2  
**Dependencies:** STEP-030  
**Estimated Effort:** 3 hours

#### Objective

Refactor ResiliencePipelines to accept configuration.

#### TDD Instructions

1. **RED**: Create `ConfigurableResiliencePipelineTests.cs`

   - Write test: Pipeline created from ResilienceOptions
   - Write test: Timeout value from configuration
   - Write test: Retry count from configuration
   - Write test: Circuit breaker settings from configuration
   - Write test: Hedging settings from configuration
   - Run tests → ALL FAIL

2. **GREEN**: Update `src/Wanderpool.Common.Infra/Policies/ResiliencePipelines.cs`

   - Accept ResilienceOptions parameter
   - Build pipeline from options
   - Maintain backward compatibility
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract builder methods per policy
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >85% coverage
- [ ] Configuration properly applied
- [ ] Backward compatible with existing code
- [ ] Each policy configurable independently

#### Deliverable

- Updated `ResiliencePipelines.cs`
- `ConfigurableResiliencePipelineTests.cs` (min 5 tests)

---

### STEP-032: Named Resilience Pipelines Registry

**Status:** PENDING  
**User Story:** US-4.1  
**Dependencies:** STEP-031  
**Estimated Effort:** 4 hours

#### Objective

Create registry for named resilience pipelines with different configurations.

#### TDD Instructions

1. **RED**: Create `NamedResiliencePipelinesTests.cs`

   - Write test: Pipeline can be registered by name
   - Write test: Pipeline can be retrieved by name
   - Write test: Multiple named pipelines can coexist
   - Write test: Unknown pipeline name throws exception
   - Write test: Pipelines are singleton per name
   - Run tests → ALL FAIL

2. **GREEN**: Create `ResiliencePipelineRegistry.cs`

   - Implement dictionary-based registry
   - Register named pipelines
   - Retrieve by name
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add thread-safe registration
   - Add validation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >90% coverage
- [ ] Thread-safe registration and retrieval
- [ ] Clear error messages for missing pipelines
- [ ] Pipelines are reusable

#### Deliverable

- `ResiliencePipelineRegistry.cs`
- `NamedResiliencePipelinesTests.cs` (min 5 tests)

---

### STEP-033: Named Pipeline Configuration from appsettings

**Status:** PENDING  
**User Story:** US-4.1  
**Dependencies:** STEP-032  
**Estimated Effort:** 3 hours

#### Objective

Load named pipeline configurations from appsettings.json.

#### TDD Instructions

1. **RED**: Create `ResiliencePipelineConfigurationTests.cs`

   - Write test: Pipelines loaded from configuration section
   - Write test: Multiple named pipelines loaded
   - Write test: Each pipeline has its own settings
   - Write test: Invalid configuration throws exception
   - Run tests → ALL FAIL

2. **GREEN**: Create `ResilienceConfigurationExtensions.cs`

   - Read "Resilience:Pipelines" section
   - Register each named pipeline
   - Validate configuration
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add configuration validation
   - Add helpful error messages
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Configuration properly parsed
- [ ] Validation catches errors early
- [ ] Multiple pipelines configurable

#### Deliverable

- `ResilienceConfigurationExtensions.cs`
- `ResiliencePipelineConfigurationTests.cs` (min 4 tests)
- Example appsettings.json structure

---

### STEP-034: Resilience Events Logging Infrastructure

**Status:** PENDING  
**User Story:** US-4.3  
**Dependencies:** STEP-007  
**Estimated Effort:** 2 hours

#### Objective

Create event handlers for Polly resilience events.

#### TDD Instructions

1. **RED**: Create `ResilienceEventsTests.cs`

   - Write test: OnRetry event is loggable
   - Write test: OnCircuitBreaker event is loggable
   - Write test: OnTimeout event is loggable
   - Write test: OnHedging event is loggable
   - Run tests → ALL FAIL

2. **GREEN**: Create `ResilienceEventHandlers.cs`

   - Define event handler delegates
   - Implement logging for each event type
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract common logging pattern
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] All event types supported
- [ ] Structured logging used

#### Deliverable

- `ResilienceEventHandlers.cs`
- `ResilienceEventsTests.cs` (min 4 tests)

---

### STEP-035: Resilience Events Integration

**Status:** PENDING  
**User Story:** US-4.3  
**Dependencies:** STEP-031, STEP-034  
**Estimated Effort:** 3 hours

#### Objective

Integrate event handlers with resilience pipeline configuration.

#### TDD Instructions

1. **RED**: Extend `ConfigurableResiliencePipelineTests.cs`

   - Write test: Retry events are logged
   - Write test: Circuit breaker state changes are logged
   - Write test: Timeout events are logged
   - Write test: Hedging events are logged
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `ResiliencePipelines.cs`

   - Wire up event handlers to each policy
   - Configure logging delegates
   - Run tests → ALL PASS

3. **REFACTOR**
   - Ensure event handlers are optional
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Events properly logged
- [ ] Log levels appropriate
- [ ] Structured data included

#### Deliverable

- Updated `ResiliencePipelines.cs`
- 4 additional tests

---

## PHASE 5: API INFRASTRUCTURE

### STEP-036: Endpoint Result Extension Methods

**Status:** PENDING  
**User Story:** US-5.1  
**Dependencies:** None  
**Estimated Effort:** 3 hours

#### Objective

Create extension methods for converting OperationResult to IResult.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Api/ResultExtensionsTests.cs`

   - Write test: Success result returns Ok with data
   - Write test: Failed result returns appropriate status code
   - Write test: ValidationException returns BadRequest with errors
   - Write test: NotFoundException returns NotFound
   - Write test: Response includes TraceId
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Api/ResultExtensions.cs`

   - Implement ToResult<T> extension
   - Map errors to status codes
   - Return ApiResponseEnvelope format
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract error mapping logic
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >90% coverage
- [ ] All error types properly mapped
- [ ] TraceId included in responses
- [ ] Easy to use in endpoints

#### Deliverable

- `ResultExtensions.cs`
- `ResultExtensionsTests.cs` (min 5 tests)

---

### STEP-037: Endpoint Response Helper Methods

**Status:** PENDING  
**User Story:** US-5.1  
**Dependencies:** STEP-036  
**Estimated Effort:** 2 hours

#### Objective

Create static helper methods for common response scenarios.

#### TDD Instructions

1. **RED**: Extend `ResultExtensionsTests.cs`

   - Write test: ApiResponse.Ok() returns 200 with data
   - Write test: ApiResponse.Created() returns 201 with location
   - Write test: ApiResponse.NoContent() returns 204
   - Write test: ApiResponse.BadRequest() returns 400 with errors
   - Write test: ApiResponse.NotFound() returns 404
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Add helper methods to `ResultExtensions.cs`

   - Implement static ApiResponse class
   - Create helper methods for common responses
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add XML documentation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Common response types covered
- [ ] Intuitive API

#### Deliverable

- Updated `ResultExtensions.cs`
- 5 additional tests

---

### STEP-038: FluentValidation Endpoint Filter - Core

**Status:** PENDING  
**User Story:** US-5.2  
**Dependencies:** None  
**Estimated Effort:** 4 hours

#### Objective

Create endpoint filter that automatically validates request objects.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Api/ValidationFilterTests.cs`

   - Write test: Valid request passes through
   - Write test: Invalid request returns 400
   - Write test: Validation errors included in response
   - Write test: Multiple validation errors collected
   - Write test: Filter only runs when validator exists
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Api/ValidationFilter.cs`

   - Implement IEndpointFilter
   - Resolve validator from DI
   - Validate request object
   - Return validation errors if failed
   - Run tests → ALL PASS

3. **REFACTOR**
   - Optimize validator resolution
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >90% coverage
- [ ] Validation automatic for requests with validators
- [ ] Error format consistent with ValidationException
- [ ] No performance impact when no validator

#### Deliverable

- `ValidationFilter.cs`
- `ValidationFilterTests.cs` (min 5 tests)
- FluentValidation package added

---

### STEP-039: Validation Filter Extension Method

**Status:** PENDING  
**User Story:** US-5.2  
**Dependencies:** STEP-038  
**Estimated Effort:** 1 hour

#### Objective

Create extension method for easy filter registration.

#### TDD Instructions

1. **RED**: Create `ValidationFilterExtensionsTests.cs`

   - Write test: AddValidation adds filter to endpoint
   - Write test: Extension works with route builders
   - Write test: Extension is chainable
   - Run tests → ALL FAIL

2. **GREEN**: Create `ValidationFilterExtensions.cs`

   - Implement AddValidation extension
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add XML documentation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Easy to use in endpoint definitions
- [ ] Chainable API

#### Deliverable

- `ValidationFilterExtensions.cs`
- `ValidationFilterExtensionsTests.cs` (min 3 tests)

---

### STEP-040: Endpoint Group Extensions - Prefixing

**Status:** PENDING  
**User Story:** US-5.3  
**Dependencies:** None  
**Estimated Effort:** 2 hours

#### Objective

Create extension methods for endpoint grouping with common prefixes.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Api/EndpointExtensionsTests.cs`

   - Write test: MapApiGroup creates group with prefix
   - Write test: Nested groups combine prefixes
   - Write test: Version prefix helper works
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Api/EndpointExtensions.cs`

   - Implement MapApiGroup extension
   - Implement MapVersionedApi extension
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract common logic
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Groups properly created
- [ ] Prefixes properly combined
- [ ] Version prefix helper works

#### Deliverable

- `EndpointExtensions.cs`
- `EndpointExtensionsTests.cs` (min 3 tests)

---

### STEP-041: Endpoint Group Extensions - Filters and Metadata

**Status:** PENDING  
**User Story:** US-5.3  
**Dependencies:** STEP-040  
**Estimated Effort:** 2 hours

#### Objective

Add methods for applying filters and metadata to groups.

#### TDD Instructions

1. **RED**: Extend `EndpointExtensionsTests.cs`

   - Write test: WithCommonFilters applies filters to all endpoints
   - Write test: WithAuthentication adds auth requirement
   - Write test: WithTags adds tags to group
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `EndpointExtensions.cs`

   - Add WithCommonFilters extension
   - Add WithAuthentication extension
   - Add WithTags extension
   - Run tests → ALL PASS

3. **REFACTOR**
   - Chain extensions fluently
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Filters apply to all group endpoints
- [ ] Metadata properly set
- [ ] Fluent API

#### Deliverable

- Updated `EndpointExtensions.cs`
- 3 additional tests

---

### STEP-042: Health Checks Infrastructure

**Status:** PENDING  
**User Story:** US-5.4  
**Dependencies:** None  
**Estimated Effort:** 3 hours

#### Objective

Create service extension for health check registration.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/HealthChecks/HealthCheckExtensionsTests.cs`

   - Write test: AddWanderpoolHealthChecks registers health checks
   - Write test: HTTP client health checks added
   - Write test: Custom health checks can be added
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/HealthChecks/HealthCheckExtensions.cs`

   - Implement AddWanderpoolHealthChecks extension
   - Register default health checks
   - Run tests → ALL PASS

3. **REFACTOR**
   - Make health checks configurable
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >85% coverage
- [ ] Health checks properly registered
- [ ] Extensible for custom checks

#### Deliverable

- `HealthCheckExtensions.cs`
- `HealthCheckExtensionsTests.cs` (min 3 tests)
- AspNetCore.HealthChecks.Uris package added

---

### STEP-043: Health Check Endpoints

**Status:** PENDING  
**User Story:** US-5.4  
**Dependencies:** STEP-042  
**Estimated Effort:** 3 hours

#### Objective

Create /health/live and /health/ready endpoints.

#### TDD Instructions

1. **RED**: Extend `HealthCheckExtensionsTests.cs`

   - Write test: /health/live always returns 200
   - Write test: /health/ready returns 200 when healthy
   - Write test: /health/ready returns 503 when unhealthy
   - Write test: Response format is JSON
   - Write test: Detailed info for authenticated requests
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `HealthCheckExtensions.cs`

   - Implement MapWanderpoolHealthChecks extension
   - Configure /health/live endpoint
   - Configure /health/ready endpoint
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract response formatting
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Both endpoints functional
- [ ] JSON response format
- [ ] Appropriate status codes

#### Deliverable

- Updated `HealthCheckExtensions.cs`
- 5 additional tests

---

## PHASE 6: INTEGRATION

### STEP-044: HTTP Client Factory Extensions - Basic

**Status:** PENDING  
**User Story:** US-6.3  
**Dependencies:** STEP-026, STEP-016  
**Estimated Effort:** 4 hours

#### Objective

Create extension method to register typed HTTP clients with all infrastructure.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Clients/HttpClientExtensionsTests.cs`

   - Write test: AddWanderpoolHttpClient registers typed client
   - Write test: Base address is configured
   - Write test: Resilience pipeline is applied
   - Write test: Logging handler is applied
   - Write test: Metrics handler is applied
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Clients/HttpClientExtensions.cs`

   - Implement AddWanderpoolHttpClient extension
   - Register typed client
   - Apply all handlers in correct order
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract handler pipeline configuration
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >85% coverage
- [ ] All handlers properly applied
- [ ] Handler order correct
- [ ] Type-safe client registration

#### Deliverable

- `HttpClientExtensions.cs`
- `HttpClientExtensionsTests.cs` (min 5 tests)

---

### STEP-045: HTTP Client Factory Extensions - Options

**Status:** PENDING  
**User Story:** US-6.3  
**Dependencies:** STEP-044  
**Estimated Effort:** 3 hours

#### Objective

Add configuration options for HTTP client registration.

#### TDD Instructions

1. **RED**: Create `HttpClientOptionsTests.cs`

   - Write test: Options specify base address
   - Write test: Options specify resilience pipeline name
   - Write test: Options specify timeout
   - Write test: Options specify token provider type
   - Write test: Options are applied correctly
   - Run tests → ALL FAIL

2. **GREEN**: Create `HttpClientOptions.cs` and update extensions

   - Define options class
   - Accept options in extension method
   - Apply options during registration
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add validation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] All options configurable
- [ ] Options properly validated
- [ ] Clear error messages

#### Deliverable

- `HttpClientOptions.cs`
- Updated `HttpClientExtensions.cs`
- `HttpClientOptionsTests.cs` (min 5 tests)

---

### STEP-046: HTTP Client Factory Extensions - Token Provider Integration

**Status:** PENDING  
**User Story:** US-6.3  
**Dependencies:** STEP-045  
**Estimated Effort:** 3 hours

#### Objective

Integrate TokenRefreshHandler when token provider is configured.

#### TDD Instructions

1. **RED**: Extend `HttpClientExtensionsTests.cs`

   - Write test: TokenRefreshHandler added when provider specified
   - Write test: TokenRefreshHandler NOT added when no provider
   - Write test: Token provider resolved from DI
   - Write test: Handler in correct position in pipeline
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `HttpClientExtensions.cs`

   - Conditionally add TokenRefreshHandler
   - Resolve token provider from DI
   - Position handler correctly
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract handler ordering logic
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Token handler conditionally added
- [ ] Handler order preserved
- [ ] Works with existing TokenRefreshHandler

#### Deliverable

- Updated `HttpClientExtensions.cs`
- 4 additional tests

---

### STEP-047: Unified Infrastructure Service Registration

**Status:** PENDING  
**User Story:** US-6.1  
**Dependencies:** STEP-007, STEP-011, STEP-014, STEP-004, STEP-042  
**Estimated Effort:** 4 hours

#### Objective

Create single extension method to register all infrastructure services.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/WanderpoolInfrastructureExtensionsTests.cs`

   - Write test: AddWanderpoolInfrastructure registers logging
   - Write test: Registers tracing
   - Write test: Registers metrics
   - Write test: Registers exception handling
   - Write test: Registers health checks
   - Write test: Registers correlation context
   - Write test: Options control what's registered
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/WanderpoolInfrastructureExtensions.cs`

   - Implement AddWanderpoolInfrastructure extension
   - Call all individual registration methods
   - Accept WanderpoolOptions configuration
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract option validation
   - Add helpful error messages
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >85% coverage
- [ ] All services registered
- [ ] Options properly respected
- [ ] Configuration validated

#### Deliverable

- `WanderpoolInfrastructureExtensions.cs`
- `WanderpoolInfrastructureExtensionsTests.cs` (min 7 tests)
- `WanderpoolOptions.cs` configuration class

---

### STEP-048: Unified Infrastructure Options

**Status:** PENDING  
**User Story:** US-6.1  
**Dependencies:** STEP-047  
**Estimated Effort:** 2 hours

#### Objective

Create comprehensive options class for infrastructure configuration.

#### TDD Instructions

1. **RED**: Create `WanderpoolOptionsTests.cs`

   - Write test: Options can enable/disable components
   - Write test: Options bind from configuration
   - Write test: Options have sensible defaults
   - Write test: Options validation works
   - Run tests → ALL FAIL

2. **GREEN**: Create `WanderpoolOptions.cs`

   - Define nested options classes
   - Set defaults
   - Add validation
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add XML documentation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] All components configurable
- [ ] Configuration binding works
- [ ] Validation catches errors

#### Deliverable

- `WanderpoolOptions.cs`
- `WanderpoolOptionsTests.cs` (min 4 tests)

---

### STEP-049: Unified Middleware Pipeline

**Status:** PENDING  
**User Story:** US-6.2  
**Dependencies:** STEP-009, STEP-004, STEP-022  
**Estimated Effort:** 3 hours

#### Objective

Create single extension method to configure all middleware in correct order.

#### TDD Instructions

1. **RED**: Create `WanderpoolMiddlewarePipelineTests.cs`

   - Write test: UseWanderpoolInfrastructure adds correlation ID middleware
   - Write test: Adds exception handling middleware
   - Write test: Adds request logging middleware
   - Write test: Middleware order is correct
   - Write test: Health check endpoints are mapped
   - Run tests → ALL FAIL

2. **GREEN**: Update `WanderpoolInfrastructureExtensions.cs`

   - Implement UseWanderpoolInfrastructure extension
   - Add all middleware in correct order
   - Run tests → ALL PASS

3. **REFACTOR**
   - Make middleware optional via options
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >90% coverage
- [ ] All middleware configured
- [ ] Order is correct and documented
- [ ] Optional middleware work

#### Deliverable

- Updated `WanderpoolInfrastructureExtensions.cs`
- `WanderpoolMiddlewarePipelineTests.cs` (min 5 tests)

---

### STEP-050: Configuration Validation

**Status:** PENDING  
**User Story:** US-6.1  
**Dependencies:** STEP-048  
**Estimated Effort:** 3 hours

#### Objective

Add comprehensive configuration validation with helpful error messages.

#### TDD Instructions

1. **RED**: Create `ConfigurationValidationTests.cs`

   - Write test: Missing required ServiceName throws exception
   - Write test: Invalid OTLP endpoint throws exception
   - Write test: Invalid resilience values throw exception
   - Write test: Error messages are helpful
   - Run tests → ALL FAIL

2. **GREEN**: Create `ConfigurationValidator.cs`

   - Implement validation logic
   - Provide clear error messages
   - Call from AddWanderpoolInfrastructure
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract validation rules
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] All critical config validated
- [ ] Error messages actionable
- [ ] Fails fast on invalid config

#### Deliverable

- `ConfigurationValidator.cs`
- `ConfigurationValidationTests.cs` (min 4 tests)
- Updated infrastructure extensions

---

### STEP-051: Problem Details Support - Mapper

**Status:** PENDING  
**User Story:** US-2.3  
**Dependencies:** STEP-004  
**Estimated Effort:** 3 hours

#### Objective

Create mapper to convert exceptions to RFC 7807 ProblemDetails.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Exceptions/ProblemDetailsMapperTests.cs`

   - Write test: ValidationException maps to ProblemDetails
   - Write test: NotFoundException maps to ProblemDetails
   - Write test: ProblemDetails includes type URI
   - Write test: ProblemDetails includes instance path
   - Write test: ProblemDetails includes traceId
   - Write test: Extension properties included
   - Run tests → ALL FAIL

2. **GREEN**: Create `src/Wanderpool.Common.Infra/Exceptions/ProblemDetailsMapper.cs`

   - Implement mapping logic
   - Set all RFC 7807 properties
   - Add extension properties
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract URI generation
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass with >90% coverage
- [ ] All RFC 7807 fields populated
- [ ] Extension properties work
- [ ] URIs point to documentation

#### Deliverable

- `ProblemDetailsMapper.cs`
- `ProblemDetailsMapperTests.cs` (min 6 tests)

---

### STEP-052: Problem Details Support - Integration

**Status:** PENDING  
**User Story:** US-2.3  
**Dependencies:** STEP-051  
**Estimated Effort:** 2 hours

#### Objective

Integrate ProblemDetails option into exception handling middleware.

#### TDD Instructions

1. **RED**: Extend `GlobalExceptionMiddlewareTests.cs`

   - Write test: ProblemDetails returned when option enabled
   - Write test: ApiResponseEnvelope returned when option disabled
   - Write test: Option configurable via WanderpoolOptions
   - Run tests → NEW TESTS FAIL

2. **GREEN**: Update `GlobalExceptionMiddleware.cs`

   - Accept UseProblemDetails option
   - Conditionally use ProblemDetailsMapper
   - Run tests → ALL PASS

3. **REFACTOR**
   - Extract response writing logic
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] Both response formats supported
- [ ] Option properly respected
- [ ] Backward compatible

#### Deliverable

- Updated `GlobalExceptionMiddleware.cs`
- 3 additional tests
- Updated `WanderpoolOptions.cs`

---

### STEP-053: Integration Testing Infrastructure

**Status:** PENDING  
**User Story:** N/A (Testing Support)  
**Dependencies:** STEP-049  
**Estimated Effort:** 4 hours

#### Objective

Create helper utilities for integration testing with TestServer.

#### TDD Instructions

1. **RED**: Create `Wanderpool.Common.Infra.Tests/Integration/TestServerBuilderTests.cs`

   - Write test: TestServer with full infrastructure can be created
   - Write test: Configuration can be customized
   - Write test: Services can be mocked
   - Write test: HTTP client from TestServer works
   - Run tests → ALL FAIL

2. **GREEN**: Create `TestHelpers/WanderpoolTestServerBuilder.cs`

   - Implement builder for TestServer
   - Allow configuration customization
   - Allow service overrides
   - Run tests → ALL PASS

3. **REFACTOR**
   - Add convenience methods
   - Run tests → ALL PASS

#### Acceptance Criteria

- [ ] All tests pass
- [ ] TestServer easy to create
- [ ] Configuration flexible
- [ ] Service mocking supported

#### Deliverable

- `WanderpoolTestServerBuilder.cs`
- `TestServerBuilderTests.cs` (min 4 tests)
- Example integration tests

---

### STEP-054: Documentation and Examples

**Status:** PENDING  
**User Story:** N/A (Documentation)  
**Dependencies:** All previous steps  
**Estimated Effort:** 4 hours

#### Objective

Create comprehensive documentation and example usage.

#### Tasks

1. Write README.md with:

   - Quick start guide
   - Feature overview
   - Configuration examples
   - Usage examples for each feature

2. Create example project demonstrating:

   - Full infrastructure setup
   - HTTP client registration
   - Endpoint definitions with validation
   - Health checks
   - Observability integration

3. Document configuration schema

4. Create troubleshooting guide

#### Acceptance Criteria

- [ ] README is clear and comprehensive
- [ ] Example project runs successfully
- [ ] All features documented
- [ ] Configuration options explained

#### Deliverable

- README.md
- Example project (separate solution)
- Configuration schema documentation
- Troubleshooting guide

---

### STEP-055: NuGet Package Preparation

**Status:** PENDING  
**User Story:** N/A (Packaging)  
**Dependencies:** All implementation steps  
**Estimated Effort:** 2 hours

#### Objective

Prepare library for NuGet distribution.

#### Tasks

1. Update .csproj with:

   - Package metadata (version, authors, description)
   - License information
   - Repository URL
   - Tags
   - Release notes

2. Add CHANGELOG.md

3. Configure symbol package generation

4. Test package installation in sample project

#### Acceptance Criteria

- [ ] Package metadata complete
- [ ] Package builds successfully
- [ ] Symbols included
- [ ] Dependencies correctly specified
- [ ] Package installs in test project

#### Deliverable

- Updated .csproj files
- CHANGELOG.md
- NuGet package (.nupkg)
- Package validation passed

---

## Summary Statistics

**Total Steps:** 55
**Total Estimated Effort:** ~150 hours

### By Phase

- **Phase 1 (Core Infrastructure):** 10 steps, ~25 hours
- **Phase 2 (Telemetry):** 10 steps, ~28 hours
- **Phase 3 (Enhanced Logging):** 9 steps, ~23 hours
- **Phase 4 (Resilience Enhancement):** 6 steps, ~17 hours
- **Phase 5 (API Infrastructure):** 7 steps, ~18 hours
- **Phase 6 (Integration):** 13 steps, ~39 hours

### Testing Strategy

- Every step includes TDD cycle (Red-Green-Refactor)
- Minimum test coverage: 85% for complex features, 90%+ for critical paths
- Integration tests for middleware and handlers
- Unit tests for business logic and utilities
- TestServer-based tests for full pipeline validation

### Delivery Approach

- Each step is independently testable and deliverable
- Steps can be assigned to different developers
- Clear dependencies allow parallel work where possible
- Regular integration testing recommended after every 3-5 steps

---

## Notes for Implementation

1. **TDD Discipline**: Follow Red-Green-Refactor strictly. Write failing tests first, implement minimal code, then refactor.

2. **Test Quality**: Tests should be:

   - Fast (< 1 second each)
   - Isolated (no shared state)
   - Deterministic (no randomness)
   - Readable (clear arrange-act-assert)

3. **Code Reviews**: Each step should be reviewed before moving to next

4. **Continuous Integration**: Run all tests on every commit

5. **Documentation**: Update XML docs as you implement

6. **Breaking Changes**: Avoid at all costs; maintain backward compatibility

7. **Performance**: Profile handlers and middleware for performance impact

8. **Security**: Always sanitize logs, redact sensitive data
```
