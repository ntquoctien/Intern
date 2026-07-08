namespace AcademicService.Application.DTOs.SubjectStudents;

public sealed class SubjectStudentListItemDto
{
    public Guid Id { get; init; }
    public Guid SubjectTeachingId { get; init; }
    public Guid StudentId { get; init; }
}
