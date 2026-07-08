namespace ExamService.Application.DTOs.QuestionSuites;

public sealed class QuestionSuiteListItemDto
{
    public Guid Id { get; init; }
    public Guid SubjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public Guid? UpdatedById { get; init; }
    public DateTime CreationTime { get; init; }
}
