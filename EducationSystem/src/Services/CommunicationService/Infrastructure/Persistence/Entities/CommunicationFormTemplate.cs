namespace CommunicationService.Infrastructure.Persistence.Entities;

public sealed class CommunicationFormTemplate
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public DateTime CreatedAtUtc { get; set; }
}