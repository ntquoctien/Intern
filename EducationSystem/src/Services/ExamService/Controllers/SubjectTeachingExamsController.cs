using ExamService.Application.DTOs.SubjectTeachingExams;
using ExamService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace ExamService.Controllers;

[ApiController]
[Route("api/exam/subject-teaching-exams")]
public sealed class SubjectTeachingExamsController : ControllerBase
{
    private readonly ISubjectTeachingExamQueryService _queryService;

    public SubjectTeachingExamsController(ISubjectTeachingExamQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SubjectTeachingExamListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<SubjectTeachingExamListItemDto>>.Ok(result));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<SubjectTeachingExamLookupDto>>>> GetLookup(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetLookupAsync(cancellationToken);
        return Ok(ApiResponse<List<SubjectTeachingExamLookupDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SubjectTeachingExamDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<SubjectTeachingExamDetailDto>.Fail("SubjectTeachingExam not found."));
        }

        return Ok(ApiResponse<SubjectTeachingExamDetailDto>.Ok(result));
    }
}
