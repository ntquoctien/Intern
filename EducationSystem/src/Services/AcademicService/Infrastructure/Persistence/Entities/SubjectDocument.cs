using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class SubjectDocument
{
    public Guid Id { get; set; }

    public Guid SubjectId { get; set; }

    public int Type { get; set; }

    public string Name { get; set; } = null!;

    public string Detail { get; set; } = null!;

    public string Url { get; set; } = null!;

    public Guid CreateById { get; set; }

    public Guid UserId { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime UpdateDate { get; set; }

    public virtual Subject Subject { get; set; } = null!;
}
