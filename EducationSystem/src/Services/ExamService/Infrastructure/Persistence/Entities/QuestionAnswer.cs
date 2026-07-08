using System;
using System.Collections.Generic;

namespace ExamService.Infrastructure.Persistence.Entities;

public partial class QuestionAnswer
{
    public Guid Id { get; set; }

    public Guid QuestionId { get; set; }

    public string AnswerText { get; set; } = null!;

    public string? ImageUrl { get; set; }

    public bool IsAnswer { get; set; }

    public virtual Question Question { get; set; } = null!;
}
