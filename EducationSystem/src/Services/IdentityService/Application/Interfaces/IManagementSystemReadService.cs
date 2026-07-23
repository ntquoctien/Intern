using IdentityService.Application.DTOs.Management;

namespace IdentityService.Application.Interfaces;

public interface IManagementSystemReadService
{
    Task<ManagementSystemOverviewDto> GetOverviewAsync(CancellationToken cancellationToken);
}
