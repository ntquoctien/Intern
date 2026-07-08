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
    public bool IsDeleted { get; init; }
}
