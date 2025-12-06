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

**Status:** PENDING

### STEP-008: Correlation ID Context Service

**Status:** PENDING

### STEP-009: Correlation ID Middleware

**Status:** PENDING

### STEP-010: Correlation ID Serilog Enricher

**Status:** PENDING

---

## PHASE 2: TELEMETRY

### STEP-011: OpenTelemetry Tracing - Basic Setup

**Status:** PENDING

### STEP-012: OpenTelemetry Tracing - Exporters

**Status:** PENDING

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

- **Completed:** 6/55 steps
- **In Progress:** 0/55 steps
- **Pending:** 49/55 steps
- **Completion Percentage:** 10.9%

## Next Steps

1. STEP-007: Serilog Configuration Infrastructure
2. STEP-008: Correlation ID Context Service
3. STEP-009: Correlation ID Middleware

---

## Notes

- All projects configured for .NET 6.0 target framework
- Testing framework: xUnit with FluentAssertions
- TDD approach strictly followed: Red-Green-Refactor
- Test coverage target: >85% for complex features, >90% for critical paths
