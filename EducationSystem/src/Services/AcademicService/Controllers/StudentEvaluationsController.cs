using AcademicService.Application.DTOs.StudentEvaluations;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/student-evaluations")]
public sealed class StudentEvaluationsController : ControllerBase
{
    private readonly IStudentEvaluationQueryService _queryService;

    public StudentEvaluationsController(IStudentEvaluationQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<StudentEvaluationListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<StudentEvaluationListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<StudentEvaluationDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<StudentEvaluationDetailDto>.Fail("StudentEvaluation not found."));
        }

        return Ok(ApiResponse<StudentEvaluationDetailDto>.Ok(result));
    }
}
