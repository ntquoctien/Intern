namespace CareerService.Infrastructure.Persistence.Entities;

public sealed class ApprovedOutcomeJsonDocument
{
    public long Id { get; set; }
    public long CurriculumVersionId { get; set; }
    public long ImportBatchId { get; set; }
    public string SchemaVersion { get; set; } = null!;
    public int DocumentVersion { get; set; }
    public string ContentJson { get; set; } = null!;
    public string ContentHash { get; set; } = null!;
    public string Status { get; set; } = null!;
    public DateTime GeneratedAt { get; set; }
    public string GeneratedByExternalId { get; set; } = null!;
    public string GeneratedByName { get; set; } = null!;
    public DateTime? SupersededAt { get; set; }
    public byte[] RowVersion { get; set; } = null!;

    public CurriculumVersion CurriculumVersion { get; set; } = null!;
    public OutcomeImportBatch ImportBatch { get; set; } = null!;
}
