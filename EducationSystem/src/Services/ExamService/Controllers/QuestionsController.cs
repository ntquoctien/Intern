using ExamService.Application.DTOs.Questions;
using ExamService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace ExamService.Controllers;

[ApiController]
[Route("api/exam/questions")]
public sealed class QuestionsController : ControllerBase
{
    private readonly IQuestionQueryService _queryService;

    public QuestionsController(IQuestionQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<QuestionListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<QuestionListItemDto>>.Ok(result));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<QuestionLookupDto>>>> GetLookup(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetLookupAsync(cancellationToken);
        return Ok(ApiResponse<List<QuestionLookupDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<QuestionDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<QuestionDetailDto>.Fail("Question not found."));
        }

        return Ok(ApiResponse<QuestionDetailDto>.Ok(result));
    }
}
