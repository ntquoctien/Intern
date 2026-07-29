using System;
using System.Collections.Generic;

namespace CareerService.Infrastructure.Persistence.Entities;

public partial class OutcomeDocumentBlock
{
    public long Id { get; set; }

    public long ImportBatchId { get; set; }

    public string BlockId { get; set; } = null!;

    public string BlockType { get; set; } = null!;

    public int Sequence { get; set; }

    public byte? HeadingLevel { get; set; }

    public string ContentJson { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public virtual OutcomeImportBatch ImportBatch { get; set; } = null!;
}
