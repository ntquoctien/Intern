using CommunicationService.Application.DTOs.StudentAccess;
using CommunicationService.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedKernel;

namespace CommunicationService.Controllers;

[ApiController]
[Authorize(Policy = StudentClaimTypes.StudentPolicy)]
[Route("api/student/me/announcements")]
public sealed class StudentAnnouncementsController(
    IStudentAnnouncementService announcementService,
    ICurrentStudentAccessor currentStudentAccessor) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<StudentAnnouncementDto>>>> GetAll(
        CancellationToken cancellationToken)
    {
        var current = currentStudentAccessor.GetRequiredStudent();
        if (!current.UserId.HasValue)
        {
            return Unauthorized(ApiResponse<IReadOnlyList<StudentAnnouncementDto>>.Fail(
                StudentErrorCodes.Unauthorized, "The student user identity is unavailable."));
        }

        return Ok(ApiResponse<IReadOnlyList<StudentAnnouncementDto>>.Ok(
            await announcementService.GetAllAsync(current.UserId.Value, cancellationToken)));
    }
}
