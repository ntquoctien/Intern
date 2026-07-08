using System;
using System.Collections.Generic;

namespace ExamService.Infrastructure.Persistence.Entities;

public partial class ExamResult
{
    public Guid Id { get; set; }

    public Guid SubjectTeachingExamId { get; set; }

    public Guid StudentId { get; set; }

    public Guid? ExamAttemptId { get; set; }

    public float? Result { get; set; }

    public float? CombinedResult { get; set; }

    public string? Notes { get; set; }

    public string? ExamResultDesc { get; set; }

    public string? ExamResultDetail { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ExamAttempt? ExamAttempt { get; set; }

    public virtual SubjectTeachingExam SubjectTeachingExam { get; set; } = null!;
}
