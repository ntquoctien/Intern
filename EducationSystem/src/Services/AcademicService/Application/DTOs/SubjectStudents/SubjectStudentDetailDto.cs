namespace AcademicService.Application.DTOs.SubjectStudents;

public sealed class SubjectStudentDetailDto
{
    public Guid Id { get; init; }
    public Guid SubjectTeachingId { get; init; }
    public Guid StudentId { get; init; }
}
