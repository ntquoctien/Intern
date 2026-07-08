namespace IdentityService.Application.DTOs.PasswordResets;

public sealed class PasswordResetDetailDto
{
    public Guid Id { get; init; }
    public Guid UserId { get; init; }
    public DateTime CreationDate { get; init; }
    public bool IsDeactive { get; init; }
}
