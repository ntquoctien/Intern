using IdentityService.Application.DTOs.Management;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/management/system")]
public sealed class ManagementSystemReadController(IManagementSystemReadService service) : ControllerBase
{
    [HttpGet("overview")]
    public async Task<ApiResponse<ManagementSystemOverviewDto>> Overview(CancellationToken ct) =>
        ApiResponse<ManagementSystemOverviewDto>.Ok(await service.GetOverviewAsync(ct));
}
