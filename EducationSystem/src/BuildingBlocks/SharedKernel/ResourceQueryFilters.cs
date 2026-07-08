namespace SharedKernel;

public sealed class ResourceQueryFilters
{
    public Guid? StudentId { get; set; }

    public Guid? SubjectScheduleId { get; set; }

    public Guid? SubjectTeachingId { get; set; }

    public Guid? SubjectTeachingExamId { get; set; }

    public Guid? UserId { get; set; }

    public int? Status { get; set; }

    public DateTime? FromDate { get; set; }

    public DateTime? ToDate { get; set; }
}
