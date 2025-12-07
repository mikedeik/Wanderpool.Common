using Microsoft.AspNetCore.Http;
using Wanderpool.Common.Contracts.ApiResponse;
using Wanderpool.Common.Contracts.Operations;
using Wanderpool.Common.Infra.Exceptions;

namespace Wanderpool.Common.Infra.Api;

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
