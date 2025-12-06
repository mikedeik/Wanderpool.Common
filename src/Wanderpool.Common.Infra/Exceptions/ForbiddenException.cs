using System.Runtime.Serialization;

namespace Wanderpool.Common.Infra.Exceptions;

/// <summary>
/// Exception thrown when access to a resource is forbidden.
/// </summary>
[Serializable]
public class ForbiddenException : WanderpoolException
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class.
    /// </summary>
    /// <param name="message">The error message explaining why access is forbidden.</param>
    public ForbiddenException(string message)
        : base("FORBIDDEN", message)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public ForbiddenException(string message, Exception innerException)
        : base("FORBIDDEN", message, innerException)
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ForbiddenException"/> class for deserialization.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    protected ForbiddenException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
    }
}
