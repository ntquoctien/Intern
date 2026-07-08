namespace AcademicService.Application.DTOs.Rooms;

public sealed class RoomListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public int NumberOfSeats { get; init; }
    public bool IsDeleted { get; init; }
}
