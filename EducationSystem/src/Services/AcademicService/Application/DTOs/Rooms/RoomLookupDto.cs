namespace AcademicService.Application.DTOs.Rooms;

public sealed class RoomLookupDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
