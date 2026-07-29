using System;
using System.Collections.Generic;

namespace CareerService.Infrastructure.Persistence.Entities;

public partial class OutcomeReviewLog
{
    public long Id { get; set; }

    public long ImportBatchId { get; set; }

    public string EntityType { get; set; } = null!;

    public string EntityDraftId { get; set; } = null!;

    public string? FieldName { get; set; }

    public string? OldValueJson { get; set; }

    public string? NewValueJson { get; set; }

    public string Action { get; set; } = null!;

    public string? Note { get; set; }

    public string ActorExternalId { get; set; } = null!;

    public string ActorName { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual OutcomeImportBatch ImportBatch { get; set; } = null!;
}
