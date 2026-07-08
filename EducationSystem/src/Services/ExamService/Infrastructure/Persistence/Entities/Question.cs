using System;
using System.Collections.Generic;

namespace ExamService.Infrastructure.Persistence.Entities;

public partial class Question
{
    public Guid Id { get; set; }

    public Guid QuestionSuiteId { get; set; }

    public string QuestionText { get; set; } = null!;

    public int Level { get; set; }

    public string? ImageUrl { get; set; }

    public virtual ICollection<QuestionAnswer> QuestionAnswers { get; set; } = new List<QuestionAnswer>();

    public virtual QuestionSuite QuestionSuite { get; set; } = null!;
}
