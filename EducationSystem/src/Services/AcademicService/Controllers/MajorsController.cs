using AcademicService.Application.DTOs.Majors;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/majors")]
public sealed class MajorsController : ControllerBase
{
    private readonly IMajorQueryService _queryService;

    public MajorsController(IMajorQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<MajorListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<MajorListItemDto>>.Ok(result));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<MajorLookupDto>>>> GetLookup(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetLookupAsync(cancellationToken);
        return Ok(ApiResponse<List<MajorLookupDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<MajorDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<MajorDetailDto>.Fail("Major not found."));
        }

        return Ok(ApiResponse<MajorDetailDto>.Ok(result));
    }
}
