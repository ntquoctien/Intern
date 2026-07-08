using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class SemesterTuition
{
    public Guid Id { get; set; }

    public Guid SemesterPlanId { get; set; }

    public Guid StudentId { get; set; }

    public decimal? Amount { get; set; }

    public DateTime? PaidDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual SemesterPlan SemesterPlan { get; set; } = null!;

    public virtual Student Student { get; set; } = null!;
}
