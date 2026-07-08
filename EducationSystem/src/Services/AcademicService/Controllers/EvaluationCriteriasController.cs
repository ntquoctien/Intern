using AcademicService.Application.DTOs.EvaluationCriterias;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/evaluation-criterias")]
public sealed class EvaluationCriteriasController : ControllerBase
{
    private readonly IEvaluationCriteriaQueryService _queryService;

    public EvaluationCriteriasController(IEvaluationCriteriaQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<EvaluationCriteriaListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<EvaluationCriteriaListItemDto>>.Ok(result));
    }

    [HttpGet("lookup")]
    public async Task<ActionResult<ApiResponse<List<EvaluationCriteriaLookupDto>>>> GetLookup(CancellationToken cancellationToken)
    {
        var result = await _queryService.GetLookupAsync(cancellationToken);
        return Ok(ApiResponse<List<EvaluationCriteriaLookupDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<EvaluationCriteriaDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<EvaluationCriteriaDetailDto>.Fail("EvaluationCriteria not found."));
        }

        return Ok(ApiResponse<EvaluationCriteriaDetailDto>.Ok(result));
    }
}
