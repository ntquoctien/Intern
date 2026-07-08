using IdentityService.Application.DTOs.AuditLogs;
using IdentityService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/identity/audit-logs")]
public sealed class AuditLogsController : ControllerBase
{
    private readonly IAuditLogQueryService _queryService;

    public AuditLogsController(IAuditLogQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<AuditLogListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<AuditLogListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<AuditLogDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<AuditLogDetailDto>.Fail("AuditLog not found."));
        }

        return Ok(ApiResponse<AuditLogDetailDto>.Ok(result));
    }
}
