# Wanderpool.Common Infrastructure - Prompt Plan

This document tracks the completion status of implementation steps from the Wanderpool.Common Infrastructure project.

## PHASE 1: CORE INFRASTRUCTURE

### STEP-001: Domain Exception Base Class

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `WanderpoolException.cs` - Base exception class with ErrorCode support
- ✅ `WanderpoolExceptionTests.cs` - Comprehensive test suite with 6 tests
  - Tests: Constructor, validation, serialization, inner exception handling, immutability

**Implementation Details:**
- Exception is fully serializable with `[Serializable]` attribute
- Implements `GetObjectData` for serialization support
- Validates ErrorCode is not null or empty
- Includes inner exception support
- All tests passing

**Notes:**
- Project targets .NET 6.0 (compatibility with available SDK)
- All validations ensure robust error handling

---

### STEP-002: ValidationException Implementation

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `ValidationException.cs` - Exception with structured validation errors dictionary
- ✅ `ValidationExceptionTests.cs` - Comprehensive test suite with 6 tests
  - Tests: Error storage, empty validation check, null check, error code, serialization, immutability, message formatting

**Implementation Details:**
- Stores validation errors as `Dictionary<string, string[]>` (readonly)
- Error code defaults to "VALIDATION_ERROR"
- Defensive copy ensures error dictionary immutability
- Proper serialization support for distributed scenarios
- Clear error message summarizing validation failures

---

### STEP-003: Remaining Domain Exceptions

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `NotFoundException.cs` - Exception for missing resources with ResourceType and ResourceId properties
- ✅ `NotFoundExceptionTests.cs` - Test suite with 3 tests
- ✅ `ForbiddenException.cs` - Exception for access denials
- ✅ `ForbiddenExceptionTests.cs` - Test suite with 2 tests
- ✅ `ConflictException.cs` - Exception for resource conflicts with ResourceIdentifier
- ✅ `ConflictExceptionTests.cs` - Test suite with 2 tests
- ✅ `BusinessRuleException.cs` - Exception for business rule violations with RuleName
- ✅ `BusinessRuleExceptionTests.cs` - Test suite with 3 tests

**Implementation Details:**
- All 4 exceptions properly inherit from `WanderpoolException`
- Each has appropriate error codes (NOT_FOUND, FORBIDDEN, CONFLICT, BUSINESS_RULE_VIOLATION)
- All are fully serializable
- Clear, actionable error messages
- Resource tracking for audit and debugging

### STEP-004: Global Exception Handler Middleware - Core

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `GlobalExceptionMiddleware.cs` - Core middleware for handling all unhandled exceptions
- ✅ `GlobalExceptionMiddlewareTests.cs` - Comprehensive test suite with 14 tests
  - Tests: Exception mapping for all types, status codes, TraceId inclusion, error levels, pass-through

**Implementation Details:**
- Exception mapping to HTTP status codes: 400 (Validation), 401 (Unauthorized), 403 (Forbidden), 404 (NotFound), 409 (Conflict), 499 (OperationCancelled), 502 (RemoteService), 500 (Generic/Error)
- Returns `ApiResponseEnvelope<object>` format with error details
- Includes TraceId from Activity or HttpContext
- Logs all exceptions with full stack trace at Error level
- Error levels determined by HTTP status code (Info for 2xx, Warning for 4xx, Error for 5xx)
- Properly passes through successful requests without modification

### STEP-005: Global Exception Handler - Production Mode

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `GlobalExceptionMiddleware.cs` - Enhanced with IWebHostEnvironment injection for environment detection
- ✅ Added 3 new production mode tests to GlobalExceptionMiddlewareTests.cs

**Implementation Details:**
- Detects production vs development environment via IWebHostEnvironment
- In Production: 5xx errors show generic message to prevent information leakage
- In Development: Full error details shown for debugging
- 4xx errors always show detailed messages (safe for client exposure)
- Full error details always logged server-side for troubleshooting
- Configurable environment via --environment parameter in WebApplicationBuilder

### STEP-006: Exception Handler Extension Methods

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `GlobalExceptionHandlingExtensions.cs` - Extension method for middleware registration
- ✅ `GlobalExceptionHandlingExtensionsTests.cs` - Comprehensive test suite with 10 tests
  - Tests: Registration, exception handling, request pass-through, method chaining, exception types, content type, TraceId, error levels

**Implementation Details:**
- `UseWanderpoolExceptionHandling()` extension method on WebApplication
- Fluent API supporting method chaining
- Returns WebApplication for integration in pipeline configuration
- Integrates seamlessly with GlobalExceptionMiddleware from previous steps

### STEP-007: Serilog Configuration Infrastructure

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `LoggingExtensions.cs` - Serilog configuration service extension
- ✅ `LoggingExtensionsTests.cs` - 12 comprehensive test cases

**Implementation Details:**
- `AddWanderpoolLogging(serviceName)` extension method
- Pre-configured enrichers: MachineName, ProcessId, ThreadId, Environment, ServiceName
- Development: Readable console format
- Production: Structured format for log aggregation
- Bootstrap logger for startup diagnostics
- Support for appsettings.json configuration override
- NuGet packages: Serilog.AspNetCore, Serilog.Sinks.Console, enrichers

### STEP-008: Correlation ID Context Service

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `ICorrelationContext.cs` - Interface for correlation ID access
- ✅ `CorrelationContext.cs` - Implementation with validation

**Implementation Details:**
- Service interface for DI-based correlation ID access
- Thread-safe implementation
- Validation for null/empty IDs

### STEP-009: Correlation ID Middleware

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `CorrelationIdMiddleware.cs` - Request processing middleware

**Implementation Details:**
- Extracts X-Correlation-Id from request headers
- Generates new GUID IDs when not provided
- Stores in HttpContext.Items for downstream access
- Adds to response headers for client tracking

### STEP-010: Correlation ID Serilog Enricher

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `CorrelationIdEnricher.cs` - Serilog log enrichment
- ✅ `CorrelationIdExtensions.cs` - Service/middleware extensions
- ✅ `CorrelationIdTests.cs` - 10 comprehensive test cases

**Implementation Details:**
- Automatic enrichment of all log entries with correlation ID
- AddWanderpoolCorrelationId() service registration
- UseWanderpoolCorrelationId() middleware registration
- Full DI integration with IHttpContextAccessor

---

## PHASE 2: TELEMETRY

### STEP-011: OpenTelemetry Tracing - Basic Setup

**Status:** ✅ COMPLETED

**Completed:** 2025-12-06

**Deliverables:**
- ✅ `TracingExtensions.cs` - Two extension methods for OpenTelemetry configuration
- ✅ `TracingExtensionsTests.cs` - Comprehensive test suite with 18 tests

