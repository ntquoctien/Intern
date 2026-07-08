using System;
using System.Collections.Generic;

namespace ExamService.Infrastructure.Persistence.Entities;

public partial class ExamAttempt
{
    public Guid Id { get; set; }

    public Guid StudentId { get; set; }

    public Guid SubjectTeachingExamId { get; set; }

    public DateTime? DraftDate { get; set; }

    public DateTime? SubmitDate { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<ExamQuestionSelection> ExamQuestionSelections { get; set; } = new List<ExamQuestionSelection>();

    public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();

    public virtual SubjectTeachingExam SubjectTeachingExam { get; set; } = null!;
}
