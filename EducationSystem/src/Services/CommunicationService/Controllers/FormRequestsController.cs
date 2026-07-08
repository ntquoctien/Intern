using CommunicationService.Application.DTOs.FormRequests;
using CommunicationService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace CommunicationService.Controllers;

[ApiController]
[Route("api/communication/form-requests")]
public sealed class FormRequestsController : ControllerBase
{
    private readonly IFormRequestQueryService _queryService;

    public FormRequestsController(IFormRequestQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<FormRequestListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<FormRequestListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<FormRequestDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<FormRequestDetailDto>.Fail("FormRequest not found."));
        }

        return Ok(ApiResponse<FormRequestDetailDto>.Ok(result));
    }
}
