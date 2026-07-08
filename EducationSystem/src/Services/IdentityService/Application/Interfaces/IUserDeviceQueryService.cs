using IdentityService.Application.DTOs.UserDevices;
using SharedKernel;

namespace IdentityService.Application.Interfaces;

public interface IUserDeviceQueryService
{
    Task<PagedResult<UserDeviceListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<UserDeviceDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
