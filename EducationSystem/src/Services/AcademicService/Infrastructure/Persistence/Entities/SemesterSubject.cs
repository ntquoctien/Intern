using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class SemesterSubject
{
    public Guid Id { get; set; }

    public Guid SemesterPlanId { get; set; }

    public Guid? SubjectId { get; set; }

    public string SubjectName { get; set; } = null!;

    public string SubjectCode { get; set; } = null!;

    public int CreditPoint { get; set; }

    public bool IsDeleted { get; set; }

    public virtual SemesterPlan SemesterPlan { get; set; } = null!;

    public virtual Subject? Subject { get; set; }
}
