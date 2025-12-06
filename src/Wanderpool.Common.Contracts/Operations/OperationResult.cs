namespace Wanderpool.Common.Contracts.Operations;

public record OperationResult(bool IsSuccess, OperationResultError? Error)
{
    
    public static OperationResult Fail(OperationResultError error)
    {
        return new OperationResult(false, error);
    }

    public static OperationResult<T> Fail<T>(OperationResultError error)
    {
        return new OperationResult<T>(default, false, error);
    }



    public static OperationResult Success()
    {
        return new OperationResult(true, null);
    }

    public static OperationResult<T> Success<T>(T value)
    {
        return new OperationResult<T>(value, true, null);
    }
    

    public static OperationResult<T> Success<T>(T value, OperationResultError warning)
    {
        return new OperationResult<T>(value, true, warning);
    }

    public static implicit operator OperationResult(OperationResultError? error)
    {
        return new OperationResult(false, error);
    }
}
public record OperationResult<T>(T? Value, bool IsSuccess, OperationResultError? Error) : OperationResult(IsSuccess, Error)
{
    public static implicit operator OperationResult<T>(T value)
    {
        return new OperationResult<T>(value, true, null);
    }

    public static implicit operator OperationResult<T>(OperationResultError error)
    {
        return new OperationResult<T>(
            default,
            error.Level != OperationResultErrorLevel.Error,
            error
        );
    }
}