namespace Wanderpool.Common.Contracts.ApiResponse;

public class ApiResponseEnvelope<T>
{
    public T? Data { get; set; }
    public bool IsSuccess { get; set; }
    public ApiResponseError? Error { get; set; }
    public string? TraceId { get; set; }

    public static ApiResponseEnvelope<T> Success(T result) => new ApiResponseEnvelope<T>
    {
        IsSuccess = true,
        Data = result,
        TraceId = null,
        Error = null,
    };

    public static ApiResponseEnvelope<T> Failure(ApiResponseError error) => new ApiResponseEnvelope<T>
    {
        IsSuccess = false,
        Data = default,
        Error = error,
    };
}