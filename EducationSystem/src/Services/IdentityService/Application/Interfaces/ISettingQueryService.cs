using IdentityService.Application.DTOs.Settings;
using SharedKernel;

namespace IdentityService.Application.Interfaces;

public interface ISettingQueryService
{
    Task<PagedResult<SettingListItemDto>> GetPagedAsync(QueryParameters query, ResourceQueryFilters? filters = null, CancellationToken cancellationToken = default);

    Task<SettingDetailDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);}
