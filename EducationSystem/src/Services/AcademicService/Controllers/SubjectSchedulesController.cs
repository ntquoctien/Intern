using AcademicService.Application.DTOs.SubjectSchedules;
using AcademicService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace AcademicService.Controllers;

[ApiController]
[Route("api/academic/subject-schedules")]
public sealed class SubjectSchedulesController : ControllerBase
{
    private readonly ISubjectScheduleQueryService _queryService;

    public SubjectSchedulesController(ISubjectScheduleQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<SubjectScheduleListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<SubjectScheduleListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<SubjectScheduleDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<SubjectScheduleDetailDto>.Fail("SubjectSchedule not found."));
        }

        return Ok(ApiResponse<SubjectScheduleDetailDto>.Ok(result));
    }
}
