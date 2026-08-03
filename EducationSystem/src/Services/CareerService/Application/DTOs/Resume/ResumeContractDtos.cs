namespace CareerService.Application;

public sealed record VectorMatchRequestContract(
    Guid StudentId,
    string JobDescription,
    IReadOnlyList<Guid>? SelectedCourseIds,
    int TopK,
    float SimilarityThreshold);

public sealed record VectorMatchResponseContract(
    Guid StudentId,
    bool TargetJobMatched,
    bool IsFallback,
    float AppliedSimilarityThreshold,
    int MatchedOutcomeCount,
    IReadOnlyList<VectorMatchedOutcomeContract> TopMatchedOutcomes,
    IReadOnlyList<VectorFallbackEvidenceContract> FallbackEvidence,
    IReadOnlyList<string> Warnings);

public sealed record VectorMatchedOutcomeContract(
    long? CloId,
    Guid? CriteriaId,
    string SourceType,
    string SubjectCode,
    string SubjectName,
    string CloCode,
    string Description,
    float SimilarityScore,
    string? ProgressionLevel);

public sealed record VectorFallbackEvidenceContract(
    string SourceType,
    int SourceId,
    string Title,
    string Description,
    float SimilarityScore);

public sealed record AcademicResumeContextContract(
    AcademicResumeStudentContract Student,
    decimal? Gpa,
    IReadOnlyList<AcademicResumeCourseContract> EligibleCourses,
    IReadOnlyList<AcademicResumeProjectContract> Projects,
    IReadOnlyList<AcademicResumeInternshipContract> ApprovedInternships);

public sealed record AcademicResumeStudentContract(
    Guid StudentId,
    Guid UserId,
    string FullName,
    string UserName,
    string MajorCode,
    string MajorName,
    string? FacultyName,
    string AcademicYear);

public sealed record AcademicResumeCourseContract(
    Guid SubjectId,
    string SubjectCode,
    string SubjectName,
    int CreditPoint,
    decimal Score,
    IReadOnlyList<AcademicResumeCourseOutcomeContract> CourseOutcomes);

public sealed record AcademicResumeCourseOutcomeContract(
    string Name,
    string? Description);

public sealed record AcademicResumeProjectContract(
    int ProjectId,
    string ProjectName,
    string TechStack,
    string? ProjectDescription,
    string? SourceCodeUrl,
    int TeamSize,
    string? MyRole,
    string? MyContributions,
    Guid? MappedCourseId,
    string? MappedCourseCode,
    string? MappedCourseName);

public sealed record AcademicResumeInternshipContract(
    int InternshipId,
    string CompanyName,
    string Position,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? TaskDescription);
