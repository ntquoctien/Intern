using IdentityService.Application.DTOs.UserDevices;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/identity/user-devices")]
public sealed class UserDevicesController : ControllerBase
{
    private readonly IUserDeviceQueryService _queryService;

    public UserDevicesController(IUserDeviceQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserDeviceListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserDeviceListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserDeviceDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<UserDeviceDetailDto>.Fail("UserDevice not found."));
        }

        return Ok(ApiResponse<UserDeviceDetailDto>.Ok(result));
    }
}
