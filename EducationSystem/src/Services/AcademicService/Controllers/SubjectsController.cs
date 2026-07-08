using AcademicService.Application.DTOs.Subjects;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/subjects")]
public sealed class SubjectsController : ControllerBase
{
    private readonly ISubjectQueryService _queryService;

    public SubjectsController(ISubjectQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SubjectListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<SubjectListItemDto>>.Ok(result));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<SubjectLookupDto>>>> GetLookup(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetLookupAsync(cancellationToken);
        return Ok(ApiResponse<List<SubjectLookupDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SubjectDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<SubjectDetailDto>.Fail("Subject not found."));
        }

        return Ok(ApiResponse<SubjectDetailDto>.Ok(result));
    }
}
