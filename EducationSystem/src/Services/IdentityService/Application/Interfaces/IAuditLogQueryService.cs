using IdentityService.Application.DTOs.AuditLogs;
using SharedKernel;

namespace IdentityService.Application.Interfaces;

public interface IAuditLogQueryService
{
    Task<PagedResult<AuditLogListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<AuditLogDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
