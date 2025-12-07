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