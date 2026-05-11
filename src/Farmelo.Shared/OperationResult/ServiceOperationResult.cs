namespace Farmelo.Shared.OperationResult;

public sealed class ServiceOperationResult<T>
{
    public T? Payload { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }
    public bool Success { get; set; }
    public string? SuccessMessage { get; set; } = string.Empty;
}

public sealed class ServiceOperationResult
{
    private ServiceOperationResult()
    {
    }

    public static ServiceOperationResult<T> CreateWithFailure<T>(string error, string? errorCode = null, T? payload = default)
        => new()
        {
            Error = error,
            ErrorCode = errorCode,
            Success = false,
            Payload = payload
        };

    public static ServiceOperationResult<T> CreateWithSuccess<T>(T result, string? message = null)
        => new()
        {
            Payload = result,
            Success = true,
            SuccessMessage = message ?? string.Empty
        };
}

public sealed class ServiceOperationPaginatedResult<T, P>
{
    public T? Payload { get; set; }
    public P? Pagination { get; set; }
    public string? Error { get; set; }
    public string? ErrorCode { get; set; }
    public bool Success { get; set; }
    public string? SuccessMessage { get; set; } = string.Empty;
}

public sealed class ServiceOperationPaginatedResult
{
    private ServiceOperationPaginatedResult()
    {
    }

    public static ServiceOperationPaginatedResult<T, P> CreateWithFailure<T, P>(string error)
        => new()
        {
            Error = error,
            Success = false,
            Payload = default,
            Pagination = default
        };

    public static ServiceOperationPaginatedResult<T, P> CreateWithSuccess<T, P>(T result, P pagination, string? message = null)
        => new()
        {
            Payload = result,
            Pagination = pagination,
            Success = true,
            SuccessMessage = message ?? string.Empty
        };
}
