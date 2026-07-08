namespace AcademicService.Application.DTOs.SemesterTuitions;

public sealed class SemesterTuitionListItemDto
{
    public Guid Id { get; init; }
    public Guid SemesterPlanId { get; init; }
    public Guid StudentId { get; init; }
    public decimal? Amount { get; init; }
    public DateTime? PaidDate { get; init; }
    public bool IsDeleted { get; init; }
}
