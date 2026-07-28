namespace CommunicationService.Application.DTOs.FormRequests;

public sealed class FormRequestDetailDto
{
    public Guid Id { get; init; }
    public DateTime CreationDate { get; init; }
    public DateTime UpdateDate { get; init; }
    public Guid StudentId { get; init; }
    public Guid? FormTemplateId { get; init; }
    public Guid? ApprovalId { get; init; }
    public string ApprovalName { get; init; } = string.Empty;
    public string Note { get; init; } = string.Empty;
    public int Status { get; init; }
    public int EmployerVerifiedStatus { get; init; }
    public string RequestType { get; init; } = string.Empty;
    public string StudentCode { get; init; } = string.Empty;
    public string StudentName { get; init; } = string.Empty;
    public string? CompanyName { get; init; }
    public string? Position { get; init; }
    public string? MentorEmail { get; init; }
    public DateOnly? StartDate { get; init; }
    public DateOnly? EndDate { get; init; }
    public string? TaskDescription { get; init; }
    public decimal? EmployerScore { get; init; }
    public string? EmployerEvaluationNotes { get; init; }
    public DateTime? EmployerVerifiedAt { get; init; }
    public bool IsDeleted { get; init; }
}
