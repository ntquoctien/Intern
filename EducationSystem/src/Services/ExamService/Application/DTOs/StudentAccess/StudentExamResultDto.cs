namespace ExamService.Application.DTOs.StudentAccess;

public sealed record StudentExamResultDto(
    Guid ExamResultId,
    Guid SubjectTeachingExamId,
    Guid SubjectTeachingId,
    Guid? ExamAttemptId,
    string ExamName,
    DateTime ExamStartDate,
    DateTime ExamEndDate,
    int ExamType,
    DateTime? AttemptSubmitDate,
    float? RawResult,
    float? RawCombinedResult,
    string? Description,
    string? Notes);
