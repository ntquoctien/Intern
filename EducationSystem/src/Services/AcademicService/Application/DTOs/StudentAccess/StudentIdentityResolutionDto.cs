namespace AcademicService.Application.DTOs.StudentAccess;

public sealed record StudentIdentityCandidateDto(
    Guid StudentId,
    Guid UserId,
    string StudentCode,
    int? StudyStatus,
    bool IsGraduated,
    bool? HasIssue);

public sealed record StudentIdentityResolutionDto(
    int MatchCount,
    StudentIdentityCandidateDto? Candidate);
