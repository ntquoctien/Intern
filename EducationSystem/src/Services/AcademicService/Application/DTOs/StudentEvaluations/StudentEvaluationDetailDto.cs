namespace AcademicService.Application.DTOs.StudentEvaluations;

public sealed class StudentEvaluationDetailDto
{
    public Guid Id { get; init; }
    public Guid StudentId { get; init; }
    public Guid? SubjectTeachingId { get; init; }
    public Guid? SemesterPlanId { get; init; }
    public Guid? SubjectTeachingExamId { get; init; }
    public Guid? QuestionId { get; init; }
    public Guid? TeacherId { get; init; }
    public string? TeacherName { get; init; }
    public int Type { get; init; }
    public string? Comment { get; init; }
    public decimal? TotalScore { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime? UpdatedDate { get; init; }
    public bool IsDeleted { get; init; }
}
