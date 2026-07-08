using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class AcademicYear
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int Year { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<SemesterPlan> SemesterPlans { get; set; } = new List<SemesterPlan>();

    public virtual ICollection<Student> Students { get; set; } = new List<Student>();
}
