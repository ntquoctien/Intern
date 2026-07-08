namespace AcademicService.Application.DTOs.Attendances;

public sealed class AttendanceListItemDto
{
    public Guid Id { get; init; }
    public Guid SubjectScheduleId { get; init; }
    public Guid StudentId { get; init; }
    public Guid CreatedById { get; init; }
    public int Status { get; init; }
    public string Notes { get; init; } = string.Empty;
    public DateTime CreationDate { get; init; }
    public bool? IsFirstTypeWarning { get; init; }
    public bool? IsSecondTypeWarning { get; init; }
}
