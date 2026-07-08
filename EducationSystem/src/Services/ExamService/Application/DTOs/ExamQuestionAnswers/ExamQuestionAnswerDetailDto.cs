namespace ExamService.Application.DTOs.ExamQuestionAnswers;

public sealed class ExamQuestionAnswerDetailDto
{
    public Guid Id { get; init; }
    public Guid ExamQuestionSelectionId { get; init; }
    public string AnswerText { get; init; } = string.Empty;
    public bool IsAnswer { get; init; }
}
