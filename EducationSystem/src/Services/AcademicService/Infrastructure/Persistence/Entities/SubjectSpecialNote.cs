using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class SubjectSpecialNote
{
    public Guid Id { get; set; }

    public Guid SubjectId { get; set; }

    public Guid StudentId { get; set; }

    public Guid CreatedById { get; set; }

    public string Notes { get; set; } = null!;

    public int Type { get; set; }

    public DateTime CreationDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual Subject Subject { get; set; } = null!;
}
