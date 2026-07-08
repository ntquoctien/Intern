using AcademicService.Application.DTOs.Students;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/students")]
public sealed class StudentsController : ControllerBase
{
    private readonly IStudentQueryService _queryService;

    public StudentsController(IStudentQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<StudentListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<StudentListItemDto>>.Ok(result));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<StudentLookupDto>>>> GetLookup(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetLookupAsync(cancellationToken);
        return Ok(ApiResponse<List<StudentLookupDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StudentDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<StudentDetailDto>.Fail("Student not found."));
        }

        return Ok(ApiResponse<StudentDetailDto>.Ok(result));
    }
}
