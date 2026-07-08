namespace AcademicService.Application.DTOs.SubjectTeachings;

public sealed class SubjectTeachingListItemDto
{
    public Guid Id { get; init; }
    public Guid SubjectId { get; init; }
    public string Name { get; init; } = string.Empty;
    public DateTime StartDate { get; init; }
    public DateTime EndDate { get; init; }
    public int TotalSessions { get; init; }
    public Guid? RoomIdDefault { get; init; }
    public bool IsDeleted { get; init; }
}
