using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class SubjectTeachingTeacher
{
    public Guid Id { get; set; }

    public Guid SubjectTeachingId { get; set; }

    public Guid? TeacherId { get; set; }

    public bool? IsMainTeacher { get; set; }

    public bool IsDeleted { get; set; }

    public virtual SubjectTeaching SubjectTeaching { get; set; } = null!;

    public virtual TeacherFaculty? Teacher { get; set; }
}
