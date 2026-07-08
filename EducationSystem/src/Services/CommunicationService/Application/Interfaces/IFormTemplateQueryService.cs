using CommunicationService.Application.DTOs.FormTemplates;
using SharedKernel;

namespace CommunicationService.Application.Interfaces;

public interface IFormTemplateQueryService
{
    Task<PagedResult<FormTemplateListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<FormTemplateDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<FormTemplateLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