**Implementation Details:**
- Two AddWanderpoolTracing() overloads for flexible configuration
- Default overload: auto-detects service name/version from assembly, uses localhost:4317 for OTLP
- Custom overload: supports custom OTLP endpoint and sampling probability (0.0-1.0)
- Validates sampling probability is within valid range (0.0-1.0)
- Configures ASP.NET Core instrumentation with request/response enrichment
- Configures HttpClient instrumentation with URI and status code enrichment
- W3C TraceContext propagation support
- Resource builder setup with service name and version from assembly

### STEP-012: OpenTelemetry Tracing - Exporters

**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `ExporterConfiguration.cs` - Configuration class for individual exporters with Type, Endpoint, Enabled, BatchSize, TimeoutMs, MaxQueueSize properties
- ✅ `ExporterFactory.cs` - Factory implementation supporting OTLP, Jaeger, Zipkin, and Console exporters
- ✅ `ExporterFactoryTests.cs` - Test suite with 13 comprehensive tests
  - Tests: OTLP, Jaeger, Zipkin, Console exporter registration
  - Disabled exporters, invalid exporter types, multiple exporters, default fallback
- ✅ `TracingExtensions.cs` - New `AddWanderpoolTracingWithConfigurableExporters()` method for configuration-driven setup
- ✅ `TracingExtensionsTests.cs` - 10 new test cases for configurable exporters
- ✅ `OpenTelemetryConfiguration.cs` - Updated to include Exporters dictionary
- ✅ `Wanderpool.Common.Infra.csproj` - Added Jaeger (1.6.0-rc.1) and Zipkin (1.5.1) exporter packages

**Implementation Details:**
- ExporterFactory uses switch expression for exporter type routing
- Supports OTLP (OpenTelemetry Protocol) as default fallback
- Jaeger exporter for distributed tracing visualization
- Zipkin exporter as alternative distributed tracing backend
- Console exporter for development/debugging
- Each exporter can be individually enabled/disabled
- Configurable batch size (default 512), timeout (default 5000ms), queue size (default 2048)
- Configuration binding from appsettings.json under "OpenTelemetry:Exporters" section

**Test Coverage:**
- 23 new tests (13 ExporterFactory + 10 TracingExtensions)
- All 113 total tests passing
- Tests verify: exporter type support, custom endpoints, disabled exporters, multiple exporters, sampling configuration

### STEP-013: OpenTelemetry Tracing - Enrichment

**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `TracingEnricher.cs` - Static enrichment extension methods for Activity enrichment
- ✅ `TracingEnricherTests.cs` - Test suite with 20 comprehensive tests
  - Tests: Correlation ID, body sizes, client IP, user agent, request paths
  - Tests: Environment info, exception details, content types, null safety
- ✅ Updated `TracingExtensions.cs` - Integrated enrichment into all 4 tracing methods

**Enrichment Methods (12 total):**
- EnrichWithCorrelationId() - Extracts correlation ID from HttpContext.Items
- EnrichWithRequestBodySize() - Tracks HTTP request body size
- EnrichWithResponseBodySize() - Tracks HTTP response body size
- EnrichWithClientIp() - Extracts client IP from connection info
- EnrichWithUserAgent() - Extracts User-Agent header
- EnrichWithRequestPath() - Tracks request path and query string
- EnrichWithEnvironmentInfo() - Adds machine name and deployment environment
- EnrichWithExceptionDetails() - Extracts exception type, message, and stack trace
- EnrichWithContentType() - Tracks request content-type
- EnrichWithResponseContentType() - Tracks response content-type
- Plus 2 helper methods

**Implementation Details:**
- All enrichment methods are null-safe (defensive against null Activity, HttpContext, etc.)
- Integrated into existing instrumentation callbacks:
  - ASP.NET Core: EnrichWithHttpRequest and EnrichWithHttpResponse
  - HttpClient: EnrichWithHttpRequestMessage and EnrichWithHttpResponseMessage
- Applied to all 4 tracing extension methods:
  - AddWanderpoolTracing() (default)
  - AddWanderpoolTracing(endpoint, sampling)
  - AddWanderpoolTracingWithExporters() - includes environment-aware enrichment
  - AddWanderpoolTracingWithConfigurableExporters()

**Test Coverage:**
- 20 new tests for enrichment functionality
- All 135 total tests passing (from 113)
- Tests verify: null-safety, enrichment execution, multiple enrichments
- Coverage for all enrichment methods and edge cases (missing headers, no content-length, etc.)

### STEP-014: OpenTelemetry Metrics - Basic Setup

**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `MetricsConfiguration.cs` - Configuration class for metrics setup with appsettings.json binding
- ✅ `MetricsExtensions.cs` - Three overloaded AddWanderpoolMetrics methods for flexible registration
- ✅ `MetricsExtensionsTests.cs` - 14 comprehensive test cases

**Implementation Details:**
- MetricsConfiguration properties: Enabled, EnableAspNetCoreMetrics, EnableHttpClientMetrics, EnableRuntimeMetrics (bool), OtlpEndpoint (string), ExportIntervalSeconds (int), MaxMetricsBufferSize (int)
- Three registration patterns:
  * Default: `AddWanderpoolMetrics()` - auto-detects service name, uses localhost:4317
  * Custom endpoint: `AddWanderpoolMetrics(otlpEndpoint)` - custom OTLP endpoint
  * Configuration-driven: `AddWanderpoolMetricsWithConfiguration(config)` - full configuration support
- Configures ASP.NET Core, HttpClient, and Runtime instrumentation with AddOtlpExporter()
- All metrics export to OTLP endpoint
- Service name auto-detection from calling assembly
- Configuration binding from "OpenTelemetryMetrics" section in appsettings.json

**Test Coverage:**
- 14 new tests for metrics configuration and registration
- Default registration with MeterProvider service registration
- Method chaining support
- Custom service names and OTLP endpoints
- Configuration-based setup with various metric combinations (all enabled, individual types)
- Disabled metrics handling (returns services without registration)
- Configuration binding from in-memory appsettings
- Default configuration values verification
- All 135 existing tests still passing
- Build successful with no compilation errors

### STEP-015: Custom HTTP Client Metrics
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `HttpClientMetricsInstruments.cs` - Data class for metric instruments (Counter and Histogram)
- ✅ `MetricsExtensions.cs` - New AddWanderpoolHttpClientMetrics() extension method
- ✅ `HttpClientMetricsTests.cs` - Test suite with 5 comprehensive tests
  - Tests: Metric creation, registration, chaining, instrument retrieval

