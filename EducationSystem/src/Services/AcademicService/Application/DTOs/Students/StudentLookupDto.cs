namespace AcademicService.Application.DTOs.Students;

public sealed class StudentLookupDto
{
    public Guid Id { get; init; }
    public string? Nickname { get; init; }
}
