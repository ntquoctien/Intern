using ExamService.Application.DTOs.QuestionAnswers;
using ExamService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace ExamService.Controllers;

[ApiController]
[Route("api/exam/question-answers")]
public sealed class QuestionAnswersController : ControllerBase
{
    private readonly IQuestionAnswerQueryService _queryService;

    public QuestionAnswersController(IQuestionAnswerQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<QuestionAnswerListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<QuestionAnswerListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<QuestionAnswerDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<QuestionAnswerDetailDto>.Fail("QuestionAnswer not found."));
        }

        return Ok(ApiResponse<QuestionAnswerDetailDto>.Ok(result));
    }
}
