namespace CommunicationService.Application.DTOs.FormTemplates;

public sealed class FormTemplateLookupDto
{
    public Guid Id { get; init; }
    public string Name { get; init; } = string.Empty;
}
