using System;
using System.Collections.Generic;

namespace CareerService.Infrastructure.Persistence.Entities;

public partial class CloPloMapping
{
    public long Id { get; set; }

    public long CurriculumVersionId { get; set; }

    public long CloId { get; set; }

    public long PloId { get; set; }

    public string ProgressionLevelCode { get; set; } = null!;

    public bool IsApproved { get; set; }

    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual CourseLearningOutcome CourseLearningOutcome { get; set; } = null!;

    public virtual CurriculumVersion CurriculumVersion { get; set; } = null!;

    public virtual ProgramLearningOutcome ProgramLearningOutcome { get; set; } = null!;

    public virtual ProgressionLevel ProgressionLevelCodeNavigation { get; set; } = null!;
}
