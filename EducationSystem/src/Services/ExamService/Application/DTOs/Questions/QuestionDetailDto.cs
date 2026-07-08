namespace ExamService.Application.DTOs.Questions;

public sealed class QuestionDetailDto
{
    public Guid Id { get; init; }
    public Guid QuestionSuiteId { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public int Level { get; init; }
    public string? ImageUrl { get; init; }
}
