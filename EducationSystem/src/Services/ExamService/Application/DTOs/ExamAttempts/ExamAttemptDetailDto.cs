namespace ExamService.Application.DTOs.ExamAttempts;

public sealed class ExamAttemptDetailDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public Guid SubjectTeachingExamId { get; init; }
    public DateTime? DraftDate { get; init; }
    public DateTime? SubmitDate { get; init; }
    public bool IsDeleted { get; init; }
}
