namespace Wanderpool.Common.Contracts.Operations;

public record OperationResultError(string Code, string Message)
{
    public OperationResultErrorLevel Level { get; init; }

    public static OperationResultError Info(string code, string message)
    {
        return new OperationResultError(code, message)
        {
            Level = OperationResultErrorLevel.Info
        };
    }

    public static OperationResultError Warning(string code, string message)
    {
        return new OperationResultError(code, message)
        {
            Level = OperationResultErrorLevel.Warning
        };
    }

    public static OperationResultError Error(string code, string message)
    {
        return new OperationResultError(code, message)
        {
            Level = OperationResultErrorLevel.Error
        };
    }


    public static implicit operator OperationResultError((string Code, string Message) tuple)
    {
        return new OperationResultError(tuple.Code, tuple.Message);
    }

    public static implicit operator OperationResultError(
        (string Code, string Message, OperationResultErrorLevel level) tuple)
    {
        return new OperationResultError(tuple.Code, tuple.Message)
        {
            Level = tuple.level
        };
    }
}