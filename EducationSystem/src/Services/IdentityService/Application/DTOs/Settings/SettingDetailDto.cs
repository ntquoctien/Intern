namespace IdentityService.Application.DTOs.Settings;

public sealed class SettingDetailDto
{
    public Guid Id { get; init; }
    public string Key { get; init; } = string.Empty;
    public string Value { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
}
