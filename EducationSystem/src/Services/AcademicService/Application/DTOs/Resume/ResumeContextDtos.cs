namespace AcademicService.Application.DTOs.Resume;

public sealed record ResumeContextDto(
    ResumeStudentDto Student,
    decimal? Gpa,
    IReadOnlyList<ResumeCourseDto> EligibleCourses,
    IReadOnlyList<ResumeProjectDto> Projects,
    IReadOnlyList<ResumeInternshipDto> ApprovedInternships);

public sealed record ResumeStudentDto(
    Guid StudentId,
    Guid UserId,
    string FullName,
    string UserName,
    string MajorCode,
    string MajorName,
    string? FacultyName,
    string AcademicYear);

public sealed record ResumeCourseDto(
    Guid SubjectId,
    string SubjectCode,
    string SubjectName,
    int CreditPoint,
    decimal Score,
    IReadOnlyList<ResumeCourseOutcomeDto> CourseOutcomes);

public sealed record ResumeCourseOutcomeDto(
    string Name,
    string? Description);

public sealed record ResumeProjectDto(
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

public sealed record ResumeInternshipDto(
    int InternshipId,
    string CompanyName,
    string Position,
    DateOnly StartDate,
    DateOnly? EndDate,
    string? TaskDescription);