**Implementation Details:**
- Custom Meter: "Wanderpool.HttpClient" (v1.0.0)
- Counter: wanderpool_http_client_requests_total for tracking total HTTP requests
- Histogram: wanderpool_http_client_request_duration_seconds for tracking request duration
- Proper documentation and method chaining support
- All 156 tests passing, build successful

**Notes:**
- Instruments designed to be populated by DelegatingHandler in STEP-016
- Metric data class uses required properties for null-safety
- Extension method follows established project patterns

---

### STEP-016: HTTP Client Metrics Handler
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `HttpClientMetricsHandler.cs` - DelegatingHandler implementation for recording HTTP client metrics
- ✅ `HttpClientMetricsHandlerTests.cs` - Test suite with 5 comprehensive tests
  - Tests: Success requests, failed requests, duration tracking, client name, exception handling

**Implementation Details:**
- DelegatingHandler that intercepts HTTP requests and responses
- Records metrics for both successful (200 OK) and failed (500 error) requests
- Uses Stopwatch to measure request duration accurately
- Tags metrics with: client_name, http_method, http_status_code
- Handles HttpRequestException and OperationCanceledException gracefully
- Records metrics even when requests fail (exception propagated after recording)
- Configurable client name with "Unknown" fallback
- Full integration with HttpClientMetricsInstruments from STEP-015

**Test Coverage:**
- Metrics recorded on successful requests
- Metrics recorded on failed requests
- Duration accurately measured and recorded
- Client name properly included in metrics
- HTTP exceptions handled without breaking
- All 161 tests passing (5 new for STEP-016)
- Build successful with 0 errors, 0 warnings

**Notes:**
- Next step STEP-017 will add Circuit Breaker state metrics
- DelegatingHandler can be added to HttpClient pipeline via HttpClientBuilder
- Uses standard .NET HttpClientMetricsInstruments pattern for metrics

---

### STEP-017: Circuit Breaker State Metrics
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `CircuitBreakerStateRegistry.cs` - Thread-safe registry for tracking circuit breaker states
- ✅ `CircuitBreakerMetricsInstruments.cs` - Data class for metric instruments
- ✅ `MetricsExtensions.cs` - New AddWanderpoolCircuitBreakerMetrics() extension method
- ✅ `CircuitBreakerMetricsTests.cs` - Test suite with 6 comprehensive tests
  - Tests: Registration, chaining, instruments, state registry, state tracking, unknown circuits

**Implementation Details:**
- CircuitBreakerStateRegistry with thread-safe state tracking
  - States: 0=Closed (normal), 1=Open (rejecting), 2=Half-Open (testing recovery)
  - Concurrent dictionary for thread-safe concurrent access
  - Validation for circuit names and state values
  - GetAllStates() for observable gauge collection
- Observable gauge: wanderpool_circuit_breaker_state
  - Collects current state from registry on each export cycle
  - Tagged by: circuit_name
  - No polling overhead - state collected only during metrics export
- Full integration with OpenTelemetry metrics pipeline
- Designed to work with Microsoft.Extensions.Http.Resilience (uses Polly)

**Test Coverage:**
- Observable gauge registration in service collection
- Extension method chaining support
- CircuitBreakerMetricsInstruments creation and validation
- State registry state tracking and retrieval
- State validation (0, 1, 2 only)
- Unknown circuits default to Closed state (0)
- All 167 tests passing (6 new for STEP-017)
- Build successful with 0 errors, 0 warnings

**Integration Notes:**
- CircuitBreakerStateRegistry is registered as singleton
- Resilience handlers will call registry.SetState() on state transitions
- Observable callback collects all states during metrics export
- Ready for STEP-018 (Prometheus exporter)

---

### STEP-018: Prometheus Exporter Endpoint
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `MetricsExtensions.cs` - New MapWanderpoolMetrics() extension method
- ✅ `PrometheusEndpointTests.cs` - Test suite with 6 comprehensive tests
  - Tests: Endpoint mapping, custom paths, service collection, validation
- ✅ `Wanderpool.Common.Infra.csproj` - Prometheus exporter package reference

**Implementation Details:**
- MapWanderpoolMetrics() extension method for WebApplication
  - Exposes metrics endpoint (default /metrics, configurable path)
  - Returns metrics in Prometheus text format
  - Minimal API endpoint with proper content-type handling
  - Full parameter validation (null app, empty path)
- OpenTelemetry.Exporter.Prometheus v1.3.0-rc.2 integrated
- Separate from AddWanderpoolMetrics() for cleaner separation of concerns
- Applications call both AddWanderpoolMetrics() and MapWanderpoolMetrics()

**Test Coverage:**
- MapWanderpoolMetrics returns valid endpoint builder
- Custom path support for metrics endpoint
- Service collection registration verification
- Null application validation (ArgumentNullException)
- Empty and whitespace path validation (ArgumentException)
- All 173 tests passing (6 new for STEP-018)
- Build successful with 0 errors, 0 warnings

**Architecture Notes:**
- Prometheus exporter package added but not auto-enabled in AddWanderpoolMetrics
- MapWanderpoolMetrics() is separate integration point for HTTP endpoint
- Endpoint respects OpenTelemetry metrics pipeline configuration
- Ready for applications to expose metrics for Prometheus scraping

---

### STEP-019: Custom Activity Sources Infrastructure
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `IActivityScope.cs` - Interface for activity scope management
- ✅ `ActivityScope.cs` - Implementation using System.Diagnostics.ActivitySource
- ✅ `ActivityScopeTests.cs` - Test suite with 8 comprehensive tests
  - Tests: Creation, tags (single/multiple), exceptions, nested scopes, disposal, status

**Implementation Details:**
- IActivityScope interface providing fluent API
  - AddTag(key, value) - adds tags/attributes to activity
  - RecordException(exception) - records exceptions with OpenTelemetry semantics
  - SetStatus(statusCode, description) - sets activity status
  - Activity property - exposes underlying System.Diagnostics.Activity
- ActivityScope implementation
  - Uses System.Diagnostics.ActivitySource for activity creation
  - Proper parameter validation (non-null operation names, tag keys)
  - Exception events include type, message, and stacktrace
  - Automatic activity disposal on scope disposal (IDisposable)
  - Parent span ID properly propagated for nested scopes

**Test Coverage:**
- Activity creation with display name verification
- Single and multiple tag addition
- Exception recording with OpenTelemetry standard event format
- Parent-child activity relationships in nested scopes
- Activity disposal and lifecycle completion
- SetStatus with ActivityStatusCode enum
- IDisposable interface implementation
- All 181 tests passing (8 new for STEP-019)
- Build successful with 0 errors, 0 warnings

