namespace IdentityService.Application.DTOs.AuditLogs;

public sealed class AuditLogDetailDto
{
    public Guid Id { get; init; }
    public int Action { get; init; }
    public string Details { get; init; } = string.Empty;
    public Guid RecordId { get; init; }
    public DateTime CreationDate { get; init; }
    public Guid UserId { get; init; }
    public int? RecordEntity { get; init; }
    public string RecordDesc { get; init; } = string.Empty;
    public bool IsDeleted { get; init; }
}
