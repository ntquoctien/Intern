namespace CommunicationService.Application.DTOs.Management;

public sealed record ManagementAnnouncementQueryDto(int PageNumber = 1, int PageSize = 10,
    string? Search = null, int? Type = null, int? NotificationType = null, int? Status = null,
    bool? EnforceRead = null, DateTime? FromDate = null, DateTime? ToDate = null,
    string? Recipient = null);
public sealed record ManagementAnnouncementItemDto(Guid AnnouncementId, int RawType,
    int? RawNotificationType, int RawStatus, string Message, DateTime CreationDate,
    bool EnforceRead, string? SafeDeepLink, string? DeepLinkParameter, Guid EntityObjectId,
    int RecipientCount, string RecipientSummary);
public sealed record ManagementAnnouncementPageDto(IReadOnlyList<ManagementAnnouncementItemDto> Items,
    int PageNumber, int PageSize, int TotalItems, IReadOnlyDictionary<int, int> StatusCounts,
    int EnforceReadCount);
