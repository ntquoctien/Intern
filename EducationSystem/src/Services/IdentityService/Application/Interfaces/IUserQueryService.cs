using IdentityService.Application.DTOs.Users;
using SharedKernel;

namespace IdentityService.Application.Interfaces;

public interface IUserQueryService
{
    Task<PagedResult<UserListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<UserDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<UserLookupDto>> GetLookupAsync(CancellationToken cancellationToken = default);
}
