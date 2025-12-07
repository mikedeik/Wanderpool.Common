using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Wanderpool.Common.Contracts.ApiResponse;
using Wanderpool.Common.Contracts.Operations;
using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Api;

/// <summary>
/// Static helper class for creating standard API responses.
/// Provides convenient methods for common response scenarios (Ok, Created, NoContent, BadRequest, NotFound).
/// </summary>
public static class ApiResponse
{
    /// <summary>
    /// Creates a 200 OK response with data.
    /// </summary>
    /// <typeparam name="T">The type of data to return.</typeparam>
    /// <param name="data">The data to include in the response.</param>
    /// <param name="httpContext">The HTTP context for including TraceId.</param>
    /// <returns>An Ok IResult with ApiResponseEnvelope.</returns>
    public static IResult Ok<T>(T data, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        var response = new ApiResponseEnvelope<T>
        {
            IsSuccess = true,
            Data = data,
            TraceId = httpContext.TraceIdentifier,
            Error = null
        };

        return Results.Ok(response);
    }

    /// <summary>
    /// Creates a 201 Created response with data and location URI.
    /// </summary>
    /// <typeparam name="T">The type of data to return.</typeparam>
    /// <param name="data">The created resource data.</param>
    /// <param name="location">The URI of the created resource.</param>
    /// <param name="httpContext">The HTTP context for including TraceId.</param>
    /// <returns>A Created IResult with ApiResponseEnvelope.</returns>
    public static IResult Created<T>(T data, string location, HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);
        ArgumentNullException.ThrowIfNull(location);

        var response = new ApiResponseEnvelope<T>
        {
            IsSuccess = true,
            Data = data,
            TraceId = httpContext.TraceIdentifier,
            Error = null
        };

        return Results.Created(location, response);
    }

    /// <summary>
    /// Creates a 204 No Content response (no body).
    /// </summary>
    /// <returns>A NoContent IResult.</returns>
    public static IResult NoContent()
    {
        return Results.NoContent();
    }

    /// <summary>
    /// Creates a 400 Bad Request response with error details.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <param name="httpContext">The HTTP context for including TraceId.</param>
    /// <returns>A Json IResult with error details.</returns>
    public static IResult BadRequest(
        OperationResultError error,
        HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(httpContext);

        var response = new ApiResponseEnvelope<object>
        {
            IsSuccess = false,
            Data = null,
            TraceId = httpContext.TraceIdentifier,
            Error = new ApiResponseError
            {
                Code = error.Code,
                Message = error.Message,
                Level = MapErrorLevel(error.Level)
            }
        };

        return Results.Json(response, statusCode: StatusCodes.Status400BadRequest);
    }

    /// <summary>
    /// Creates a 404 Not Found response with error details.
    /// </summary>
    /// <param name="error">The error that occurred.</param>
    /// <param name="httpContext">The HTTP context for including TraceId.</param>
    /// <returns>A Json IResult with error details.</returns>
    public static IResult NotFound(
        OperationResultError error,
        HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(error);
        ArgumentNullException.ThrowIfNull(httpContext);

        var response = new ApiResponseEnvelope<object>
        {
            IsSuccess = false,
            Data = null,
            TraceId = httpContext.TraceIdentifier,
            Error = new ApiResponseError
            {
                Code = error.Code,
                Message = error.Message,
                Level = MapErrorLevel(error.Level)
            }
        };

        return Results.Json(response, statusCode: StatusCodes.Status404NotFound);
    }

    /// <summary>
    /// Maps operation result error levels to API response error levels.
    /// </summary>
    private static ApiResponseErrorLevel MapErrorLevel(OperationResultErrorLevel level)
    {
        return level switch
        {
            OperationResultErrorLevel.Info => ApiResponseErrorLevel.Info,
            OperationResultErrorLevel.Warning => ApiResponseErrorLevel.Warning,
            OperationResultErrorLevel.Error => ApiResponseErrorLevel.Error,
            _ => ApiResponseErrorLevel.Error
        };
    }
}

/// <summary>
/// Extension methods for converting OperationResult to IResult for ASP.NET Core endpoints.
/// Provides convenient mapping from domain operations to HTTP responses.
/// </summary>
public static class ResultExtensions
{
    /// <summary>
    /// Converts an OperationResult to an ASP.NET Core IResult.
    /// Maps operation errors to appropriate HTTP status codes and includes TraceId.
    /// </summary>
    /// <typeparam name="T">The type of data in the operation result.</typeparam>
    /// <param name="result">The operation result to convert.</param>
    /// <param name="httpContext">The HTTP context for including TraceId.</param>
    /// <returns>An IResult for returning from an endpoint.</returns>
    public static IResult ToResult<T>(
        this OperationResult<T> result,
        HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(result);
        ArgumentNullException.ThrowIfNull(httpContext);

        if (result.IsSuccess)
        {
            var successResponse = new ApiResponseEnvelope<T>
            {
                IsSuccess = true,
                Data = result.Value,
                TraceId = httpContext.TraceIdentifier,
                Error = null
            };
            return Results.Ok(successResponse);
        }

        // Handle failure case
        var error = result.Error;
        if (error == null)
        {
            // Shouldn't happen, but handle gracefully
            return Results.StatusCode(StatusCodes.Status500InternalServerError);
        }

        var statusCode = MapErrorToStatusCode(error.Code);
        var errorResponse = new ApiResponseEnvelope<T>
        {
            IsSuccess = false,
            Data = default,
            TraceId = httpContext.TraceIdentifier,
            Error = new ApiResponseError
            {
                Code = error.Code,
                Message = error.Message,
                Level = MapErrorLevel(error.Level)
            }
        };

        return Results.Json(errorResponse, statusCode: statusCode);
    }

    /// <summary>
    /// Maps error codes to HTTP status codes.
    /// </summary>
    /// <param name="errorCode">The error code from the operation result.</param>
    /// <returns>The appropriate HTTP status code.</returns>
    private static int MapErrorToStatusCode(string errorCode)
    {
        return errorCode switch
        {
            "VALIDATION_ERROR" => StatusCodes.Status400BadRequest,
            "NOT_FOUND" => StatusCodes.Status404NotFound,
            "FORBIDDEN" => StatusCodes.Status403Forbidden,
            "CONFLICT" => StatusCodes.Status409Conflict,
            "UNAUTHORIZED" => StatusCodes.Status401Unauthorized,
            "BUSINESS_RULE_VIOLATION" => StatusCodes.Status400BadRequest,
            _ => StatusCodes.Status500InternalServerError
        };
    }

    /// <summary>
    /// Maps operation result error levels to API response error levels.
    /// </summary>
    /// <param name="level">The operation result error level.</param>
    /// <returns>The corresponding API response error level.</returns>
    private static ApiResponseErrorLevel MapErrorLevel(OperationResultErrorLevel level)
    {
        return level switch
        {
            OperationResultErrorLevel.Info => ApiResponseErrorLevel.Info,
            OperationResultErrorLevel.Warning => ApiResponseErrorLevel.Warning,
            OperationResultErrorLevel.Error => ApiResponseErrorLevel.Error,
            _ => ApiResponseErrorLevel.Error
        };
    }
}
