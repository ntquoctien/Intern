using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class SemesterPlan
{
    public Guid Id { get; set; }

    public Guid AcademicYearId { get; set; }

    public Guid MajorId { get; set; }

    public int Semester { get; set; }

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public bool IsActive { get; set; }

    public bool IsDeleted { get; set; }

    public virtual AcademicYear AcademicYear { get; set; } = null!;

    public virtual Major Major { get; set; } = null!;

    public virtual ICollection<SemesterSubject> SemesterSubjects { get; set; } = new List<SemesterSubject>();

    public virtual ICollection<SemesterTuition> SemesterTuitions { get; set; } = new List<SemesterTuition>();

    public virtual ICollection<StudentEvaluation> StudentEvaluations { get; set; } = new List<StudentEvaluation>();
}
