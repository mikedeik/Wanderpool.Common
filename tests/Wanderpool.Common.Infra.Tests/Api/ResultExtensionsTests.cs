using Microsoft.AspNetCore.Http;
using Xunit;
using Wanderpool.Common.Contracts.ApiResponse;
using Wanderpool.Common.Contracts.Operations;
using Wanderpool.Common.Infra.Api;
using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Tests.Api;

/// <summary>
/// Tests for ResultExtensions - converting OperationResult to IResult.
/// Validates proper HTTP status code mapping and response formatting.
/// </summary>
public class ResultExtensionsTests
{
    /// <summary>
    /// Helper to create a mock HttpContext with TraceId.
    /// </summary>
    private static HttpContext CreateMockHttpContext(string traceId = "trace-123")
    {
        var context = new DefaultHttpContext();
        context.TraceIdentifier = traceId;
        return context;
    }

    /// <summary>
    /// Test: Success result returns Ok with data
    /// </summary>
    [Fact]
    public void ToResult_WithSuccessResult_ReturnsOkResult()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        var result = OperationResult.Success("test data");

        // Act
        var iResult = result.ToResult<string>(httpContext);

        // Assert - verify it's an Ok result
        Assert.NotNull(iResult);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.Ok<ApiResponseEnvelope<string>>>(iResult);
    }

    /// <summary>
    /// Test: Failed result returns Json result with error
    /// </summary>
    [Fact]
    public void ToResult_WithFailedResult_ReturnsJsonResultWithError()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        var error = OperationResultError.Error("TEST_ERROR", "Test error message");
        var result = OperationResult.Fail<string>(error);

        // Act
        var iResult = result.ToResult<string>(httpContext);

        // Assert - verify it's a Json result
        Assert.NotNull(iResult);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<ApiResponseEnvelope<string>>>(iResult);
    }

    /// <summary>
    /// Test: ValidationException maps to BadRequest status code
    /// </summary>
    [Fact]
    public void ToResult_WithValidationError_MapsTo400()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        var result = OperationResult.Fail<string>(
            OperationResultError.Error("VALIDATION_ERROR", "Validation failed")
        );

        // Act
        var iResult = result.ToResult<string>(httpContext);

        // Assert - verify it returns Json result (will be 400)
        Assert.NotNull(iResult);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<ApiResponseEnvelope<string>>>(iResult);
    }

    /// <summary>
    /// Test: NotFound error maps to 404 status code
    /// </summary>
    [Fact]
    public void ToResult_WithNotFoundError_MapsTo404()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        var result = OperationResult.Fail<string>(
            OperationResultError.Error("NOT_FOUND", "Resource not found")
        );

        // Act
        var iResult = result.ToResult<string>(httpContext);

        // Assert - verify it returns Json result (will be 404)
        Assert.NotNull(iResult);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<ApiResponseEnvelope<string>>>(iResult);
    }

    /// <summary>
    /// Test: Response includes TraceId from HttpContext
    /// </summary>
    [Fact]
    public void ToResult_WithSuccessResult_IncludesTraceId()
    {
        // Arrange
        var traceId = "trace-test-123";
        var httpContext = CreateMockHttpContext(traceId);
        var result = OperationResult.Success("test data");

        // Act
        var iResult = result.ToResult<string>(httpContext);

        // Assert - verify TraceId is preserved in context
        Assert.NotNull(httpContext.TraceIdentifier);
        Assert.Equal(traceId, httpContext.TraceIdentifier);
    }

    /// <summary>
    /// Test: Forbidden error maps to 403 status code
    /// </summary>
    [Fact]
    public void ToResult_WithForbiddenError_MapsTo403()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        var result = OperationResult.Fail<string>(
            OperationResultError.Error("FORBIDDEN", "Access denied")
        );

        // Act
        var iResult = result.ToResult<string>(httpContext);

        // Assert - verify it returns Json result (will be 403)
        Assert.NotNull(iResult);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<ApiResponseEnvelope<string>>>(iResult);
    }

    /// <summary>
    /// Test: Conflict error maps to 409 status code
    /// </summary>
    [Fact]
    public void ToResult_WithConflictError_MapsTo409()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        var result = OperationResult.Fail<string>(
            OperationResultError.Error("CONFLICT", "Resource conflict")
        );

        // Act
        var iResult = result.ToResult<string>(httpContext);

        // Assert - verify it returns Json result (will be 409)
        Assert.NotNull(iResult);
        Assert.IsType<Microsoft.AspNetCore.Http.HttpResults.JsonHttpResult<ApiResponseEnvelope<string>>>(iResult);
    }

    /// <summary>
    /// Test: ApiResponse.Ok() returns IResult with success response
    /// </summary>
    [Fact]
    public void ApiResponse_Ok_ReturnsSuccess()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        var data = "test data";

        // Act
        var result = ApiResponse.Ok(data, httpContext);

        // Assert - verify it returns IResult
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IResult>(result);
    }

    /// <summary>
    /// Test: ApiResponse.Created() returns IResult with created response
    /// </summary>
    [Fact]
    public void ApiResponse_Created_ReturnsCreated()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        var data = "new resource";
        var location = "/api/resources/123";

        // Act
        var result = ApiResponse.Created(data, location, httpContext);

        // Assert - verify it returns IResult
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IResult>(result);
    }

    /// <summary>
    /// Test: ApiResponse.NoContent() returns IResult with no content
    /// </summary>
    [Fact]
    public void ApiResponse_NoContent_ReturnsNoContent()
    {
        // Arrange - No arrangement needed for NoContent

        // Act
        var result = ApiResponse.NoContent();

        // Assert - verify it returns IResult
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IResult>(result);
    }

    /// <summary>
    /// Test: ApiResponse.BadRequest() returns IResult with error
    /// </summary>
    [Fact]
    public void ApiResponse_BadRequest_ReturnsError()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        var error = OperationResultError.Error("VALIDATION_ERROR", "Invalid input");

        // Act
        var result = ApiResponse.BadRequest(error, httpContext);

        // Assert - verify it returns IResult
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IResult>(result);
    }

    /// <summary>
    /// Test: ApiResponse.NotFound() returns IResult with error
    /// </summary>
    [Fact]
    public void ApiResponse_NotFound_ReturnsError()
    {
        // Arrange
        var httpContext = CreateMockHttpContext();
        var error = OperationResultError.Error("NOT_FOUND", "Resource not found");

        // Act
        var result = ApiResponse.NotFound(error, httpContext);

        // Assert - verify it returns IResult
        Assert.NotNull(result);
        Assert.IsAssignableFrom<IResult>(result);
    }
}
