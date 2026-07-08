using IdentityService.Application.DTOs.PasswordResets;
using SharedKernel;

namespace IdentityService.Application.Interfaces;

public interface IPasswordResetQueryService
{
    Task<PagedResult<PasswordResetListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<PasswordResetDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
