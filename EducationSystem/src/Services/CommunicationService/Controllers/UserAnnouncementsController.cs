using CommunicationService.Application.DTOs.UserAnnouncements;
using CommunicationService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace CommunicationService.Controllers;

[ApiController]
[Route("api/communication/user-announcements")]
public sealed class UserAnnouncementsController : ControllerBase
{
    private readonly IUserAnnouncementQueryService _queryService;

    public UserAnnouncementsController(IUserAnnouncementQueryService queryService)
    {
        _queryService = queryService;
    }

    [HttpGet]
    public async Task<ActionResult<ApiResponse<PagedResult<UserAnnouncementListItemDto>>>> GetPaged(
        [FromQuery] QueryParameters query,
        [FromQuery] ResourceQueryFilters filters,
        CancellationToken cancellationToken)
    {
        var result = await _queryService.GetPagedAsync(query, filters, cancellationToken);
        return Ok(ApiResponse<PagedResult<UserAnnouncementListItemDto>>.Ok(result));
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ApiResponse<UserAnnouncementDetailDto>>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var result = await _queryService.GetByIdAsync(id, cancellationToken);
        if (result is null)
        {
            return NotFound(ApiResponse<UserAnnouncementDetailDto>.Fail("UserAnnouncement not found."));
        }

        return Ok(ApiResponse<UserAnnouncementDetailDto>.Ok(result));
    }
}
