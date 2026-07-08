namespace AcademicService.Application.DTOs.Majors;

public sealed class MajorDetailDto
{
    public Guid Id { get; init; }
    public Guid? FacultyId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public int TrainingType { get; init; }
    public bool IsDeleted { get; init; }
}
