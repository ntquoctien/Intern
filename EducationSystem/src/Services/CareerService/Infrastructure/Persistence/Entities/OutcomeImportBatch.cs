using System;
using System.Collections.Generic;

namespace CareerService.Infrastructure.Persistence.Entities;

public partial class OutcomeImportBatch
{
    public long Id { get; set; }

    public long CurriculumVersionId { get; set; }

    public string OriginalFileName { get; set; } = null!;

    public string StorageKey { get; set; } = null!;

    public string ContentType { get; set; } = null!;

    public long FileSize { get; set; }

    public string FileHash { get; set; } = null!;

    public string? Provider { get; set; }

    public string? ModelName { get; set; }

    public string? PromptVersion { get; set; }

    public string? RawExtractionJson { get; set; }

    public string? ReviewedJson { get; set; }

    public string? ValidationJson { get; set; }

    public string? QualityReportJson { get; set; }

    public string Status { get; set; } = null!;

    public string? ProcessingStage { get; set; }

    public byte ProgressPercent { get; set; }

    public string UploadedByExternalId { get; set; } = null!;

    public string UploadedByName { get; set; } = null!;

    public string? ReviewedByExternalId { get; set; }

    public string? ReviewedByName { get; set; }

    public string? ApprovedByExternalId { get; set; }

    public string? ApprovedByName { get; set; }

    public DateTime? ProcessingStartedAt { get; set; }

    public DateTime? ProcessingCompletedAt { get; set; }

    public DateTime? ReviewedAt { get; set; }

    public DateTime? ApprovedAt { get; set; }

    public string? ErrorCode { get; set; }

    public string? ErrorMessage { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public Guid? SelectedSubjectExternalId { get; set; }

    public string? SelectedSubjectCode { get; set; }

    public string? SelectedSubjectName { get; set; }

    public virtual CurriculumVersion CurriculumVersion { get; set; } = null!;

    public virtual ApprovedOutcomeJsonDocument? ApprovedOutcomeJsonDocument { get; set; }

    public virtual ICollection<OutcomeDocumentBlock> OutcomeDocumentBlocks { get; set; } = new List<OutcomeDocumentBlock>();

    public virtual ICollection<OutcomeReviewLog> OutcomeReviewLogs { get; set; } = new List<OutcomeReviewLog>();
}
