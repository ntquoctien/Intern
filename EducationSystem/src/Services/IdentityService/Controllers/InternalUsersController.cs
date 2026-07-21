using IdentityService.Application.StudentAccess;
using IdentityService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SharedKernel;

namespace IdentityService.Controllers;

[ApiController]
[Route("api/internal/users")]
public sealed class InternalUsersController(IdentityDbContext dbContext) : ControllerBase
{
    [HttpGet("summaries")]
    public async Task<ActionResult<ApiResponse<IReadOnlyList<SafeUserProfileDto>>>> GetSummaries(
        [FromQuery] Guid[] ids,
        CancellationToken cancellationToken)
    {
        var safeIds = ids.Distinct().Take(100).ToArray();
        var profiles = await dbContext.Users.AsNoTracking()
            .Where(user => safeIds.Contains(user.Id) && user.IsActived && !user.IsDeleted)
            .OrderBy(user => user.FullName)
            .Select(user => new SafeUserProfileDto(
                user.Id,
                user.FullName,
                user.UserName,
                user.ProfilePicUrl,
                true))
            .ToListAsync(cancellationToken);
        return Ok(ApiResponse<IReadOnlyList<SafeUserProfileDto>>.Ok(profiles));
    }

    [HttpGet("{userId:guid}/student-profile")]
    public async Task<ActionResult<ApiResponse<SafeUserProfileDto>>> GetStudentProfile(
        Guid userId,
        CancellationToken cancellationToken)
    {
        var profile = await dbContext.Users.AsNoTracking()
            .Where(user => user.Id == userId && user.IsActived && !user.IsDeleted)
            .Select(user => new SafeUserProfileDto(
                user.Id,
                user.FullName,
                user.UserName,
                user.ProfilePicUrl,
                true))
            .SingleOrDefaultAsync(cancellationToken);
        return profile is null
            ? NotFound(ApiResponse<SafeUserProfileDto>.Fail(
                StudentErrorCodes.AccountUnavailable,
                "The linked student account is unavailable."))
            : Ok(ApiResponse<SafeUserProfileDto>.Ok(profile));
    }
}
