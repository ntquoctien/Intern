namespace AcademicService.Application.DTOs.Subjects;

public sealed class SubjectListItemDto
{
    public Guid Id { get; init; }
    public Guid? FacultyId { get; init; }
    public string SubjectCode { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public int CreditPoint { get; init; }
    public int? TotalHours { get; init; }
    public string Note { get; init; } = string.Empty;
    public bool IsActived { get; init; }
    public bool IsDeleted { get; init; }
}
