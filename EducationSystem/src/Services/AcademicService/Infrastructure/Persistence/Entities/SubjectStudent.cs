using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class SubjectStudent
{
    public Guid Id { get; set; }

    public Guid SubjectTeachingId { get; set; }

    public Guid StudentId { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual SubjectTeaching SubjectTeaching { get; set; } = null!;
}
