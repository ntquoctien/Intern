using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class SubjectTeaching
{
    public Guid Id { get; set; }

    public Guid SubjectId { get; set; }

    public string Name { get; set; } = null!;

    public DateTime StartDate { get; set; }

    public DateTime EndDate { get; set; }

    public int TotalSessions { get; set; }

    public Guid? RoomIdDefault { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Room? RoomIdDefaultNavigation { get; set; }

    public virtual ICollection<StudentEvaluation> StudentEvaluations { get; set; } = new List<StudentEvaluation>();

    public virtual Subject Subject { get; set; } = null!;

    public virtual ICollection<SubjectSchedule> SubjectSchedules { get; set; } = new List<SubjectSchedule>();

    public virtual ICollection<SubjectStudent> SubjectStudents { get; set; } = new List<SubjectStudent>();

    public virtual ICollection<SubjectTeachingTeacher> SubjectTeachingTeachers { get; set; } = new List<SubjectTeachingTeacher>();
}
