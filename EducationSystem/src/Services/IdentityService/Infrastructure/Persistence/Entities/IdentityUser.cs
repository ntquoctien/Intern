namespace IdentityService.Infrastructure.Persistence.Entities;

public sealed class IdentityUser
{
    public int Id { get; set; }

    public string UserName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}