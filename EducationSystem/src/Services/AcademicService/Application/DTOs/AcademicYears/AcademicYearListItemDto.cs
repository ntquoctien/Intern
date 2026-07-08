namespace AcademicService.Application.DTOs.AcademicYears;

public sealed class AcademicYearListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int Year { get; init; }
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public bool IsDeleted { get; init; }
}
