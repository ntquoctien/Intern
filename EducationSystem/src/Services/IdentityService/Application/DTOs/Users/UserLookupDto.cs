namespace IdentityService.Application.DTOs.Users;

public sealed class UserLookupDto
{
    public Guid Id { get; init; }
    public string UserName { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string UserInternalId { get; init; } = string.Empty;
}
