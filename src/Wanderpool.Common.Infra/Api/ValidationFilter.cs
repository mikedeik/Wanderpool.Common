using FluentValidation;
using Microsoft.AspNetCore.Http;
using Wanderpool.Common.Contracts.Operations;

namespace Wanderpool.Common.Infra.Api;

/// <summary>
/// Endpoint filter for automatic request validation using FluentValidation.
/// Validates incoming requests and returns 400 Bad Request if validation fails.
/// </summary>
/// <typeparam name="T">The request type to validate.</typeparam>
public class ValidationFilter<T> : IEndpointFilter
{
    /// <summary>
    /// Invokes the validation filter on the endpoint.
    /// </summary>
    /// <param name="context">The endpoint filter invocation context.</param>
    /// <param name="next">The next endpoint filter delegate.</param>
    /// <returns>The result from the endpoint or validation error response.</returns>
    public async ValueTask<object?> InvokeAsync(
        EndpointFilterInvocationContext context,
        EndpointFilterDelegate next)
    
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentNullException.ThrowIfNull(next);

        // Get the validator from DI
        var validator = context.HttpContext.RequestServices.GetService(typeof(IValidator<T>)) as IValidator<T>;

        // If no validator is registered, skip validation and call the next filter
        if (validator == null)
        {
            return await next(context);
        }

        // Get the request object from the arguments
        var request = context.Arguments.OfType<T>().FirstOrDefault();

        if (request == null)
        {
            // No request object found, skip validation
            return await next(context);
        }

        // Validate the request
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
        {
            // Return validation error response
            var error = new OperationResultError(
                "VALIDATION_ERROR",
                "Request validation failed")
            {
                Level = OperationResultErrorLevel.Error
            };

            return ApiResponse.BadRequest(error, context.HttpContext);
        }

        // Request is valid, continue to the next filter
        return await next(context);
    }
}
