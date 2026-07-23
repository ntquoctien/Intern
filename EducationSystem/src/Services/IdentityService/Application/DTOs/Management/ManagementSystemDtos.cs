namespace IdentityService.Application.DTOs.Management;

public sealed record SafeSettingDto(Guid SettingId, string Key, string DisplayValue, bool IsMasked);
public sealed record DeviceTypeCountDto(int RawDeviceType, int Count);
public sealed record SafeAuditDto(Guid AuditId, int RawAction, DateTime CreationDate,
    int? RawRecordEntity, string RecordDescription);
public sealed record ManagementSystemOverviewDto(IReadOnlyList<SafeSettingDto> Settings,
    int SettingCount, int AuditCount, int DeviceCount, int ResetRequestCount,
    IReadOnlyList<DeviceTypeCountDto> DevicesByType, IReadOnlyList<SafeAuditDto> RecentAudits);
