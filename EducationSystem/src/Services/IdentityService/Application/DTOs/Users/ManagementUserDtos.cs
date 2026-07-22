namespace IdentityService.Application.DTOs.Users;

public sealed record ManagementUserDto(
    Guid Id,
    string UserName,
    string FullName,
    DateTime? BirthDate,
    DateTime? IdentificationDate,
    string? MaskedIdentificationNumber,
    string UserInternalId,
    string? MaskedMobile,
    string? ProfilePicUrl,
    int Role,
    bool IsActive,
    DateTime? LastEnforceAnnouncementRead);
