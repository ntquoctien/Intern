using System;
using System.Collections.Generic;

namespace ExamService.Infrastructure.Persistence.Entities;

public partial class ExamQuestionAnswer
{
    public Guid Id { get; set; }

    public Guid ExamQuestionSelectionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public bool IsAnswer { get; set; }

    public virtual ExamQuestionSelection ExamQuestionSelection { get; set; } = null!;
}
