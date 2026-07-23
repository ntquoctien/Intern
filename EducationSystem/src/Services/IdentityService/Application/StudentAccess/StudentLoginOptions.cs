namespace IdentityService.Application.StudentAccess;

public enum StudentCodeSource
{
    Nickname,
    UserInternalId,
    UserName
}

public sealed class StudentLoginOptions
{
    public const string SectionName = "StudentLogin";

    public StudentCodeSource CodeSource { get; set; } = StudentCodeSource.Nickname;

    public string AcademicServiceBaseUrl { get; set; } = "http://localhost:5002";

    public int AcademicLookupTimeoutSeconds { get; set; } = 3;

    public string DefaultCredential { get; set; } = "1";
}
