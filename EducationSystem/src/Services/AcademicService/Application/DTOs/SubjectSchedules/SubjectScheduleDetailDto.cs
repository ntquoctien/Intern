namespace AcademicService.Application.DTOs.SubjectSchedules;

public sealed class SubjectScheduleDetailDto
{
    public Guid Id { get; init; }
    public Guid SubjectTeachingId { get; init; }
    public Guid? RoomId { get; init; }
    public Guid? TeacherId { get; init; }
    public DateTime StartDateTime { get; init; }
    public DateTime EndDateTime { get; init; }
    public int? ScheduleType { get; init; }
    public string Note { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
}
