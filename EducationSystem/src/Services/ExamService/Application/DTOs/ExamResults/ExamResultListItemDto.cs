namespace ExamService.Application.DTOs.ExamResults;

public sealed class ExamResultListItemDto
{
    public Guid Id { get; init; }
    public Guid SubjectTeachingExamId { get; init; }
    public Guid StudentId { get; init; }
    public Guid? ExamAttemptId { get; init; }
    public float? Result { get; init; }
    public float? CombinedResult { get; init; }
    public string? Notes { get; init; }
    public string? ExamResultDesc { get; init; }
    public string? ExamResultDetail { get; init; }
    public bool IsDeleted { get; init; }
}