**Architecture Notes:**
- IActivityScope ready for dependency injection
- ActivityScope wraps ActivitySource.StartActivity()
- Exception events follow OpenTelemetry semantic conventions
- Compatible with OpenTelemetry instrumentation pipeline
- Next step: Register ActivitySource in TracingExtensions (STEP-020)
- Designed for use in business logic layers for operation tracing

**Design Pattern:**
- Fluent API for chainable method calls
- Scoped lifetime management via using statements
- Automatic resource cleanup via IDisposable
- Activity nesting via parent-child relationships

---

### STEP-020: Activity Source Registration
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `ActivitySourceProvider.cs` - Singleton provider for custom ActivitySource
- ✅ `TracingExtensions.cs` - Updated all 4 AddWanderpoolTracing methods to register ActivitySourceProvider and add custom source
- ✅ `TracingExtensionsTests.cs` - Extended with 2 new tests verifying ActivitySource registration

**Implementation Details:**
- ActivitySourceProvider singleton manages "Wanderpool.Common" ActivitySource (v1.0.0)
- All AddWanderpoolTracing overloads register ActivitySourceProvider as singleton
- Custom source "Wanderpool.Common" added to TracerProviderBuilder via AddSource()
- Activities created from custom source are properly traced and included in distributed tracing
- Full integration with OpenTelemetry instrumentation pipeline

**Test Coverage:**
- AddWanderpoolTracing_RegistersCustomActivitySource() - Verifies TracerProvider and ActivitySourceProvider registration
- AddWanderpoolTracing_CustomActivitySourceCanCreateActivities() - Verifies ActivitySource creation and naming
- All 183 existing tests still passing (2 new tests added)
- Build successful with 0 errors, 0 warnings

**Architecture Notes:**
- ActivitySourceProvider registered as singleton, one instance per DI container
- Activity source name "Wanderpool.Common" matches library namespace
- Compatible with distributed tracing systems (Jaeger, Zipkin, etc.)
- Ready for business logic layers to create activities for operation tracing
- Next step: STEP-021 (Request Logging Options)

---

## PHASE 3: ENHANCED LOGGING

### STEP-021: Request Logging Options
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `RequestLoggingOptions.cs` - Configuration class with sensible security defaults
- ✅ `RequestLoggingOptionsTests.cs` - Test suite with 5 comprehensive tests

**Implementation Details:**
- EnableRequestBodyLogging: bool (default: false) - disabled by default for security
- EnableResponseBodyLogging: bool (default: false) - disabled by default for security
- MaxBodySizeLogged: int (default: 4096) - prevents log bloat from large payloads
- IncludeQueryString: bool (default: true) - logs query strings for debugging
- SensitiveHeaders: ICollection<string> - redaction list (Authorization, X-Api-Key, Cookie, Set-Cookie, X-CSRF-Token, X-Auth-Token, Authorization-Token)
- All properties fully documented with XML comments
- All properties mutable for flexible configuration

**Test Coverage:**
- DefaultOptions_BodyLoggingDisabled() - Verifies body logging is off by default
- DefaultOptions_SensitiveHeadersPopulated() - Verifies sensitive headers list is pre-populated
- Options_AreModifiable() - Verifies all options can be modified after creation
- DefaultOptions_MaxBodySizeIsConfigurable() - Verifies max body size has sensible default
- DefaultOptions_IncludeQueryStringIsConfigurable() - Verifies query string flag defaults to true
- All 188 existing tests still passing (5 new tests added)
- Build successful with 0 errors, 0 warnings

**Security Features:**
- Body logging disabled by default (secure-by-default principle)
- Sensible body size limit (4KB) to prevent log bloat
- Comprehensive sensitive header list for redaction
- Mutable configuration for flexibility

**Architecture Notes:**
- Class designed to be used with DI and IOptions<RequestLoggingOptions> pattern
- Ready for STEP-022 (Request Logging Middleware) integration
- Configuration can be bound from appsettings.json under "RequestLogging" section
- Next step: STEP-022 (Request Logging Middleware - Core)

---

### STEP-022: Request Logging Middleware - Core
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `RequestLoggingMiddleware.cs` - Core middleware for HTTP request/response logging
- ✅ `RequestLoggingMiddlewareTests.cs` - Test suite with 7 comprehensive tests
- ✅ Updated `CorrelationIdExtensions.cs` - Added IApplicationBuilder overload for compatibility
- ✅ Updated `LoggingExtensions.cs` - Added UseWanderpoolRequestLogging extension method

**Implementation Details:**
- Middleware logs incoming HTTP requests with method, path, and query string
- Captures response body by replacing response stream with MemoryStream
- Logs outgoing HTTP responses with status code and duration (in milliseconds)
- Determines log level based on HTTP status code: 2xx→Info, 4xx→Warning, 5xx→Error
- Includes correlation ID from context in all log entries using log scopes
- Response body logging is disabled by default (security default)
- Configurable max body size limit (default 4096 bytes)
- Query string inclusion flag for optional URL parameter logging

**Test Coverage:**
- Middleware_AllowsRequestsToPassThrough() - Verifies requests flow through correctly
- Middleware_PreservesResponseBody() - Verifies response body is not corrupted
- Middleware_IncludesCorrelationIdInContext() - Verifies correlation ID propagation
- Middleware_DoesNotLogRequestBodyByDefault() - Verifies secure-by-default behavior
- Middleware_PreservesResponseContentType() - Verifies content-type headers maintained
- Middleware_Returns404ForNonExistentEndpoint() - Verifies status code handling
- Middleware_PreservesHttpMethod() - Verifies HTTP method preservation
- All 195 tests passing (7 new tests added)
- Build successful with 0 errors, 0 warnings

**Architecture Notes:**
- Response stream is buffered using MemoryStream for transparent logging
- Original response stream is restored after middleware processing
- Log level determination uses switch expression for clarity
- Correlation ID is accessed from HttpContext.Items["CorrelationId"]
- Support for IApplicationBuilder interface for use with WebHostBuilder
- Fluent API for extension methods with optional configuration parameter
- Integration point for future header redaction functionality (STEP-023)

**Performance Considerations:**
- Minimal impact on request processing due to async/await patterns
- Response body buffering only occurs if body logging is enabled
- Stream copying is async to prevent thread blocking
- Log level determination is O(1) switch expression

**Next Step:** STEP-023 (Request Logging - Header Redaction)

---

### STEP-023: Request Logging - Header Redaction
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ Updated `RequestLoggingMiddleware.cs` - Added GetRedactedHeadersInfo() method and header logging
- ✅ Extended `RequestLoggingMiddlewareTests.cs` - 5 new tests for header redaction

