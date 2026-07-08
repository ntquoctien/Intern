using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class SubjectSchedule
{
    public Guid Id { get; set; }

    public Guid SubjectTeachingId { get; set; }

    public Guid? RoomId { get; set; }

    public Guid? TeacherId { get; set; }

    public DateTime StartDateTime { get; set; }

    public DateTime EndDateTime { get; set; }

    public int? ScheduleType { get; set; }

    public string Note { get; set; } = null!;

    public bool IsDeleted { get; set; }

    public virtual ICollection<Attendance> Attendances { get; set; } = new List<Attendance>();

    public virtual Room? Room { get; set; }

    public virtual SubjectTeaching SubjectTeaching { get; set; } = null!;

    public virtual TeacherFaculty? Teacher { get; set; }
}
