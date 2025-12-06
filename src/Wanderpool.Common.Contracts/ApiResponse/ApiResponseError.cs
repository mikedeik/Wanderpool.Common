using System.Globalization;

namespace Wanderpool.Common.Contracts.ApiResponse;

public class ApiResponseError
{
    public required string Code { get; set; }
    public required string Message { get; set; }
    public ApiResponseErrorLevel Level { get; set; }

}

