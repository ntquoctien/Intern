using System;
using System.Collections.Generic;

namespace IdentityService.Infrastructure.Persistence.Entities;

public partial class AuditLog
{
    public Guid Id { get; set; }

    public int Action { get; set; }

    public string Details { get; set; } = null!;

    public Guid RecordId { get; set; }

    public DateTime CreationDate { get; set; }

    public Guid UserId { get; set; }

    public int? RecordEntity { get; set; }

    public string RecordDesc { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual User User { get; set; } = null!;
}
