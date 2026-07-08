namespace ExamService.Application.DTOs.QuestionSuites;

public sealed class QuestionSuiteLookupDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
