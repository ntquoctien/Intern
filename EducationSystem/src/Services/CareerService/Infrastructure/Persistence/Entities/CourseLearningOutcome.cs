using System;
using System.Collections.Generic;

namespace CareerService.Infrastructure.Persistence.Entities;

public partial class CourseLearningOutcome
{
    public long Id { get; set; }

    public long CurriculumVersionId { get; set; }

    public Guid? SubjectExternalId { get; set; }

    public string SubjectCode { get; set; } = null!;

    public string SubjectName { get; set; } = null!;

    public int? Credits { get; set; }

    public string CloCode { get; set; } = null!;

    public string Description { get; set; } = null!;

    public string? OriginalDescription { get; set; }

    public string? NormalizedDescription { get; set; }

    public int SortOrder { get; set; }

    public string Status { get; set; } = null!;

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<CloPloMapping> CloPloMappings { get; set; } = new List<CloPloMapping>();

    public virtual CurriculumVersion CurriculumVersion { get; set; } = null!;
}
