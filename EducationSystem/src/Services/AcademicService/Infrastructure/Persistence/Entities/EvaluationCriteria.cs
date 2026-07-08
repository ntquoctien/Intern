using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class EvaluationCriteria
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Description { get; set; } = null!;

    public int Type { get; set; }

    public decimal Score { get; set; }

    public Guid? ParentId { get; set; }

    public Guid? QuestionId { get; set; }

    public virtual ICollection<EvaluationCriteria> InverseParent { get; set; } = new List<EvaluationCriteria>();

    public virtual EvaluationCriteria? Parent { get; set; }

    public virtual ICollection<StudentEvaluationDetail> StudentEvaluationDetails { get; set; } = new List<StudentEvaluationDetail>();
}
