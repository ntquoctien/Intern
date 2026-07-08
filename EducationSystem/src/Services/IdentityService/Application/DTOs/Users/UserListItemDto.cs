namespace IdentityService.Application.DTOs.Users;

public sealed class UserListItemDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public DateTime? BirthDate { get; init; }
    public DateTime? IdentificationDate { get; init; }
    public string? IdentificationNumber { get; init; }
    public string UserInternalId { get; init; } = string.Empty;
    public string? Mobile { get; init; }
    public string? ProfilePicUrl { get; init; }
    public int Role { get; init; }
    public bool IsActived { get; init; }
    public DateTime? LastEnforceAnnouncementRead { get; init; }
    public bool IsDeleted { get; init; }
}
