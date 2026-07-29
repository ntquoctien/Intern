using System;
using System.Collections.Generic;

namespace CareerService.Infrastructure.Persistence.Entities;

public partial class CurriculumVersion
{
    public long Id { get; set; }

    public Guid? MajorExternalId { get; set; }

    public string MajorCode { get; set; } = null!;

    public string MajorName { get; set; } = null!;

    public string CurriculumCode { get; set; } = null!;

    public string? CurriculumName { get; set; }

    public string Version { get; set; } = null!;

    public DateOnly? EffectiveFrom { get; set; }

    public DateOnly? EffectiveTo { get; set; }

    public string Status { get; set; } = null!;

    public string? Description { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime UpdatedAt { get; set; }

    public byte[] RowVersion { get; set; } = null!;

    public virtual ICollection<CloPloMapping> CloPloMappings { get; set; } = new List<CloPloMapping>();

    public virtual ICollection<ApprovedOutcomeJsonDocument> ApprovedOutcomeJsonDocuments { get; set; } = new List<ApprovedOutcomeJsonDocument>();

    public virtual ICollection<CourseLearningOutcome> CourseLearningOutcomes { get; set; } = new List<CourseLearningOutcome>();

    public virtual ICollection<OutcomeImportBatch> OutcomeImportBatches { get; set; } = new List<OutcomeImportBatch>();

    public virtual ICollection<ProgramLearningOutcome> ProgramLearningOutcomes { get; set; } = new List<ProgramLearningOutcome>();
}
