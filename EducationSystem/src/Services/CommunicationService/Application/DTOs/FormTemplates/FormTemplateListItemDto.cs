namespace CommunicationService.Application.DTOs.FormTemplates;

public sealed class FormTemplateListItemDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? DocumentUrl { get; init; }
    public bool IsDeleted { get; init; }
}