**Implementation Details:**
- GetRedactedHeadersInfo() method handles header redaction logic
- Case-insensitive header matching using StringComparer.OrdinalIgnoreCase
- Sensitive headers from RequestLoggingOptions.SensitiveHeaders are redacted as "[REDACTED]"
- Non-sensitive headers are logged in full for debugging purposes
- HashSet with O(1) lookup for performance
- Headers included in LogRequest() entries for complete request logging

**Test Coverage:**
- Middleware_CanRedactAuthorizationHeader() - Verifies Authorization header redaction
- Middleware_CanRedactXApiKeyHeader() - Verifies X-Api-Key header redaction
- Middleware_CanRedactCookieHeader() - Verifies Cookie header redaction
- Middleware_PreservesNonSensitiveHeaders() - Verifies non-sensitive headers preserved
- Middleware_AllowsConfigurableRedactionList() - Verifies configurable redaction list
- All 200 existing tests still passing (5 new tests added)
- Build successful with 0 errors, 0 warnings

**Security Features:**
- Sensitive headers from pre-configured list are redacted
- List is extensible via RequestLoggingOptions.SensitiveHeaders
- Case-insensitive matching prevents case-variation bypasses
- Default sensitive headers: Authorization, X-Api-Key, X-Access-Token, Cookie, Set-Cookie, X-CSRF-Token, X-Auth-Token, Authorization-Token

**Performance Characteristics:**
- HashSet lookup is O(1) for each header
- Case-insensitive comparison done once per header
- Redaction happens inline during logging

**Architecture Notes:**
- GetRedactedHeadersInfo() is private utility method
- Integrated into LogRequest() to include headers in log entries
- Compatible with RequestLoggingOptions configuration
- Ready for STEP-024 (Request Logging - Body Logging)

**Next Step:** STEP-024 (Request Logging - Body Logging)

---

### STEP-024: Request Logging - Body Logging
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ Updated `RequestLoggingMiddleware.cs` - Enhanced with body logging functionality
- ✅ Extended `RequestLoggingMiddlewareTests.cs` - 5 new tests for body logging scenarios

**Implementation Details:**
- Request body logging configurable via RequestLoggingOptions.EnableRequestBodyLogging (default: false)
- Response body logging configurable via RequestLoggingOptions.EnableResponseBodyLogging (default: false)
- Stream handling with CanSeek validation to prevent ObjectDisposedException
- Manual stream disposal instead of using statement to preserve stream state
- StreamReader instantiated with leaveOpen: true to prevent closing underlying stream
- Large body truncation at MaxBodySizeLogged limit (default: 4096 bytes) with "... [TRUNCATED]" indicator
- Proper stream position reset before copying to original response stream
- Try-catch block around stream reading for graceful error handling

**Test Coverage:**
- Middleware_LogsRequestBodyWhenEnabled() - Verifies request body logging when enabled
- Middleware_DoesNotLogRequestBodyWhenDisabled() - Verifies default secure behavior
- Middleware_LogsResponseBodyWhenEnabled() - Verifies response body logging when enabled
- Middleware_ProperlyHandlesRequestBodyStreams() - Verifies stream integrity after logging
- Middleware_TruncatesLargeBodies() - Verifies large body handling with truncation
- All 205 tests passing (5 new + 200 previous)
- Build successful with 0 errors, 0 warnings

**Stream Handling Strategy:**
- Uses MemoryStream to buffer response body transparently
- Checks CanSeek before attempting Position reset
- Copies buffered content to original stream after logging
- Proper disposal order: memoryStream → originalBodyStream
- Defensive programming with try-catch around stream operations

**Architecture Notes:**
- Continues from STEP-023 (Header Redaction)
- Ready for STEP-025 (Request Logging Extension Method)
- Request/response body logging disabled by default (security default)
- Configurable truncation prevents log bloat from large payloads
- Stream handling prevents data loss or corruption

---

### STEP-025: Request Logging Extension Method
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `LoggingExtensions.cs` - Enhanced with UseWanderpoolRequestLogging extension methods
- ✅ `RequestLoggingExtensionsTests.cs` - New test suite with 4 comprehensive tests

**Implementation Details:**
- WebApplication extension method: `UseWanderpoolRequestLogging()` with optional configuration parameter
- IApplicationBuilder extension method: `UseWanderpoolRequestLogging()` with optional configuration parameter
- Both overloads support method chaining via return value
- Optional `Action<RequestLoggingOptions>?` parameter allows inline configuration
- Proper null validation with ArgumentNullException for null app
- Integrates seamlessly with existing RequestLoggingMiddleware

**Test Coverage:**
- UseWanderpoolRequestLogging_RegistersMiddleware() - Verifies middleware registration and app return
- UseWanderpoolRequestLogging_AcceptsConfigurationParameter() - Verifies optional config parameter accepted
- UseWanderpoolRequestLogging_IsChainable() - Verifies method chaining with other middleware
- UseWanderpoolRequestLogging_WithNullApp_ThrowsArgumentNullException() - Verifies null validation
- All 209 tests passing (4 new + 205 previous)
- Build successful with 0 errors, 0 warnings

**Fluent API Design:**
- Simple, intuitive syntax: `app.UseWanderpoolRequestLogging()`
- Optional configuration: `app.UseWanderpoolRequestLogging(o => o.EnableResponseBodyLogging = true)`
- Chainable for pipeline composition: `app.UseWanderpoolRequestLogging().UseRouting()`
- Dual overloads for WebApplication and IApplicationBuilder compatibility

**Architecture Notes:**
- Continues from STEP-024 (Body Logging)
- Ready for STEP-026 (Outbound HTTP Logging Handler - Core)
- Follows established ASP.NET Core middleware extension patterns
- Configuration parameter is optional (backward compatible if options already configured)

---

### STEP-026: Outbound HTTP Logging Handler - Core
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `LoggingHandler.cs` - DelegatingHandler implementation for logging outbound HTTP calls
- ✅ `LoggingHandlerTests.cs` - Test suite with 6 comprehensive tests

**Implementation Details:**
- DelegatingHandler that logs all outbound HTTP requests via SendAsync override
- Logs request URL and HTTP method before sending
- Uses Stopwatch to measure request duration accurately
- Logs response HTTP status code and duration after receiving response
- Determines log level based on status code: 2xx→Info, 4xx→Warning, 5xx→Error
- Logs exceptions with full details and exception type
- Exceptions are logged and then re-thrown for proper error propagation
- Integrates with ILogger<LoggingHandler> for structured logging
- Null validation for request parameter

