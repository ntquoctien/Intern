using ExamService.Application.DTOs.ExamQuestionSelections;
using ExamService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace ExamService.Controllers;

[ApiController]
[Route("api/exam/exam-question-selections")]
public sealed class ExamQuestionSelectionsController : ControllerBase
{
    private readonly IExamQuestionSelectionQueryService _queryService;

    public ExamQuestionSelectionsController(IExamQuestionSelectionQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ExamQuestionSelectionListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExamQuestionSelectionListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ExamQuestionSelectionDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<ExamQuestionSelectionDetailDto>.Fail("ExamQuestionSelection not found."));
        }

        return Ok(ApiResponse<ExamQuestionSelectionDetailDto>.Ok(result));
    }
}
