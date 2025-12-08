using System.Runtime.Serialization;

namespace Wanderpool.Common.Infra.Exceptions;

/// <summary>
/// Exception thrown when a business rule violation is detected.
/// </summary>
[Serializable]
public class BusinessRuleException : WanderpoolException
{
    /// <summary>
    /// Gets the name of the business rule that was violated.
    /// </summary>
    public string RuleName { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class.
    /// </summary>
    /// <param name="ruleName">The name of the business rule that was violated.</param>
    /// <param name="message">The error message describing the rule violation.</param>
    public BusinessRuleException(string ruleName, string message)
        : base("BUSINESS_RULE_VIOLATION", $"[{ruleName}] {message}")
    {
        RuleName = ruleName;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BusinessRuleException"/> class with an inner exception.
    /// </summary>
    /// <param name="ruleName">The name of the business rule that was violated.</param>
    /// <param name="message">The error message.</param>
    /// <param name="innerException">The inner exception.</param>
    public BusinessRuleException(string ruleName, string message, Exception innerException)
        : base("BUSINESS_RULE_VIOLATION", $"[{ruleName}] {message}", innerException)
    {
        RuleName = ruleName;
    }
    
    
}
