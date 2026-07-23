using CommunicationService.Application.DTOs.Management;
using CommunicationService.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace CommunicationService.Controllers;

[ApiController]
[Route("api/management/announcements")]
public sealed class ManagementAnnouncementReadController(IManagementAnnouncementReadService service) : ControllerBase
{
    [HttpGet] public async Task<ApiResponse<ManagementAnnouncementPageDto>> Page([FromQuery] ManagementAnnouncementQueryDto query, CancellationToken ct) => ApiResponse<ManagementAnnouncementPageDto>.Ok(await service.GetPageAsync(query, ct));
    [HttpGet("{id:guid}")] public async Task<ActionResult<ApiResponse<ManagementAnnouncementItemDto>>> Detail(Guid id, CancellationToken ct) { var item = await service.GetAsync(id, ct); return item is null ? NotFound(ApiResponse<ManagementAnnouncementItemDto>.Fail("Announcement not found.")) : Ok(ApiResponse<ManagementAnnouncementItemDto>.Ok(item)); }
}
