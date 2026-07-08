using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class StudentEvaluationDetail
{
    public Guid Id { get; set; }

    public Guid StudentEvaluationId { get; set; }

    public Guid? EvaluationCriteriaId { get; set; }

    public string? EvaluationName { get; set; }

    public decimal? StudentScore { get; set; }

    public decimal? Score { get; set; }

    public bool IsDeleted { get; set; }

    public virtual EvaluationCriteria? EvaluationCriteria { get; set; }

    public virtual StudentEvaluation StudentEvaluation { get; set; } = null!;
}
