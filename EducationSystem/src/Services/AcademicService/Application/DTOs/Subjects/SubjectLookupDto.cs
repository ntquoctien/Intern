namespace AcademicService.Application.DTOs.Subjects;

public sealed class SubjectLookupDto
{
    public Guid Id { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
}
