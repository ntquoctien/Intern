using ExamService.Application.DTOs.ExamAttempts;
using ExamService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace ExamService.Controllers;

[ApiController]
[Route("api/exam/exam-attempts")]
public sealed class ExamAttemptsController : ControllerBase
{
    private readonly IExamAttemptQueryService _queryService;

    public ExamAttemptsController(IExamAttemptQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ExamAttemptListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExamAttemptListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ExamAttemptDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<ExamAttemptDetailDto>.Fail("ExamAttempt not found."));
        }

        return Ok(ApiResponse<ExamAttemptDetailDto>.Ok(result));
    }
}
