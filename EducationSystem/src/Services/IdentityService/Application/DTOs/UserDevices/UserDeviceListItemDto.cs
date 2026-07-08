namespace IdentityService.Application.DTOs.UserDevices;

public sealed class UserDeviceListItemDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public int UserRole { get; init; }
    public int DeviceType { get; init; }
    public string Identifier { get; init; } = string.Empty;
    public string PushId { get; init; } = string.Empty;
}
