namespace CareerService.Application;

public sealed class DownstreamApiException(
    string errorCode,
    string message,
    int statusCode) : Exception(message)
{
    public string ErrorCode { get; } = errorCode;
    public int StatusCode { get; } = statusCode;
}
