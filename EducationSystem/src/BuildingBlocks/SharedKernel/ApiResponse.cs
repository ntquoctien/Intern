namespace SharedKernel;

public sealed class ApiResponse<T>
{
    public bool Success { get; init; }

    public string Message { get; init; } = string.Empty;

    public T? Data { get; init; }

    public ApiError? Error { get; init; }

    public static ApiResponse<T> Ok(T? data, string message = "Success")
    {
        return new ApiResponse<T>
        {
            Success = true,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<T> Fail(string code, string message, T? data = default)
    {
        return new ApiResponse<T>
        {
            Success = false,
            Message = message,
            Data = data,
            Error = new ApiError(code, message)
        };
    }
}
