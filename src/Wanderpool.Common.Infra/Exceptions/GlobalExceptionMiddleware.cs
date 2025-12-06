using System.Diagnostics;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Wanderpool.Common.Infra.Clients.Exceptions;
using Wanderpool.Common.Contracts.ApiResponse;

namespace Wanderpool.Common.Infra.Exceptions;

/// <summary>
/// Middleware for handling all unhandled exceptions and returning consistent API responses.
/// Maps exceptions to appropriate HTTP status codes and error formats.
/// Supports production mode to hide detailed error information.
/// </summary>
public class GlobalExceptionMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionMiddleware> _logger;
    private readonly IWebHostEnvironment _environment;

    public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger, IWebHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// Invokes the middleware to process the request and handle any exceptions.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception exception)
        {
            await HandleExceptionAsync(context, exception);
        }
    }

    /// <summary>
    /// Handles the exception by mapping it to an appropriate HTTP response.
    /// </summary>
    /// <param name="context">The HTTP context for the current request.</param>
    /// <param name="exception">The exception that was thrown.</param>
    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var response = context.Response;
        response.ContentType = "application/json";

        var traceId = Activity.Current?.Id ?? context.TraceIdentifier;

        // Log the exception
        _logger.LogError(exception, "Unhandled exception occurred. TraceId: {TraceId}", traceId);

        // Map exception to appropriate status code and error response
        var (statusCode, errorCode, message) = MapExceptionToResponse(exception);

        // In production, hide detailed error messages for 5xx errors
        if (_environment.IsProduction() && statusCode >= StatusCodes.Status500InternalServerError)
        {
            message = "An unexpected error occurred. Please try again later.";
        }

        response.StatusCode = statusCode;

        var errorResponse = new ApiResponseEnvelope<object>
        {
            IsSuccess = false,
            Data = null,
            TraceId = traceId,
            Error = new ApiResponseError
            {
                Code = errorCode,
                Message = message,
                Level = GetErrorLevel(statusCode)
            }
        };

        await response.WriteAsJsonAsync(errorResponse);
    }

    /// <summary>
    /// Maps an exception to appropriate HTTP status code and error code.
    /// </summary>
    /// <param name="exception">The exception to map.</param>
    /// <returns>A tuple of (statusCode, errorCode, message).</returns>
    private static (int statusCode, string errorCode, string message) MapExceptionToResponse(Exception exception)
    {
        return exception switch
        {
            ValidationException validationEx => (
                StatusCodes.Status400BadRequest,
                validationEx.ErrorCode,
                validationEx.Message
            ),
            UnauthorizedAccessException unauthorizedEx => (
                StatusCodes.Status401Unauthorized,
                "UNAUTHORIZED",
                unauthorizedEx.Message ?? "Unauthorized access."
            ),
            ForbiddenException forbiddenEx => (
                StatusCodes.Status403Forbidden,
                forbiddenEx.ErrorCode,
                forbiddenEx.Message
            ),
            NotFoundException notFoundEx => (
                StatusCodes.Status404NotFound,
                notFoundEx.ErrorCode,
                notFoundEx.Message
            ),
            ConflictException conflictEx => (
                StatusCodes.Status409Conflict,
                conflictEx.ErrorCode,
                conflictEx.Message
            ),
            OperationCanceledException cancelledEx => (
                StatusCodes.Status499ClientClosedRequest,
                "CLIENT_CLOSED",
                cancelledEx.Message ?? "The operation was cancelled by the client."
            ),
            RemoteServiceException remoteServiceEx => (
                StatusCodes.Status502BadGateway,
                "UPSTREAM_ERROR",
                remoteServiceEx.Message ?? "Remote service call failed."
            ),
            WanderpoolException wanderpoolEx => (
                StatusCodes.Status500InternalServerError,
                wanderpoolEx.ErrorCode,
                wanderpoolEx.Message
            ),
            _ => (
                StatusCodes.Status500InternalServerError,
                "INTERNAL_ERROR",
                "An unexpected error occurred. Please try again later."
            )
        };
    }

    /// <summary>
    /// Determines the error level based on HTTP status code.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <returns>The appropriate error level.</returns>
    private static ApiResponseErrorLevel GetErrorLevel(int statusCode)
    {
        return statusCode switch
        {
            >= 500 => ApiResponseErrorLevel.Error,
            >= 400 => ApiResponseErrorLevel.Warning,
            _ => ApiResponseErrorLevel.Info
        };
    }
}
