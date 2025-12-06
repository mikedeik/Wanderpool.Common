using Wanderpool.Common.Contracts.ApiResponse;

namespace Wanderpool.Common.Contracts.Operations.Mappers;

public static class ErrorMapper
{
    
    private static readonly Dictionary<ApiResponseErrorLevel, OperationResultErrorLevel> ToOpResultLevel =
        new()
        {
            { ApiResponseErrorLevel.Info, OperationResultErrorLevel.Info },
            { ApiResponseErrorLevel.Warning, OperationResultErrorLevel.Warning },
            { ApiResponseErrorLevel.Error, OperationResultErrorLevel.Error }
        };

    private static readonly Dictionary<OperationResultErrorLevel, ApiResponseErrorLevel> ToResponseLevel =
        new()
        {
            { OperationResultErrorLevel.Info, ApiResponseErrorLevel.Info },
            { OperationResultErrorLevel.Warning, ApiResponseErrorLevel.Warning },
            { OperationResultErrorLevel.Error, ApiResponseErrorLevel.Error }
        };
    public static ApiResponseError? Map(OperationResultError? error)
    {
        if (error == null) return null;
        return new ApiResponseError
        {
            Code = error.Code,
            Message = error.Message,
            Level = ToResponseLevel.GetValueOrDefault(error.Level, ApiResponseErrorLevel.Error),
        };
    }

    public static OperationResultError? Map(ApiResponseError? error)
    {
        if (error == null) return null;
        return new OperationResultError(error.Code, error.Message)
        {
            Level = ToOpResultLevel.GetValueOrDefault(error.Level, OperationResultErrorLevel.Error),
        };
    }
}