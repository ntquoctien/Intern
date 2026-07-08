namespace ExamService.Application.DTOs.QuestionAnswers;

public sealed class QuestionAnswerDetailDto
{
    public Guid Id { get; init; }
    public Guid QuestionId { get; init; }
    public string AnswerText { get; init; } = string.Empty;
    public string? ImageUrl { get; init; }
    public bool IsAnswer { get; init; }
}
