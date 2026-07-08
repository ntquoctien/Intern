using IdentityService.Application.DTOs.PasswordResets;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/identity/password-resets")]
public sealed class PasswordResetsController : ControllerBase
{
    private readonly IPasswordResetQueryService _queryService;

    public PasswordResetsController(IPasswordResetQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<PasswordResetListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<PasswordResetListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<PasswordResetDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<PasswordResetDetailDto>.Fail("PasswordReset not found."));
        }

        return Ok(ApiResponse<PasswordResetDetailDto>.Ok(result));
    }
}
