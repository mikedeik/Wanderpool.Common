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

**Status:** PENDING

### STEP-014: OpenTelemetry Metrics - Basic Setup

**Status:** PENDING

### STEP-015: Custom HTTP Client Metrics

**Status:** PENDING

### STEP-016: HTTP Client Metrics Handler

**Status:** PENDING

### STEP-017: Circuit Breaker State Metrics

**Status:** PENDING

### STEP-018: Prometheus Exporter Endpoint

**Status:** PENDING

### STEP-019: Custom Activity Sources Infrastructure

**Status:** PENDING

### STEP-020: Activity Source Registration

**Status:** PENDING

---

## PHASE 3: ENHANCED LOGGING

### STEP-021: Request Logging Options

**Status:** PENDING

### STEP-022: Request Logging Middleware - Core

**Status:** PENDING

### STEP-023: Request Logging - Header Redaction

**Status:** PENDING

### STEP-024: Request Logging - Body Logging

**Status:** PENDING

### STEP-025: Request Logging Extension Method

**Status:** PENDING

### STEP-026: Outbound HTTP Logging Handler - Core

**Status:** PENDING

### STEP-027: Outbound HTTP Logging - URL Redaction

**Status:** PENDING

### STEP-028: Outbound HTTP Logging - Client Name

**Status:** PENDING

### STEP-029: Outbound HTTP Logging - Retry Detection

**Status:** PENDING

---

## PHASE 4: RESILIENCE ENHANCEMENT

### STEP-030: Resilience Options Configuration Class

**Status:** PENDING

### STEP-031: Configurable Resilience Pipeline Builder

**Status:** PENDING

### STEP-032: Named Resilience Pipelines Registry

**Status:** PENDING

### STEP-033: Named Pipeline Configuration from appsettings

**Status:** PENDING

### STEP-034: Resilience Events Logging Infrastructure

**Status:** PENDING

### STEP-035: Resilience Events Integration

**Status:** PENDING

---

## PHASE 5: API INFRASTRUCTURE

### STEP-036: Endpoint Result Extension Methods

**Status:** PENDING

### STEP-037: Endpoint Response Helper Methods

**Status:** PENDING

### STEP-038: FluentValidation Endpoint Filter - Core

**Status:** PENDING

### STEP-039: Validation Filter Extension Method

**Status:** PENDING

### STEP-040: Endpoint Group Extensions - Prefixing

**Status:** PENDING

### STEP-041: Endpoint Group Extensions - Filters and Metadata

**Status:** PENDING

### STEP-042: Health Checks Infrastructure

**Status:** PENDING

### STEP-043: Health Check Endpoints

**Status:** PENDING

---

## PHASE 6: INTEGRATION

### STEP-044: HTTP Client Factory Extensions - Basic

**Status:** PENDING

### STEP-045: HTTP Client Factory Extensions - Options

**Status:** PENDING

### STEP-046: HTTP Client Factory Extensions - Token Provider Integration

**Status:** PENDING

### STEP-047: Unified Infrastructure Service Registration

**Status:** PENDING

### STEP-048: Unified Infrastructure Options

**Status:** PENDING

### STEP-049: Unified Middleware Pipeline

**Status:** PENDING

### STEP-050: Configuration Validation

**Status:** PENDING

### STEP-051: Problem Details Support - Mapper

**Status:** PENDING

### STEP-052: Problem Details Support - Integration

**Status:** PENDING

### STEP-053: Integration Testing Infrastructure

**Status:** PENDING

### STEP-054: Documentation and Examples

**Status:** PENDING

### STEP-055: NuGet Package Preparation

**Status:** PENDING

---

## Summary

- **Completed:** 12/55 steps
- **In Progress:** 0/55 steps
- **Pending:** 43/55 steps
- **Completion Percentage:** 22%

## Next Steps

1. STEP-013: OpenTelemetry Tracing - Enrichment
2. STEP-014: OpenTelemetry Metrics - Basic Setup
3. STEP-015: Custom HTTP Client Metrics

---

## Notes

- All projects configured for .NET 6.0 target framework
- Testing framework: xUnit with FluentAssertions
- TDD approach strictly followed: Red-Green-Refactor
- Test coverage target: >85% for complex features, >90% for critical paths
