using System;
using System.Collections.Generic;

namespace ExamService.Infrastructure.Persistence.Entities;

public partial class QuestionSuite
{
    public Guid Id { get; set; }

    public Guid SubjectId { get; set; }

    public string Name { get; set; } = null!;

    public Guid? UpdatedById { get; set; }

    public DateTime CreationTime { get; set; }

    public virtual ICollection<Question> Questions { get; set; } = new List<Question>();

    public virtual ICollection<SubjectTeachingExam> SubjectTeachingExams { get; set; } = new List<SubjectTeachingExam>();
}
