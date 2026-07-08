using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class Room
{
    public Guid Id { get; set; }

    public string Name { get; set; } = null!;

    public int NumberOfSeats { get; set; }

    public bool IsDeleted { get; set; }

    public virtual ICollection<SubjectSchedule> SubjectSchedules { get; set; } = new List<SubjectSchedule>();

    public virtual ICollection<SubjectTeaching> SubjectTeachings { get; set; } = new List<SubjectTeaching>();
}
