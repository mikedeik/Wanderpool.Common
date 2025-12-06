using System.Runtime.Serialization;

namespace Wanderpool.Common.Infra.Exceptions;

/// <summary>
/// Exception thrown when a requested resource is not found.
/// </summary>
[Serializable]
public class NotFoundException : WanderpoolException
{
    /// <summary>
    /// Gets the type of resource that was not found (e.g., "User", "Order").
    /// </summary>
    public string ResourceType { get; }

    /// <summary>
    /// Gets the identifier of the resource that was not found.
    /// </summary>
    public string ResourceId { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class.
    /// </summary>
    /// <param name="resourceType">The type of resource that was not found.</param>
    /// <param name="resourceId">The identifier of the resource that was not found.</param>
    public NotFoundException(string resourceType, string resourceId)
        : base("NOT_FOUND", $"{resourceType} with id '{resourceId}' was not found.")
    {
        ResourceType = resourceType;
        ResourceId = resourceId;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="NotFoundException"/> class for deserialization.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    protected NotFoundException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        ResourceType = info.GetString(nameof(ResourceType)) ?? "";
        ResourceId = info.GetString(nameof(ResourceId)) ?? "";
    }

    /// <summary>
    /// Populates the serialization info with exception data.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);
        info.AddValue(nameof(ResourceType), ResourceType);
        info.AddValue(nameof(ResourceId), ResourceId);
    }
}
