using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class StudentEvaluation
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid? SubjectTeachingId { get; set; }

    public Guid? SemesterPlanId { get; set; }

    public Guid? SubjectTeachingExamId { get; set; }

    public Guid? QuestionId { get; set; }

    public Guid? TeacherId { get; set; }

    public string? TeacherName { get; set; }

    public int Type { get; set; }

    public string? Comment { get; set; }

    public decimal? TotalScore { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? UpdatedDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual SemesterPlan? SemesterPlan { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual ICollection<StudentEvaluationDetail> StudentEvaluationDetails { get; set; } = new List<StudentEvaluationDetail>();

    public virtual SubjectTeaching? SubjectTeaching { get; set; }

    public virtual TeacherFaculty? Teacher { get; set; }
}
