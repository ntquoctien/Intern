using AcademicService.Application.DTOs.SubjectStudents;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/subject-students")]
public sealed class SubjectStudentsController : ControllerBase
{
    private readonly ISubjectStudentQueryService _queryService;

    public SubjectStudentsController(ISubjectStudentQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SubjectStudentListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<SubjectStudentListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SubjectStudentDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<SubjectStudentDetailDto>.Fail("SubjectStudent not found."));
        }

        return Ok(ApiResponse<SubjectStudentDetailDto>.Ok(result));
    }
}
