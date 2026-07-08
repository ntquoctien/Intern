namespace ExamService.Application.DTOs.SubjectTeachingExams;

public sealed class SubjectTeachingExamDetailDto
{
    public Guid Id { get; init; }
    public Guid SubjectTeachingId { get; init; }
    public Guid? QuestionSuiteId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public Guid? RoomId { get; init; }
    public Guid? TeacherId { get; init; }
    public string Notes { get; init; } = string.Empty;
    public int Type { get; init; }
    public int Count { get; init; }
    public int NumOfEasy { get; init; }
    public int NumOfNormal { get; init; }
    public int NumOfHard { get; init; }
    public int NumOfPractice { get; init; }
    public int? Method { get; init; }
    public bool AllowNotifyStudent { get; init; }
    public bool IsDeleted { get; init; }
}
