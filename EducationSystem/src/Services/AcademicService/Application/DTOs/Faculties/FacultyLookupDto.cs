namespace AcademicService.Application.DTOs.Faculties;

public sealed class FacultyLookupDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
