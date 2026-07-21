namespace CommunicationService.Application.DTOs.StudentAccess;

public sealed record StudentFormRequestDto(
    Guid FormRequestId,
    Guid? FormTemplateId,
    string? FormTemplateName,
    string? DocumentUrl,
    DateTime CreationDate,
    DateTime UpdateDate,
    int RawStatus,
    Guid? ApprovalId,
    string ApprovalName,
    string Note);
