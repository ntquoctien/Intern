namespace AcademicService.Application.DTOs.Majors;

public sealed class MajorLookupDto
{
    public Guid Id { get; init; }
    public string Code { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
