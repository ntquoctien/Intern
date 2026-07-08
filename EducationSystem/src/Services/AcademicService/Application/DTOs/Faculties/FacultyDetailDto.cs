namespace AcademicService.Application.DTOs.Faculties;

public sealed class FacultyDetailDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Code { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
}
