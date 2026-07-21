namespace SharedKernel;

public sealed record ApiError(string Code, string Message);

public static class StudentErrorCodes
{
    public const string Unauthorized = "STUDENT_UNAUTHORIZED";
    public const string TokenExpired = "STUDENT_TOKEN_EXPIRED";
    public const string AccountUnavailable = "STUDENT_ACCOUNT_UNAVAILABLE";
    public const string NotFound = "STUDENT_NOT_FOUND";
    public const string CodeAmbiguous = "STUDENT_CODE_AMBIGUOUS";
    public const string OwnedResourceNotFound = "STUDENT_RESOURCE_NOT_FOUND";
    public const string IdentityServiceUnavailable = "STUDENT_IDENTITY_SERVICE_UNAVAILABLE";
}
