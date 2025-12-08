namespace Wanderpool.Common.Infra.Exceptions;

/// <summary>
/// Base exception class for all Wanderpool domain exceptions.
/// Includes error codes for consistent error handling across services.
/// </summary>
public class WanderpoolException : Exception
{
    /// <summary>
    /// Gets the error code associated with this exception.
    /// </summary>
    public string ErrorCode { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="WanderpoolException"/> class.
    /// </summary>
    /// <param name="errorCode">The error code for this exception. Cannot be null or empty.</param>
    /// <param name="message">The error message.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errorCode"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errorCode"/> is empty.</exception>
    public WanderpoolException(string errorCode, string message)
        : base(message)
    {
        if (errorCode == null)
            throw new ArgumentNullException(nameof(errorCode));

        if (string.IsNullOrWhiteSpace(errorCode))
            throw new ArgumentException("Error code cannot be empty.", nameof(errorCode));

        ErrorCode = errorCode;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="WanderpoolException"/> class with an inner exception.
    /// </summary>
    /// <param name="errorCode">The error code for this exception. Cannot be null or empty.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception that caused this exception.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errorCode"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errorCode"/> is empty.</exception>
    public WanderpoolException(string errorCode, string message, Exception innerException)
        : base(message, innerException)
    {
        if (errorCode == null)
            throw new ArgumentNullException(nameof(errorCode));

        if (string.IsNullOrWhiteSpace(errorCode))
            throw new ArgumentException("Error code cannot be empty.", nameof(errorCode));

        ErrorCode = errorCode;
    }
}
