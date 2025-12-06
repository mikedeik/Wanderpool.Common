using Wanderpool.Common.Contracts.ApiResponse;

namespace Wanderpool.Common.Contracts.Operations.Mappers;

public class OperationResultMapper
{
    public static ApiResponseEnvelope<T> Map<T>(OperationResult<T> operationResult)
    {
        return new ApiResponseEnvelope<T>
        {
            Data = operationResult.Value,
            Error = ErrorMapper.Map(operationResult.Error),
            IsSuccess =  operationResult.IsSuccess
        };
    }

    public static ApiResponseEnvelope<object> Map(OperationResult operationResult)
    {
        return new ApiResponseEnvelope<object>()
        {
            Error = ErrorMapper.Map(operationResult.Error),
            IsSuccess = operationResult.IsSuccess
        };
    }
}