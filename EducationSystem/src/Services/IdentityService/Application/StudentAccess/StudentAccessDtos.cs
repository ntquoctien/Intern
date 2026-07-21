namespace IdentityService.Application.StudentAccess;

public sealed record StudentLoginRequest(string StudentCode);

public sealed record StudentSessionDto(
    Guid StudentId,
    Guid UserId,
    string StudentCode,
    string FullName,
    string UserName,
    string? ProfilePicUrl,
    DateTimeOffset ExpiresAt);

public sealed record StudentLoginResponse(string AccessToken, StudentSessionDto Session);

public sealed record SafeUserProfileDto(
    Guid UserId,
    string FullName,
    string UserName,
    string? ProfilePicUrl,
    bool IsActive);

public sealed record AcademicStudentCandidate(
    Guid StudentId,
    Guid UserId,
    string StudentCode,
    int? StudyStatus,
    bool IsGraduated,
    bool? HasIssue);

public sealed record AcademicStudentResolution(int MatchCount, AcademicStudentCandidate? Candidate);

public sealed record StudentLoginResult(StudentLoginResponse? Response, string? ErrorCode, string? ErrorMessage)
{
    public bool Success => Response is not null;
}
