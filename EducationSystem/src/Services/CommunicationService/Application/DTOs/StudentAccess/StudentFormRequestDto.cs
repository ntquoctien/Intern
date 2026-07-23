namespace CommunicationService.Application.DTOs.StudentAccess;

public sealed record StudentFormRequestDto(
    Guid FormRequestId,
    Guid? FormTemplateId,
    string? FormTemplateName,
    string? SafeDocumentUrl,
    DateTime CreationDate,
    DateTime UpdateDate,
    int RawStatus,
    string ApprovalName,
    string Note);

public sealed record StudentFormTemplateDto(
    Guid FormTemplateId,
    string Name,
    string? SafeDocumentUrl);
