namespace CommunicationService.Application.DTOs.UserAnnouncements;

public sealed class UserAnnouncementDetailDto
{
    public Guid Id { get; init; }
    public int Type { get; init; }
    public string UserIds { get; init; } = string.Empty;
    public string Message { get; init; } = string.Empty;
    public DateTime CreationDate { get; init; }
    public int Status { get; init; }
    public string DeepLink { get; init; } = string.Empty;
    public string DeepLinkParam { get; init; } = string.Empty;
    public Guid EntityObjectId { get; init; }
    public bool EnforceRead { get; init; }
    public int? NotificationType { get; init; }
    public bool IsDeleted { get; init; }
}