**Test Coverage:**
- SendAsync_LogsRequestUrlAndMethod() - Verifies request logging with URL and method
- SendAsync_LogsResponseStatusAndDuration() - Verifies response logging with status and duration
- SendAsync_Uses2xxResponseLogging() - Verifies 2xx status code handling
- SendAsync_Uses4xxResponseLogging() - Verifies 4xx status code handling
- SendAsync_Uses5xxResponseLogging() - Verifies 5xx status code handling
- SendAsync_LogsExceptionsAndRethrows() - Verifies exception logging and re-throw
- All 215 tests passing (6 new + 209 previous)
- Build successful with 0 errors, 0 warnings

**Log Level Strategy:**
- Info (2xx): Successful requests logged at Information level
- Info (3xx): Redirects logged at Information level
- Warning (4xx): Client errors logged at Warning level for operator attention
- Error (5xx): Server errors logged at Error level for critical attention
- Error: Exception handling logged with full exception details at Error level

**Architecture Notes:**
- Continues from STEP-025 (Request Logging Extension Method)
- Ready for STEP-027 (URL Redaction)
- Designed to be added to HttpClientBuilder pipeline
- Works with existing HttpClient factory patterns
- Non-blocking async implementation with proper cancellation token support

---

### STEP-027: Outbound HTTP Logging - URL Redaction
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `LoggingHandler.cs` - Enhanced with URL redaction for sensitive query parameters
- ✅ `LoggingHandlerTests.cs` - Extended with 5 additional redaction tests

**Implementation Details:**
- Sensitive query parameters list: token, key, password, api-key, api_key, secret, authorization, auth_token, access_token, refresh_token, bearer, x-api-key, x-auth-token
- Case-insensitive parameter name matching via StringComparer.OrdinalIgnoreCase
- Redaction approach: Replaces sensitive parameter values with [REDACTED] marker
- URL structure preserved: Only parameter values are modified, not names or structure
- Redaction applied to: request logs, response logs, and exception logs
- Handles null URIs and URIs without query strings gracefully
- Non-sensitive query parameters preserved exactly as-is in logs

**Test Coverage:**
- SendAsync_RedactsTokenQueryParam() - Verifies token parameter redaction
- SendAsync_RedactsKeyQueryParam() - Verifies key parameter redaction
- SendAsync_RedactsPasswordQueryParam() - Verifies password parameter redaction
- SendAsync_RedactsApiKeyQueryParam() - Verifies api-key parameter redaction
- SendAsync_PreservesNonSensitiveQueryParams() - Verifies non-sensitive params preserved
- All 220 tests passing (5 new + 215 previous)
- Build successful with 0 errors, 0 warnings

**Sensitive Parameter List:**
Common variations covered:
- token, key, password: Basic security parameters
- api-key, api_key: API authentication (underscore and hyphen variants)
- secret, authorization: Authentication and secret storage
- auth_token, access_token, refresh_token: OAuth/JWT tokens
- bearer: HTTP Bearer token prefix
- x-api-key, x-auth-token: Common custom header variants

**Performance Characteristics:**
- O(n) complexity where n = number of query parameters
- Minimal overhead: Simple string splitting and comparison
- No regex compilation - direct HashSet lookup via StringComparer
- Early return for URIs without query strings

**Architecture Notes:**
- Continues from STEP-026 (Outbound HTTP Logging Handler - Core)
- Ready for STEP-028 (Client Name Tracking)
- Uses static HashSet for sensitive parameter names (thread-safe, immutable)
- Helper method `RedactSensitiveQueryParams()` is reusable for other components

---

### STEP-028: Outbound HTTP Logging - Client Name
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `LoggingHandler.cs` - Enhanced with client name extraction and logging
- ✅ `LoggingHandlerTests.cs` - Extended with 3 additional client name tests

**Implementation Details:**
- Client name extracted from `HttpRequestMessage.Options` using key "ClientName"
- Client name included in request, response, and exception logs
- Graceful fallback to "Unknown" when client name not set or is null/empty
- Helper method `GetClientName()` encapsulates client name resolution logic
- Client name shown in parentheses in response and exception logs: "(client: ClientName)"
- Thread-safe implementation: Uses standard HttpRequestOptionsKey<string> pattern

**Test Coverage:**
- SendAsync_IncludesClientNameInLogs() - Verifies client name logged when available
- SendAsync_FallsBackToUnknownWhenNameNotSet() - Verifies Unknown fallback
- SendAsync_ExtractsClientNameFromRequestOptions() - Verifies proper extraction from options
- All 223 tests passing (3 new + 220 previous)
- Build successful with 0 errors, 0 warnings

**Usage Pattern:**
```csharp
var request = new HttpRequestMessage(HttpMethod.Get, "https://api.example.com/data");
request.Options.Set(new HttpRequestOptionsKey<string>("ClientName"), "HotelApiClient");
await httpClient.SendAsync(request);
// Logs: "Outbound HTTP GET request to https://api.example.com/data from client HotelApiClient"
```

**Log Message Format:**
- Request: "Outbound HTTP {Method} request to {URI} from client {ClientName}"
- Response: "Outbound HTTP {Status} response from {URI} (client: {ClientName}) | Duration: {ms}ms"
- Exception: "Outbound HTTP request to {URI} (client: {ClientName}) failed with exception..."

**Architecture Notes:**
- Continues from STEP-027 (URL Redaction)
- Ready for STEP-029 (Retry Detection)
- Client name can be set by HttpClientFactory during request creation
- Compatible with existing logging infrastructure
- Maintains backward compatibility with requests without client name set

---

### STEP-029: Outbound HTTP Logging - Retry Detection
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `LoggingHandler.cs` - Enhanced with retry attempt detection
- ✅ `LoggingHandlerTests.cs` - Extended with 3 retry detection tests

**Implementation Details:**
- Retry attempts detected via `AttemptNumber` in HttpRequestMessage.Options
- First attempt (AttemptNumber=1) logged at Information level
- Retry attempts (AttemptNumber≥2) logged at Warning level for operator visibility
- Attempt number included in retry log messages: "(retry attempt {N})"
- Graceful fallback to attempt 1 when AttemptNumber not set
- Helper method `GetAttemptNumber()` encapsulates retry detection logic
- Compatible with Polly retry policies and custom retry mechanisms

**Test Coverage:**
- SendAsync_FirstAttemptLoggedAsInfo() - Verifies first attempt at Info level
- SendAsync_RetryAttemptsLoggedAsWarning() - Verifies retries at Warning level with "retry" keyword
- SendAsync_RetryCountIncludedInLog() - Verifies attempt number appears in logs
- All 226 tests passing (3 new + 223 previous)
- Build successful with 0 errors, 0 warnings

