namespace CommunicationService.Application.DTOs.StudentAccess;

public sealed record StudentAnnouncementDto(
    Guid AnnouncementId,
    int RawType,
    string Message,
    DateTime CreationDate,
    int RawStatus,
    bool EnforceRead,
    int? RawNotificationType,
    string? SafeDeepLink);
