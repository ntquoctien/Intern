using System;
using System.Collections.Generic;

namespace AcademicService.Infrastructure.Persistence.Entities;

public partial class Attendance
{
    public Guid Id { get; set; }

    public Guid SubjectScheduleId { get; set; }

    public Guid StudentId { get; set; }

    public Guid CreatedById { get; set; }

    public int Status { get; set; }

    public string Notes { get; set; } = null!;

    public DateTime CreationDate { get; set; }

    public bool? IsFirstTypeWarning { get; set; }

    public bool? IsSecondTypeWarning { get; set; }

    public virtual Student Student { get; set; } = null!;

    public virtual SubjectSchedule SubjectSchedule { get; set; } = null!;
}
