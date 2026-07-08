namespace ExamService.Infrastructure.Persistence.Entities;

public sealed class ExamQuestionSuite
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}