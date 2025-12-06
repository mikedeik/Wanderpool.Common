using System.Runtime.Serialization;

namespace Wanderpool.Common.Infra.Exceptions;

/// <summary>
/// Exception thrown when validation of request data fails.
/// Contains structured validation errors organized by field name.
/// </summary>
[Serializable]
public class ValidationException : WanderpoolException
{
    /// <summary>
    /// Gets the validation errors organized by field name.
    /// Keys are field names, values are arrays of error messages for that field.
    /// </summary>
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class.
    /// </summary>
    /// <param name="errors">Dictionary of validation errors. Cannot be null or empty.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="errors"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="errors"/> is empty.</exception>
    public ValidationException(Dictionary<string, string[]> errors)
        : base("VALIDATION_ERROR", FormatErrorMessage(errors))
    {
        if (errors == null)
            throw new ArgumentNullException(nameof(errors));

        if (errors.Count == 0)
            throw new ArgumentException("Validation errors dictionary cannot be empty.", nameof(errors));

        // Create a defensive copy to ensure immutability
        Errors = new Dictionary<string, string[]>(errors);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ValidationException"/> class for deserialization.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    protected ValidationException(SerializationInfo info, StreamingContext context)
        : base(info, context)
    {
        var errorCount = info.GetInt32("ErrorCount");
        var errors = new Dictionary<string, string[]>();

        for (int i = 0; i < errorCount; i++)
        {
            var fieldName = info.GetString($"Field_{i}") ?? "";
            var errorMessages = (string[]?)info.GetValue($"Errors_{i}", typeof(string[])) ?? Array.Empty<string>();
            errors[fieldName] = errorMessages;
        }

        Errors = errors;
    }

    /// <summary>
    /// Populates the serialization info with exception data.
    /// </summary>
    /// <param name="info">The serialization info.</param>
    /// <param name="context">The streaming context.</param>
    public override void GetObjectData(SerializationInfo info, StreamingContext context)
    {
        base.GetObjectData(info, context);

        info.AddValue("ErrorCount", Errors.Count);
        int index = 0;
        foreach (var kvp in Errors)
        {
            info.AddValue($"Field_{index}", kvp.Key);
            info.AddValue($"Errors_{index}", kvp.Value);
            index++;
        }
    }

    /// <summary>
    /// Formats validation errors into a readable error message.
    /// </summary>
    /// <param name="errors">The validation errors dictionary.</param>
    /// <returns>A formatted error message summarizing the validation errors.</returns>
    private static string FormatErrorMessage(Dictionary<string, string[]> errors)
    {
        if (errors == null || errors.Count == 0)
            return "Validation failed.";

        var fieldCount = errors.Count;
        var errorCount = errors.Values.Sum(arr => arr.Length);

        return $"Validation failed with {errorCount} error(s) in {fieldCount} field(s).";
    }
}
