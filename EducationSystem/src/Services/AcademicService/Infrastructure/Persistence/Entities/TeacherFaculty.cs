using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class TeacherFaculty
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public Guid FacultyId { get; set; }

    public bool IsHeadOfFaculty { get; set; }

    public bool IsDeleted { get; set; }

    public virtual Faculty Faculty { get; set; } = null!;

    public virtual ICollection<StudentEvaluation> StudentEvaluations { get; set; } = new List<StudentEvaluation>();

    public virtual ICollection<SubjectSchedule> SubjectSchedules { get; set; } = new List<SubjectSchedule>();

    public virtual ICollection<SubjectTeachingTeacher> SubjectTeachingTeachers { get; set; } = new List<SubjectTeachingTeacher>();
}