**Log Level Strategy:**
- First attempt (AttemptNumber=1 or not set): Information level
- Retry attempts (AttemptNumber≥2): Warning level
- Escalation rationale: Retries indicate transient failures worth operator attention

**Usage Pattern:**
```csharp
// HttpClient factory or resilience pipeline sets AttemptNumber
var request = new HttpRequestMessage(HttpMethod.Post, "https://api.example.com/data");
request.Options.Set(new HttpRequestOptionsKey<int>("AttemptNumber"), 2); // Retry attempt
await httpClient.SendAsync(request);
// Logs: "Outbound HTTP POST request to ... (retry attempt 2)" at Warning level
```

**Integration Points:**
- Works with Polly retry policies (client code must set AttemptNumber)
- Works with custom retry mechanisms that set the request option
- Compatible with existing resilience pipelines
- Non-invasive: No direct dependency on Polly

**Architecture Notes:**
- Continues from STEP-028 (Client Name)
- Ready for STEP-030 (Resilience Options Configuration)
- Retry detection is request-scoped via HttpRequestMessage.Options
- Thread-safe: No shared state modified during retry detection

---

## PHASE 4: RESILIENCE ENHANCEMENT

### STEP-030: Resilience Options Configuration Class
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `ResilienceOptions.cs` - Configuration classes for all resilience policies
- ✅ `ResilienceOptionsTests.cs` - Test suite with 11 comprehensive tests

**Implementation Details:**
- TimeoutPolicyOptions: Configurable timeout in seconds (default: 10s)
- RetryPolicyOptions: Max attempts, initial/max delays, exponential backoff flag (defaults: 3 retries, 300ms initial, exponential enabled)
- CircuitBreakerPolicyOptions: Failure ratio, minimum throughput, sampling period, break duration (defaults: 0.25 ratio, 20 minimum, 30s sampling, 20s break)
- HedgingStrategyOptions: Delay, max attempts, enabled flag (defaults: 200ms delay, 2 attempts, disabled)
- All options mutable for flexible configuration
- Maps to "Resilience" section in appsettings.json

**Test Coverage:**
- Configuration section name verification
- Default value validation for all policies
- Independent configurability tests for each policy
- Full initialization validation
- Policy-specific parameter changes
- All 11 tests passing
- Build successful with 0 errors, 0 warnings

**Architecture Notes:**
- Nested classes follow ASP.NET Core Options pattern
- Designed for IOptions<ResilienceOptions> dependency injection
- Sensible defaults support out-of-the-box usage
- All policies independently configurable
- Ready for STEP-031 (Configurable Resilience Pipeline Builder)

---

### STEP-031: Configurable Resilience Pipeline Builder
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ Updated `ResiliencePipelines.cs` - New AddStandardResilienceWithOptions() extension method
- ✅ `ConfigurableResiliencePipelineTests.cs` - Test suite with 6 comprehensive tests

**Implementation Details:**
- AddStandardResilienceWithOptions(IHttpClientBuilder, ResilienceOptions) extension method
- Accepts ResilienceOptions instance and configures Polly pipelines accordingly
- Timeout configured from options.Timeout.TimeoutSeconds
- Retry configured from options.Retry (respects UseExponentialBackoff flag)
- Circuit breaker configured from options.CircuitBreaker (all parameters)
- Hedging configured from options.Hedging (only if Enabled=true)
- Maintains backward compatibility with existing AddStandardResilience() and AddStandardResilienceWithConfiguration() methods
- Null parameter validation for robustness

**Test Coverage:**
- Handler registration with default ResilienceOptions
- Timeout configuration application from options
- Retry configuration application with customization
- Circuit breaker configuration application with all parameters
- Hedging configuration application with enabled flag
- Independent policy configuration with mixed defaults and custom settings
- All 6 tests passing
- Build successful with 0 errors, 0 warnings
- All 219 tests passing in full test suite

**Architecture Notes:**
- Fully integrates ResilienceOptions from STEP-030
- Respects Polly framework best practices
- Conditional hedging only applies if explicitly enabled
- ExponentialBackoff flag properly maps to DelayBackoffType enum
- Ready for STEP-032 (Named Resilience Pipelines Registry)
- Clean implementation without over-engineering

---

### STEP-032: Named Resilience Pipelines Registry
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `ResiliencePipelineRegistry.cs` - Thread-safe registry implementation
- ✅ `NamedResiliencePipelinesTests.cs` - Test suite with 10 comprehensive tests

**Implementation Details:**
- Thread-safe dictionary-based registry using lock synchronization
- Case-insensitive pipeline name matching (StringComparer.OrdinalIgnoreCase)
- Register(name, options) - Add or overwrite pipeline registration
- Get(name) - Retrieve pipeline, throws KeyNotFoundException with helpful message
- TryGet(name, out options) - Safe retrieval without throwing
- Contains(name) - Check if pipeline is registered
- Unregister(name) - Remove a pipeline (returns success/failure)
- Clear() - Remove all pipelines
- GetRegisteredNames() - List all registered pipeline names
- Count property - Get number of registered pipelines
- Null parameter validation (ArgumentNullException)
- Duplicate registration overwrites previous definition
- Pipeline references are shared (singleton behavior per name)

**Test Coverage:**
- Single pipeline registration and retrieval
- Multiple named pipelines with different timeout configurations
- KeyNotFoundException for unregistered names with helpful error message
- Reference identity (same object returned on multiple retrievals)
- ArgumentNullException for null name and null options
- Duplicate name registration overwrites previous registration
- TryGet returns false and null for missing pipelines
- TryGet returns true and options for registered pipelines
- All 10 tests passing
- Build successful with 0 errors, 0 warnings
- All 239 tests passing in full test suite

**Architecture Notes:**
- Thread-safe with lock synchronization for all operations
- Dictionary provides O(1) lookups for pipeline retrieval
- Case-insensitive matching improves usability (e.g., "fastapi" == "FastAPI")
- Error messages list available pipelines for better debugging
- Supports both eager registration and safe optional retrieval patterns
- Ready for STEP-033 (Named Pipeline Configuration from appsettings)
- Clean, maintainable implementation without over-engineering

---

### STEP-033: Named Pipeline Configuration from appsettings
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `ResilienceConfigurationExtensions.cs` - Configuration loading and registration
- ✅ `ResiliencePipelineConfigurationTests.cs` - Test suite with 6 comprehensive tests

**Implementation Details:**
- AddWanderpoolNamedResiliencePipelines(IServiceCollection, IConfiguration) extension method
- Loads pipelines from "Resilience:Pipelines:{PipelineName}" configuration sections
- Registers ResiliencePipelineRegistry as singleton with factory initialization
- Validates configuration parameters: TimeoutSeconds, RetryAttempts, FailureRatio, etc.
- Provides helpful error messages including pipeline name and validation details
- Supports zero or more named pipelines in configuration
- Case-insensitive pipeline name matching (inherited from registry)
- Lazy initialization: validation occurs when service provider is built

