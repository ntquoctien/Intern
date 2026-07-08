using ExamService.Application.DTOs.ExamQuestionAnswers;
using ExamService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace ExamService.Controllers;

[ApiController]
[Route("api/exam/exam-question-answers")]
public sealed class ExamQuestionAnswersController : ControllerBase
{
    private readonly IExamQuestionAnswerQueryService _queryService;

    public ExamQuestionAnswersController(IExamQuestionAnswerQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<ExamQuestionAnswerListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<ExamQuestionAnswerListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<ExamQuestionAnswerDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<ExamQuestionAnswerDetailDto>.Fail("ExamQuestionAnswer not found."));
        }

        return Ok(ApiResponse<ExamQuestionAnswerDetailDto>.Ok(result));
    }
}
