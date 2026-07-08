using AcademicService.Application.DTOs.SemesterTuitions;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/semester-tuitions")]
public sealed class SemesterTuitionsController : ControllerBase
{
    private readonly ISemesterTuitionQueryService _queryService;

    public SemesterTuitionsController(ISemesterTuitionQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SemesterTuitionListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<SemesterTuitionListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SemesterTuitionDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<SemesterTuitionDetailDto>.Fail("SemesterTuition not found."));
        }

        return Ok(ApiResponse<SemesterTuitionDetailDto>.Ok(result));
    }
}
