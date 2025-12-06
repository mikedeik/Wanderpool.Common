using System.Runtime.Serialization;

namespace Wanderpool.Common.Infra.Exceptions;

/// <summary>
/// Exception thrown when a resource conflict is detected (e.g., duplicate key, concurrent modification).
/// </summary>
[Serializable]
public class ConflictException : WanderpoolException
{
    /// <summary>
    /// Gets the identifier of the resource involved in the conflict.
    /// </summary>
    public string ResourceIdentifier { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class.
    /// </summary>
    /// <param name="message">The error message describing the conflict.</param>
    /// <param name="resourceIdentifier">The identifier of the conflicting resource.</param>
    public ConflictException(string message, string resourceIdentifier)
        : base("CONFLICT", message)
    {
        ResourceIdentifier = resourceIdentifier;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class with an inner exception.
    /// </summary>
    /// <param name="message">The error message.</param>
    /// <param name="resourceIdentifier">The identifier of the conflicting resource.</param>
    /// <param name="innerException">The inner exception.</param>
    public ConflictException(string message, string resourceIdentifier, Exception innerException)
        : base("CONFLICT", message, innerException)
    {
        ResourceIdentifier = resourceIdentifier;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ConflictException"/> class for deserialization.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    protected ConflictException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ResourceIdentifier = info.GetString(nameof(ResourceIdentifier)) ?? "";
    }

    /// <summary>
    /// Populates the serialization info with exception data.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ResourceIdentifier), ResourceIdentifier);
    }
}
