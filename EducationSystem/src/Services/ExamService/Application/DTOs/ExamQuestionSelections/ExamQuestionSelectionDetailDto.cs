namespace ExamService.Application.DTOs.ExamQuestionSelections;

public sealed class ExamQuestionSelectionDetailDto
{
    public Guid Id { get; init; }
    public Guid ExamAttemptId { get; init; }
    public Guid? AnswerId { get; init; }
    public string QuestionText { get; init; } = string.Empty;
    public string ImageUrl { get; init; } = string.Empty;
    public int Order { get; init; }
    public bool IsFlag { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? SubmitDate { get; init; }
}