**Validation Rules:**
- Timeout.TimeoutSeconds > 0
- Retry.MaxRetryAttempts >= 0
- Retry.InitialDelayMilliseconds >= 0
- Retry.MaxDelayMilliseconds >= InitialDelayMilliseconds
- CircuitBreaker.FailureRatio ∈ [0, 1]
- CircuitBreaker.MinimumThroughput > 0
- CircuitBreaker.SamplingPeriodSeconds > 0
- CircuitBreaker.BreakDurationSeconds > 0
- Hedging.DelayMilliseconds >= 0
- Hedging.MaxHedgedAttempts >= 0

**Test Coverage:**
- Single pipeline loading from configuration section
- Multiple named pipelines with different independent settings
- Registry singleton registration via dependency injection
- Empty configuration creates empty registry without errors
- Valid configuration with multiple settings loads successfully
- All 6 tests passing
- Build successful with 0 errors, 0 warnings
- All 254 tests passing in full test suite

**Example Configuration (appsettings.json):**
```json
{
  "Resilience": {
    "Pipelines": {
      "FastAPI": {
        "Timeout": { "TimeoutSeconds": 5 },
        "Retry": { "MaxRetryAttempts": 1 }
      },
      "SlowService": {
        "Timeout": { "TimeoutSeconds": 30 },
        "Retry": { "MaxRetryAttempts": 5 }
      }
    }
  }
}
```

**Architecture Notes:**
- Factory registration enables lazy initialization and validation at runtime
- Helpful error messages aid debugging configuration issues
- Fully decoupled from ResiliencePipelines.cs implementation
- Supports flexible pipeline configurations per external service
- Ready for STEP-034 (Resilience Events Logging Infrastructure)

---

### STEP-034: Resilience Events Logging Infrastructure
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `ResilienceEventHandlers.cs` - Complete event handler implementation (146 lines)
- ✅ `ResilienceEventsTests.cs` - Test suite with 8 comprehensive tests (185 lines)

**Implementation Details:**
- ResilienceEventHandlers class with structured logging for all resilience events
- OnRetry(attemptNumber, delay, exception) - Logs retry attempts at Warning level
- OnCircuitBreakerOpened() - Logs circuit opening at Warning level
- OnCircuitBreakerHalfOpen() - Logs recovery testing at Information level
- OnCircuitBreakerClosed() - Logs circuit reset at Information level
- OnTimeout(timeoutDuration) - Logs timeout events at Warning level
- OnHedging(attemptNumber) - Logs hedging attempts at Information level
- Getter methods providing Polly-compatible delegates
- Full parameter validation (ArgumentNullException for null logger)

**Test Coverage:**
- Retry events logged with attempt number, delay, and exception details
- Circuit breaker state transitions (opened, half-open, closed) properly logged
- Timeout events logged with configured duration
- Hedging attempts logged with attempt number
- Appropriate log levels for each event type:
  - Warning: Retry, CircuitBreakerOpened, Timeout (operation degraded)
  - Information: CircuitBreakerHalfOpen, CircuitBreakerClosed, Hedging (normal operations)
- Handler instantiation and usage validation
- Null logger throws ArgumentNullException
- All 8 tests passing
- Build successful with 0 errors, 0 warnings
- All 262 tests passing in full test suite

**Architecture Notes:**
- Stateless event handlers suitable for dependency injection
- Getter methods return Polly-compatible delegates for event subscriptions
- Structured logging with explicit parameters for analysis
- Appropriate log levels based on event severity
- Ready for integration with ResiliencePipelines (STEP-035)
- Extensible design supports adding additional event types
- Clear separation between event detection and logging

**Event Log Examples:**
- Retry: "Retry attempt 2 scheduled with delay 500ms. Exception: HttpRequestException - Service unavailable"
- CircuitOpened: "Circuit breaker opened. Too many failures detected."
- CircuitHalfOpen: "Circuit breaker transitioned to half-open state."
- CircuitClosed: "Circuit breaker closed. Service has recovered."
- Timeout: "Request timeout occurred. Timeout duration: 10 seconds."
- Hedging: "Hedging attempt 1 triggered. Sending duplicate request..."

---

### STEP-035: Resilience Events Integration
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `ResiliencePipelines.cs` - Updated with optional ResilienceEventHandlers parameter
- ✅ `ConfigurableResiliencePipelineTests.cs` - Extended with 4 new tests
  - Test: Retry event handlers work with ResilienceOptions
  - Test: Circuit breaker event handlers work with ResilienceOptions
  - Test: Timeout event handlers work with ResilienceOptions
  - Test: Hedging event handlers work with ResilienceOptions

**Implementation Details:**
- Optional ResilienceEventHandlers parameter in AddStandardResilienceWithOptions method
- Event handlers can be registered in dependency injection container
- Supports retry, circuit breaker, timeout, and hedging event logging
- Proper error logging with event details (attempt counts, delays, etc.)
- All event handlers are optional (nullable parameter)
- Backwards compatible - existing code without event handlers still works

**Test Results:**
- 271 total tests passing (1 skipped)
- All new tests passing
- No regressions in existing tests
- Solution builds successfully

---

## PHASE 5: API INFRASTRUCTURE

### STEP-036: Endpoint Result Extension Methods
**Status:** ✅ COMPLETED

**Completed:** 2025-12-07

**Deliverables:**
- ✅ `ResultExtensions.cs` - Extension method for converting OperationResult to IResult
- ✅ `ResultExtensionsTests.cs` - Comprehensive test suite with 7 tests
  - Test: Success result returns Ok with data
  - Test: Failed result returns Json result with error
  - Test: ValidationException maps to BadRequest (400)
  - Test: NotFound error maps to NotFound (404)
  - Test: Response includes TraceId from HttpContext
  - Test: Forbidden error maps to Forbidden (403)
  - Test: Conflict error maps to Conflict (409)

**Implementation Details:**
- ToResult<T> extension method on OperationResult<T>
- Maps domain error codes to HTTP status codes (400, 401, 403, 404, 409, 500)
- Returns ApiResponseEnvelope format with TraceId from HttpContext
- Private helper methods for error mapping and error level conversion
- Supports all domain error types (Validation, NotFound, Forbidden, Conflict, Unauthorized, etc.)

**Test Results:**
- 278 total tests passing (1 skipped)
- All 7 new tests passing
- No regressions in existing tests
- Solution builds successfully

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