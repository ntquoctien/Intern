using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class Faculty
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public string Code { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Major> Majors { get; set; } = new List<Major>();

    public virtual ICollection<Subject> Subjects { get; set; } = new List<Subject>();

    public virtual ICollection<TeacherFaculty> TeacherFaculties { get; set; } = new List<TeacherFaculty>();
}
