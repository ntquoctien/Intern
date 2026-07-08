using CommunicationService.Application.DTOs.FormRequests;
using SharedKernel;

namespace CommunicationService.Application.Interfaces;

public interface IFormRequestQueryService
{
    Task<PagedResult<FormRequestListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<FormRequestDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
