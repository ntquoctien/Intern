using System;
using System.Collections.Generic;

namespace ExamService.Infrastructure.Persistence.Entities;

public partial class SubjectTeachingExam
{
    public Guid Id { get; set; }

    public Guid SubjectTeachingId { get; set; }

    public Guid? QuestionSuiteId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public Guid? RoomId { get; set; }

    public Guid? TeacherId { get; set; }

    public string Notes { get; set; } = null!;

    public int Type { get; set; }

    public int Count { get; set; }

    public int NumOfEasy { get; set; }

    public int NumOfNormal { get; set; }

    public int NumOfHard { get; set; }

    public int NumOfPractice { get; set; }

    public int? Method { get; set; }

    public bool AllowNotifyStudent { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<ExamAttempt> ExamAttempts { get; set; } = new List<ExamAttempt>();

    public virtual ICollection<ExamResult> ExamResults { get; set; } = new List<ExamResult>();

    public virtual QuestionSuite? QuestionSuite { get; set; }
}
