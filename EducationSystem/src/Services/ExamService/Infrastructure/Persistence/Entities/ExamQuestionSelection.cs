using System;
using System.Collections.Generic;

namespace ExamService.Infrastructure.Persistence.Entities;

public partial class ExamQuestionSelection
{
    public Guid Id { get; set; }

    public Guid ExamAttemptId { get; set; }

    public Guid? AnswerId { get; set; }

    public string QuestionText { get; set; } = null!;

    public string ImageUrl { get; set; } = null!;

    public int Order { get; set; }

    public bool IsFlag { get; set; }

    public DateTime CreationDate { get; set; }

    public DateTime? SubmitDate { get; set; }

    public virtual ExamAttempt ExamAttempt { get; set; } = null!;

    public virtual ICollection<ExamQuestionAnswer> ExamQuestionAnswers { get; set; } = new List<ExamQuestionAnswer>();
}
